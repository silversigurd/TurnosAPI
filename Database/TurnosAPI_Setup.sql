-- ============================================================
-- TurnosAPI_Setup.sql
-- Script de configuración completo de base de datos para TurnosAPI
--
-- Uso:
--   Opción A (Recomendada): Correr el proyecto con F5 primero (EF crea las tablas),
--                           luego ejecutar SOLO la sección de Stored Procedures.
--   Opción B (Standalone):  Ejecutar este script completo para setup manual sin correr el proyecto.
--
-- Requiere: SQL Server o LocalDB
-- ============================================================

USE TurnosDB;
GO

-- ============================================================
-- TABLAS
-- Se incluyen para referencia y setup manual.
-- Si ya corriste el proyecto (EF las crea automáticamente),
-- estas sentencias no harán nada (IF NOT EXISTS).
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        Id       INT            IDENTITY(1,1) PRIMARY KEY,
        Nombre   NVARCHAR(100)  NOT NULL,
        Telefono NVARCHAR(20)   NULL,
        Email    NVARCHAR(150)  NULL
    );
    PRINT 'Tabla Clientes creada.';
END
ELSE
    PRINT 'Tabla Clientes ya existe — omitida.';
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Profesionales')
BEGIN
    CREATE TABLE Profesionales (
        Id           INT            IDENTITY(1,1) PRIMARY KEY,
        Nombre       NVARCHAR(100)  NOT NULL,
        Especialidad NVARCHAR(100)  NOT NULL
    );
    PRINT 'Tabla Profesionales creada.';
END
ELSE
    PRINT 'Tabla Profesionales ya existe — omitida.';
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Turnos')
BEGIN
    CREATE TABLE Turnos (
        Id              INT       IDENTITY(1,1) PRIMARY KEY,
        ClienteId       INT       NOT NULL,
        ProfesionalId   INT       NOT NULL,
        FechaHoraInicio DATETIME2 NOT NULL,
        FechaHoraFin    DATETIME2 NOT NULL,
        -- Estado: 0=Pendiente, 1=Confirmado, 2=Cancelado (enum almacenado como INT)
        Estado          INT       NOT NULL DEFAULT 0,
        CONSTRAINT FK_Turnos_Clientes
            FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
        CONSTRAINT FK_Turnos_Profesionales
            FOREIGN KEY (ProfesionalId) REFERENCES Profesionales(Id)
    );
    PRINT 'Tabla Turnos creada.';
END
ELSE
    PRINT 'Tabla Turnos ya existe — omitida.';
GO


-- ============================================================
-- STORED PROCEDURE 1: sp_ValidarSuperposicion
-- ============================================================
-- Propósito: verificar si existe algún turno NO cancelado para un
-- profesional dado que se superponga con el rango [FechaHoraInicio, FechaHoraFin].
--
-- Lógica matemática de superposición de rangos:
--   Dos intervalos [A,B] y [C,D] se superponen si:  A < D  AND  B > C
--   (equivalente a: NO (B <= C OR A >= D))
--
-- Devuelve: un entero (COUNT) — si > 0, hay superposición.
-- El parámetro @ExcluirTurnoId es para uso futuro (si se agrega edición de turnos).
-- ============================================================
CREATE OR ALTER PROCEDURE sp_ValidarSuperposicion
    @ProfesionalId   INT,
    @FechaHoraInicio DATETIME2,
    @FechaHoraFin    DATETIME2,
    @ExcluirTurnoId  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)
    FROM Turnos
    WHERE ProfesionalId = @ProfesionalId
      AND Estado != 2                         -- excluir cancelados (soft-deleted)
      AND FechaHoraInicio < @FechaHoraFin    -- el turno existente empieza antes del fin del nuevo
      AND FechaHoraFin    > @FechaHoraInicio -- el turno existente termina después del inicio del nuevo
      AND (@ExcluirTurnoId IS NULL OR Id != @ExcluirTurnoId);
END;
GO


-- ============================================================
-- STORED PROCEDURE 2: sp_TurnosPorCliente
-- ============================================================
-- Propósito: devolver todos los turnos de un cliente con el nombre
-- del profesional incluido (JOIN entre Turnos y Profesionales).
--
-- Por qué SP en vez de LINQ/Include():
--   Solo necesitamos el nombre del profesional, no el objeto completo.
--   El SP con JOIN proyecta exactamente los campos necesarios,
--   sin cargar entidades relacionadas en memoria innecesariamente.
--   El Estado se convierte a texto en SQL para que el cliente no necesite
--   conocer los valores del enum (0, 1, 2).
-- ============================================================
CREATE OR ALTER PROCEDURE sp_TurnosPorCliente
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.Id,
        t.ClienteId,
        t.ProfesionalId,
        p.Nombre AS NombreProfesional,
        t.FechaHoraInicio,
        t.FechaHoraFin,
        -- Convertir el enum a texto directamente en SQL
        CASE t.Estado
            WHEN 0 THEN 'Pendiente'
            WHEN 1 THEN 'Confirmado'
            WHEN 2 THEN 'Cancelado'
            ELSE 'Desconocido'
        END AS Estado
    FROM Turnos t
    INNER JOIN Profesionales p ON t.ProfesionalId = p.Id
    WHERE t.ClienteId = @ClienteId
    ORDER BY t.FechaHoraInicio DESC;
END;
GO


-- ============================================================
-- STORED PROCEDURE 3: sp_ProximoTurnoDisponible
-- ============================================================
-- Propósito: encontrar el primer slot de tiempo libre para un profesional
-- dado, a partir de una fecha indicada, con la duración solicitada.
--
-- Algoritmo (iterativo con WHILE):
--   1. Candidato inicial: [@FechaDesde, @FechaDesde + @DuracionMinutos]
--   2. ¿Hay algún turno no cancelado que se superponga con el candidato?
--      - SÍ: saltar al final del turno más tardío que interfiere (MAX de FechaHoraFin).
--            Recalcular el candidato. Repetir.
--      - NO: ese slot está libre. Retornar.
--   3. Límite de 100 iteraciones como medida de seguridad contra bucles infinitos.
--
-- Por qué SP en vez de LINQ:
--   La búsqueda de huecos con WHILE en SQL evita cargar todos los turnos futuros
--   en memoria C# para buscar un hueco. Más eficiente y la lógica es más clara en SQL.
-- ============================================================
CREATE OR ALTER PROCEDURE sp_ProximoTurnoDisponible
    @ProfesionalId  INT,
    @DuracionMinutos INT,
    @FechaDesde     DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ProximoInicio DATETIME2 = @FechaDesde;
    DECLARE @ProximoFin    DATETIME2 = DATEADD(MINUTE, @DuracionMinutos, @FechaDesde);
    DECLARE @HayConflicto  BIT       = 1;
    DECLARE @MaxIteraciones INT      = 100; -- límite de seguridad
    DECLARE @Iteracion      INT      = 0;

    WHILE @HayConflicto = 1 AND @Iteracion < @MaxIteraciones
    BEGIN
        -- ¿Hay algún turno (no cancelado) que interfiera con el candidato actual?
        IF EXISTS (
            SELECT 1
            FROM   Turnos
            WHERE  ProfesionalId = @ProfesionalId
              AND  Estado != 2
              AND  FechaHoraInicio < @ProximoFin
              AND  FechaHoraFin    > @ProximoInicio
        )
        BEGIN
            -- Hay conflicto: saltar al final del bloque de turnos superpuestos.
            -- MAX(FechaHoraFin) maneja el caso de múltiples turnos encadenados que se solapan.
            SELECT @ProximoInicio = MAX(FechaHoraFin)
            FROM   Turnos
            WHERE  ProfesionalId = @ProfesionalId
              AND  Estado != 2
              AND  FechaHoraInicio < @ProximoFin
              AND  FechaHoraFin    > @ProximoInicio;

            SET @ProximoFin = DATEADD(MINUTE, @DuracionMinutos, @ProximoInicio);
        END
        ELSE
        BEGIN
            -- Sin conflicto: slot disponible encontrado
            SET @HayConflicto = 0;
        END

        SET @Iteracion = @Iteracion + 1;
    END

    -- Devolver el slot disponible encontrado
    SELECT
        @ProximoInicio AS FechaHoraInicio,
        @ProximoFin    AS FechaHoraFin;
END;
GO

PRINT '✓ Stored procedures creados correctamente.';
PRINT '  - sp_ValidarSuperposicion';
PRINT '  - sp_TurnosPorCliente';
PRINT '  - sp_ProximoTurnoDisponible';
GO

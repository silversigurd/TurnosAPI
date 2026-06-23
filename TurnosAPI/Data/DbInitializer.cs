using Microsoft.EntityFrameworkCore;

namespace TurnosAPI.Data;

// Crea la base de datos, las tablas y los stored procedures al arrancar la app.
// Se ejecuta solo en Development. Los SPs usan CREATE OR ALTER, así que son seguros de correr siempre.
public static class DbInitializer
{
    public static async Task InicializarAsync(TurnosDbContext context)
    {
        context.Database.EnsureCreated();
        await CrearStoredProceduresAsync(context);
    }

    private static async Task CrearStoredProceduresAsync(TurnosDbContext context)
    {
        // Cada SP va en su propio ExecuteSqlRawAsync porque SQL Server no permite
        // mezclar CREATE/ALTER PROCEDURE con otras instrucciones en el mismo batch.

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE OR ALTER PROCEDURE sp_ValidarSuperposicion
                @ProfesionalId   INT,
                @FechaHoraInicio DATETIME2,
                @FechaHoraFin    DATETIME2,
                @ExcluirTurnoId  INT = NULL
            AS
            BEGIN
                SET NOCOUNT ON;

                -- Dos rangos [A,B] y [C,D] se superponen si A < D AND B > C
                SELECT COUNT(*)
                FROM Turnos
                WHERE ProfesionalId = @ProfesionalId
                  AND Estado != 2
                  AND FechaHoraInicio < @FechaHoraFin
                  AND FechaHoraFin    > @FechaHoraInicio
                  AND (@ExcluirTurnoId IS NULL OR Id != @ExcluirTurnoId);
            END;
        ");

        await context.Database.ExecuteSqlRawAsync(@"
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
        ");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE OR ALTER PROCEDURE sp_ProximoTurnoDisponible
                @ProfesionalId   INT,
                @DuracionMinutos INT,
                @FechaDesde      DATETIME2
            AS
            BEGIN
                SET NOCOUNT ON;

                DECLARE @ProximoInicio DATETIME2 = @FechaDesde;
                DECLARE @ProximoFin    DATETIME2 = DATEADD(MINUTE, @DuracionMinutos, @FechaDesde);
                DECLARE @HayConflicto  BIT       = 1;
                DECLARE @MaxIteraciones INT      = 100;
                DECLARE @Iteracion      INT      = 0;

                -- Buscar iterativamente el primer slot libre
                WHILE @HayConflicto = 1 AND @Iteracion < @MaxIteraciones
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM Turnos
                        WHERE  ProfesionalId = @ProfesionalId
                          AND  Estado != 2
                          AND  FechaHoraInicio < @ProximoFin
                          AND  FechaHoraFin    > @ProximoInicio
                    )
                    BEGIN
                        -- Hay conflicto: saltar al final del bloque de turnos superpuestos
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
                        SET @HayConflicto = 0;
                    END

                    SET @Iteracion = @Iteracion + 1;
                END

                SELECT @ProximoInicio AS FechaHoraInicio, @ProximoFin AS FechaHoraFin;
            END;
        ");
    }
}

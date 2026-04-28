-- =====================================================
-- SCRIPT 2: ÍNDICES Y TRIGGERS
-- Sistema de Reservas tipo Booking
-- Ejecutar DESPUÉS de 01_CrearBaseDatos_Tablas.sql
-- =====================================================

USE BookingDB;
GO

-- =====================================================
-- ÍNDICES PARA RENDIMIENTO
-- =====================================================

-- Búsquedas de propiedades por ciudad y tipo
CREATE NONCLUSTERED INDEX IX_Propiedades_Ciudad ON Propiedades(CiudadId) WHERE EliminadoLogico = 0;
CREATE NONCLUSTERED INDEX IX_Propiedades_Tipo ON Propiedades(TipoAlojamientoId) WHERE EliminadoLogico = 0;
CREATE NONCLUSTERED INDEX IX_Propiedades_Colaborador ON Propiedades(ColaboradorId) WHERE EliminadoLogico = 0;
CREATE NONCLUSTERED INDEX IX_Propiedades_Estado ON Propiedades(Estado) WHERE EliminadoLogico = 0;

-- Búsquedas de disponibilidad
CREATE NONCLUSTERED INDEX IX_Disponibilidad_Fecha ON Disponibilidad(Fecha, Disponible);

-- Reservas por cliente y propiedad
CREATE NONCLUSTERED INDEX IX_Reservas_Cliente ON Reservas(ClienteId) WHERE EliminadoLogico = 0;
CREATE NONCLUSTERED INDEX IX_Reservas_Propiedad ON Reservas(PropiedadId) WHERE EliminadoLogico = 0;
CREATE NONCLUSTERED INDEX IX_Reservas_Estado ON Reservas(Estado) WHERE EliminadoLogico = 0;
CREATE NONCLUSTERED INDEX IX_Reservas_Fechas ON Reservas(FechaCheckIn, FechaCheckOut);

-- Usuarios por email
CREATE NONCLUSTERED INDEX IX_Usuarios_Email ON Usuarios(Email) WHERE EliminadoLogico = 0;

-- Habitaciones por propiedad
CREATE NONCLUSTERED INDEX IX_Habitaciones_Propiedad ON Habitaciones(PropiedadId) WHERE EliminadoLogico = 0;

-- Tarifas por habitación y fecha
CREATE NONCLUSTERED INDEX IX_Tarifas_Habitacion_Fecha ON Tarifas(HabitacionId, FechaInicio, FechaFin) WHERE EliminadoLogico = 0;

-- Auditoría
CREATE NONCLUSTERED INDEX IX_Auditoria_Tabla ON AuditoriaGeneral(NombreTabla, FechaAccion);

PRINT '✅ Índices creados correctamente';
GO

-- =====================================================
-- TRIGGERS DE AUDITORÍA GENÉRICOS
-- =====================================================

-- Trigger de auditoría para Usuarios
CREATE OR ALTER TRIGGER trg_Audit_Usuarios
ON Usuarios
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Operacion NVARCHAR(10);
    
    IF EXISTS(SELECT 1 FROM inserted) AND EXISTS(SELECT 1 FROM deleted)
        SET @Operacion = 'UPDATE';
    ELSE IF EXISTS(SELECT 1 FROM inserted)
        SET @Operacion = 'INSERT';
    ELSE
        SET @Operacion = 'DELETE';

    IF @Operacion IN ('INSERT', 'UPDATE')
    BEGIN
        INSERT INTO AuditoriaGeneral (NombreTabla, Operacion, RegistroId, DatosNuevos, UsuarioAccion)
        SELECT 'Usuarios', @Operacion, CAST(i.UsuarioId AS NVARCHAR(50)),
            CONCAT('Email:', i.Email, '|Nombre:', i.NombreCompleto, '|RolId:', i.RolId, '|Estado:', i.Estado),
            i.UsuarioCreacion
        FROM inserted i;
    END

    IF @Operacion IN ('UPDATE', 'DELETE')
    BEGIN
        UPDATE ag SET DatosAnteriores = CONCAT('Email:', d.Email, '|Nombre:', d.NombreCompleto, '|RolId:', d.RolId, '|Estado:', d.Estado)
        FROM AuditoriaGeneral ag
        INNER JOIN deleted d ON CAST(d.UsuarioId AS NVARCHAR(50)) = ag.RegistroId
        WHERE ag.NombreTabla = 'Usuarios' AND ag.Operacion = @Operacion
          AND ag.AuditoriaId = (SELECT MAX(AuditoriaId) FROM AuditoriaGeneral WHERE NombreTabla = 'Usuarios' AND RegistroId = CAST(d.UsuarioId AS NVARCHAR(50)));
    END

    IF @Operacion = 'DELETE'
    BEGIN
        INSERT INTO AuditoriaGeneral (NombreTabla, Operacion, RegistroId, DatosAnteriores)
        SELECT 'Usuarios', 'DELETE', CAST(d.UsuarioId AS NVARCHAR(50)),
            CONCAT('Email:', d.Email, '|Nombre:', d.NombreCompleto, '|RolId:', d.RolId)
        FROM deleted d;
    END
END;
GO

-- Trigger de auditoría para Clientes
CREATE OR ALTER TRIGGER trg_Audit_Clientes
ON Clientes
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Operacion NVARCHAR(10);
    
    IF EXISTS(SELECT 1 FROM inserted) AND EXISTS(SELECT 1 FROM deleted)
        SET @Operacion = 'UPDATE';
    ELSE IF EXISTS(SELECT 1 FROM inserted)
        SET @Operacion = 'INSERT';
    ELSE
        SET @Operacion = 'DELETE';

    IF @Operacion IN ('INSERT', 'UPDATE')
    BEGIN
        INSERT INTO AuditoriaGeneral (NombreTabla, Operacion, RegistroId, DatosNuevos, UsuarioAccion)
        SELECT 'Clientes', @Operacion, CAST(i.ClienteId AS NVARCHAR(50)),
            CONCAT('UsuarioId:', i.UsuarioId, '|Telefono:', ISNULL(i.Telefono,''), '|Calificacion:', i.Calificacion),
            i.UsuarioCreacion
        FROM inserted i;
    END

    IF @Operacion = 'DELETE'
    BEGIN
        INSERT INTO AuditoriaGeneral (NombreTabla, Operacion, RegistroId, DatosAnteriores)
        SELECT 'Clientes', 'DELETE', CAST(d.ClienteId AS NVARCHAR(50)),
            CONCAT('UsuarioId:', d.UsuarioId, '|Telefono:', ISNULL(d.Telefono,''))
        FROM deleted d;
    END
END;
GO

-- Trigger de auditoría para Propiedades
CREATE OR ALTER TRIGGER trg_Audit_Propiedades
ON Propiedades
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Operacion NVARCHAR(10);
    
    IF EXISTS(SELECT 1 FROM inserted) AND EXISTS(SELECT 1 FROM deleted)
        SET @Operacion = 'UPDATE';
    ELSE IF EXISTS(SELECT 1 FROM inserted)
        SET @Operacion = 'INSERT';
    ELSE
        SET @Operacion = 'DELETE';

    IF @Operacion IN ('INSERT', 'UPDATE')
    BEGIN
        INSERT INTO AuditoriaGeneral (NombreTabla, Operacion, RegistroId, DatosNuevos, UsuarioAccion)
        SELECT 'Propiedades', @Operacion, CAST(i.PropiedadId AS NVARCHAR(50)),
            CONCAT('Nombre:', i.Nombre, '|Ciudad:', i.CiudadId, '|Tipo:', i.TipoAlojamientoId, '|Estado:', i.Estado),
            i.UsuarioCreacion
        FROM inserted i;
    END

    IF @Operacion = 'DELETE'
    BEGIN
        INSERT INTO AuditoriaGeneral (NombreTabla, Operacion, RegistroId, DatosAnteriores)
        SELECT 'Propiedades', 'DELETE', CAST(d.PropiedadId AS NVARCHAR(50)),
            CONCAT('Nombre:', d.Nombre, '|Ciudad:', d.CiudadId)
        FROM deleted d;
    END
END;
GO

-- Trigger de auditoría para Reservas
CREATE OR ALTER TRIGGER trg_Audit_Reservas
ON Reservas
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Operacion NVARCHAR(10);
    
    IF EXISTS(SELECT 1 FROM inserted) AND EXISTS(SELECT 1 FROM deleted)
        SET @Operacion = 'UPDATE';
    ELSE IF EXISTS(SELECT 1 FROM inserted)
        SET @Operacion = 'INSERT';
    ELSE
        SET @Operacion = 'DELETE';

    IF @Operacion IN ('INSERT', 'UPDATE')
    BEGIN
        INSERT INTO AuditoriaGeneral (NombreTabla, Operacion, RegistroId, DatosNuevos, UsuarioAccion)
        SELECT 'Reservas', @Operacion, CAST(i.ReservaId AS NVARCHAR(50)),
            CONCAT('Codigo:', i.CodigoReserva, '|Cliente:', i.ClienteId, '|Propiedad:', i.PropiedadId, 
                   '|CheckIn:', CONVERT(NVARCHAR(10), i.FechaCheckIn, 120), '|Total:', i.Total, '|Estado:', i.Estado),
            i.UsuarioCreacion
        FROM inserted i;
    END

    IF @Operacion = 'DELETE'
    BEGIN
        INSERT INTO AuditoriaGeneral (NombreTabla, Operacion, RegistroId, DatosAnteriores)
        SELECT 'Reservas', 'DELETE', CAST(d.ReservaId AS NVARCHAR(50)),
            CONCAT('Codigo:', d.CodigoReserva, '|Cliente:', d.ClienteId, '|Total:', d.Total)
        FROM deleted d;
    END
END;
GO

-- =====================================================
-- TRIGGERS DE LÓGICA DE NEGOCIO
-- =====================================================

-- Trigger: Al cambiar reserva a NoShow, crear advertencia automática
CREATE OR ALTER TRIGGER trg_Advertencia_NoShow
ON Reservas
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO AdvertenciasCliente (ClienteId, ReservaId, Tipo, Descripcion, Severidad, UsuarioCreacion)
    SELECT i.ClienteId, i.ReservaId, 'NoShow',
        CONCAT('No-Show en reserva ', i.CodigoReserva, ' para propiedad ID: ', i.PropiedadId, 
               '. Check-in: ', CONVERT(NVARCHAR(10), i.FechaCheckIn, 120)),
        3, -- Severidad media-alta
        i.UsuarioModificacion
    FROM inserted i
    INNER JOIN deleted d ON i.ReservaId = d.ReservaId
    WHERE i.Estado = 'NoShow' AND d.Estado <> 'NoShow';
END;
GO

-- Trigger: Al confirmar reserva, marcar disponibilidad como ocupada
CREATE OR ALTER TRIGGER trg_Disponibilidad_ReservaConfirmada
ON Reservas
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Solo actuar cuando el estado cambia a 'Confirmada'
    IF EXISTS (
        SELECT 1 FROM inserted i 
        INNER JOIN deleted d ON i.ReservaId = d.ReservaId 
        WHERE i.Estado = 'Confirmada' AND d.Estado <> 'Confirmada'
    )
    BEGIN
        ;WITH FechasReserva AS (
            SELECT rdh.HabitacionId, 
                   DATEADD(DAY, n.Number, i.FechaCheckIn) AS Fecha
            FROM inserted i
            INNER JOIN deleted d ON i.ReservaId = d.ReservaId
            INNER JOIN ReservaDetalleHabitacion rdh ON rdh.ReservaId = i.ReservaId
            CROSS APPLY (
                SELECT TOP (DATEDIFF(DAY, i.FechaCheckIn, i.FechaCheckOut))
                    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS Number
                FROM sys.objects a CROSS JOIN sys.objects b
            ) n
            WHERE i.Estado = 'Confirmada' AND d.Estado <> 'Confirmada'
        )
        MERGE Disponibilidad AS target
        USING FechasReserva AS source
        ON target.HabitacionId = source.HabitacionId AND target.Fecha = source.Fecha
        WHEN MATCHED THEN
            UPDATE SET Disponible = 0, FechaModificacion = GETDATE()
        WHEN NOT MATCHED THEN
            INSERT (HabitacionId, Fecha, Disponible)
            VALUES (source.HabitacionId, source.Fecha, 0);
    END
END;
GO

-- Trigger: Al completar encuesta, otorgar puntos al cliente
CREATE OR ALTER TRIGGER trg_Puntos_Encuesta
ON EncuestaExperiencia
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @PuntosPorEncuesta INT = 50;

    -- Registrar los puntos
    INSERT INTO PuntosCliente (ClienteId, Cantidad, Tipo, Descripcion, ReservaId)
    SELECT i.ClienteId, @PuntosPorEncuesta, 'Encuesta',
        CONCAT('Puntos por completar encuesta de experiencia - Reserva ID: ', i.ReservaId),
        i.ReservaId
    FROM inserted i;

    -- Actualizar puntos acumulados del cliente
    UPDATE c SET PuntosAcumulados = c.PuntosAcumulados + @PuntosPorEncuesta
    FROM Clientes c
    INNER JOIN inserted i ON c.ClienteId = i.ClienteId;

    -- Actualizar los puntos otorgados en la encuesta
    UPDATE e SET PuntosOtorgados = @PuntosPorEncuesta
    FROM EncuestaExperiencia e
    INNER JOIN inserted i ON e.EncuestaId = i.EncuestaId;
END;
GO

PRINT '✅ Índices y Triggers creados correctamente';
GO

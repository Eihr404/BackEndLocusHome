-- =====================================================
-- SCRIPT 3: STORED PROCEDURES
-- Sistema de Reservas tipo Booking
-- Ejecutar DESPUÉS de 02_Indices_Triggers.sql
-- =====================================================

USE BookingDB;
GO

-- =====================================================
-- SP: BÚSQUEDA DE ALOJAMIENTOS CON FILTROS
-- =====================================================
CREATE OR ALTER PROCEDURE sp_BuscarAlojamientos
    @CiudadId           INT = NULL,
    @PaisId             INT = NULL,
    @FechaCheckIn       DATE = NULL,
    @FechaCheckOut      DATE = NULL,
    @NumAdultos         INT = 1,
    @NumNinos           INT = 0,
    @LlevaMascotas      BIT = 0,
    @NumHabitaciones     INT = 1,
    @PresupuestoMin     DECIMAL(10,2) = NULL,
    @PresupuestoMax     DECIMAL(10,2) = NULL,
    @TipoAlojamientoId  INT = NULL,
    @CalificacionMin    DECIMAL(3,1) = NULL,
    @InstalacionIds     NVARCHAR(200) = NULL, -- Lista separada por comas: '1,3,5'
    @ComidaIds          NVARCHAR(200) = NULL, -- Lista separada por comas: '1,2'
    @MonedaId           INT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        p.PropiedadId, p.Nombre, p.Descripcion, p.Direccion,
        p.Latitud, p.Longitud, p.Estrellas, p.CalificacionPromedio,
        p.TotalResenas, p.AdmiteMascotas,
        ta.Nombre AS TipoAlojamiento,
        c.Nombre AS Ciudad, pa.Nombre AS Pais,
        pf.Url AS FotoPrincipal,
        MIN(t.PrecioPorNoche) AS PrecioDesde,
        m.Simbolo AS SimboloMoneda
    FROM Propiedades p
    INNER JOIN TiposAlojamiento ta ON p.TipoAlojamientoId = ta.TipoAlojamientoId
    INNER JOIN Ciudades c ON p.CiudadId = c.CiudadId
    INNER JOIN Paises pa ON c.PaisId = pa.PaisId
    LEFT JOIN PropiedadFotos pf ON p.PropiedadId = pf.PropiedadId AND pf.EsPrincipal = 1
    INNER JOIN Habitaciones h ON p.PropiedadId = h.PropiedadId AND h.EliminadoLogico = 0
    LEFT JOIN Tarifas t ON h.HabitacionId = t.HabitacionId AND t.Estado = 1
        AND (@FechaCheckIn IS NULL OR (t.FechaInicio <= @FechaCheckIn AND t.FechaFin >= @FechaCheckIn))
    LEFT JOIN Monedas m ON t.MonedaId = m.MonedaId
    WHERE p.EliminadoLogico = 0
      AND p.Estado = 'Activa'
      AND (@CiudadId IS NULL OR p.CiudadId = @CiudadId)
      AND (@PaisId IS NULL OR c.PaisId = @PaisId)
      AND (@TipoAlojamientoId IS NULL OR p.TipoAlojamientoId = @TipoAlojamientoId)
      AND (@LlevaMascotas = 0 OR p.AdmiteMascotas = 1)
      AND (@CalificacionMin IS NULL OR p.CalificacionPromedio >= @CalificacionMin)
      AND h.CapacidadAdultos >= @NumAdultos
      AND (@LlevaMascotas = 0 OR h.AdmiteMascotas = 1)
      -- Filtro de instalaciones
      AND (@InstalacionIds IS NULL OR NOT EXISTS(
          SELECT value FROM STRING_SPLIT(@InstalacionIds, ',')
          WHERE CAST(value AS INT) NOT IN (
              SELECT pi2.InstalacionId FROM PropiedadInstalaciones pi2 WHERE pi2.PropiedadId = p.PropiedadId)))
      -- Filtro de comidas
      AND (@ComidaIds IS NULL OR NOT EXISTS(
          SELECT value FROM STRING_SPLIT(@ComidaIds, ',')
          WHERE CAST(value AS INT) NOT IN (
              SELECT pc.ComidaId FROM PropiedadComidas pc WHERE pc.PropiedadId = p.PropiedadId)))
    GROUP BY p.PropiedadId, p.Nombre, p.Descripcion, p.Direccion,
        p.Latitud, p.Longitud, p.Estrellas, p.CalificacionPromedio,
        p.TotalResenas, p.AdmiteMascotas, ta.Nombre, c.Nombre, pa.Nombre,
        pf.Url, m.Simbolo
    HAVING (@PresupuestoMin IS NULL OR MIN(t.PrecioPorNoche) >= @PresupuestoMin)
       AND (@PresupuestoMax IS NULL OR MIN(t.PrecioPorNoche) <= @PresupuestoMax)
    ORDER BY p.CalificacionPromedio DESC, p.TotalResenas DESC;
END;
GO

-- =====================================================
-- SP: CREAR RESERVA
-- =====================================================
CREATE OR ALTER PROCEDURE sp_CrearReserva
    @ClienteId      INT,
    @PropiedadId    INT,
    @FechaCheckIn   DATE,
    @FechaCheckOut  DATE,
    @NumAdultos     INT,
    @NumNinos       INT = 0,
    @LlevaMascotas  BIT = 0,
    @HabitacionIds  NVARCHAR(500), -- CSV de IDs de habitaciones
    @MonedaId       INT,
    @IpOrigen       NVARCHAR(45) = NULL,
    @ReservaId      INT OUTPUT,
    @CodigoReserva  NVARCHAR(20) OUTPUT,
    @Resultado      NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = '';
    SET @ReservaId = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validar disponibilidad de todas las habitaciones
        DECLARE @HabitacionesNoDisponibles INT;
        SELECT @HabitacionesNoDisponibles = COUNT(*)
        FROM STRING_SPLIT(@HabitacionIds, ',') s
        CROSS APPLY (
            SELECT DATEADD(DAY, n.Number, @FechaCheckIn) AS Fecha
            FROM (SELECT TOP (DATEDIFF(DAY, @FechaCheckIn, @FechaCheckOut))
                    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS Number
                  FROM sys.objects a CROSS JOIN sys.objects b) n
        ) fechas
        INNER JOIN Disponibilidad d ON d.HabitacionId = CAST(s.value AS INT) 
            AND d.Fecha = fechas.Fecha AND d.Disponible = 0;

        IF @HabitacionesNoDisponibles > 0
        BEGIN
            SET @Resultado = 'ERROR: Una o más habitaciones no están disponibles en las fechas seleccionadas.';
            ROLLBACK;
            RETURN;
        END

        -- Generar código de reserva único
        SET @CodigoReserva = CONCAT('BK', FORMAT(GETDATE(), 'yyyyMMdd'), '-', RIGHT('0000' + CAST(NEXT VALUE FOR dbo.SeqReserva AS NVARCHAR(4)), 4));

        -- Calcular totales
        DECLARE @NumNoches INT = DATEDIFF(DAY, @FechaCheckIn, @FechaCheckOut);
        DECLARE @TotalReserva DECIMAL(12,2) = 0;

        SELECT @TotalReserva = SUM(t.PrecioPorNoche * @NumNoches)
        FROM STRING_SPLIT(@HabitacionIds, ',') s
        INNER JOIN Tarifas t ON t.HabitacionId = CAST(s.value AS INT)
            AND t.MonedaId = @MonedaId AND t.Estado = 1
            AND t.FechaInicio <= @FechaCheckIn AND t.FechaFin >= @FechaCheckIn;

        IF @TotalReserva IS NULL OR @TotalReserva = 0
        BEGIN
            SET @Resultado = 'ERROR: No se encontraron tarifas vigentes para las habitaciones seleccionadas.';
            ROLLBACK;
            RETURN;
        END

        -- Crear la reserva
        DECLARE @NumHabs INT;
        SELECT @NumHabs = COUNT(*) FROM STRING_SPLIT(@HabitacionIds, ',');

        INSERT INTO Reservas (ClienteId, PropiedadId, FechaCheckIn, FechaCheckOut,
            NumAdultos, NumNinos, LlevaMascotas, NumHabitaciones, MonedaId,
            SubTotal, Total, CodigoReserva, Estado, UsuarioCreacion, IpOrigen)
        VALUES (@ClienteId, @PropiedadId, @FechaCheckIn, @FechaCheckOut,
            @NumAdultos, @NumNinos, @LlevaMascotas, @NumHabs, @MonedaId,
            @TotalReserva, @TotalReserva, @CodigoReserva, 'Pendiente', 
            CAST(@ClienteId AS NVARCHAR(100)), @IpOrigen);

        SET @ReservaId = SCOPE_IDENTITY();

        -- Insertar detalle por habitación
        INSERT INTO ReservaDetalleHabitacion (ReservaId, HabitacionId, PrecioPorNoche, NumNoches, SubTotalHabitacion)
        SELECT @ReservaId, CAST(s.value AS INT), t.PrecioPorNoche, @NumNoches,
            t.PrecioPorNoche * @NumNoches
        FROM STRING_SPLIT(@HabitacionIds, ',') s
        INNER JOIN Tarifas t ON t.HabitacionId = CAST(s.value AS INT)
            AND t.MonedaId = @MonedaId AND t.Estado = 1
            AND t.FechaInicio <= @FechaCheckIn AND t.FechaFin >= @FechaCheckIn;

        -- Actualizar total de reservas del cliente
        UPDATE Clientes SET TotalReservas = TotalReservas + 1 WHERE ClienteId = @ClienteId;

        COMMIT;
        SET @Resultado = 'OK: Reserva creada exitosamente.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @Resultado = CONCAT('ERROR: ', ERROR_MESSAGE());
    END CATCH
END;
GO

-- Secuencia para códigos de reserva
IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'SeqReserva')
    CREATE SEQUENCE dbo.SeqReserva START WITH 1 INCREMENT BY 1;
GO

-- =====================================================
-- SP: CONFIRMAR PAGO
-- =====================================================
CREATE OR ALTER PROCEDURE sp_ConfirmarPago
    @ReservaId      INT,
    @MetodoPagoId   INT = NULL,
    @TipoPago       NVARCHAR(30), -- 'Tarjeta' o 'EnSitio'
    @ReferenciaPago NVARCHAR(100) = NULL,
    @IpOrigen       NVARCHAR(45) = NULL,
    @Resultado      NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Monto DECIMAL(12,2), @MonedaId INT, @UsuarioId NVARCHAR(100);
        SELECT @Monto = Total, @MonedaId = MonedaId, @UsuarioId = UsuarioCreacion
        FROM Reservas WHERE ReservaId = @ReservaId AND Estado = 'Pendiente';

        IF @Monto IS NULL
        BEGIN
            SET @Resultado = 'ERROR: Reserva no encontrada o no está en estado Pendiente.';
            ROLLBACK; RETURN;
        END

        INSERT INTO Pagos (ReservaId, MetodoPagoId, TipoPago, Monto, MonedaId, Estado,
            ReferenciaPago, FechaPago, UsuarioCreacion, IpOrigen)
        VALUES (@ReservaId, @MetodoPagoId, @TipoPago, @Monto, @MonedaId, 'Procesado',
            @ReferenciaPago, GETDATE(), @UsuarioId, @IpOrigen);

        -- Confirmar la reserva (esto disparará el trigger de disponibilidad)
        UPDATE Reservas SET Estado = 'Confirmada', FechaModificacion = GETDATE(),
            UsuarioModificacion = @UsuarioId
        WHERE ReservaId = @ReservaId;

        COMMIT;
        SET @Resultado = 'OK: Pago procesado y reserva confirmada.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @Resultado = CONCAT('ERROR: ', ERROR_MESSAGE());
    END CATCH
END;
GO

-- =====================================================
-- SP: CALIFICAR HOTEL (Cliente califica)
-- =====================================================
CREATE OR ALTER PROCEDURE sp_CalificarHotel
    @ClienteId      INT,
    @ReservaId      INT,
    @Puntuacion     DECIMAL(3,1),
    @Comentario     NVARCHAR(MAX) = NULL,
    @IpOrigen       NVARCHAR(45) = NULL,
    @Resultado      NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @PropiedadId INT;
        SELECT @PropiedadId = PropiedadId FROM Reservas 
        WHERE ReservaId = @ReservaId AND ClienteId = @ClienteId AND Estado = 'Completada';

        IF @PropiedadId IS NULL
        BEGIN
            SET @Resultado = 'ERROR: Solo puedes calificar reservas completadas que te pertenezcan.';
            RETURN;
        END

        INSERT INTO CalificacionHotel (ClienteId, PropiedadId, ReservaId, Puntuacion, Comentario, 
            UsuarioCreacion, IpOrigen)
        VALUES (@ClienteId, @PropiedadId, @ReservaId, @Puntuacion, @Comentario,
            CAST(@ClienteId AS NVARCHAR(100)), @IpOrigen);

        -- Actualizar promedio de calificación de la propiedad
        UPDATE Propiedades SET 
            CalificacionPromedio = (SELECT AVG(Puntuacion) FROM CalificacionHotel WHERE PropiedadId = @PropiedadId AND EliminadoLogico = 0),
            TotalResenas = (SELECT COUNT(*) FROM CalificacionHotel WHERE PropiedadId = @PropiedadId AND EliminadoLogico = 0)
        WHERE PropiedadId = @PropiedadId;

        SET @Resultado = 'OK: Calificación registrada exitosamente.';
    END TRY
    BEGIN CATCH
        SET @Resultado = CONCAT('ERROR: ', ERROR_MESSAGE());
    END CATCH
END;
GO

-- =====================================================
-- SP: CALIFICAR CLIENTE (Colaborador califica al huésped)
-- =====================================================
CREATE OR ALTER PROCEDURE sp_CalificarCliente
    @ColaboradorId  INT,
    @ReservaId      INT,
    @Puntuacion     DECIMAL(3,1),
    @Comentario     NVARCHAR(500) = NULL,
    @EsNoShow       BIT = 0,
    @IpOrigen       NVARCHAR(45) = NULL,
    @Resultado      NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @ClienteId INT, @PropiedadId INT;
        SELECT @ClienteId = r.ClienteId, @PropiedadId = r.PropiedadId
        FROM Reservas r
        INNER JOIN Propiedades p ON r.PropiedadId = p.PropiedadId
        WHERE r.ReservaId = @ReservaId AND p.ColaboradorId = @ColaboradorId
          AND r.Estado IN ('Completada', 'NoShow');

        IF @ClienteId IS NULL
        BEGIN
            SET @Resultado = 'ERROR: Reserva no encontrada o no pertenece a tus propiedades.';
            RETURN;
        END

        INSERT INTO CalificacionCliente (ColaboradorId, ClienteId, ReservaId, Puntuacion, 
            Comentario, EsNoShow, UsuarioCreacion, IpOrigen)
        VALUES (@ColaboradorId, @ClienteId, @ReservaId, @Puntuacion,
            @Comentario, @EsNoShow, CAST(@ColaboradorId AS NVARCHAR(100)), @IpOrigen);

        -- Actualizar calificación promedio del cliente
        UPDATE Clientes SET 
            Calificacion = (SELECT AVG(Puntuacion) FROM CalificacionCliente WHERE ClienteId = @ClienteId AND EliminadoLogico = 0)
        WHERE ClienteId = @ClienteId;

        -- Si es NoShow, actualizar estado de reserva (disparará trigger de advertencia)
        IF @EsNoShow = 1
        BEGIN
            UPDATE Reservas SET Estado = 'NoShow', FechaModificacion = GETDATE(),
                UsuarioModificacion = CAST(@ColaboradorId AS NVARCHAR(100))
            WHERE ReservaId = @ReservaId;
        END

        SET @Resultado = 'OK: Calificación de cliente registrada.';
    END TRY
    BEGIN CATCH
        SET @Resultado = CONCAT('ERROR: ', ERROR_MESSAGE());
    END CATCH
END;
GO

-- =====================================================
-- SP: DASHBOARD DEL COLABORADOR
-- =====================================================
CREATE OR ALTER PROCEDURE sp_DashboardColaborador
    @ColaboradorId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Resumen de propiedades
    SELECT 
        COUNT(*) AS TotalPropiedades,
        SUM(CASE WHEN Estado = 'Activa' THEN 1 ELSE 0 END) AS PropiedadesActivas,
        SUM(ClicksAnuncio) AS TotalClicks
    FROM Propiedades WHERE ColaboradorId = @ColaboradorId AND EliminadoLogico = 0;

    -- Resumen de reservas del mes actual
    SELECT 
        COUNT(*) AS ReservasMes,
        SUM(CASE WHEN r.Estado = 'Confirmada' THEN 1 ELSE 0 END) AS Confirmadas,
        SUM(CASE WHEN r.Estado = 'Completada' THEN 1 ELSE 0 END) AS Completadas,
        SUM(CASE WHEN r.Estado = 'Cancelada' THEN 1 ELSE 0 END) AS Canceladas,
        SUM(CASE WHEN r.Estado = 'NoShow' THEN 1 ELSE 0 END) AS NoShows
    FROM Reservas r
    INNER JOIN Propiedades p ON r.PropiedadId = p.PropiedadId
    WHERE p.ColaboradorId = @ColaboradorId 
      AND MONTH(r.FechaCreacion) = MONTH(GETDATE()) AND YEAR(r.FechaCreacion) = YEAR(GETDATE());

    -- Ganancias del mes
    SELECT ISNULL(SUM(pg.Monto), 0) AS GananciasMes
    FROM Pagos pg
    INNER JOIN Reservas r ON pg.ReservaId = r.ReservaId
    INNER JOIN Propiedades p ON r.PropiedadId = p.PropiedadId
    WHERE p.ColaboradorId = @ColaboradorId AND pg.Estado = 'Procesado'
      AND MONTH(pg.FechaPago) = MONTH(GETDATE()) AND YEAR(pg.FechaPago) = YEAR(GETDATE());

    -- Promociones activas
    SELECT pr.PromocionId, pr.Nombre, pr.PorcentajeDescuento, pr.FechaInicio, pr.FechaFin,
        p.Nombre AS Propiedad
    FROM Promociones pr
    INNER JOIN Propiedades p ON pr.PropiedadId = p.PropiedadId
    WHERE p.ColaboradorId = @ColaboradorId AND pr.Estado = 1 AND pr.FechaFin >= GETDATE();
END;
GO

-- =====================================================
-- SP: OBTENER TASA DE CAMBIO
-- =====================================================
CREATE OR ALTER PROCEDURE sp_ObtenerTasaCambio
    @MonedaOrigenId  INT,
    @MonedaDestinoId INT,
    @Monto           DECIMAL(18,2),
    @MontoConvertido DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Tasa DECIMAL(18,6);

    IF @MonedaOrigenId = @MonedaDestinoId
    BEGIN
        SET @MontoConvertido = @Monto;
        RETURN;
    END

    SELECT TOP 1 @Tasa = Tasa FROM TasasCambio 
    WHERE MonedaOrigenId = @MonedaOrigenId AND MonedaDestinoId = @MonedaDestinoId AND Estado = 1
    ORDER BY FechaVigencia DESC;

    IF @Tasa IS NULL
        SET @MontoConvertido = NULL;
    ELSE
        SET @MontoConvertido = ROUND(@Monto * @Tasa, 2);
END;
GO

-- =====================================================
-- SP: APLICAR DESCUENTO
-- =====================================================
CREATE OR ALTER PROCEDURE sp_AplicarDescuento
    @ReservaId      INT,
    @PromocionId    INT = NULL,
    @UsarPuntos     BIT = 0,
    @PuntosACanjear INT = 0,
    @Resultado      NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @SubTotal DECIMAL(12,2), @ClienteId INT, @Porcentaje DECIMAL(5,2) = 0;
        DECLARE @MontoDescuento DECIMAL(12,2) = 0, @Origen NVARCHAR(50);
        
        SELECT @SubTotal = SubTotal, @ClienteId = ClienteId 
        FROM Reservas WHERE ReservaId = @ReservaId AND Estado = 'Pendiente';

        IF @SubTotal IS NULL BEGIN SET @Resultado = 'ERROR: Reserva no válida.'; RETURN; END

        IF @PromocionId IS NOT NULL
        BEGIN
            SELECT @Porcentaje = PorcentajeDescuento FROM Promociones 
            WHERE PromocionId = @PromocionId AND Estado = 1 AND GETDATE() BETWEEN FechaInicio AND FechaFin;
            
            IF @Porcentaje > 0
            BEGIN
                SET @MontoDescuento = ROUND(@SubTotal * @Porcentaje / 100, 2);
                SET @Origen = 'ComisionPlataforma';
            END
        END
        ELSE IF @UsarPuntos = 1 AND @PuntosACanjear > 0
        BEGIN
            DECLARE @PuntosDisponibles INT;
            SELECT @PuntosDisponibles = PuntosAcumulados FROM Clientes WHERE ClienteId = @ClienteId;
            
            IF @PuntosACanjear > @PuntosDisponibles
            BEGIN SET @Resultado = 'ERROR: Puntos insuficientes.'; RETURN; END

            SET @MontoDescuento = @PuntosACanjear * 0.10; -- 1 punto = $0.10
            SET @Porcentaje = ROUND((@MontoDescuento / @SubTotal) * 100, 2);
            SET @Origen = 'Puntos';

            INSERT INTO PuntosCliente (ClienteId, Cantidad, Tipo, Descripcion, ReservaId)
            VALUES (@ClienteId, -@PuntosACanjear, 'Canje', 'Canje de puntos en reserva', @ReservaId);

            UPDATE Clientes SET PuntosAcumulados = PuntosAcumulados - @PuntosACanjear WHERE ClienteId = @ClienteId;
        END

        IF @MontoDescuento > 0
        BEGIN
            INSERT INTO Descuentos (ReservaId, ClienteId, PromocionId, Origen, Porcentaje, MontoDescuento, UsuarioCreacion)
            VALUES (@ReservaId, @ClienteId, @PromocionId, @Origen, @Porcentaje, @MontoDescuento, CAST(@ClienteId AS NVARCHAR(100)));

            UPDATE Reservas SET Descuento = @MontoDescuento, Total = SubTotal - @MontoDescuento,
                FechaModificacion = GETDATE()
            WHERE ReservaId = @ReservaId;
        END

        SET @Resultado = CONCAT('OK: Descuento de $', @MontoDescuento, ' aplicado (', @Porcentaje, '%).');
    END TRY
    BEGIN CATCH
        SET @Resultado = CONCAT('ERROR: ', ERROR_MESSAGE());
    END CATCH
END;
GO

PRINT '✅ Stored Procedures creados correctamente';
GO

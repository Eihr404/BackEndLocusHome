-- =====================================================
-- SCRIPT 3: STORED PROCEDURES → FUNCIONES PL/pgSQL
-- Sistema de Reservas tipo Booking
-- PostgreSQL / Supabase
-- =====================================================

-- SECUENCIA para códigos de reserva (equivalente a dbo.SeqReserva de SQL Server)
CREATE SEQUENCE IF NOT EXISTS seq_reserva START 1 INCREMENT 1;


-- =====================================================
-- FUNCIÓN: sp_BuscarAlojamientos
-- Búsqueda de propiedades con filtros avanzados
-- =====================================================
CREATE OR REPLACE FUNCTION sp_buscaralojamientos(
  p_ciudadid           INT     DEFAULT NULL,
  p_paisid             INT     DEFAULT NULL,
  p_fechacheckin       DATE    DEFAULT NULL,
  p_fechacheckout      DATE    DEFAULT NULL,
  p_numadultos         INT     DEFAULT 1,
  p_numninos           INT     DEFAULT 0,
  p_llevamascotas      BOOLEAN DEFAULT FALSE,
  p_numhabitaciones    INT     DEFAULT 1,
  p_presupuestomin     NUMERIC(10,2) DEFAULT NULL,
  p_presupuestomax     NUMERIC(10,2) DEFAULT NULL,
  p_tipoalojamientoid  INT     DEFAULT NULL,
  p_calificacionmin    NUMERIC(3,1)  DEFAULT NULL,
  p_instalacionids     TEXT    DEFAULT NULL,  -- CSV: '1,3,5'
  p_comidaids          TEXT    DEFAULT NULL,  -- CSV: '1,2'
  p_monedaid           INT     DEFAULT 1
)
RETURNS TABLE (
  propiedadid         INT,
  nombre              TEXT,
  descripcion         TEXT,
  direccion           TEXT,
  latitud             NUMERIC,
  longitud            NUMERIC,
  estrellas           INT,
  calificacionpromedio NUMERIC,
  totalresenas        INT,
  admitemascotas      BOOLEAN,
  tipoalojamiento     TEXT,
  ciudad              TEXT,
  pais                TEXT,
  fotoprincipal       TEXT,
  preciodesde         NUMERIC,
  simbolomoneda       TEXT
)
LANGUAGE plpgsql AS $$
BEGIN
  RETURN QUERY
  SELECT DISTINCT
    p.propiedadid,
    p.nombre::TEXT,
    p.descripcion::TEXT,
    p.direccion::TEXT,
    p.latitud,
    p.longitud,
    p.estrellas,
    p.calificacionpromedio,
    p.totalresenas,
    p.admitemascotas,
    ta.nombre::TEXT AS tipoalojamiento,
    c.nombre::TEXT  AS ciudad,
    pa.nombre::TEXT AS pais,
    pf.url::TEXT    AS fotoprincipal,
    MIN(t.precioportnoche) AS preciodesde,
    m.simbolo::TEXT AS simbolomoneda
  FROM propiedades p
  INNER JOIN tiposalojamiento ta ON p.tipoalojamientoid = ta.tipoalojamientoid
  INNER JOIN ciudades c ON p.ciudadid = c.ciudadid
  INNER JOIN paises pa ON c.paisid = pa.paisid
  LEFT JOIN propiedadfotos pf ON p.propiedadid = pf.propiedadid AND pf.esprincipal = true
  INNER JOIN habitaciones h ON p.propiedadid = h.propiedadid AND h.eliminadologico = false
  LEFT JOIN tarifas t ON h.habitacionid = t.habitacionid AND t.estado = true
    AND (p_fechacheckin IS NULL OR (t.fechainicio <= p_fechacheckin AND t.fechafin >= p_fechacheckin))
  LEFT JOIN monedas m ON t.monedaid = m.monedaid
  WHERE p.eliminadologico = false
    AND p.estado = 'Activa'
    AND (p_ciudadid IS NULL OR p.ciudadid = p_ciudadid)
    AND (p_paisid IS NULL OR c.paisid = p_paisid)
    AND (p_tipoalojamientoid IS NULL OR p.tipoalojamientoid = p_tipoalojamientoid)
    AND (NOT p_llevamascotas OR p.admitemascotas = true)
    AND (p_calificacionmin IS NULL OR p.calificacionpromedio >= p_calificacionmin)
    AND h.capacidadadultos >= p_numadultos
    AND (NOT p_llevamascotas OR h.admitemascotas = true)
    -- Filtro instalaciones (todos los IDs del CSV deben estar en la propiedad)
    AND (p_instalacionids IS NULL OR NOT EXISTS (
          SELECT unnest(string_to_array(p_instalacionids, ','))::INT AS iid
          EXCEPT
          SELECT pi2.instalacionid FROM propiedadinstalaciones pi2 WHERE pi2.propiedadid = p.propiedadid))
    -- Filtro comidas
    AND (p_comidaids IS NULL OR NOT EXISTS (
          SELECT unnest(string_to_array(p_comidaids, ','))::INT AS cid
          EXCEPT
          SELECT pc.comidaid FROM propiedadcomidas pc WHERE pc.propiedadid = p.propiedadid))
  GROUP BY p.propiedadid, p.nombre, p.descripcion, p.direccion,
           p.latitud, p.longitud, p.estrellas, p.calificacionpromedio,
           p.totalresenas, p.admitemascotas, ta.nombre, c.nombre, pa.nombre,
           pf.url, m.simbolo
  HAVING (p_presupuestomin IS NULL OR MIN(t.precioportnoche) >= p_presupuestomin)
     AND (p_presupuestomax IS NULL OR MIN(t.precioportnoche) <= p_presupuestomax)
  ORDER BY p.calificacionpromedio DESC, p.totalresenas DESC;
END;
$$;


-- =====================================================
-- FUNCIÓN: sp_CrearReserva
-- Crea una reserva con validación de disponibilidad
-- =====================================================
CREATE OR REPLACE FUNCTION sp_crearreserva(
  p_clienteid      INT,
  p_propiedadid    INT,
  p_fechacheckin   DATE,
  p_fechacheckout  DATE,
  p_numadultos     INT,
  p_numninos       INT     DEFAULT 0,
  p_llevamascotas  BOOLEAN DEFAULT FALSE,
  p_habitacionids  TEXT    DEFAULT '',     -- CSV: '3,5,7'
  p_monedaid       INT     DEFAULT 1,
  p_iporigen       TEXT    DEFAULT NULL
)
RETURNS TABLE (
  reservaid      INT,
  codigoreserva  TEXT,
  resultado      TEXT
)
LANGUAGE plpgsql AS $$
DECLARE
  v_reservaid           INT;
  v_codigoreserva       TEXT;
  v_resultado           TEXT;
  v_noches              INT;
  v_total               NUMERIC(12,2);
  v_numhabs             INT;
  v_hab_no_disponibles  INT;
  v_seq                 BIGINT;
  v_fecha               DATE;
  v_hab_id              INT;
BEGIN
  -- Validar disponibilidad de todas las habitaciones
  SELECT COUNT(*) INTO v_hab_no_disponibles
  FROM (SELECT unnest(string_to_array(p_habitacionids, ','))::INT AS hid) ids
  CROSS JOIN (
    SELECT generate_series(p_fechacheckin, p_fechacheckout - 1, INTERVAL '1 day')::DATE AS fecha
  ) fechas
  INNER JOIN disponibilidad d ON d.habitacionid = ids.hid
    AND d.fecha = fechas.fecha AND d.disponible = false;

  IF v_hab_no_disponibles > 0 THEN
    RETURN QUERY SELECT 0, ''::TEXT, 'ERROR: Una o más habitaciones no están disponibles en las fechas seleccionadas.'::TEXT;
    RETURN;
  END IF;

  -- Calcular noches y número de habitaciones
  v_noches  := p_fechacheckout - p_fechacheckin;
  SELECT COUNT(*) INTO v_numhabs FROM unnest(string_to_array(p_habitacionids, ','));

  -- Calcular total con tarifas vigentes
  SELECT COALESCE(SUM(t.precioportnoche * v_noches), 0) INTO v_total
  FROM (SELECT unnest(string_to_array(p_habitacionids, ','))::INT AS hid) ids
  INNER JOIN tarifas t ON t.habitacionid = ids.hid
    AND t.monedaid = p_monedaid AND t.estado = true
    AND t.fechainicio <= p_fechacheckin AND t.fechafin >= p_fechacheckin;

  IF v_total = 0 THEN
    RETURN QUERY SELECT 0, ''::TEXT, 'ERROR: No se encontraron tarifas vigentes para las habitaciones seleccionadas.'::TEXT;
    RETURN;
  END IF;

  -- Generar código único de reserva
  v_seq := nextval('seq_reserva');
  v_codigoreserva := 'BK' || TO_CHAR(NOW(), 'YYYYMMDD') || '-' || LPAD(v_seq::TEXT, 4, '0');

  -- Insertar reserva
  INSERT INTO reservas (
    clienteid, propiedadid, fechacheckin, fechacheckout,
    numadultos, numninos, llevamascotas, numhabitaciones, monedaid,
    subtotal, total, codigoreserva, estado, usuariocreacion, iporigen
  ) VALUES (
    p_clienteid, p_propiedadid, p_fechacheckin, p_fechacheckout,
    p_numadultos, p_numninos, p_llevamascotas, v_numhabs, p_monedaid,
    v_total, v_total, v_codigoreserva, 'Pendiente',
    p_clienteid::TEXT, p_iporigen
  ) RETURNING reservaid INTO v_reservaid;

  -- Insertar detalles por habitación
  INSERT INTO reservadetallehabitacion (reservaid, habitacionid, precioportnoche, numnoches, subtotalhabitacion)
  SELECT v_reservaid, ids.hid, t.precioportnoche, v_noches, t.precioportnoche * v_noches
  FROM (SELECT unnest(string_to_array(p_habitacionids, ','))::INT AS hid) ids
  INNER JOIN tarifas t ON t.habitacionid = ids.hid
    AND t.monedaid = p_monedaid AND t.estado = true
    AND t.fechainicio <= p_fechacheckin AND t.fechafin >= p_fechacheckin;

  -- Actualizar contador del cliente
  UPDATE clientes SET totalreservas = totalreservas + 1 WHERE clienteid = p_clienteid;

  RETURN QUERY SELECT v_reservaid, v_codigoreserva, 'OK: Reserva creada exitosamente.'::TEXT;

EXCEPTION WHEN OTHERS THEN
  RETURN QUERY SELECT 0, ''::TEXT, ('ERROR: ' || SQLERRM)::TEXT;
END;
$$;


-- =====================================================
-- FUNCIÓN: sp_ConfirmarPago
-- =====================================================
CREATE OR REPLACE FUNCTION sp_confirmarpago(
  p_reservaid      INT,
  p_metodopagoid   INT     DEFAULT NULL,
  p_tipopago       TEXT    DEFAULT 'Tarjeta',
  p_referenciapago TEXT    DEFAULT NULL,
  p_iporigen       TEXT    DEFAULT NULL
)
RETURNS TEXT LANGUAGE plpgsql AS $$
DECLARE
  v_monto     NUMERIC(12,2);
  v_monedaid  INT;
  v_usuario   TEXT;
BEGIN
  SELECT total, monedaid, usuariocreacion
  INTO v_monto, v_monedaid, v_usuario
  FROM reservas WHERE reservaid = p_reservaid AND estado = 'Pendiente';

  IF v_monto IS NULL THEN
    RETURN 'ERROR: Reserva no encontrada o no está en estado Pendiente.';
  END IF;

  INSERT INTO pagos (reservaid, metodopagoid, tipopago, monto, monedaid, estado,
                     referenciapago, fechapago, usuariocreacion, iporigen)
  VALUES (p_reservaid, p_metodopagoid, p_tipopago, v_monto, v_monedaid, 'Procesado',
          p_referenciapago, NOW(), v_usuario, p_iporigen);

  -- Confirmar reserva (disparará el trigger de disponibilidad)
  UPDATE reservas
  SET estado = 'Confirmada', fechamodificacion = NOW(), usuariomodificacion = v_usuario
  WHERE reservaid = p_reservaid;

  RETURN 'OK: Pago procesado y reserva confirmada.';
EXCEPTION WHEN OTHERS THEN
  RETURN 'ERROR: ' || SQLERRM;
END;
$$;


-- =====================================================
-- FUNCIÓN: sp_CalificarHotel
-- =====================================================
CREATE OR REPLACE FUNCTION sp_calificarhotel(
  p_clienteid   INT,
  p_reservaid   INT,
  p_puntuacion  NUMERIC(3,1),
  p_comentario  TEXT DEFAULT NULL,
  p_iporigen    TEXT DEFAULT NULL
)
RETURNS TEXT LANGUAGE plpgsql AS $$
DECLARE
  v_propiedadid INT;
BEGIN
  SELECT propiedadid INTO v_propiedadid
  FROM reservas
  WHERE reservaid = p_reservaid AND clienteid = p_clienteid AND estado = 'Completada';

  IF v_propiedadid IS NULL THEN
    RETURN 'ERROR: Solo puedes calificar reservas completadas que te pertenezcan.';
  END IF;

  INSERT INTO calificacionhotel (clienteid, propiedadid, reservaid, puntuacion,
                                  comentario, usuariocreacion, iporigen)
  VALUES (p_clienteid, v_propiedadid, p_reservaid, p_puntuacion,
          p_comentario, p_clienteid::TEXT, p_iporigen);

  -- Actualizar promedio de la propiedad
  UPDATE propiedades SET
    calificacionpromedio = (SELECT AVG(puntuacion) FROM calificacionhotel WHERE propiedadid = v_propiedadid AND eliminadologico = false),
    totalresenas         = (SELECT COUNT(*) FROM calificacionhotel WHERE propiedadid = v_propiedadid AND eliminadologico = false)
  WHERE propiedadid = v_propiedadid;

  RETURN 'OK: Calificación registrada exitosamente.';
EXCEPTION WHEN OTHERS THEN
  RETURN 'ERROR: ' || SQLERRM;
END;
$$;


-- =====================================================
-- FUNCIÓN: sp_CalificarCliente
-- =====================================================
CREATE OR REPLACE FUNCTION sp_calificarcliente(
  p_colaboradorid INT,
  p_reservaid     INT,
  p_puntuacion    NUMERIC(3,1),
  p_comentario    TEXT    DEFAULT NULL,
  p_esnoshow      BOOLEAN DEFAULT FALSE,
  p_iporigen      TEXT    DEFAULT NULL
)
RETURNS TEXT LANGUAGE plpgsql AS $$
DECLARE
  v_clienteid   INT;
  v_propiedadid INT;
BEGIN
  SELECT r.clienteid, r.propiedadid INTO v_clienteid, v_propiedadid
  FROM reservas r
  INNER JOIN propiedades p ON r.propiedadid = p.propiedadid
  WHERE r.reservaid = p_reservaid
    AND p.colaboradorid = p_colaboradorid
    AND r.estado IN ('Completada', 'NoShow');

  IF v_clienteid IS NULL THEN
    RETURN 'ERROR: Reserva no encontrada o no pertenece a tus propiedades.';
  END IF;

  INSERT INTO calificacioncliente (colaboradorid, clienteid, reservaid, puntuacion,
                                    comentario, esnoshow, usuariocreacion, iporigen)
  VALUES (p_colaboradorid, v_clienteid, p_reservaid, p_puntuacion,
          p_comentario, p_esnoshow, p_colaboradorid::TEXT, p_iporigen);

  -- Actualizar calificación promedio del cliente
  UPDATE clientes SET
    calificacion = (SELECT AVG(puntuacion) FROM calificacioncliente WHERE clienteid = v_clienteid AND eliminadologico = false)
  WHERE clienteid = v_clienteid;

  -- Si es NoShow, actualizar estado (dispara el trigger de advertencia)
  IF p_esnoshow THEN
    UPDATE reservas SET estado = 'NoShow', fechamodificacion = NOW(),
      usuariomodificacion = p_colaboradorid::TEXT
    WHERE reservaid = p_reservaid;
  END IF;

  RETURN 'OK: Calificación de cliente registrada.';
EXCEPTION WHEN OTHERS THEN
  RETURN 'ERROR: ' || SQLERRM;
END;
$$;


-- =====================================================
-- FUNCIÓN: sp_DashboardColaborador
-- Retorna 4 result sets como tablas separadas
-- (llamar en 4 queries distintos o usar JSON)
-- =====================================================
CREATE OR REPLACE FUNCTION sp_dashboardcolaborador_resumen(p_colaboradorid INT)
RETURNS TABLE (totalpropiedades BIGINT, propiedadesactivas BIGINT, totalclicks BIGINT)
LANGUAGE sql AS $$
  SELECT
    COUNT(*),
    SUM(CASE WHEN estado = 'Activa' THEN 1 ELSE 0 END),
    SUM(clicksanuncio)
  FROM propiedades WHERE colaboradorid = p_colaboradorid AND eliminadologico = false;
$$;

CREATE OR REPLACE FUNCTION sp_dashboardcolaborador_reservas(p_colaboradorid INT)
RETURNS TABLE (reservasmes BIGINT, confirmadas BIGINT, completadas BIGINT, canceladas BIGINT, noshows BIGINT)
LANGUAGE sql AS $$
  SELECT
    COUNT(*),
    SUM(CASE WHEN r.estado = 'Confirmada'  THEN 1 ELSE 0 END),
    SUM(CASE WHEN r.estado = 'Completada'  THEN 1 ELSE 0 END),
    SUM(CASE WHEN r.estado = 'Cancelada'   THEN 1 ELSE 0 END),
    SUM(CASE WHEN r.estado = 'NoShow'      THEN 1 ELSE 0 END)
  FROM reservas r
  INNER JOIN propiedades p ON r.propiedadid = p.propiedadid
  WHERE p.colaboradorid = p_colaboradorid
    AND EXTRACT(MONTH FROM r.fechacreacion) = EXTRACT(MONTH FROM NOW())
    AND EXTRACT(YEAR  FROM r.fechacreacion) = EXTRACT(YEAR  FROM NOW());
$$;

CREATE OR REPLACE FUNCTION sp_dashboardcolaborador_ganancias(p_colaboradorid INT)
RETURNS TABLE (gananciastmes NUMERIC)
LANGUAGE sql AS $$
  SELECT COALESCE(SUM(pg.monto), 0)
  FROM pagos pg
  INNER JOIN reservas r ON pg.reservaid = r.reservaid
  INNER JOIN propiedades p ON r.propiedadid = p.propiedadid
  WHERE p.colaboradorid = p_colaboradorid AND pg.estado = 'Procesado'
    AND EXTRACT(MONTH FROM pg.fechapago) = EXTRACT(MONTH FROM NOW())
    AND EXTRACT(YEAR  FROM pg.fechapago) = EXTRACT(YEAR  FROM NOW());
$$;


-- =====================================================
-- FUNCIÓN: sp_ObtenerTasaCambio
-- =====================================================
CREATE OR REPLACE FUNCTION sp_obtenertasacambio(
  p_monedaorigenid  INT,
  p_monedadestinoid INT,
  p_monto           NUMERIC(18,2)
)
RETURNS NUMERIC(18,2) LANGUAGE plpgsql AS $$
DECLARE
  v_tasa NUMERIC(18,6);
BEGIN
  IF p_monedaorigenid = p_monedadestinoid THEN
    RETURN p_monto;
  END IF;

  SELECT tasa INTO v_tasa
  FROM tasascambio
  WHERE monedaorigenid = p_monedaorigenid
    AND monedadestinoid = p_monedadestinoid
    AND estado = true
  ORDER BY fechavigencia DESC
  LIMIT 1;

  IF v_tasa IS NULL THEN
    RETURN NULL;
  END IF;

  RETURN ROUND(p_monto * v_tasa, 2);
END;
$$;


-- =====================================================
-- FUNCIÓN: sp_AplicarDescuento
-- =====================================================
CREATE OR REPLACE FUNCTION sp_aplicardescuento(
  p_reservaid      INT,
  p_promocionid    INT     DEFAULT NULL,
  p_usarpuntos     BOOLEAN DEFAULT FALSE,
  p_puntosacanjear INT     DEFAULT 0
)
RETURNS TEXT LANGUAGE plpgsql AS $$
DECLARE
  v_subtotal        NUMERIC(12,2);
  v_clienteid       INT;
  v_porcentaje      NUMERIC(5,2) := 0;
  v_montodescuento  NUMERIC(12,2) := 0;
  v_origen          TEXT;
  v_puntosdisp      INT;
BEGIN
  SELECT subtotal, clienteid INTO v_subtotal, v_clienteid
  FROM reservas WHERE reservaid = p_reservaid AND estado = 'Pendiente';

  IF v_subtotal IS NULL THEN RETURN 'ERROR: Reserva no válida.'; END IF;

  IF p_promocionid IS NOT NULL THEN
    SELECT porcentajedescuento INTO v_porcentaje
    FROM promociones
    WHERE promocionid = p_promocionid AND estado = true
      AND NOW() BETWEEN fechainicio AND fechafin;

    IF v_porcentaje > 0 THEN
      v_montodescuento := ROUND(v_subtotal * v_porcentaje / 100, 2);
      v_origen := 'ComisionPlataforma';
    END IF;

  ELSIF p_usarpuntos AND p_puntosacanjear > 0 THEN
    SELECT puntosacumulados INTO v_puntosdisp FROM clientes WHERE clienteid = v_clienteid;

    IF p_puntosacanjear > v_puntosdisp THEN
      RETURN 'ERROR: Puntos insuficientes.';
    END IF;

    v_montodescuento := p_puntosacanjear * 0.10;  -- 1 punto = $0.10
    v_porcentaje     := ROUND((v_montodescuento / v_subtotal) * 100, 2);
    v_origen         := 'Puntos';

    INSERT INTO puntoscliente (clienteid, cantidad, tipo, descripcion, reservaid)
    VALUES (v_clienteid, -p_puntosacanjear, 'Canje', 'Canje de puntos en reserva', p_reservaid);

    UPDATE clientes SET puntosacumulados = puntosacumulados - p_puntosacanjear
    WHERE clienteid = v_clienteid;
  END IF;

  IF v_montodescuento > 0 THEN
    INSERT INTO descuentos (reservaid, clienteid, promocionid, origen, porcentaje, montodescuento, usuariocreacion)
    VALUES (p_reservaid, v_clienteid, p_promocionid, v_origen, v_porcentaje, v_montodescuento, v_clienteid::TEXT);

    UPDATE reservas SET descuento = v_montodescuento, total = subtotal - v_montodescuento,
      fechamodificacion = NOW()
    WHERE reservaid = p_reservaid;
  END IF;

  RETURN 'OK: Descuento de $' || v_montodescuento || ' aplicado (' || v_porcentaje || '%).';
EXCEPTION WHEN OTHERS THEN
  RETURN 'ERROR: ' || SQLERRM;
END;
$$;

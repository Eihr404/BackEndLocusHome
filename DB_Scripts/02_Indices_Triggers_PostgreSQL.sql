-- =====================================================
-- SCRIPT 2: ÍNDICES Y TRIGGERS (PostgreSQL / Supabase)
-- Convertido desde T-SQL a PL/pgSQL
-- =====================================================

-- =====================================================
-- ÍNDICES DE RENDIMIENTO
-- (PostgreSQL usa índices parciales igual que SQL Server)
-- =====================================================

CREATE INDEX IF NOT EXISTS ix_propiedades_ciudad     ON propiedades(ciudadid)           WHERE eliminadologico = false;
CREATE INDEX IF NOT EXISTS ix_propiedades_tipo        ON propiedades(tipoalojamientoid)  WHERE eliminadologico = false;
CREATE INDEX IF NOT EXISTS ix_propiedades_colaborador ON propiedades(colaboradorid)      WHERE eliminadologico = false;
CREATE INDEX IF NOT EXISTS ix_propiedades_estado      ON propiedades(estado)             WHERE eliminadologico = false;

CREATE INDEX IF NOT EXISTS ix_disponibilidad_fecha    ON disponibilidad(fecha, disponible);

CREATE INDEX IF NOT EXISTS ix_reservas_cliente        ON reservas(clienteid)             WHERE eliminadologico = false;
CREATE INDEX IF NOT EXISTS ix_reservas_propiedad      ON reservas(propiedadid)           WHERE eliminadologico = false;
CREATE INDEX IF NOT EXISTS ix_reservas_estado         ON reservas(estado)                WHERE eliminadologico = false;
CREATE INDEX IF NOT EXISTS ix_reservas_fechas         ON reservas(fechacheckin, fechacheckout);

CREATE INDEX IF NOT EXISTS ix_usuarios_email          ON usuarios(email)                  WHERE eliminadologico = false;
CREATE INDEX IF NOT EXISTS ix_habitaciones_propiedad  ON habitaciones(propiedadid)        WHERE eliminadologico = false;
CREATE INDEX IF NOT EXISTS ix_tarifas_habitacion_fecha ON tarifas(habitacionid, fechainicio, fechafin) WHERE eliminadologico = false;
CREATE INDEX IF NOT EXISTS ix_auditoria_tabla         ON auditoriageneral(nombretabla, fechaaccion);


-- =====================================================
-- FUNCIÓN + TRIGGER DE AUDITORÍA: USUARIOS
-- =====================================================

CREATE OR REPLACE FUNCTION fn_audit_usuarios()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
  v_operacion TEXT;
  v_datos_nuevos TEXT;
  v_datos_anteriores TEXT;
BEGIN
  IF (TG_OP = 'INSERT') THEN
    v_operacion := 'INSERT';
  ELSIF (TG_OP = 'UPDATE') THEN
    v_operacion := 'UPDATE';
  ELSE
    v_operacion := 'DELETE';
  END IF;

  IF TG_OP IN ('INSERT', 'UPDATE') THEN
    v_datos_nuevos := 'Email:' || COALESCE(NEW.email,'') ||
                      '|Nombre:' || COALESCE(NEW.nombrecompleto,'') ||
                      '|RolId:' || COALESCE(NEW.rolid::TEXT,'') ||
                      '|Estado:' || COALESCE(NEW.estado::TEXT,'');
    INSERT INTO auditoriageneral (nombretabla, operacion, registroid, datosnuevos, usuarioaccion)
    VALUES ('Usuarios', v_operacion, NEW.usuarioid::TEXT, v_datos_nuevos, NEW.usuariocreacion);
  END IF;

  IF TG_OP = 'DELETE' THEN
    v_datos_anteriores := 'Email:' || COALESCE(OLD.email,'') ||
                          '|Nombre:' || COALESCE(OLD.nombrecompleto,'') ||
                          '|RolId:' || COALESCE(OLD.rolid::TEXT,'');
    INSERT INTO auditoriageneral (nombretabla, operacion, registroid, datosanteriores)
    VALUES ('Usuarios', 'DELETE', OLD.usuarioid::TEXT, v_datos_anteriores);
  END IF;

  RETURN COALESCE(NEW, OLD);
END;
$$;

DROP TRIGGER IF EXISTS trg_audit_usuarios ON usuarios;
CREATE TRIGGER trg_audit_usuarios
AFTER INSERT OR UPDATE OR DELETE ON usuarios
FOR EACH ROW EXECUTE FUNCTION fn_audit_usuarios();


-- =====================================================
-- FUNCIÓN + TRIGGER DE AUDITORÍA: CLIENTES
-- =====================================================

CREATE OR REPLACE FUNCTION fn_audit_clientes()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
  IF TG_OP IN ('INSERT', 'UPDATE') THEN
    INSERT INTO auditoriageneral (nombretabla, operacion, registroid, datosnuevos, usuarioaccion)
    VALUES ('Clientes', TG_OP, NEW.clienteid::TEXT,
            'UsuarioId:' || COALESCE(NEW.usuarioid::TEXT,'') ||
            '|Telefono:' || COALESCE(NEW.telefono,'') ||
            '|Calificacion:' || COALESCE(NEW.calificacion::TEXT,''),
            NEW.usuariocreacion);
  END IF;

  IF TG_OP = 'DELETE' THEN
    INSERT INTO auditoriageneral (nombretabla, operacion, registroid, datosanteriores)
    VALUES ('Clientes', 'DELETE', OLD.clienteid::TEXT,
            'UsuarioId:' || COALESCE(OLD.usuarioid::TEXT,'') ||
            '|Telefono:' || COALESCE(OLD.telefono,''));
  END IF;

  RETURN COALESCE(NEW, OLD);
END;
$$;

DROP TRIGGER IF EXISTS trg_audit_clientes ON clientes;
CREATE TRIGGER trg_audit_clientes
AFTER INSERT OR UPDATE OR DELETE ON clientes
FOR EACH ROW EXECUTE FUNCTION fn_audit_clientes();


-- =====================================================
-- FUNCIÓN + TRIGGER DE AUDITORÍA: PROPIEDADES
-- =====================================================

CREATE OR REPLACE FUNCTION fn_audit_propiedades()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
  IF TG_OP IN ('INSERT', 'UPDATE') THEN
    INSERT INTO auditoriageneral (nombretabla, operacion, registroid, datosnuevos, usuarioaccion)
    VALUES ('Propiedades', TG_OP, NEW.propiedadid::TEXT,
            'Nombre:' || COALESCE(NEW.nombre,'') ||
            '|Ciudad:' || COALESCE(NEW.ciudadid::TEXT,'') ||
            '|Tipo:' || COALESCE(NEW.tipoalojamientoid::TEXT,'') ||
            '|Estado:' || COALESCE(NEW.estado,''),
            NEW.usuariocreacion);
  END IF;

  IF TG_OP = 'DELETE' THEN
    INSERT INTO auditoriageneral (nombretabla, operacion, registroid, datosanteriores)
    VALUES ('Propiedades', 'DELETE', OLD.propiedadid::TEXT,
            'Nombre:' || COALESCE(OLD.nombre,'') ||
            '|Ciudad:' || COALESCE(OLD.ciudadid::TEXT,''));
  END IF;

  RETURN COALESCE(NEW, OLD);
END;
$$;

DROP TRIGGER IF EXISTS trg_audit_propiedades ON propiedades;
CREATE TRIGGER trg_audit_propiedades
AFTER INSERT OR UPDATE OR DELETE ON propiedades
FOR EACH ROW EXECUTE FUNCTION fn_audit_propiedades();


-- =====================================================
-- FUNCIÓN + TRIGGER DE AUDITORÍA: RESERVAS
-- =====================================================

CREATE OR REPLACE FUNCTION fn_audit_reservas()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
  IF TG_OP IN ('INSERT', 'UPDATE') THEN
    INSERT INTO auditoriageneral (nombretabla, operacion, registroid, datosnuevos, usuarioaccion)
    VALUES ('Reservas', TG_OP, NEW.reservaid::TEXT,
            'Codigo:' || COALESCE(NEW.codigoreserva,'') ||
            '|Cliente:' || COALESCE(NEW.clienteid::TEXT,'') ||
            '|Propiedad:' || COALESCE(NEW.propiedadid::TEXT,'') ||
            '|CheckIn:' || COALESCE(NEW.fechacheckin::TEXT,'') ||
            '|Total:' || COALESCE(NEW.total::TEXT,'') ||
            '|Estado:' || COALESCE(NEW.estado,''),
            NEW.usuariocreacion);
  END IF;

  IF TG_OP = 'DELETE' THEN
    INSERT INTO auditoriageneral (nombretabla, operacion, registroid, datosanteriores)
    VALUES ('Reservas', 'DELETE', OLD.reservaid::TEXT,
            'Codigo:' || COALESCE(OLD.codigoreserva,'') ||
            '|Cliente:' || COALESCE(OLD.clienteid::TEXT,'') ||
            '|Total:' || COALESCE(OLD.total::TEXT,''));
  END IF;

  RETURN COALESCE(NEW, OLD);
END;
$$;

DROP TRIGGER IF EXISTS trg_audit_reservas ON reservas;
CREATE TRIGGER trg_audit_reservas
AFTER INSERT OR UPDATE OR DELETE ON reservas
FOR EACH ROW EXECUTE FUNCTION fn_audit_reservas();


-- =====================================================
-- FUNCIÓN + TRIGGER: ADVERTENCIA NOSHOW
-- =====================================================

CREATE OR REPLACE FUNCTION fn_advertencia_noshow()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
  -- Solo actuar cuando el estado cambia A 'NoShow'
  IF NEW.estado = 'NoShow' AND OLD.estado <> 'NoShow' THEN
    INSERT INTO advertenciascliente (clienteid, reservaid, tipo, descripcion, severidad, usuariocreacion)
    VALUES (
      NEW.clienteid,
      NEW.reservaid,
      'NoShow',
      'No-Show en reserva ' || COALESCE(NEW.codigoreserva,'') ||
        ' para propiedad ID: ' || NEW.propiedadid::TEXT ||
        '. Check-in: ' || NEW.fechacheckin::TEXT,
      3,
      NEW.usuariomodificacion
    );
  END IF;
  RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_advertencia_noshow ON reservas;
CREATE TRIGGER trg_advertencia_noshow
AFTER UPDATE ON reservas
FOR EACH ROW EXECUTE FUNCTION fn_advertencia_noshow();


-- =====================================================
-- FUNCIÓN + TRIGGER: BLOQUEAR DISPONIBILIDAD AL CONFIRMAR
-- =====================================================

CREATE OR REPLACE FUNCTION fn_disponibilidad_reserva_confirmada()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
  v_fecha DATE;
  v_hab   RECORD;
BEGIN
  -- Solo actuar cuando el estado cambia A 'Confirmada'
  IF NEW.estado = 'Confirmada' AND OLD.estado <> 'Confirmada' THEN
    FOR v_hab IN
      SELECT habitacionid FROM reservadetallehabitacion WHERE reservaid = NEW.reservaid
    LOOP
      v_fecha := NEW.fechacheckin;
      WHILE v_fecha < NEW.fechacheckout LOOP
        INSERT INTO disponibilidad (habitacionid, fecha, disponible)
        VALUES (v_hab.habitacionid, v_fecha, false)
        ON CONFLICT (habitacionid, fecha)
        DO UPDATE SET disponible = false, fechamodificacion = NOW();
        v_fecha := v_fecha + INTERVAL '1 day';
      END LOOP;
    END LOOP;
  END IF;
  RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_disponibilidad_reserva_confirmada ON reservas;
CREATE TRIGGER trg_disponibilidad_reserva_confirmada
AFTER UPDATE ON reservas
FOR EACH ROW EXECUTE FUNCTION fn_disponibilidad_reserva_confirmada();


-- =====================================================
-- FUNCIÓN + TRIGGER: PUNTOS POR ENCUESTA
-- =====================================================

CREATE OR REPLACE FUNCTION fn_puntos_encuesta()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
  v_puntos INT := 50;
BEGIN
  -- Registrar movimiento de puntos
  INSERT INTO puntoscliente (clienteid, cantidad, tipo, descripcion, reservaid)
  VALUES (NEW.clienteid, v_puntos, 'Encuesta',
          'Puntos por completar encuesta de experiencia - Reserva ID: ' || NEW.reservaid::TEXT,
          NEW.reservaid);

  -- Actualizar acumulado del cliente
  UPDATE clientes SET puntosacumulados = puntosacumulados + v_puntos
  WHERE clienteid = NEW.clienteid;

  -- Marcar puntos otorgados en la encuesta
  UPDATE encuestaexperiencia SET puntosotorgados = v_puntos
  WHERE encuestaid = NEW.encuestaid;

  RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_puntos_encuesta ON encuestaexperiencia;
CREATE TRIGGER trg_puntos_encuesta
AFTER INSERT ON encuestaexperiencia
FOR EACH ROW EXECUTE FUNCTION fn_puntos_encuesta();

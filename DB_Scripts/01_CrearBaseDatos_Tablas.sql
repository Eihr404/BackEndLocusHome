-- =====================================================
-- SCRIPT 1: CREACIÓN DE BASE DE DATOS Y TABLAS
-- Sistema de Reservas tipo Booking
-- =====================================================

-- Crear la base de datos
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BookingDB')
BEGIN
    CREATE DATABASE BookingDB;
END
GO

USE BookingDB;
GO

-- =====================================================
-- MÓDULO GEOGRÁFICO Y MONEDAS
-- =====================================================

CREATE TABLE Paises (
    PaisId          INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          NVARCHAR(100) NOT NULL UNIQUE,
    CodigoISO       CHAR(2) NOT NULL UNIQUE,
    Estado          BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL
);
GO

CREATE TABLE Ciudades (
    CiudadId        INT IDENTITY(1,1) PRIMARY KEY,
    PaisId          INT NOT NULL,
    Nombre          NVARCHAR(150) NOT NULL,
    EsCapital       BIT DEFAULT 0,
    Estado          BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Ciudades_Paises FOREIGN KEY (PaisId) REFERENCES Paises(PaisId)
);
GO

CREATE TABLE Monedas (
    MonedaId        INT IDENTITY(1,1) PRIMARY KEY,
    PaisId          INT NOT NULL,
    Nombre          NVARCHAR(50) NOT NULL,
    Codigo          CHAR(3) NOT NULL UNIQUE,
    Simbolo         NVARCHAR(5) NOT NULL,
    Estado          BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Monedas_Paises FOREIGN KEY (PaisId) REFERENCES Paises(PaisId)
);
GO

CREATE TABLE TasasCambio (
    TasaCambioId        INT IDENTITY(1,1) PRIMARY KEY,
    MonedaOrigenId      INT NOT NULL,
    MonedaDestinoId     INT NOT NULL,
    Tasa                DECIMAL(18,6) NOT NULL,
    FechaVigencia       DATETIME2 DEFAULT GETDATE(),
    Estado              BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_TasasCambio_MonedaOrigen FOREIGN KEY (MonedaOrigenId) REFERENCES Monedas(MonedaId),
    CONSTRAINT FK_TasasCambio_MonedaDestino FOREIGN KEY (MonedaDestinoId) REFERENCES Monedas(MonedaId)
);
GO

-- =====================================================
-- MÓDULO USUARIOS Y AUTENTICACIÓN
-- =====================================================

CREATE TABLE Roles (
    RolId           INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion     NVARCHAR(200) NULL,
    Estado          BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL
);
GO

CREATE TABLE Usuarios (
    UsuarioId       INT IDENTITY(1,1) PRIMARY KEY,
    RolId           INT NOT NULL,
    Email           NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(500) NOT NULL,
    NombreCompleto  NVARCHAR(200) NOT NULL,
    EmailVerificado BIT DEFAULT 0,
    Estado          BIT DEFAULT 1,
    UltimoAcceso    DATETIME2 NULL,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RolId) REFERENCES Roles(RolId)
);
GO

CREATE TABLE Clientes (
    ClienteId       INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId       INT NOT NULL UNIQUE,
    FotoUrl         NVARCHAR(500) NULL,
    Telefono        NVARCHAR(20) NULL,
    Domicilio       NVARCHAR(300) NULL,
    CiudadId        INT NULL,
    Calificacion    DECIMAL(3,2) DEFAULT 5.00,
    TotalReservas   INT DEFAULT 0,
    PuntosAcumulados INT DEFAULT 0,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Clientes_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT FK_Clientes_Ciudades FOREIGN KEY (CiudadId) REFERENCES Ciudades(CiudadId)
);
GO

CREATE TABLE Colaboradores (
    ColaboradorId       INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId           INT NOT NULL UNIQUE,
    FotoUrl             NVARCHAR(500) NULL,
    Telefono            NVARCHAR(20) NULL,
    NombreEmpresa       NVARCHAR(200) NULL,
    CuentaBancaria      NVARCHAR(100) NULL,
    Verificado          BIT DEFAULT 0,
    PuntosAcumulados    INT DEFAULT 0,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Colaboradores_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId)
);
GO

-- =====================================================
-- MÓDULO PROPIEDADES / ALOJAMIENTOS
-- =====================================================

CREATE TABLE TiposAlojamiento (
    TipoAlojamientoId   INT IDENTITY(1,1) PRIMARY KEY,
    Nombre              NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion         NVARCHAR(200) NULL,
    Estado              BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL
);
GO

CREATE TABLE Propiedades (
    PropiedadId         INT IDENTITY(1,1) PRIMARY KEY,
    ColaboradorId       INT NOT NULL,
    TipoAlojamientoId   INT NOT NULL,
    CiudadId            INT NOT NULL,
    Nombre              NVARCHAR(200) NOT NULL,
    Descripcion         NVARCHAR(MAX) NULL,
    Direccion           NVARCHAR(300) NOT NULL,
    Latitud             DECIMAL(10,7) NULL,
    Longitud            DECIMAL(10,7) NULL,
    Estrellas           INT NULL CHECK (Estrellas BETWEEN 1 AND 5),
    CalificacionPromedio DECIMAL(3,2) DEFAULT 0,
    TotalResenas        INT DEFAULT 0,
    AdmiteMascotas      BIT DEFAULT 0,
    Verificada          BIT DEFAULT 0,
    Estado              NVARCHAR(20) DEFAULT 'Pendiente', -- Pendiente, Activa, Suspendida
    ClicksAnuncio       INT DEFAULT 0,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Propiedades_Colaboradores FOREIGN KEY (ColaboradorId) REFERENCES Colaboradores(ColaboradorId),
    CONSTRAINT FK_Propiedades_TipoAlojamiento FOREIGN KEY (TipoAlojamientoId) REFERENCES TiposAlojamiento(TipoAlojamientoId),
    CONSTRAINT FK_Propiedades_Ciudades FOREIGN KEY (CiudadId) REFERENCES Ciudades(CiudadId)
);
GO

CREATE TABLE PropiedadFotos (
    FotoId          INT IDENTITY(1,1) PRIMARY KEY,
    PropiedadId     INT NOT NULL,
    Url             NVARCHAR(500) NOT NULL,
    Descripcion     NVARCHAR(200) NULL,
    EsPrincipal     BIT DEFAULT 0,
    Orden           INT DEFAULT 0,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    EliminadoLogico     BIT DEFAULT 0,
    CONSTRAINT FK_PropiedadFotos_Propiedades FOREIGN KEY (PropiedadId) REFERENCES Propiedades(PropiedadId)
);
GO

CREATE TABLE CatalogoInstalaciones (
    InstalacionId   INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          NVARCHAR(100) NOT NULL UNIQUE,
    Icono           NVARCHAR(50) NULL,
    Estado          BIT DEFAULT 1
);
GO

CREATE TABLE PropiedadInstalaciones (
    PropiedadInstalacionId INT IDENTITY(1,1) PRIMARY KEY,
    PropiedadId     INT NOT NULL,
    InstalacionId   INT NOT NULL,
    CONSTRAINT FK_PI_Propiedades FOREIGN KEY (PropiedadId) REFERENCES Propiedades(PropiedadId),
    CONSTRAINT FK_PI_Instalaciones FOREIGN KEY (InstalacionId) REFERENCES CatalogoInstalaciones(InstalacionId),
    CONSTRAINT UQ_Propiedad_Instalacion UNIQUE (PropiedadId, InstalacionId)
);
GO

CREATE TABLE CatalogoComidas (
    ComidaId        INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion     NVARCHAR(200) NULL,
    Estado          BIT DEFAULT 1
);
GO

CREATE TABLE PropiedadComidas (
    PropiedadComidaId   INT IDENTITY(1,1) PRIMARY KEY,
    PropiedadId         INT NOT NULL,
    ComidaId            INT NOT NULL,
    PrecioAdicional     DECIMAL(10,2) DEFAULT 0,
    CONSTRAINT FK_PC_Propiedades FOREIGN KEY (PropiedadId) REFERENCES Propiedades(PropiedadId),
    CONSTRAINT FK_PC_Comidas FOREIGN KEY (ComidaId) REFERENCES CatalogoComidas(ComidaId),
    CONSTRAINT UQ_Propiedad_Comida UNIQUE (PropiedadId, ComidaId)
);
GO

CREATE TABLE Habitaciones (
    HabitacionId        INT IDENTITY(1,1) PRIMARY KEY,
    PropiedadId         INT NOT NULL,
    Nombre              NVARCHAR(100) NOT NULL,
    Descripcion         NVARCHAR(500) NULL,
    CapacidadAdultos    INT NOT NULL DEFAULT 2,
    CapacidadNinos      INT NOT NULL DEFAULT 0,
    NumBanos            INT NOT NULL DEFAULT 1,
    NumDormitorios      INT NOT NULL DEFAULT 1,
    AdmiteMascotas      BIT DEFAULT 0,
    TieneCocina         BIT DEFAULT 0,
    TieneAireAcondicionado BIT DEFAULT 0,
    SuperficieM2        DECIMAL(6,2) NULL,
    Estado              BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Habitaciones_Propiedades FOREIGN KEY (PropiedadId) REFERENCES Propiedades(PropiedadId)
);
GO

CREATE TABLE HabitacionFotos (
    FotoId          INT IDENTITY(1,1) PRIMARY KEY,
    HabitacionId    INT NOT NULL,
    Url             NVARCHAR(500) NOT NULL,
    Descripcion     NVARCHAR(200) NULL,
    EsPrincipal     BIT DEFAULT 0,
    Orden           INT DEFAULT 0,
    FechaCreacion   DATETIME2 DEFAULT GETDATE(),
    EliminadoLogico BIT DEFAULT 0,
    CONSTRAINT FK_HabitacionFotos_Habitaciones FOREIGN KEY (HabitacionId) REFERENCES Habitaciones(HabitacionId)
);
GO

CREATE TABLE Tarifas (
    TarifaId        INT IDENTITY(1,1) PRIMARY KEY,
    HabitacionId    INT NOT NULL,
    MonedaId        INT NOT NULL,
    PrecioPorNoche  DECIMAL(10,2) NOT NULL,
    FechaInicio     DATE NOT NULL,
    FechaFin        DATE NOT NULL,
    Estado          BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Tarifas_Habitaciones FOREIGN KEY (HabitacionId) REFERENCES Habitaciones(HabitacionId),
    CONSTRAINT FK_Tarifas_Monedas FOREIGN KEY (MonedaId) REFERENCES Monedas(MonedaId),
    CONSTRAINT CK_Tarifas_Fechas CHECK (FechaFin >= FechaInicio)
);
GO

CREATE TABLE Disponibilidad (
    DisponibilidadId    INT IDENTITY(1,1) PRIMARY KEY,
    HabitacionId        INT NOT NULL,
    Fecha               DATE NOT NULL,
    Disponible          BIT DEFAULT 1,
    FechaModificacion   DATETIME2 NULL,
    CONSTRAINT FK_Disponibilidad_Habitaciones FOREIGN KEY (HabitacionId) REFERENCES Habitaciones(HabitacionId),
    CONSTRAINT UQ_Habitacion_Fecha UNIQUE (HabitacionId, Fecha)
);
GO

PRINT '✅ Módulo Propiedades creado correctamente';
GO

-- =====================================================
-- MÓDULO RESERVAS Y PAGOS
-- =====================================================

CREATE TABLE MetodosPagoCliente (
    MetodoPagoId    INT IDENTITY(1,1) PRIMARY KEY,
    ClienteId       INT NOT NULL,
    Tipo            NVARCHAR(30) NOT NULL, -- Tarjeta, EnSitio
    NumeroTarjeta   NVARCHAR(20) NULL,
    NombreTitular   NVARCHAR(200) NULL,
    FechaExpiracion NVARCHAR(7) NULL, -- MM/YYYY
    EsPrincipal     BIT DEFAULT 0,
    Estado          BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_MetodosPago_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(ClienteId)
);
GO

CREATE TABLE Reservas (
    ReservaId       INT IDENTITY(1,1) PRIMARY KEY,
    ClienteId       INT NOT NULL,
    PropiedadId     INT NOT NULL,
    FechaCheckIn    DATE NOT NULL,
    FechaCheckOut   DATE NOT NULL,
    NumAdultos      INT NOT NULL DEFAULT 1,
    NumNinos        INT NOT NULL DEFAULT 0,
    LlevaMascotas   BIT DEFAULT 0,
    NumHabitaciones INT NOT NULL DEFAULT 1,
    MonedaId        INT NOT NULL,
    SubTotal        DECIMAL(12,2) NOT NULL,
    Descuento       DECIMAL(12,2) DEFAULT 0,
    Total           DECIMAL(12,2) NOT NULL,
    Estado          NVARCHAR(30) DEFAULT 'Pendiente', -- Pendiente, Confirmada, EnCurso, Completada, Cancelada, NoShow
    CodigoReserva   NVARCHAR(20) NOT NULL UNIQUE,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Reservas_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(ClienteId),
    CONSTRAINT FK_Reservas_Propiedades FOREIGN KEY (PropiedadId) REFERENCES Propiedades(PropiedadId),
    CONSTRAINT FK_Reservas_Monedas FOREIGN KEY (MonedaId) REFERENCES Monedas(MonedaId),
    CONSTRAINT CK_Reservas_Fechas CHECK (FechaCheckOut > FechaCheckIn)
);
GO

CREATE TABLE ReservaDetalleHabitacion (
    DetalleId       INT IDENTITY(1,1) PRIMARY KEY,
    ReservaId       INT NOT NULL,
    HabitacionId    INT NOT NULL,
    PrecioPorNoche  DECIMAL(10,2) NOT NULL,
    NumNoches       INT NOT NULL,
    SubTotalHabitacion DECIMAL(12,2) NOT NULL,
    CONSTRAINT FK_RDH_Reservas FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId),
    CONSTRAINT FK_RDH_Habitaciones FOREIGN KEY (HabitacionId) REFERENCES Habitaciones(HabitacionId)
);
GO

CREATE TABLE Pagos (
    PagoId          INT IDENTITY(1,1) PRIMARY KEY,
    ReservaId       INT NOT NULL,
    MetodoPagoId    INT NULL,
    TipoPago        NVARCHAR(30) NOT NULL, -- Tarjeta, EnSitio
    Monto           DECIMAL(12,2) NOT NULL,
    MonedaId        INT NOT NULL,
    Estado          NVARCHAR(30) DEFAULT 'Pendiente', -- Pendiente, Procesado, Rechazado, Reembolsado
    ReferenciaPago  NVARCHAR(100) NULL,
    FechaPago       DATETIME2 NULL,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Pagos_Reservas FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId),
    CONSTRAINT FK_Pagos_MetodosPago FOREIGN KEY (MetodoPagoId) REFERENCES MetodosPagoCliente(MetodoPagoId),
    CONSTRAINT FK_Pagos_Monedas FOREIGN KEY (MonedaId) REFERENCES Monedas(MonedaId)
);
GO

CREATE TABLE CorreosVerificacion (
    VerificacionId  INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId       INT NOT NULL,
    ReservaId       INT NULL,
    Token           NVARCHAR(200) NOT NULL UNIQUE,
    Tipo            NVARCHAR(30) NOT NULL, -- RegistroCuenta, ConfirmarReserva
    FechaExpiracion DATETIME2 NOT NULL,
    Verificado      BIT DEFAULT 0,
    FechaVerificacion DATETIME2 NULL,
    FechaCreacion   DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Correos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT FK_Correos_Reservas FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId)
);
GO

PRINT '✅ Módulo Reservas y Pagos creado correctamente';
GO

-- =====================================================
-- MÓDULO CALIFICACIONES Y ADVERTENCIAS
-- =====================================================

CREATE TABLE CalificacionHotel (
    CalificacionId  INT IDENTITY(1,1) PRIMARY KEY,
    ClienteId       INT NOT NULL,
    PropiedadId     INT NOT NULL,
    ReservaId       INT NOT NULL,
    Puntuacion      DECIMAL(3,1) NOT NULL CHECK (Puntuacion BETWEEN 1.0 AND 10.0),
    Comentario      NVARCHAR(MAX) NULL,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_CalifHotel_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(ClienteId),
    CONSTRAINT FK_CalifHotel_Propiedades FOREIGN KEY (PropiedadId) REFERENCES Propiedades(PropiedadId),
    CONSTRAINT FK_CalifHotel_Reservas FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId),
    CONSTRAINT UQ_CalifHotel_Reserva UNIQUE (ReservaId)
);
GO

CREATE TABLE CalificacionCliente (
    CalificacionId  INT IDENTITY(1,1) PRIMARY KEY,
    ColaboradorId   INT NOT NULL,
    ClienteId       INT NOT NULL,
    ReservaId       INT NOT NULL,
    Puntuacion      DECIMAL(3,1) NOT NULL CHECK (Puntuacion BETWEEN 1.0 AND 10.0),
    Comentario      NVARCHAR(500) NULL,
    EsNoShow        BIT DEFAULT 0,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_CalifCliente_Colaboradores FOREIGN KEY (ColaboradorId) REFERENCES Colaboradores(ColaboradorId),
    CONSTRAINT FK_CalifCliente_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(ClienteId),
    CONSTRAINT FK_CalifCliente_Reservas FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId),
    CONSTRAINT UQ_CalifCliente_Reserva UNIQUE (ReservaId)
);
GO

CREATE TABLE AdvertenciasCliente (
    AdvertenciaId   INT IDENTITY(1,1) PRIMARY KEY,
    ClienteId       INT NOT NULL,
    ReservaId       INT NULL,
    Tipo            NVARCHAR(50) NOT NULL, -- NoShow, Danios, Comportamiento
    Descripcion     NVARCHAR(500) NULL,
    Severidad       INT DEFAULT 1 CHECK (Severidad BETWEEN 1 AND 5),
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Advertencias_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(ClienteId),
    CONSTRAINT FK_Advertencias_Reservas FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId)
);
GO

CREATE TABLE EncuestaExperiencia (
    EncuestaId      INT IDENTITY(1,1) PRIMARY KEY,
    ClienteId       INT NOT NULL,
    ReservaId       INT NOT NULL,
    PropiedadId     INT NOT NULL,
    CalificacionGeneral DECIMAL(3,1) NOT NULL CHECK (CalificacionGeneral BETWEEN 1.0 AND 10.0),
    Limpieza        DECIMAL(3,1) NULL CHECK (Limpieza BETWEEN 1.0 AND 10.0),
    Ubicacion       DECIMAL(3,1) NULL CHECK (Ubicacion BETWEEN 1.0 AND 10.0),
    Servicio        DECIMAL(3,1) NULL CHECK (Servicio BETWEEN 1.0 AND 10.0),
    RelacionCalidadPrecio DECIMAL(3,1) NULL CHECK (RelacionCalidadPrecio BETWEEN 1.0 AND 10.0),
    Comentarios     NVARCHAR(MAX) NULL,
    PuntosOtorgados INT DEFAULT 0,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Encuesta_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(ClienteId),
    CONSTRAINT FK_Encuesta_Reservas FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId),
    CONSTRAINT FK_Encuesta_Propiedades FOREIGN KEY (PropiedadId) REFERENCES Propiedades(PropiedadId),
    CONSTRAINT UQ_Encuesta_Reserva UNIQUE (ReservaId)
);
GO

PRINT '✅ Módulo Calificaciones y Advertencias creado correctamente';
GO

-- =====================================================
-- MÓDULO PUNTOS, PROMOCIONES Y DESCUENTOS
-- =====================================================

CREATE TABLE PuntosCliente (
    PuntoId         INT IDENTITY(1,1) PRIMARY KEY,
    ClienteId       INT NOT NULL,
    Cantidad        INT NOT NULL,
    Tipo            NVARCHAR(50) NOT NULL, -- Encuesta, Reserva, Canje, Bonus
    Descripcion     NVARCHAR(200) NULL,
    ReservaId       INT NULL,
    FechaCreacion   DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_PuntosCliente_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(ClienteId),
    CONSTRAINT FK_PuntosCliente_Reservas FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId)
);
GO

CREATE TABLE PuntosColaborador (
    PuntoId         INT IDENTITY(1,1) PRIMARY KEY,
    ColaboradorId   INT NOT NULL,
    Cantidad        INT NOT NULL,
    Tipo            NVARCHAR(50) NOT NULL, -- Promocion, Visibilidad, Canje
    Descripcion     NVARCHAR(200) NULL,
    FechaCreacion   DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_PuntosColab_Colaboradores FOREIGN KEY (ColaboradorId) REFERENCES Colaboradores(ColaboradorId)
);
GO

CREATE TABLE ComisionPlataforma (
    ComisionId      INT IDENTITY(1,1) PRIMARY KEY,
    Porcentaje      DECIMAL(5,2) NOT NULL, -- Ej: 15.00 = 15%
    Descripcion     NVARCHAR(200) NULL,
    FechaInicio     DATE NOT NULL,
    FechaFin        DATE NULL,
    Estado          BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL
);
GO

CREATE TABLE Promociones (
    PromocionId     INT IDENTITY(1,1) PRIMARY KEY,
    PropiedadId     INT NOT NULL,
    Nombre          NVARCHAR(200) NOT NULL,
    Descripcion     NVARCHAR(500) NULL,
    PorcentajeDescuento DECIMAL(5,2) NOT NULL,
    FechaInicio     DATE NOT NULL,
    FechaFin        DATE NOT NULL,
    Estado          BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    FechaModificacion   DATETIME2 NULL,
    UsuarioModificacion NVARCHAR(100) NULL,
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Promociones_Propiedades FOREIGN KEY (PropiedadId) REFERENCES Propiedades(PropiedadId),
    CONSTRAINT CK_Promociones_Fechas CHECK (FechaFin >= FechaInicio)
);
GO

CREATE TABLE Descuentos (
    DescuentoId     INT IDENTITY(1,1) PRIMARY KEY,
    ReservaId       INT NULL,
    ClienteId       INT NULL,
    PromocionId     INT NULL,
    Origen          NVARCHAR(50) NOT NULL, -- ComisionPlataforma, AprobadoPorDueno, Puntos
    Porcentaje      DECIMAL(5,2) NOT NULL,
    MontoDescuento  DECIMAL(12,2) NOT NULL,
    AprobadoPorDueno BIT DEFAULT 0,
    Estado          BIT DEFAULT 1,
    FechaCreacion       DATETIME2 DEFAULT GETDATE(),
    UsuarioCreacion     NVARCHAR(100),
    EliminadoLogico     BIT DEFAULT 0,
    IpOrigen            NVARCHAR(45) NULL,
    CONSTRAINT FK_Descuentos_Reservas FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId),
    CONSTRAINT FK_Descuentos_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(ClienteId),
    CONSTRAINT FK_Descuentos_Promociones FOREIGN KEY (PromocionId) REFERENCES Promociones(PromocionId)
);
GO

PRINT '✅ Módulo Puntos, Promociones y Descuentos creado correctamente';
GO

-- =====================================================
-- MÓDULO AUDITORÍA
-- =====================================================

CREATE TABLE AuditoriaGeneral (
    AuditoriaId     BIGINT IDENTITY(1,1) PRIMARY KEY,
    NombreTabla     NVARCHAR(100) NOT NULL,
    Operacion       NVARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    RegistroId      NVARCHAR(50) NOT NULL,
    DatosAnteriores NVARCHAR(MAX) NULL,
    DatosNuevos     NVARCHAR(MAX) NULL,
    UsuarioAccion   NVARCHAR(100) NULL,
    FechaAccion     DATETIME2 DEFAULT GETDATE(),
    IpOrigen        NVARCHAR(45) NULL
);
GO

PRINT '✅ Módulo Auditoría creado correctamente';
PRINT '=============================================';
PRINT '✅ TODAS LAS 34 TABLAS CREADAS EXITOSAMENTE';
PRINT '=============================================';
GO

-- =====================================================
-- SCRIPT 4: DATOS INICIALES (SEEDS)
-- Sistema de Reservas tipo Booking
-- Ejecutar DESPUÉS de 03_StoredProcedures.sql
-- =====================================================

USE BookingDB;
GO

-- =====================================================
-- ROLES
-- =====================================================
INSERT INTO Roles (Nombre, Descripcion, UsuarioCreacion) VALUES
('Administrador', 'Administrador general de la plataforma', 'SYSTEM'),
('Cliente', 'Usuario huésped que reserva alojamientos', 'SYSTEM'),
('Colaborador', 'Dueño/administrador de propiedades hoteleras', 'SYSTEM');
GO

-- =====================================================
-- TIPOS DE ALOJAMIENTO
-- =====================================================
INSERT INTO TiposAlojamiento (Nombre, Descripcion, UsuarioCreacion) VALUES
('Hotel', 'Establecimiento hotelero tradicional con servicio completo', 'SYSTEM'),
('Suite', 'Suite de lujo con servicios premium y espacios amplios', 'SYSTEM'),
('Departamento', 'Departamento o apartamento independiente para estadías', 'SYSTEM');
GO

-- =====================================================
-- CATÁLOGO DE INSTALACIONES
-- =====================================================
INSERT INTO CatalogoInstalaciones (Nombre, Icono) VALUES
('Piscina', 'pool'),
('Spa', 'spa'),
('Parking', 'local_parking'),
('Aire Acondicionado', 'ac_unit'),
('Cocina', 'kitchen'),
('WiFi Gratuito', 'wifi'),
('Gimnasio', 'fitness_center'),
('Restaurante', 'restaurant'),
('Lavandería', 'local_laundry_service'),
('Room Service', 'room_service'),
('Bar', 'local_bar'),
('Terraza', 'deck');
GO

-- =====================================================
-- CATÁLOGO DE COMIDAS
-- =====================================================
INSERT INTO CatalogoComidas (Nombre, Descripcion) VALUES
('Desayuno', 'Desayuno buffet o continental incluido'),
('Cena', 'Cena en restaurante del establecimiento'),
('Desayuno y Cena', 'Plan de media pensión: desayuno y cena incluidos');
GO

-- =====================================================
-- PAÍSES (10 + Ecuador = 11)
-- =====================================================
INSERT INTO Paises (Nombre, CodigoISO, UsuarioCreacion) VALUES
('Estados Unidos', 'US', 'SYSTEM'),
('México', 'MX', 'SYSTEM'),
('España', 'ES', 'SYSTEM'),
('Francia', 'FR', 'SYSTEM'),
('Italia', 'IT', 'SYSTEM'),
('Alemania', 'DE', 'SYSTEM'),
('Japón', 'JP', 'SYSTEM'),
('Brasil', 'BR', 'SYSTEM'),
('Argentina', 'AR', 'SYSTEM'),
('Colombia', 'CO', 'SYSTEM'),
('Ecuador', 'EC', 'SYSTEM');
GO

-- =====================================================
-- MONEDAS
-- =====================================================
INSERT INTO Monedas (PaisId, Nombre, Codigo, Simbolo, UsuarioCreacion) VALUES
((SELECT PaisId FROM Paises WHERE CodigoISO='US'), 'Dólar Estadounidense', 'USD', '$', 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='MX'), 'Peso Mexicano', 'MXN', '$', 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='ES'), 'Euro', 'EUR', '€', 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='FR'), 'Euro (Francia)', 'EUF', '€', 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='IT'), 'Euro (Italia)', 'EUI', '€', 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='DE'), 'Euro (Alemania)', 'EUD', '€', 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='JP'), 'Yen Japonés', 'JPY', '¥', 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='BR'), 'Real Brasileño', 'BRL', 'R$', 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='AR'), 'Peso Argentino', 'ARS', '$', 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='CO'), 'Peso Colombiano', 'COP', '$', 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='EC'), 'Dólar (Ecuador)', 'UEC', '$', 'SYSTEM');
GO

-- =====================================================
-- CIUDADES (2 por país = 22)
-- =====================================================
INSERT INTO Ciudades (PaisId, Nombre, EsCapital, UsuarioCreacion) VALUES
-- Estados Unidos
((SELECT PaisId FROM Paises WHERE CodigoISO='US'), 'Washington D.C.', 1, 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='US'), 'Nueva York', 0, 'SYSTEM'),
-- México
((SELECT PaisId FROM Paises WHERE CodigoISO='MX'), 'Ciudad de México', 1, 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='MX'), 'Cancún', 0, 'SYSTEM'),
-- España
((SELECT PaisId FROM Paises WHERE CodigoISO='ES'), 'Madrid', 1, 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='ES'), 'Barcelona', 0, 'SYSTEM'),
-- Francia
((SELECT PaisId FROM Paises WHERE CodigoISO='FR'), 'París', 1, 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='FR'), 'Niza', 0, 'SYSTEM'),
-- Italia
((SELECT PaisId FROM Paises WHERE CodigoISO='IT'), 'Roma', 1, 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='IT'), 'Milán', 0, 'SYSTEM'),
-- Alemania
((SELECT PaisId FROM Paises WHERE CodigoISO='DE'), 'Berlín', 1, 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='DE'), 'Múnich', 0, 'SYSTEM'),
-- Japón
((SELECT PaisId FROM Paises WHERE CodigoISO='JP'), 'Tokio', 1, 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='JP'), 'Kioto', 0, 'SYSTEM'),
-- Brasil
((SELECT PaisId FROM Paises WHERE CodigoISO='BR'), 'Brasilia', 1, 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='BR'), 'Río de Janeiro', 0, 'SYSTEM'),
-- Argentina
((SELECT PaisId FROM Paises WHERE CodigoISO='AR'), 'Buenos Aires', 1, 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='AR'), 'Bariloche', 0, 'SYSTEM'),
-- Colombia
((SELECT PaisId FROM Paises WHERE CodigoISO='CO'), 'Bogotá', 1, 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='CO'), 'Cartagena', 0, 'SYSTEM'),
-- Ecuador
((SELECT PaisId FROM Paises WHERE CodigoISO='EC'), 'Quito', 1, 'SYSTEM'),
((SELECT PaisId FROM Paises WHERE CodigoISO='EC'), 'Guayaquil', 0, 'SYSTEM');
GO

-- =====================================================
-- TASAS DE CAMBIO (relativas al USD)
-- =====================================================
INSERT INTO TasasCambio (MonedaOrigenId, MonedaDestinoId, Tasa, UsuarioCreacion)
SELECT mo.MonedaId, md.MonedaId, tc.Tasa, 'SYSTEM'
FROM (VALUES 
    ('USD','MXN',17.20), ('USD','EUR',0.92), ('USD','JPY',149.50),
    ('USD','BRL',4.97), ('USD','ARS',875.00), ('USD','COP',3950.00),
    ('MXN','USD',0.058), ('EUR','USD',1.087), ('JPY','USD',0.0067),
    ('BRL','USD',0.201), ('ARS','USD',0.00114), ('COP','USD',0.000253)
) AS tc(Origen, Destino, Tasa)
INNER JOIN Monedas mo ON mo.Codigo = tc.Origen
INNER JOIN Monedas md ON md.Codigo = tc.Destino;
GO

-- =====================================================
-- COMISIÓN PLATAFORMA INICIAL
-- =====================================================
INSERT INTO ComisionPlataforma (Porcentaje, Descripcion, FechaInicio, UsuarioCreacion)
VALUES (15.00, 'Comisión estándar de la plataforma sobre cada reserva', '2026-01-01', 'SYSTEM');
GO

-- =====================================================
-- USUARIO ADMINISTRADOR DE PRUEBA
-- =====================================================
INSERT INTO Usuarios (RolId, Email, PasswordHash, NombreCompleto, EmailVerificado, UsuarioCreacion)
VALUES (1, 'admin@bookingapp.com', 'HASH_TEMPORAL_CAMBIAR', 'Administrador General', 1, 'SYSTEM');
GO

-- =====================================================
-- 55 PROPIEDADES FICTICIAS (5 por país)
-- Requiere primero crear usuarios colaboradores
-- =====================================================

-- Crear 11 usuarios colaboradores (1 por país)
DECLARE @i INT = 1;
WHILE @i <= 11
BEGIN
    INSERT INTO Usuarios (RolId, Email, PasswordHash, NombreCompleto, EmailVerificado, UsuarioCreacion)
    VALUES (3, CONCAT('colaborador', @i, '@bookingapp.com'), 'HASH_TEMPORAL_CAMBIAR', 
        CONCAT('Colaborador Demo ', @i), 1, 'SYSTEM');
    
    INSERT INTO Colaboradores (UsuarioId, NombreEmpresa, Verificado, UsuarioCreacion)
    VALUES (SCOPE_IDENTITY(), CONCAT('Empresa Hotelera ', @i), 1, 'SYSTEM');
    
    SET @i = @i + 1;
END;
GO

-- Insertar 55 propiedades (5 por país, distribuidas entre las 2 ciudades)
DECLARE @PropData TABLE (Nombre NVARCHAR(200), CiudadNombre NVARCHAR(150), Tipo INT, Mascotas BIT);
INSERT INTO @PropData VALUES
-- EE.UU. - Washington D.C. y Nueva York
('The Capitol Grand Hotel', 'Washington D.C.', 1, 0),
('Potomac Riverfront Suites', 'Washington D.C.', 2, 1),
('Manhattan Skyline Hotel', 'Nueva York', 1, 0),
('Central Park Luxury Apartments', 'Nueva York', 3, 1),
('Times Square Premium Hotel', 'Nueva York', 1, 0),
-- México
('Hotel Reforma Palace', 'Ciudad de México', 1, 0),
('Chapultepec Executive Suites', 'Ciudad de México', 2, 1),
('Caribe Maya Resort', 'Cancún', 1, 1),
('Playa del Sol Apartments', 'Cancún', 3, 1),
('Riviera Cancún Grand Hotel', 'Cancún', 1, 0),
-- España
('Hotel Puerta del Sol', 'Madrid', 1, 0),
('Gran Vía Luxury Suites', 'Madrid', 2, 0),
('La Rambla Boutique Hotel', 'Barcelona', 1, 1),
('Gaudí Apartments Barcelona', 'Barcelona', 3, 1),
('Sagrada Familia Premium Hotel', 'Barcelona', 1, 0),
-- Francia
('Le Marais Élégance Hotel', 'París', 1, 0),
('Champs-Élysées Royal Suites', 'París', 2, 0),
('Tour Eiffel View Apartments', 'París', 3, 1),
('Côte d''Azur Beach Hotel', 'Niza', 1, 1),
('Promenade Nice Suites', 'Niza', 2, 0),
-- Italia
('Hotel Colosseo Roma', 'Roma', 1, 0),
('Fontana di Trevi Boutique Suites', 'Roma', 2, 0),
('Trastevere Apartments Roma', 'Roma', 3, 1),
('Duomo Milano Grand Hotel', 'Milán', 1, 0),
('Navigli Design Apartments', 'Milán', 3, 1),
-- Alemania
('Brandenburg Gate Hotel', 'Berlín', 1, 0),
('Mitte Berlin Design Suites', 'Berlín', 2, 1),
('Berliner Stadthaus Apartments', 'Berlín', 3, 1),
('Marienplatz Bavarian Hotel', 'Múnich', 1, 1),
('Englischer Garten Suites', 'Múnich', 2, 0),
-- Japón
('Shibuya Sakura Hotel', 'Tokio', 1, 0),
('Ginza Imperial Suites', 'Tokio', 2, 0),
('Shinjuku Modern Apartments', 'Tokio', 3, 0),
('Kiyomizu Temple View Hotel', 'Kioto', 1, 0),
('Geisha District Traditional Suites', 'Kioto', 2, 0),
-- Brasil
('Brasília Palace Hotel', 'Brasilia', 1, 0),
('Lago Paranoá Suites', 'Brasilia', 2, 1),
('Copacabana Beach Grand Hotel', 'Río de Janeiro', 1, 1),
('Ipanema Luxury Apartments', 'Río de Janeiro', 3, 1),
('Cristo Redentor View Hotel', 'Río de Janeiro', 1, 0),
-- Argentina
('Obelisco Buenos Aires Hotel', 'Buenos Aires', 1, 0),
('Palermo Soho Design Suites', 'Buenos Aires', 2, 1),
('Recoleta Premium Apartments', 'Buenos Aires', 3, 0),
('Nahuel Huapi Lakeside Hotel', 'Bariloche', 1, 1),
('Cerro Catedral Mountain Suites', 'Bariloche', 2, 0),
-- Colombia
('Hotel Zona Rosa Bogotá', 'Bogotá', 1, 0),
('Usaquén Colonial Suites', 'Bogotá', 2, 0),
('Candelaria Boutique Apartments', 'Bogotá', 3, 1),
('Cartagena Walled City Hotel', 'Cartagena', 1, 1),
('Bocagrande Beach Suites', 'Cartagena', 2, 0),
-- Ecuador
('Hotel Plaza Grande Quito', 'Quito', 1, 0),
('La Mariscal Suites Quito', 'Quito', 2, 1),
('Centro Histórico Apartments', 'Quito', 3, 0),
('Malecón 2000 Grand Hotel', 'Guayaquil', 1, 0),
('Las Peñas Boutique Suites', 'Guayaquil', 2, 1);

-- Ahora insertar las propiedades con las referencias correctas
DECLARE @ColabCounter INT = 1;
DECLARE @PaisCounter INT = 0;
DECLARE @PrevCiudad NVARCHAR(150) = '';
DECLARE @CurColId INT;

DECLARE prop_cursor CURSOR FOR 
    SELECT pd.Nombre, pd.CiudadNombre, pd.Tipo, pd.Mascotas FROM @PropData pd;

DECLARE @pNombre NVARCHAR(200), @pCiudadNombre NVARCHAR(150), @pTipo INT, @pMascotas BIT;
DECLARE @pCiudadId INT, @pColabId INT;
DECLARE @PropCount INT = 0;

OPEN prop_cursor;
FETCH NEXT FROM prop_cursor INTO @pNombre, @pCiudadNombre, @pTipo, @pMascotas;

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @pCiudadId = CiudadId FROM Ciudades WHERE Nombre = @pCiudadNombre;
    
    -- Rotar colaboradores (cada 5 propiedades un colaborador distinto)
    SET @PropCount = @PropCount + 1;
    SET @pColabId = ((@PropCount - 1) / 5) + 1;

    INSERT INTO Propiedades (ColaboradorId, TipoAlojamientoId, CiudadId, Nombre, 
        Descripcion, Direccion, Estrellas, AdmiteMascotas, Estado, Verificada, UsuarioCreacion)
    VALUES (@pColabId, @pTipo, @pCiudadId, @pNombre,
        CONCAT('Excelente alojamiento en ', @pCiudadNombre, '. Disfrute de una experiencia única con las mejores comodidades y ubicación privilegiada.'),
        CONCAT('Calle Principal #', @PropCount * 10, ', ', @pCiudadNombre),
        CASE @pTipo WHEN 2 THEN 5 ELSE 3 + ABS(CHECKSUM(NEWID())) % 3 END,
        @pMascotas, 'Activa', 1, 'SYSTEM');

    FETCH NEXT FROM prop_cursor INTO @pNombre, @pCiudadNombre, @pTipo, @pMascotas;
END;

CLOSE prop_cursor;
DEALLOCATE prop_cursor;
GO

PRINT '=============================================';
PRINT '✅ DATOS INICIALES INSERTADOS EXITOSAMENTE';
PRINT '  - 3 Roles';
PRINT '  - 3 Tipos de Alojamiento';
PRINT '  - 12 Instalaciones';
PRINT '  - 3 Planes de Comida';
PRINT '  - 11 Países';
PRINT '  - 11 Monedas';
PRINT '  - 22 Ciudades';
PRINT '  - Tasas de Cambio';
PRINT '  - 1 Comisión Plataforma (15%)';
PRINT '  - 1 Usuario Admin';
PRINT '  - 11 Colaboradores Demo';
PRINT '  - 55 Propiedades Ficticias';
PRINT '=============================================';
GO

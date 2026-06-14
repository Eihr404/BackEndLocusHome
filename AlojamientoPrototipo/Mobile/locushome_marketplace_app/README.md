# LocusHome Marketplace App

Cliente movil en `Flutter` para el marketplace de LocusHome, consumiendo solo el `GraphQL Gateway`.

## Requisitos

- Flutter SDK 3.24 o superior
- GraphQL Gateway corriendo en local
- En Windows: `Developer Mode` habilitado para compilar con plugins

## Habilitar Developer Mode en Windows

Si Flutter muestra el error de `symlink support`, abre la configuracion de desarrollador:

```powershell
start ms-settings:developers
```

Luego habilita `Developer Mode` y reinicia la terminal.

## Preparacion inicial

Dentro de la carpeta de la app:

```powershell
C:\Users\israe\flutter\bin\flutter.bat pub get
C:\Users\israe\flutter\bin\flutter.bat analyze
C:\Users\israe\flutter\bin\flutter.bat test
```

## Ejecutar en local

### Windows desktop

```powershell
C:\Users\israe\flutter\bin\flutter.bat run -d windows --dart-define=GRAPHQL_ENDPOINT=http://localhost:5095/graphql
```

### Android emulator

```powershell
C:\Users\israe\flutter\bin\flutter.bat run -d emulator-5554 --dart-define=GRAPHQL_ENDPOINT=http://10.0.2.2:5095/graphql
```

## Endpoints sugeridos

- Windows desktop: `http://localhost:5095/graphql`
- Android Emulator: `http://10.0.2.2:5095/graphql`
- iOS Simulator: `http://localhost:5095/graphql`
- Dispositivo fisico: usar la IP LAN de la maquina host, por ejemplo `http://192.168.x.x:5095/graphql`

## Alcance actual

- login
- catalogo del marketplace
- detalle de alojamiento
- crear reserva
- crear factura
- mis reservas

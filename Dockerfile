# ── ETAPA 1: Compilar el Frontend (React/Vite) ────────────────
FROM node:20 AS build-node
WORKDIR /app
COPY booking-frontend/package*.json ./
RUN npm install
COPY booking-frontend/ .
RUN npm run build

# ── ETAPA 2: Compilar el Backend (.NET) ────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-net
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Microservicio.Clientes.Api/Microservicio.Clientes.Api.csproj", "Microservicio.Clientes.Api/"]
COPY ["Microservicio.Clientes.Business/Microservicio.Clientes.Business.csproj", "Microservicio.Clientes.Business/"]
COPY ["Microservicio.Clientes.DataManagement/Microservicio.Clientes.DataManagement.csproj", "Microservicio.Clientes.DataManagement/"]
COPY ["Microservicio.Cliente.DatAccess/Microservicio.Cliente.DatAccess.csproj", "Microservicio.Cliente.DatAccess/"]

RUN dotnet restore "Microservicio.Clientes.Api/Microservicio.Clientes.Api.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/Microservicio.Clientes.Api"

# Copiar el Front-end compilado a la carpeta wwwroot de la API
COPY --from=build-node /app/dist ./wwwroot

RUN dotnet publish "Microservicio.Clientes.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── ETAPA 3: Runtime Final ────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build-net /app/publish .

# Render usa el puerto 10000 por defecto
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Microservicio.Clientes.Api.dll"]

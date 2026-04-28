# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["Microservicio.Clientes.Api/Microservicio.Clientes.Api.csproj", "Microservicio.Clientes.Api/"]
COPY ["Microservicio.Clientes.Business/Microservicio.Clientes.Business.csproj", "Microservicio.Clientes.Business/"]
COPY ["Microservicio.Clientes.DataManagement/Microservicio.Clientes.DataManagement.csproj", "Microservicio.Clientes.DataManagement/"]
COPY ["Microservicio.Cliente.DatAccess/Microservicio.Cliente.DatAccess.csproj", "Microservicio.Cliente.DatAccess/"]

RUN dotnet restore "Microservicio.Clientes.Api/Microservicio.Clientes.Api.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/Microservicio.Clientes.Api"
RUN dotnet build "Microservicio.Clientes.Api.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "Microservicio.Clientes.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Puerto que usa Render por defecto
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Microservicio.Clientes.Api.dll"]

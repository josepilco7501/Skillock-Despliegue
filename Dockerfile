# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Skillock/Skillock.csproj", "Skillock/"]
COPY ["Skillock.Application/Skillock.Application.csproj", "Skillock.Application/"]
COPY ["Skillock.Domain/Skillock.Domain.csproj", "Skillock.Domain/"]
COPY ["Skillock.Infrastructure/Skillock.Infrastructure.csproj", "Skillock.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "Skillock/Skillock.csproj"

# Copiar todo el código
COPY . .

# Compilar en Release
RUN dotnet build "Skillock/Skillock.csproj" -c Release -o /app/build

# Publicar
RUN dotnet publish "Skillock/Skillock.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar archivo publicado desde build stage
COPY --from=build /app/publish .

# Exponer puerto (Render asignará el puerto a través de variable de entorno PORT)
EXPOSE 8080

# Comando de inicio
ENTRYPOINT ["dotnet", "Skillock.dll", "--urls", "http://0.0.0.0:8080"]


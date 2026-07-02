# 🐘 Configuración de PostgreSQL para Skillock

## Descripción

El proyecto está configurado para usar **PostgreSQL** instalado localmente en tu sistema:
- ✅ **Desarrollo**: PostgreSQL local (instalación del sistema)
- ✅ **Producción**: PostgreSQL en servidor Linux
- ✅ **ORM**: Entity Framework Core con Npgsql

---

## 🚀 Instalación y Configuración

### Requisitos:
- PostgreSQL 14+ instalado en tu sistema
- pgAdmin (opcional, para interfaz gráfica)

### Pasos:

#### 1. Crear la base de datos para desarrollo

```bash
# Conectarse como superusuario
psql -U postgres

# Una vez en la consola de PostgreSQL, ejecutar:
CREATE DATABASE skillock_dev;

# Verificar que se creó
\l

# Salir
\q
```

#### 2. Verificar connection string en `appsettings.Development.json`

El archivo ya está configurado con:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=skillock_dev;Username=postgres;Password=postgres;Include Error Detail=true"
}
```

**Si tu contraseña de PostgreSQL es diferente**, ajusta en el archivo.

---

## 📋 Archivos Configurados

| Archivo | Cambio |
|---------|--------|
| `Skillock-ProyectoFinal/Program.cs` | ✅ Agregado DbContext con `.UseNpgsql()` |
| `Skillock-ProyectoFinal/appsettings.json` | ✅ Connection string producción |
| `Skillock-ProyectoFinal/appsettings.Development.json` | ✅ Connection string desarrollo (localhost) |
| `Skillock.Infrastructure/Skillock.Infrastructure.csproj` | ✅ Paquete `Npgsql.EntityFrameworkCore.PostgreSQL` |
| `Skillock.Infrastructure/Persistence/SkilockDbContext.cs` | ✅ Nuevo DbContext para PostgreSQL |

---

## 🔧 Próximos Pasos: Crear Migraciones

Una vez PostgreSQL esté corriendo localmente:

```bash
# Desde la carpeta del proyecto principal
cd /home/josepilco/RiderProjects/Skillock-ProyectoFinal

# 1. Restaurar paquetes NuGet
dotnet restore

# 2. Crear primera migración
dotnet ef migrations add InitialCreate --project ../Skillock.Infrastructure --startup-project .

# 3. Aplicar migración a la BD
dotnet ef database update --project ../Skillock.Infrastructure --startup-project .
```

---

## 📝 Connection Strings Referencia

### Desarrollo (Local)
```
Host=localhost;Port=5432;Database=skillock_dev;Username=postgres;Password=postgres;Include Error Detail=true
```

**Ajusta `Password=postgres` si es diferente en tu instalación.**

### Producción (servidor Linux)
```
Host=your-prod-server;Port=5432;Database=skillock_prod;Username=skillock_user;Password=your-secure-password;Include Error Detail=true
```

---

## ✅ Verificación

Desde la terminal:

```bash
# Verificar que PostgreSQL está corriendo
psql -U postgres -d skillock_dev -c "SELECT 1"

# Resultado esperado:
# ?column?
# ----------
#        1
# (1 row)
```

Desde Visual Studio/Rider:

```bash
# Build proyecto
dotnet build

# Si todo compila sin errores, listo para migraciones
dotnet ef database update
```

---

## 🛠️ Troubleshooting

### Error: "cannot connect to server"
- Verifica que PostgreSQL está corriendo: `psql -U postgres`
- Comprueba el puerto (default: 5432)
- Revisa la contraseña en la connection string

### Error: "database skillock_dev does not exist"
```bash
psql -U postgres -c "CREATE DATABASE skillock_dev;"
```

### Limpiar y reiniciar
```bash
# Borrar BD completamente
psql -U postgres -c "DROP DATABASE skillock_dev;"

# Recrearla
psql -U postgres -c "CREATE DATABASE skillock_dev;"

# Replicar migraciones
dotnet ef database update
```

---

¡PostgreSQL instalado localmente está listo! 🎉




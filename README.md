# 🚀 ASP.NET Core 8 Web API

## 📋 Requisitos Previos

Antes de comenzar, asegúrate de tener instalado lo siguiente en tu sistema:

- **.NET 8 SDK**
  - Descárgalo desde: https://dotnet.microsoft.com/download/dotnet/8.0
  - Para verificar si ya lo tienes instalado, ejecuta: `dotnet --version`
- **Visual Studio 2022** (recomendado)
  - Visual Studio Code: https://code.visualstudio.com/
- **SQL Server**
  - SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads

## 🚀 Instalación y Configuración

### Usando Visual Studio 2022

#### 1️⃣ Clonar el Repositorio

```bash
git clone [URL_DEL_REPOSITORIO]
cd [NOMBRE_DEL_PROYECTO]
```

#### 2️⃣ Abrir la Solución

- Abre **Visual Studio 2022**
- Selecciona **File → Open → Project/Solution**
- Navega hasta la carpeta del proyecto y abre el archivo `.sln`

#### 3️⃣ Restaurar Paquetes NuGet

Visual Studio restaurará automáticamente los paquetes. Si no lo hace:
- Clic derecho en la solución en el **Solution Explorer**
- Selecciona **Restore NuGet Packages**

#### 4️⃣ Configurar la Cadena de Conexión

Edita o verifica el archivo `appsettings.json` o `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CHUBB_PRUEBA;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
  }
}
```

#### 5️⃣ Ejecutar la Aplicación

Presiona **F5** o haz clic en el botón **Play** en la barra de herramientas.


## 🌐 Acceder a la API

Una vez que la aplicación esté en ejecución, podrás acceder a ella en:

```
https://localhost:[PUERTO]
```

El puerto se mostrará en la consola al iniciar la aplicación. Normalmente es:
- **https://localhost:7179** (HTTPS)
- **http://localhost:5039** (HTTP)

### 📄 Swagger UI (Documentación de la API)

Si el proyecto tiene Swagger habilitado, accede a:

```
https://localhost:[PUERTO]/swagger
```

Aquí podrás ver y probar todos los endpoints de la API.

---
# 🚀 Base de datos SQL Server

## 📋 Requisitos Previos

#### 1️⃣ Crear base de datos:

```sql
CREATE DATABASE CHUBB_PRUEBA;
```

#### 2️⃣ Posteriormente ejecutar script de base de datos compartido o adjunto por  correo.
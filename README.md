# TestTalenty
.Net 8 C# AI Anormaly Carbon emission register

Este repositorio contiene la API desarrollada en .NET para el análisis y detección de anomalías en emisiones de CO2.

## Requisitos Previos

- .NET 8.0 SDK o superior.
- Visual Studio 2022 (opcional, para desarrollo con interfaz gráfica).

## 🚀 Ejecución desde la Línea de Comandos

1. Abre tu terminal (CMD, PowerShell o Terminal de Git) y navega hasta la carpeta del proyecto de la API (donde se encuentra el archivo `.csproj`):
   ```bash
   cd Co2AnomalyDetection.Api

2. Ejecuta el comando de inicio:
       dotnet run
3. La consola te indicará los puertos en los que está escuchando la aplicación (por ejemplo, http://localhost:5030 o mediante HTTPS).

4. Puedes acceder a la interfaz interactiva de Swagger en el navegador en la ruta:
    https://localhost:7156/swagger o http://localhost:5030/swagger.

## 🛠️ Ejecución desde Visual Studio 2022
1. Abre Visual Studio 2022 y selecciona Abrir un proyecto o solución.

2. Busca y selecciona la solución o el archivo del proyecto de la API (.csproj).

3. En la barra de herramientas superior, asegúrate de que el perfil de ejecución esté configurado en modo https o http (por ejemplo, seleccionando el botón de ejecución con el nombre del perfil del proyecto o IIS Express).

4. Haz clic en el botón Iniciar (o presiona la tecla F5 para depurar, o Ctrl + F5 para ejecutar sin depurar).

5. Visual Studio compilará el proyecto y abrirá automáticamente tu navegador predeterminado mostrando la interfaz de Swagger configurada en /swagger.
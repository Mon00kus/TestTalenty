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

## Jsons usados
{
  "records": [
    { "id": 4, "site": "Madrid", "month": "2026-05", "energyKwh": 25000, "co2Kg": 5900 }
  ],
  "operationalContexts": [
    {
      "site": "Madrid",
      "month": "2026-05",
      "reason": "Ampliación de fábrica y nueva línea de producción en curso",
      "expectedEnergyMultiplier": 2.5
    }
  ],
  "enableAiAssistance": true
}

__________________________________________

{
  "records": [
    { "id": 1, "site": "Madrid", "month": "2026-01", "energyKwh": 12000, "co2Kg": 2800 },
    { "id": 2, "site": "Madrid", "month": "2026-02", "energyKwh": 12500, "co2Kg": 2900 },
    { "id": 3, "site": "Madrid", "month": "2026-03", "energyKwh": 12800, "co2Kg": 2950 },
    { "id": 4, "site": "Madrid", "month": "2026-04", "energyKwh": 79000, "co2Kg": 18200 },
    { "id": 7, "site": "Barcelona", "month": "2026-03", "energyKwh": -900, "co2Kg": -210 },
    { "id": 8, "site": "Barcelona", "month": "2026-04", "energyKwh": 8900, "co2Kg": 8500 }
  ],
  "operationalContexts": [],
  "enableAiAssistance": false
}

__________________________________________

Aquí tienes un ejemplo de JSON exacto que cumplirá con estas condiciones y forzará la llamada simulada al LLM:

JSON
{
  "records": [
    {
      "id": 1,
      "site": "Barcelona",
      "month": "2026-01",
      "energyKwh": 10000,
      "co2Kg": 2500
    },
    {
      "id": 2,
      "site": "Barcelona",
      "month": "2026-02",
      "energyKwh": 11000,
      "co2Kg": 2700
    },
    {
      "id": 3,
      "site": "Barcelona",
      "month": "2026-03",
      "energyKwh": 50000,
      "co2Kg": 12000
    }
  ],
  "operationalContexts": [],
  "enableAiAssistance": true
}
__________________________________________

{
  "records": [
    {
      "id": 1,
      "site": "Madrid",
      "month": "2026-01",
      "energyKwh": 12000,
      "co2Kg": 2800
    },
    {
      "id": 4,
      "site": "Madrid",
      "month": "2026-05",
      "energyKwh": 75000,
      "co2Kg": 19000
    }
  ],
  "operationalContexts": [
    {
      "site": "Madrid",
      "month": "2026-05",
      "reason": "Ampliación de fábrica y nueva línea de producción",
      "expectedEnergyMultiplier": 3.0
    }
  ],
  "enableAiAssistance": false
}

__________________________________________

{
  "records": [
    { "id": 1, "site": "Madrid", "month": "2026-01", "energyKwh": 12000, "co2Kg": 2800 },
    { "id": 2, "site": "Madrid", "month": "2026-02", "energyKwh": 12500, "co2Kg": 2900 }
  ],
  "operationalContexts": [],
  "enableAiAssistance": false
}

__________________________________________

{
  "records": [
    { "id": 1, "site": "Madrid", "month": "2026-01", "energyKwh": 12000, "co2Kg": 2800 },
    { "id": 2, "site": "Madrid", "month": "2026-02", "energyKwh": 12500, "co2Kg": 2900 },
    { "id": 3, "site": "Madrid", "month": "2026-03", "energyKwh": 12800, "co2Kg": 2950 },
    { "id": 4, "site": "Madrid", "month": "2026-04", "energyKwh": 79000, "co2Kg": 18200 },
    { "id": 7, "site": "Barcelona", "month": "2026-03", "energyKwh": -900, "co2Kg": -210 },
    { "id": 8, "site": "Barcelona", "month": "2026-04", "energyKwh": 8900, "co2Kg": 8500 }
  ],
  "operationalContexts": [],
  "enableAiAssistance": false
}
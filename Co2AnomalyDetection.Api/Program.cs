var builder = WebApplication.CreateBuilder(args);

// Configurar System.Text.Json para hacer la API más tolerante (permite comas finales y case-insensitivity)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.AllowTrailingCommas = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// Registrar el servicio de detección de anomalías en el contenedor de dependencias (DI)
builder.Services.AddSingleton<ICo2AnomalyDetectorService, Co2AnomalyDetectorService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint principal que recibe la lista de registros y aplica la heurística
app.MapPost("/api/emissions/analyze", (List<EmissionRecord> records, ICo2AnomalyDetectorService detectorService) =>
{
    if (records == null || records.Count == 0)
    {
        return Results.BadRequest(new { message = "El listado de registros está vacío o es inválido." });
    }

    var results = detectorService.AnalyzeBatch(records);
    return Results.Ok(results);
})
.WithName("AnalyzeEmissions")
.WithOpenApi();

app.Run();

// ==========================================
// 1. MODELOS DE DATOS (Records en .NET 8)
// ==========================================

public record EmissionRecord(
    int Id,
    string Site,
    string Month,
    double EnergyKwh,
    double Co2Kg
);

public record AnomalyResult(
    int Id,
    bool RequiresReview,
    string Reason,
    string Severity
);

// ==========================================
// 2. CONTRATO DEL SERVICIO
// ==========================================

public interface ICo2AnomalyDetectorService
{
    List<AnomalyResult> AnalyzeBatch(List<EmissionRecord> records);
}

// ==========================================
// 3. IMPLEMENTACIÓN DE LA LÓGICA HEURÍSTICA
// ==========================================

public class Co2AnomalyDetectorService : ICo2AnomalyDetectorService
{
    public List<AnomalyResult> AnalyzeBatch(List<EmissionRecord> records)
    {
        var results = new List<AnomalyResult>();

        // Agrupamos por sede para poder analizar el comportamiento histórico de cada una
        var recordsBySite = records.GroupBy(r => r.Site).ToList();

        foreach (var siteGroup in recordsBySite)
        {
            var siteRecords = siteGroup.OrderBy(r => r.Month).ToList();

            // Calculamos estadísticas básicas de la sede (si hay suficientes datos)
            double avgEnergy = siteRecords.Where(r => r.EnergyKwh >= 0).Select(r => r.EnergyKwh).DefaultIfEmpty(0).Average();

            foreach (var record in siteRecords)
            {
                // REGLA 1: Valores Imposibles / Inválidos (ej. Negativos)
                if (record.EnergyKwh < 0 || record.Co2Kg < 0)
                {
                    results.Add(new AnomalyResult(
                        record.Id,
                        RequiresReview: true,
                        Reason: "Valores físicos imposibles o negativos detectados (Energía o CO₂ < 0).",
                        Severity: "High"
                    ));
                    continue;
                }

                // REGLA 2: Cambios Anómalos en el Consumo (Spikes temporales)
                // Si el consumo excede más del doble (> 200%) de la media histórica de la sede
                if (avgEnergy > 0 && record.EnergyKwh > (avgEnergy * 2.5))
                {
                    results.Add(new AnomalyResult(
                        record.Id,
                        RequiresReview: true,
                        Reason: $"El consumo energético ({record.EnergyKwh} kWh) excede significativamente el comportamiento histórico promedio de la sede ({avgEnergy:F0} kWh).",
                        Severity: "High"
                    ));
                    continue;
                }

                // REGLA 3: Relaciones Sospechosas (Desproporción entre Energía y CO₂)
                // Evaluamos la tasa de emisión (CO2 / kWh). Una tasa extremadamente alta o fuera de rango indica anomalía.
                if (record.EnergyKwh > 0)
                {
                    double emissionRatio = record.Co2Kg / record.EnergyKwh;

                    // Umbral genérico de alerta: si emite más de 0.8 kg de CO2 por cada kWh consumido (ej. valor atípico desproporcionado)
                    // O si comparamos contra el ratio medio de los demás registros válidos de la misma sede.
                    double typicalRatio = 0.35; // Ratio base estimado sostenible/industrial estándar aproximado

                    if (emissionRatio > (typicalRatio * 3.0)) // Si la proporción se dispara triplicando lo normal
                    {
                        results.Add(new AnomalyResult(
                            record.Id,
                            RequiresReview: true,
                            Reason: $"Relación sospechosa: La emisión de CO₂ ({record.Co2Kg} kg) es desproporcionada respecto al consumo energético ({record.EnergyKwh} kWh).",
                            Severity: "Medium"
                        ));
                        continue;
                    }
                }

                // Si pasa todas las validaciones, se marca como normal
                results.Add(new AnomalyResult(
                    record.Id,
                    RequiresReview: false,
                    Reason: "Registro normal y coherente.",
                    Severity: "None"
                ));
            }
        }

        return results;
    }
}
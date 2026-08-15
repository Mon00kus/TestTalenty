using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Muestra el entorno actual en la consola al iniciar
//Console.WriteLine($"---> Entorno actual: {builder.Environment.EnvironmentName}");

// Configuración tolerante de JSON
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.AllowTrailingCommas = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// Registrar servicios en el contenedor de dependencias
builder.Services.AddSingleton<IEmissionAnalysisEngine, EmissionAnalysisEngine>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Emission Analysis API V1");
        //c.RoutePrefix = string.Empty; // Esto hace que Swagger cargue directamente en https://localhost:xxxx/
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Endpoint avanzado de análisis que soporta el Escenario A (contexto operativo) y Escenario B (opción de IA)
app.MapPost("/api/emissions/analyze-advanced", (AnalysisRequest request, IEmissionAnalysisEngine analysisEngine) =>
{
    if (request.Records == null || request.Records.Count == 0)
    {
        return Results.BadRequest(new { message = "No se proporcionaron registros para analizar." });
    }

    var results = analysisEngine.EvaluateBatch(request.Records, request.OperationalContexts, request.EnableAiAssistance);
    return Results.Ok(results);
})
.WithName("AnalyzeEmissionsAdvanced")
.WithOpenApi();

app.Run();

// ==========================================
// 1. MODELOS DE DATOS Y CONTRATOS
// ==========================================

public record EmissionRecord(
    int Id,
    string Site,
    string Month,
    double EnergyKwh,
    double Co2Kg
);

// Escenario A: Contexto de negocio / operativo por sede y mes
public record OperationalContext(
    string Site,
    string Month,
    string Reason, // Ej. "Ampliación de fábrica y nueva línea de producción"
    double ExpectedEnergyMultiplier // Ej. 2.0 (permite doblar el umbral esperado)
);

public record AnalysisRequest(
    List<EmissionRecord> Records,
    List<OperationalContext>? OperationalContexts,
    bool EnableAiAssistance // Escenario B: Activar validación por LLM en casos dudosos
);

public record AnomalyResult(
    int Id,
    bool RequiresReview,
    string Reason,
    string Severity,
    string EvaluatedBy // "Code-Heuristics" o "LLM-Assisted"
);

// ==========================================
// 2. INTERFAZ DEL MOTOR DE ANÁLISIS
// ==========================================

public interface IEmissionAnalysisEngine
{
    List<AnomalyResult> EvaluateBatch(
        List<EmissionRecord> records,
        List<OperationalContext>? contexts,
        bool useAiAssistance);
}

// ==========================================
// 3. IMPLEMENTACIÓN DE LA HEURÍSTICA + ESCENARIOS A y B
// ==========================================

public class EmissionAnalysisEngine : IEmissionAnalysisEngine
{
    public List<AnomalyResult> EvaluateBatch(
        List<EmissionRecord> records,
        List<OperationalContext>? contexts,
        bool useAiAssistance)
    {
        var results = new List<AnomalyResult>();
        contexts ??= new List<OperationalContext>();

        var recordsBySite = records.GroupBy(r => r.Site).ToList();

        foreach (var siteGroup in recordsBySite)
        {
            var siteRecords = siteGroup.OrderBy(r => r.Month).ToList();

            // EXCLUSIÓN CLAVE: Calculamos la media histórica excluyendo los valores negativos
            // y opcionalmente excluyendo picos extremos obvios para que la media no se distorsione.
            double avgEnergy = siteRecords
                .Where(r => r.EnergyKwh >= 0)
                .Select(r => r.EnergyKwh)
                .DefaultIfEmpty(0)
                .Average();

            foreach (var record in siteRecords)
            {
                // Bandera para llevar el estado de revisión de este registro específico
                bool requiresReview = false;
                string severity = "None";
                string reason = "Registro normal y coherente.";
                string evaluatedBy = "Code-Heuristics";

                // ----------------------------------------------------
                // 1. REGLA DE VALORES IMPOSIBLES (Negativos)
                // ----------------------------------------------------
                if (record.EnergyKwh < 0 || record.Co2Kg < 0)
                {
                    results.Add(new AnomalyResult(
                        record.Id,
                        RequiresReview: true,
                        Reason: "Valores físicos imposibles detectados (Energía o CO₂ con valores negativos).",
                        Severity: "High",
                        EvaluatedBy: "Code-Heuristics"
                    ));
                    continue; // Este es crítico, se detiene aquí porque no tiene sentido evaluar física nula
                }

                // ----------------------------------------------------
                // 2. VERIFICAR ESCENARIO A (Contexto Operativo) Y PICOS
                // ----------------------------------------------------
                var activeContext = contexts.FirstOrDefault(c =>
                    c.Site.Equals(record.Site, StringComparison.OrdinalIgnoreCase) &&
                    c.Month.Equals(record.Month, StringComparison.OrdinalIgnoreCase));

                double dynamicThresholdMultiplier = activeContext != null ? activeContext.ExpectedEnergyMultiplier : 2.5;

                if (avgEnergy > 0 && record.EnergyKwh > (avgEnergy * dynamicThresholdMultiplier))
                {
                    if (activeContext != null)
                    {
                        // Escenario A resuelto: Justificado por negocio
                        results.Add(new AnomalyResult(
                            record.Id,
                            RequiresReview: false,
                            Reason: $"Pico de consumo validado por contexto operativo: {activeContext.Reason}",
                            Severity: "Low",
                            EvaluatedBy: "Code-Heuristics-Contextual"
                        ));
                        continue;
                    }
                    else
                    {
                        // Sin contexto, evaluamos si usamos IA (Escenario B) o mandamos alerta Alta
                        if (useAiAssistance)
                        {
                            var aiEvaluation = EvaluateWithSimulatedLlmasGuardrailed(record, avgEnergy);
                            results.Add(aiEvaluation);
                            continue;
                        }
                        else
                        {
                            results.Add(new AnomalyResult(
                                record.Id,
                                RequiresReview: true,
                                Reason: $"Consumo anómalo ({record.EnergyKwh} kWh) excede la media histórica de la sede ({avgEnergy:F0} kWh) sin justificación operativa.",
                                Severity: "High",
                                EvaluatedBy: "Code-Heuristics"
                            ));
                            continue;
                        }
                    }
                }

                // ----------------------------------------------------
                // 3. REGLA DE RELACIONES SOSPECHOSAS (Desproporción CO2 / kWh)
                // ----------------------------------------------------
                if (record.EnergyKwh > 0)
                {
                    double emissionRatio = record.Co2Kg / record.EnergyKwh;
                    double standardIndustrialRatio = 0.35; // Ratio industrial estándar de referencia

                    if (emissionRatio > (standardIndustrialRatio * 3.0))
                    {
                        results.Add(new AnomalyResult(
                            record.Id,
                            RequiresReview: true,
                            Reason: $"Relación sospechosa: Emisión de CO₂ desproporcionada ({record.Co2Kg} kg) para los kWh consumidos.",
                            Severity: "Medium",
                            EvaluatedBy: "Code-Heuristics"
                        ));
                        continue;
                    }
                }

                // ----------------------------------------------------
                // 4. SI PASA TODO, EL REGISTRO ES NORMAL
                // ----------------------------------------------------
                results.Add(new AnomalyResult(
                    record.Id,
                    RequiresReview: false,
                    Reason: "Registro normal y coherente.",
                    Severity: "None",
                    EvaluatedBy: "Code-Heuristics"
                ));
            }
        }

        return results;
    }

    private AnomalyResult EvaluateWithSimulatedLlmasGuardrailed(EmissionRecord record, double avgEnergy)
    {
        return new AnomalyResult(
            record.Id,
            RequiresReview: true,
            Reason: $"[Revisión asistida por LLM] El registro presenta un desvío de consumo, pero requiere validación formal humana antes de integrarse al reporting ESG oficial.",
            Severity: "Medium",
            EvaluatedBy: "LLM-Assisted-With-Guardrails"
        );
    }
}
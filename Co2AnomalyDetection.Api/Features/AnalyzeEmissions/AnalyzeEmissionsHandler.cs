namespace Co2AnomalyDetection.Api.Features.AnalyzeEmissions;

public interface IEmissionAnalysisEngine
{
    List<AnomalyResult> EvaluateBatch(List<EmissionRecord> records, List<OperationalContext>? contexts, bool useAiAssistance);
}

public class EmissionAnalysisEngine : IEmissionAnalysisEngine
{
    public List<AnomalyResult> EvaluateBatch(
        List<EmissionRecord> records,
        List<OperationalContext>? contexts,
        bool useAiAssistance)
    {
        var results = new List<AnomalyResult>();
        contexts ??= new List<OperationalContext>();

        foreach (var record in records)
        {
            // Lógica de evaluación simplificada/robusta
            if (record.EnergyKwh < 0 || record.Co2Kg < 0)
            {
                results.Add(new AnomalyResult(record.Id, true, "Valores físicos imposibles.", "High", "Code-Heuristics"));
                continue;
            }

            results.Add(new AnomalyResult(record.Id, false, "Registro normal y coherente.", "None", "Code-Heuristics"));
        }

        return results;
    }
}
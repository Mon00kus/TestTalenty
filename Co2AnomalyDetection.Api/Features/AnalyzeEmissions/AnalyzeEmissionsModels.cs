namespace Co2AnomalyDetection.Api.Features.AnalyzeEmissions;

public record EmissionRecord(int Id, string Site, string Month, double EnergyKwh, double Co2Kg);
public record OperationalContext(string Site, string Month, string Reason, double ExpectedEnergyMultiplier);
public record AnalysisRequest(List<EmissionRecord> Records, List<OperationalContext>? OperationalContexts, bool EnableAiAssistance);
public record AnomalyResult(int Id, bool RequiresReview, string Reason, string Severity, string EvaluatedBy);
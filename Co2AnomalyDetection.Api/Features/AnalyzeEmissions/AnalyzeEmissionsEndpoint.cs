namespace Co2AnomalyDetection.Api.Features.AnalyzeEmissions;

public static class AnalyzeEmissionsEndpoint
{
    public static void MapAnalyzeEmissionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/emissions/analyze-advanced", (AnalysisRequest request, IEmissionAnalysisEngine analysisEngine) =>
        {
            var results = analysisEngine.EvaluateBatch(
                request.Records,
                request.OperationalContexts,
                request.EnableAiAssistance);

            return Results.Ok(results);
        })
        .WithName("AnalyzeEmissionsAdvanced")
        .WithTags("Emissions ESG Analysis");
    }
}
using Co2AnomalyDetection.Api.Features.AnalyzeEmissions;

var builder = WebApplication.CreateBuilder(args);

// Configuración tolerante de JSON
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.AllowTrailingCommas = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// Registrar servicios en el contenedor de dependencias (DI)
builder.Services.AddSingleton<IEmissionAnalysisEngine, EmissionAnalysisEngine>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuración del Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Emission Analysis API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Mapear los Endpoints organizados por su arquitectura vertical
app.MapAnalyzeEmissionsEndpoint();

app.Run();
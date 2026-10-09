using Carter;
using ConfyAI.API.Behavior;
using ConfyAI.API.Configurations;
using ConfyAI.API.Services.AnalyzeText;
using FluentValidation;
using Microsoft.Extensions.Options;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

//add configuration for Ollama
builder.Services.Configure<OllamaConfiguration>(builder.Configuration.GetSection("Ollama"));
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IOptions<OllamaConfiguration>>().Value;
    var endpoint = config.Endpoint;
    var modelName = config.ModelName;

    return new OllamaApiClient(new Uri(endpoint), modelName);
});
builder.Services.AddSingleton<IClauseAnalyzer, ClauseAnalyzer>();

//register fluent validation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
//regiter global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// register mediatr
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins("https://localhost:7151", "http://localhost:5248")
    .AllowAnyHeader()
    .AllowAnyMethod()));
// register carter
builder.Services.AddCarter();

var app = builder.Build();
app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapCarter();
app.Run();

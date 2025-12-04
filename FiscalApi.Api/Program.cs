using FiscalApi.Application.Factories;
using FiscalApi.Application.Services;
using FiscalApi.Domain.Interfaces;
using FiscalApi.Infrastructure.Parsers;
using FiscalApi.Infrastructure.Repositories;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Configuração MongoDB
builder.Services.AddSingleton<IMongoDatabase>(sp => {
    var client = new MongoClient(builder.Configuration.GetConnectionString("MongoDb"));
    return client.GetDatabase("FiscalDb");
});

builder.Services.AddScoped<IFiscalDocumentRepository, FiscalDocumentRepository>();
builder.Services.AddScoped<IFiscalDocumentService, FiscalDocumentService>();


builder.Services.AddScoped<IDocumentParserStrategy, NfeParserStrategy>();
builder.Services.AddScoped<IDocumentParserStrategy, NfseRecifeParserStrategy>();
builder.Services.AddScoped<IParserFactory, ParserFactory>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.Run();

//Para utilizar em testes de integração
public partial class Program { }
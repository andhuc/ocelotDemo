using Contract.Service.Models;
using Contract.Service.Models.Implements;
using Contract.Service.Services;
using Contract.Service.Services.Implements;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<sampleContext>();

builder.Services.AddScoped<ISignService, SignService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<ISignatureService, SignatureService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

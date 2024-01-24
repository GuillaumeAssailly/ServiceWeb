using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HistoryService.Data;
using HistoryService.Entities;
using HistoryService.Controllers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HistoryServiceContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("HistoryServiceContext") ?? throw new InvalidOperationException("Connection string 'UserServiceContext' not found.")));

// Add services to the container.
builder.Services.AddScoped<HistoryEntryService>();

builder.Services.AddControllers();
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

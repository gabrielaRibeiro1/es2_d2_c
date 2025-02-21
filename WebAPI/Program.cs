using ESOF.WebApp.DBLayer.Context;
using Helpers.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

app.UseHttpsRedirection();

app.MapGet("/ingredients", handler: () =>
    {
        var db = new ApplicationDbContext();
        return db.Ingredients.Select(i => new IngredientViewModel
        {
            Id = i.IngredientId,
            Name = i.Name,
            Stock = i.Stock,
            Proteins = i.Proteins,
            Fats = i.Fats,
            Carbohydrates = i.Carbohydrates
        }).ToListAsync();
    })
    .WithName("GetIngredients")
    .WithOpenApi();

app.Run();
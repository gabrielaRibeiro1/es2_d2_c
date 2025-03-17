using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 Register ApplicationDbContext with Dependency Injection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Update route to inject `ApplicationDbContext`
app.MapPost("/create_account", async (User user, [FromServices] ApplicationDbContext db) =>
    {
        var newUser = new User
        {
            username = user.username,
            password = user.password, 
            fk_role_id = user.fk_role_id
        };

        db.Users.Add(newUser);
        await db.SaveChangesAsync();

        return Results.Created($"/users/{newUser.user_id}", newUser);
    })
    .WithName("CreateUser")
    .WithOpenApi();

app.UseHttpsRedirection();
app.Run();
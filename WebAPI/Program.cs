using System.Security.Claims;
using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using ESOF.WebApp.DBLayer.Helpers;
using ESOF.WebApp.WebAPI.Models;
using Helpers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Register ApplicationDbContext with the DI container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Exemplo de configuração
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("sua-chave-secreta"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();



app.MapPost("/create_account", async (string username, string password, int fk_role_id, ApplicationDbContext db) =>
{
    // Step 1: Check if the password is provided
    if (string.IsNullOrEmpty(password))
    {
        return Results.BadRequest("Password cannot be empty.");
    }

    // Step 2: Generate the password hash and salt
    PasswordHelper.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

    // Step 3: Ensure passwordHash and passwordSalt are not null
    if (passwordHash == null || passwordSalt == null)
    {
        return Results.BadRequest("Error generating password hash and salt.");
    }

    // Step 4: Create the new user without storing the raw password, but with the hashed password and salt
    var newUser = new User
    {
        username = username,  // Use the username directly
        passwordHash = passwordHash, // Store hashed password (byte[])
        passwordSalt = passwordSalt, // Store salt (byte[])
        fk_role_id = fk_role_id  // Use fk_role_id directly
    };

    // Step 5: Save the new user to the database
    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{newUser.user_id}", newUser);
});



app.MapPost("/login", async ([FromBody] LoginModel login, ApplicationDbContext db) =>
{
    // Verificar se login está a ser recebido corretamente
  

    var user = await db.Users.FirstOrDefaultAsync(u => u.username == login.Username);
    if (user == null)
        return Results.Unauthorized();
    Console.WriteLine($"Username: {login.Username}");
    Console.WriteLine($"Password: {login.Password}");

    if (!PasswordHelper.VerifyPassword(login.Password, user.passwordHash, user.passwordSalt))
        return Results.Unauthorized();
    
    
    return Results.Ok(new
    {
        user.user_id,
        user.username,
        user.fk_role_id
    });
});



// endpoint para retornar dados do user loggado
app.MapGet("/me", (HttpContext httpContext) =>
    {
        var user = httpContext.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            // Extraia os dados do usuário dos Claims
            var username = user.Identity.Name;
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            return Results.Ok(new { Username = username, RoleId = roleClaim });
        }
        return Results.Unauthorized();
    })
    .RequireAuthorization();

app.UseHttpsRedirection();

app.Run();
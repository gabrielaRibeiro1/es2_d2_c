using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using ESOF.WebApp.DBLayer.Helpers;
using ESOF.WebApp.WebAPI.Models;
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

// Configure JWT settings
var jwtKey     = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("Falta configuração Jwt:Key em appsettings.json!");
var jwtIssuer  = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var keyBytes  = Encoding.UTF8.GetBytes(jwtKey);

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer   = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        
        NameClaimType    = JwtRegisteredClaimNames.Sub,
        RoleClaimType    = "role",
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

// Login endpoint - now generates JWT
app.MapPost("/login", async ([FromBody] LoginModel login, ApplicationDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == login.Username);
    if (user == null || !PasswordHelper.VerifyPassword(login.Password, user.passwordHash, user.passwordSalt))
        return Results.Unauthorized();

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.username),
        new Claim(JwtRegisteredClaimNames.Name, user.username),
        new Claim("role", user.fk_role_id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: creds
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new LoginResponse { Token = tokenString, Username = user.username, Role = user.fk_role_id });
});

app.MapPost("/create_account2", async ([FromBody] CreateUserDto dto, ApplicationDbContext db) =>
{
    if (string.IsNullOrEmpty(dto.Password))
        return Results.BadRequest("Password cannot be empty.");

    PasswordHelper.CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

    if (passwordHash == null || passwordSalt == null)
        return Results.BadRequest("Error generating password hash and salt.");

    var newUser = new User
    {
        username     = dto.Username,
        passwordHash = passwordHash,
        passwordSalt = passwordSalt,
        fk_role_id   = 3
    };

    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{newUser.user_id}", newUser);
});

app.Run();

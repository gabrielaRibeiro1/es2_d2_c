using ESOF.Factories;
using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using ESOF.WebApp.DBLayer.Helpers;
using Helpers.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Register ApplicationDbContext with the DI container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

app.MapPost("/login", async (string username, string password, ApplicationDbContext db) =>
{
    // Find user by username
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
    if (user == null)
        return Results.Unauthorized();

    // Verify the password
    if (!PasswordHelper.VerifyPassword(password, user.passwordHash, user.passwordSalt))
        return Results.Unauthorized();

    // Return user info upon successful login
    return Results.Ok(new
    {
        user.user_id,
        user.username,
        user.fk_role_id
    });
});

app.MapDelete("/delete_user/{username}", async (string username, ApplicationDbContext db) =>
{
    // Find user by username
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // Remove user from database
    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.Ok("User deleted successfully.");
});
app.MapPut("/update_user/{username}", async (string username, string? newPassword, int? newRoleId, ApplicationDbContext db) =>
{
    // Find user by username
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // Update password if provided
    if (!string.IsNullOrEmpty(newPassword))
    {
        PasswordHelper.CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
        user.passwordHash = passwordHash;
        user.passwordSalt = passwordSalt;
    }

    // Update role if provided
    if (newRoleId.HasValue)
    {
        user.fk_role_id = newRoleId.Value;
    }

    await db.SaveChangesAsync();
    return Results.Ok("User updated successfully.");
});


app.MapPost("/logout", () =>
{
   
    return Results.Ok("User logged out successfully.");
});


app.MapGet("/get_user/{username}", async (string username, ApplicationDbContext db) =>
{
    // Find user by username
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // Return user info
    return Results.Ok(new
    {
        user.user_id,
        user.username,
        user.fk_role_id
    });
});



app.MapGet("/get_all_users", async (ApplicationDbContext db) =>
{
    var users = await db.Users
        .Join(db.Roles, 
            u => u.fk_role_id,   // fk_role_id da tabela Users
            r => r.role_id,           // id da tabela Roles
            (u, r) => new        // Resultado do Join
            {
                u.user_id,
                u.username,
                u.fk_role_id,
                RoleName = r.role  
            })
        .ToListAsync();

    return Results.Ok(users);
});



app.MapDelete("/delete_user_by_id/{id:int}", async (int id, ApplicationDbContext db) =>
{
    Console.WriteLine($"Muhhhh {id}");
    // Encontrar usuário pelo ID
    var user = await db.Users.FindAsync(id);
    if (user == null)
    {
        Console.WriteLine("user not found");
        return Results.NotFound("User not found.");
    }
    Console.WriteLine("found");

    // Remover usuário do banco de dados
    db.Users.Remove(user);
    Console.WriteLine("removed");
    await db.SaveChangesAsync();

    Console.WriteLine($"done {id}");
    return Results.Ok($"User with ID {id} deleted successfully.");
});

app.MapPut("/update_user/{id:int}", async (int id, string? newPassword, int? newRoleId, ApplicationDbContext db) =>
{
    // Localiza o usuário pelo ID
    var user = await db.Users.FirstOrDefaultAsync(u => u.user_id == id);
    if (user == null)
    {
        return Results.NotFound("Usuário não encontrado.");
    }

    // Atualiza a senha, se fornecida
    if (!string.IsNullOrEmpty(newPassword))
    {
        PasswordHelper.CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
        user.passwordHash = passwordHash;
        user.passwordSalt = passwordSalt;
    }

    // Atualiza a role, se fornecida
    if (newRoleId.HasValue)
    {
        user.fk_role_id = newRoleId.Value;
    }

    await db.SaveChangesAsync();
    return Results.Ok("Usuário atualizado com sucesso.");
});

app.MapPost("/create_account_factory", async (string username, string password, int fk_role_id, ApplicationDbContext db) =>
{
    try
    {
        
        var newUser = UserFactory.CreateUser(username, password, fk_role_id);
        
        
        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        
        return Results.Created($"/users/{newUser.user_id}", newUser);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});
app.UseHttpsRedirection();

app.Run();
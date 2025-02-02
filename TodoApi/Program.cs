using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;



var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// הוספת CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});


// הוספת הגדרות JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key")) // החלף במפתח סודי שלך
    };
});
app.UseCors("AllowAllOrigins");

app.MapGet("/tasks", [Authorize] async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

app.UseAuthentication(); // הוספת Authentication pipeline

// הוספת endpoint להרשמה
app.MapPost("/register", async (ToDoDbContext db, User user) =>
{
    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); // hashing password
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
});

// הוספת endpoint להתחברות
app.MapPost("/login", async (ToDoDbContext db, User loginUser) =>
{
    var user = await db.Users.SingleOrDefaultAsync(u => u.Username == loginUser.Username);
    if (user == null || !BCrypt.Net.BCrypt.Verify(loginUser.PasswordHash, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var token = GenerateJwtToken(user); // פונקציה ליצירת JWT
    return Results.Ok(new { Token = token });
});

string GenerateJwtToken(User user)
{
    var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Username) };
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key")); // החלף במפתח סודי שלך
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: null,
        audience: null,
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

// הוספת Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הוספת ה-DbContext עם ה-Connection String
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
                     new MySqlServerVersion(new Version(8, 0, 21))));


// הפעלת CORS
app.UseCors("AllowAllOrigins");

// הפעלת Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // מאפשר גישה ל-Swagger ב-root URL
    });
}

// הוספת משימה לדאטה בייס בעת הרצת התוכנית
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();

    // הוספת משימה לדוגמה
    var newItem = new Item
    {
        Name = "Example Task",
        IsComplete = false
    };

    db.Items.Add(newItem);
    await db.SaveChangesAsync(); // שמירת השינויים
}

app.MapGet("/tasks", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

app.MapPost("/tasks", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{item.Id}", item);
});

app.MapPut("/tasks/{id}", async (int id, ToDoDbContext db, Item updatedItem) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = updatedItem.Name;
    item.IsComplete = updatedItem.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/tasks/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

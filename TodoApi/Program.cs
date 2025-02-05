using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// ... הקוד הקיים שלך ...

var builder = WebApplication.CreateBuilder(args);

// הוספת שירותי CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// הוספת שירותי JWT
var key = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// הוספת DbContext
builder.Services.AddDbContext<ToDoDbContext>(opt =>
    opt.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
                 new MySqlServerVersion(new Version(8, 0, 41))));

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthentication(); // הוספת Middleware לאימות
app.UseAuthorization(); // הוספת Middleware להרשאות

// ... שאר הקוד הקיים שלך ...

// הוספת נתיב לרישום
app.MapPost("/register", async (UserDto userDto, ToDoDbContext db) =>
{
    var user = new User { Username = userDto.Username, PasswordHash = userDto.Password }; // יש להוסיף Hashing לסיסמה
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// הוספת נתיב להזדהות
app.MapPost("/login", async (UserDto userDto, ToDoDbContext db) =>
{
    var user = await db.Users.SingleOrDefaultAsync(x => x.Username == userDto.Username && x.PasswordHash == userDto.Password); // יש להוסיף Hashing לסיסמה
    if (user == null) return Results.Unauthorized();

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenKey = Encoding.UTF8.GetBytes(key);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, user.Username)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new { Token = tokenHandler.WriteToken(token) });
});

app.Run();

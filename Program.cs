using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlanNGo_Backend.Service;
using System.Security.Claims;
using System.Text;
using WebAppApi13.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AuthService>();
// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSecret = builder.Configuration["JwtConfig:Secret"];
var jwtIssuer = builder.Configuration["JwtConfig:Issuer"];
var jwtAudience = builder.Configuration["JwtConfig:Audience"];

if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new Exception("JWT Secret is missing in appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret)
        )
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();

var excludedPaths = new[] { "/api/Auth", "/swagger"};

app.UseWhen(context =>
    context.Request.Path.StartsWithSegments("/api") &&
   !excludedPaths.Any(p => context.Request.Path.StartsWithSegments(p)),
appBuilder =>
{
    appBuilder.Use(async (context, next) =>
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

        var path = context.Request.Path;
        

        if (path.StartsWithSegments("/api/Admin") && role != "admin")
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Admins only");
            return;
        }

        
        context.Items["UserId"] = userId;
        await next();
    });
});

app.UseAuthorization();

app.UseAuthorization();
app.MapControllers();
app.Run();

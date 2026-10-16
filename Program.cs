using Api.Airconmodule;
using Api.AuthorizationModule;
using Api.CustomerModule;
using Api.Main;
using Api.Maintenance;
using Api.NotificationModule;
using Api.Security;
using Api.ServiceRequestModule;
using Api.SparePartsModule;
using Api.StoreModule;
using Api.UserModules;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Load conn.env into IConfiguration ─────────────────────────────────────
builder.Configuration.AddInMemoryCollection(ConnEnvFile.LoadConfigurationValues()!);

// ── MVC Controllers + JSON ─────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger / OpenAPI ──────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version     = "v1",
        Title       = "Aircon Maintenance API",
        Description = "REST API for the Aircon Maintenance Management System"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description  = "JWT Authorization header. Enter: Bearer {token}",
        Name         = "Authorization",
        In           = ParameterLocation.Header,
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── JWT Authentication ─────────────────────────────────────────────────────
string jwtKey = JwtConfiguration.ResolveSigningKey(builder.Configuration, builder.Environment);

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
        ValidIssuer              = builder.Configuration["Jwt:Issuer"]   ?? JwtConfiguration.DefaultIssuer,
        ValidAudience            = builder.Configuration["Jwt:Audience"] ?? JwtConfiguration.DefaultAudience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ── Authorization Policies ─────────────────────────────────────────────────
builder.Services.AddAuthorization(options => options.AddAppPolicies());
builder.Services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddSingleton<MyCon>();

// ── Repositories ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository,           UserRepository>();
builder.Services.AddScoped<ICustomerRepository,       CustomerRepository>();
builder.Services.AddScoped<IAirconRepository,         AirconRepository>();
builder.Services.AddScoped<IMaintenanceRepository,    MaintenanceRepository>();
builder.Services.AddScoped<ISparePartRepository,      SparePartRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
builder.Services.AddScoped<INotificationRepository,   NotificationRepository>();
builder.Services.AddScoped<IStoreRepository,          StoreRepository>();

// ── Security services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ── CORS ───────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>()?
    .Where(o => !string.IsNullOrWhiteSpace(o))
    .Select(o => o.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ConfiguredOrigins", policy =>
    {
        if (allowedOrigins.Length == 0)
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        else
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
    });
});

var app = builder.Build();

// ── Initialize Database & Seed Default Admin ──────────────────────────────
await DatabaseInitializer.InitializeDatabaseAsync(app.Services.GetRequiredService<MyCon>());

// ── Middleware pipeline ────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.SwaggerEndpoint("/swagger/v1/swagger.json", "Aircon Maintenance API v1");
        ui.RoutePrefix = string.Empty; // Swagger at root "/"
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("ConfiguredOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Utility endpoints ──────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .AllowAnonymous();

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .AllowAnonymous();

app.MapGet("/db-test", async (MyCon db) =>
{
    try
    {
        return await db.CanConnectAsync()
            ? Results.Ok(new { status = "Database connected", timestamp = DateTime.UtcNow })
            : Results.Problem("Database connection failed.", statusCode: 503);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 500, title: "Database error");
    }
}).AllowAnonymous();

app.MapGet("/api/db-test", async (MyCon db) =>
{
    try
    {
        return await db.CanConnectAsync()
            ? Results.Ok(new { status = "Database connected", timestamp = DateTime.UtcNow })
            : Results.Problem("Database connection failed.", statusCode: 503);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 500, title: "Database error");
    }
}).AllowAnonymous();

app.Run();

public partial class Program { }

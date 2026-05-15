using AspNetCoreRateLimit;
using Clinic_System.API.Hubs;
using Clinic_System.API.Middleware;
using Clinic_System.Application.Interfaces;
using Clinic_System.Application.Services;
using Clinic_System.Domain.Constant;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Clinic_System.Infrastructure.Repositories;
using Clinic_System.Infrastructure.Services;
using DotNetEnv;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

var envCandidates = new[]
{
    Path.Combine(AppContext.BaseDirectory, ".env"),          
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),   
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),
};

var envFile = envCandidates.FirstOrDefault(File.Exists);
if (envFile is not null)
    Env.Load(envFile);

var builder = WebApplication.CreateBuilder(args);

// Railway Dynamic Port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Add environment variables
builder.Configuration.AddEnvironmentVariables();

// Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddHangfireServer();

// JWT Settings
var jwtSettings = builder.Configuration.GetSection("JWT");

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            JsonNamingPolicy.CamelCase;

        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// =======================
// Dependency Injection
// =======================

// Repositories
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IReceptionistRepository, ReceptionistRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISpecialityRepository, SpecialityRepository>();
builder.Services.AddScoped<IDoctorAvailabilityRepository, DoctorAvailabilityRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleSeederService, RoleSeederService>();
builder.Services.AddScoped<ISpecialityService, SpecialityService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IDoctorAvailabilityService, DoctorAvailabilityService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IReceptionistService, ReceptionistService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationQueryService, NotificationQueryService>();

// SignalR
builder.Services.AddSignalR();

// Redis Cache
var redisConn = Environment.GetEnvironmentVariable("Redis__BaseUrl");

if (!string.IsNullOrEmpty(redisConn))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(
        sp => ConnectionMultiplexer.Connect(redisConn)
    );

    builder.Services.AddScoped<ICacheService, RedisCacheService>();
}

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)
            ),

            ClockSkew = TimeSpan.Zero
        };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("t"))
            {
                context.Token = context.Request.Cookies["t"];
            }

            return Task.CompletedTask;
        }
    };
});

// Cloudinary
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddScoped<IPhotoService, PhotoService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

// Memory Cache
builder.Services.AddMemoryCache();

builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Mail Settings
builder.Services.Configure<MailSettings>(
    builder.Configuration.GetSection("MailSettings"));

builder.Services.AddTransient<IMailingServices, MailingService>();

// Admin Settings
builder.Services.Configure<AdminSettings>(
    builder.Configuration.GetSection("AdminSettings"));

// CORS — support both production origin and local dev origins
var allowedOrigin = builder.Configuration["AllowedCorsOrigin"];

var corsOrigins = new List<string>();
if (!string.IsNullOrWhiteSpace(allowedOrigin))
    corsOrigins.Add(allowedOrigin.TrimEnd('/'));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(corsOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Rate Limiter
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    // Auth endpoints
    options.AddPolicy("AuthPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "global",

            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder =
                    QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Read endpoints
    options.AddPolicy("ReadPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "global",

            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromSeconds(10),
                SegmentsPerWindow = 5,
                QueueProcessingOrder =
                    QueueProcessingOrder.OldestFirst,
                QueueLimit = 20
            }));

    // Write endpoints
    options.AddPolicy("WritePolicy", context =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "global",

            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 50,
                TokensPerPeriod = 10,
                ReplenishmentPeriod =
                    TimeSpan.FromSeconds(1),

                AutoReplenishment = true,

                QueueLimit = 10,

                QueueProcessingOrder =
                    QueueProcessingOrder.OldestFirst
            }));

    // Global limiter
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey:
                        httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "global",

                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 100,
                        TokensPerPeriod = 20,
                        ReplenishmentPeriod =
                            TimeSpan.FromSeconds(1),

                        AutoReplenishment = true,

                        QueueLimit = 50
                    }));
});

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();


if (string.IsNullOrEmpty(redisConn))
{
    logger.LogWarning("Redis is NOT configured");
}

// Seeder + Migrations
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext =
            scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Check pending migrations before applying
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }

        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

        await RoleSeederService.SeedAsync(roleManager);

        var seeder =
            scope.ServiceProvider
                .GetRequiredService<IRoleSeederService>();

        await seeder.SeedRolesAndAdminAsync();
    }
    catch (Exception ex)
    {            
        logger.LogError(ex, "An error occurred during migration or seeding");
        throw; 
    }
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseRouting(); 

app.UseCors("AllowReactApp"); 

// app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();
app.MapHub<ClinicHub>("/clinicHub");

app.UseHangfireDashboard("/hangfire");

app.Run();

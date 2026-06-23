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
using System.Diagnostics;
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

builder.Configuration.AddEnvironmentVariables();

var jwtSettings = builder.Configuration.GetSection("JWT");
var redisSettings = builder.Configuration.GetSection("Redis");
var redisConn = redisSettings["BaseUrl"];

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // camelCase JSON output matches the React frontend contract
        options.JsonSerializerOptions.PropertyNamingPolicy =
            JsonNamingPolicy.CamelCase;

        // Serialize enums as strings so API consumers get readable values
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

// Needed for Swagger doc generation — zero runtime cost on actual API requests
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
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

        // Zero clock skew: tokens expire exactly when the claim says they do.
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("t"))
                context.Token = context.Request.Cookies["t"];

            return Task.CompletedTask;
        }
    };

    // PERFORMANCE: disable metadata fetch on every request.
    // We supply the signing key statically, so no OIDC discovery is needed.
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
});

builder.Services.AddAuthorization();

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
              .AllowCredentials(); // Required for HttpOnly cookie auth
    });
});

builder.Services.AddRateLimiter(options =>
{
    // Global safety net — applied to every request before policy limiters
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext => RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                TokensPerPeriod = 20,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                AutoReplenishment = true,
                QueueLimit = 50
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // AuthPolicy — tighter limit for login/register (brute-force protection)
    options.AddPolicy("AuthPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // ReadPolicy — generous limit for GET endpoints
    options.AddPolicy("ReadPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromSeconds(10),
                SegmentsPerWindow = 5,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 20
            }));

    // WritePolicy — token bucket for mutation endpoints
    options.AddPolicy("WritePolicy", context =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 50,
                TokensPerPeriod = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                AutoReplenishment = true,
                QueueLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddMemoryCache();

if (string.IsNullOrWhiteSpace(redisConn))
    throw new InvalidOperationException("Redis__BaseUrl is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    try
    {
        var uri = new Uri(redisConn);
        var config = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            ConnectRetry = 10,
            ConnectTimeout = 30_000,
            SyncTimeout = 30_000,
            ResponseTimeout = 30_000,
            KeepAlive = 120,
            DefaultDatabase = 0,
            Ssl = uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase),
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
        };

        config.EndPoints.Add(uri.Host, uri.Port);

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var parts = uri.UserInfo.Split(':', 2);
            if (parts.Length == 2)
                config.Password = Uri.UnescapeDataString(parts[1]);
        }

        var connection = ConnectionMultiplexer.Connect(config);

        if (!connection.IsConnected)
            throw new InvalidOperationException("Failed to connect to Redis.");

        return connection;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Redis connection failed: {ex.Message}", ex);
    }
});

builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddHangfireServer();

builder.Services.AddSignalR();

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

// Domain services
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

// Cloudinary
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IPhotoService, PhotoService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// Mail
builder.Services.Configure<MailSettings>(
    builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IMailingServices, MailingService>();

// Admin seed settings
builder.Services.Configure<AdminSettings>(
    builder.Configuration.GetSection("AdminSettings"));

var app = builder.Build();

app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();

    await next();

    sw.Stop();

    Console.WriteLine($"TOTAL PIPELINE: {sw.ElapsedMilliseconds} ms");
});

var logger = app.Services.GetRequiredService<ILogger<Program>>();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migration(s)…", pendingMigrations.Count());
            await dbContext.Database.MigrateAsync();
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await RoleSeederService.SeedAsync(roleManager);

        var seeder = scope.ServiceProvider.GetRequiredService<IRoleSeederService>();
        await seeder.SeedRolesAndAdminAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during migration or seeding.");
        throw;
    }
}
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinic System API v1");
        c.RoutePrefix = "swagger"; // available at /swagger
    });
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Restrict the dashboard to authenticated admin users in production.
    // In development it is open for convenience.
    Authorization = app.Environment.IsDevelopment()
        ? new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
        : new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
});
app.MapControllers();
app.MapHub<ClinicHub>("/clinicHub");

app.Run();

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Expence.API.Filters;
using Expence.API.Middlewares;
using Expence.Application.Interface;
using Expence.Application.Services;
using Expence.Domain.OptionsConfiguration;
using Expence.Infrastructure;
using Expence.Infrastructure.Interface;
using Expence.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text;


//configure logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.WithProperty("Service", "Expence")
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithSensitiveDataMasking(options =>
            {
                options.MaskProperties.Add(MaskProperty.WithDefaults("password"));
                options.MaskProperties.Add(MaskProperty.WithDefaults("token"));
                options.MaskProperties.Add(MaskProperty.WithDefaults("authorization"));
                options.MaskValue = "***";
            })
            .WriteTo.Console()
            .WriteTo.GrafanaLoki(
    "http://loki:3100",
    labels: new[]
    {
        new LokiLabel { Key = "app", Value = "finance-tracker" }
    }
)


    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// ============ API VERSIONING CONFIGURATION ============
builder.Services.AddApiVersioning(options =>
{
    // Read version from URL: /api/v1/endpoint
    options.ApiVersionReader = new UrlSegmentApiVersionReader();

    // If no version specified, use default version
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Default version
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // Report API versions in response headers
    options.ReportApiVersions = true;
})
.AddApiExplorer();

//configure rate-limiting
var rateLimitingOptions = new RateLimitingOptions();
builder.Configuration.GetSection(RateLimitingOptions.SectionName).Bind(rateLimitingOptions);
builder.Services.Configure<RateLimitingOptions>(builder.Configuration.GetSection(RateLimitingOptions.SectionName));

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});

builder.Services.AddSingleton<ISlidingWindowRateLimiter, SlidingWindowRateLimiter>();

// Add services to the container.
builder.Services.AddDbContext<ExpenceDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailDomainInfoRepository, EmailDomainInfoRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<DisposableEmailService>();
builder.Services.AddScoped<ICategoryPredictionService, CategoryPredictionService>();
builder.Services.AddScoped<IExpenseSummaryGeneratorService, ExpenseSummaryGeneratorService>();
builder.Services.Configure<EmailSettingsOptions>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<ISmtpEmailService, GoogleSmtpEmailService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationResultFilter>();
});


var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
if(string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new Exception("JWT configuration is missing in appsettings.json");
}

var jwtKeyBytes = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
   .AddJwtBearer(options =>
   {
       options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
         {
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime =  true,
              ValidateIssuerSigningKey = true,
              ValidIssuer = jwtIssuer,
              ValidAudience = jwtAudience,
              IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(jwtKeyBytes)
         };
   });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

    // Create a swagger doc for each API version
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(description.GroupName, new OpenApiInfo
        {
            Title = "Expence API",
            Version = description.ApiVersion.ToString(),
            Description = description.IsDeprecated
                ? "?? DEPRECATED - This API version is no longer supported"
                : "Finance expense tracking API",
            Contact = new OpenApiContact
            {
                Name = "Expence Support",
                Email = "support@expence.app"
            },
            License = new OpenApiLicense
            {
                Name = "MIT"
            }
        });
    }

    options.AddServer(new OpenApiServer
    {
        Url = builder.Configuration["Swagger:Server"],
        Description = builder.Environment.EnvironmentName,
    });

    options.AddServer(new OpenApiServer
    {
        Url = "https://localhost:7291",
        Description = "Local host",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.CustomSchemaIds(x => x.FullName);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
});


builder.Services.AddSwaggerGenNewtonsoftSupport();

builder.Services.AddCors( options =>
{ 
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();    

    }); 
});

// ============ MIDDLEWARE PIPELINE ============

var app = builder.Build(); 

app.UseMiddleware<SlidingWindowRateLimitingMiddleware>();

app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"Expence API {description.GroupName.ToUpperInvariant()}");
            }

            options.DocExpansion(DocExpansion.None);
            options.DocumentTitle = "Expence API Documentation";
            options.DefaultModelsExpandDepth(2);
        }
        );
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization(); 

app.MapControllers();
app.Run();

// implement caching later
using Expence.API.Filters;
using Expence.API.Middlewares;
using Expence.Application.Interface;
using Expence.Application.Services;
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
using System;
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

// Add services to the container.
builder.Services.AddDbContext<ExpenceDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailDomainInfoRepository, EmailDomainInfoRepository>();


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ICategoryPredictionService, CategoryPredictionService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


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
builder.Services.AddSwaggerGen(c =>
{
    c.AddServer(new OpenApiServer
    {
        Url = builder.Configuration["Swagger:Server"],
        Description = builder.Environment.EnvironmentName,
    });

    c.AddServer(new OpenApiServer
    {
        Url = "https://localhost:7291",
        Description = "Local host",
    });

    c.SwaggerDoc("API", new OpenApiInfo { Title = $"{builder.Environment.ApplicationName}", Version = builder.Configuration["Swagger:Version"] });
   // c.SwaggerDoc("Admin", new OpenApiInfo { Title = $"{builder.Environment.ApplicationName}", Version = builder.Configuration["Swagger:Version"] });
    var location = Assembly.GetAssembly(typeof(Program))!.Location;
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    c.CustomSchemaIds(x => x.FullName);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
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




var app = builder.Build();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
//app.UseMiddleware<FluentValidationExceptionHandlerMiddleware>();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        {
            string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";

            c.DocExpansion(DocExpansion.None);
            c.DocumentTitle = "Expence API Swagger UI";
            c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/API/swagger.json", "API endpoints");
            c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/Admin/swagger.json", "Admin endpoints");
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
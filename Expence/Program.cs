using Expence.Application.Interface;
using Expence.Application.Services;
using Expence.Infrastructure;
using Expence.Infrastructure.Interface;
using Expence.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ExpenceDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<EmailService>();


builder.Services.AddHttpContextAccessor();
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
    c.SwaggerDoc("Admin", new OpenApiInfo { Title = $"{builder.Environment.ApplicationName}", Version = builder.Configuration["Swagger:Version"] });
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








var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.Run();

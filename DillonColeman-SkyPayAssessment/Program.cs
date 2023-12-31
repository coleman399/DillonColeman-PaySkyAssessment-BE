global using AutoMapper;
global using DillonColeman_SkyPayAssessment.Dtos.UserDtos;
global using DillonColeman_SkyPayAssessment.Dtos.VacancyDtos;
global using DillonColeman_SkyPayAssessment.Exceptions;
global using DillonColeman_SkyPayAssessment.Helpers;
global using DillonColeman_SkyPayAssessment.Models.UserModel;
global using DillonColeman_SkyPayAssessment.Models.VacancyModel;
global using DillonColeman_SkyPayAssessment.Service.UserService;
global using DillonColeman_SkyPayAssessment.Service.VacancyService;
global using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container.

    builder.Services.AddControllers();
    builder.Services.AddAutoMapper(typeof(Program), typeof(AutoMapperProfile));
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddDbContext<UserContext>(options => options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));
    builder.Services.AddScoped<IVacancyService, VacancyService>();
    builder.Services.AddDbContext<VacancyContext>(options => options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "DillonColeman-SkyPayAssessment", Version = GlobalConstants.VERSION });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: Bearer <token>",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer",
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme,
                    },
                },
                Array.Empty<string>()
            },
            });
    });
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTSecurityKey"]!)),
            };
        });

    var app = builder.Build();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
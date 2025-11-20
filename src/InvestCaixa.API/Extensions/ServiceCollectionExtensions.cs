namespace InvestCaixa.API.Extensions;

using FluentValidation;
using FluentValidation.AspNetCore;
using InvestCaixa.API.Middlewares;
using InvestCaixa.Application.Handlers;
using InvestCaixa.Application.Interfaces;
using InvestCaixa.Application.Mappings;
using InvestCaixa.Application.Services;
using InvestCaixa.Application.Validators;
using InvestCaixa.Domain.Interfaces;
using InvestCaixa.Infrastructure.Data;
using InvestCaixa.Infrastructure.HealthChecks;
using InvestCaixa.Infrastructure.Repositories;
using InvestCaixa.Infrastructure.Repositories.Decorators;
using InvestCaixa.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StackExchange.Redis;
using System;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Controllers com FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining(typeof(SimularInvestimentoValidator));
        services.AddValidatorsFromAssemblyContaining(typeof(LoginRequestValidator));

        // Swagger/OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "InvesteCaixa API",
                Version = "v1.0",
                Description = @"
**API para simulação e gestão de investimento**

Esta API permite:
- Simular Investimentos com cálculos financeiros precisos.
- Gerenciar perfis de risco dos clientes.
- Consultar produtos de investimento disponíveis.
- Calcular suitability (CVM 539)",
                Contact = new OpenApiContact
                {
                    Name = "Gabriel Lisboa Espindola Florencio",
                    Email = "gabriel.florencio@caixa.gov.br",
                    Url = new Uri("https://github.com/lisbiel/InvestCaixa")
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = @"
                **Autenticação JWT**
                Informe o token JWT, insira apenas o token, sem o 'Bearer '.

                **Para obter o token:**
                1. Use o endpoint '/api/auth/login'
                2. Copie o token da resposta
                3. Cole aqui para autorizar os requests.


                Refresh Tokens implementados, caso seu token expire."
            });

            options.AddSecurityRequirement(doc =>
            {
                var schemeRef = new OpenApiSecuritySchemeReference("Bearer", doc, null);

                var requirement = new OpenApiSecurityRequirement();
                requirement.Add(schemeRef, new List<string>());

                return requirement;
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

            // JWT Authentication
            var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret não configurado");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        // Exception Handler
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            }); ;
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

        // Application Services
        services.AddScoped<ISimulacaoService, SimulacaoService>();
        services.AddScoped<IPerfilRiscoService, PerfilRiscoService>();
        services.AddScoped<ITelemetriaService, TelemetriaService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<FinalizarInvestimentoCommandHandler>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ObterPerfilRiscoQueryHandler>());

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<InvestimentoDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(InvestimentoDbContext).Assembly.FullName)));

        services.AddScoped<ISimulacaoRepository, SimulacaoRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IInvestimentoFinalizadoRepository, InvestimentoFinalizadoRepository>();
        services.AddScoped<IPerfilFinanceiroRepository, PerfilFinanceiroRepository>();

        services.Decorate<IProdutoRepository, CachingProdutoRepository>();

        // Unit of Work & Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Infrastructure Services
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddMemoryCache();

        var redisConnection = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("ConnectionStrings:Redis não configurada.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "InvestCaixa_";
        });

        services.AddHealthChecks()
            .AddDbContextCheck<InvestimentoDbContext>("Database", tags: new[] { "db", "ready" })
            .AddRedis(redisConnection, "Redis", tags: new[] {"cache", "ready"})
            .AddCheck<ApplicationHealthChecks>("application", tags: new[] {"app", "live"});

        return services;
    }
}
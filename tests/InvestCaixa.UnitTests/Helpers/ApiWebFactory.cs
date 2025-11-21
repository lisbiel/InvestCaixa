using InvestCaixa.Application.Handlers;
using InvestCaixa.Application.Interfaces;
using InvestCaixa.Application.Mappings;
using InvestCaixa.Application.Services;
using InvestCaixa.Domain.Interfaces;
using InvestCaixa.Infrastructure.Data;
using InvestCaixa.Infrastructure.Repositories;
using InvestCaixa.Infrastructure.Repositories.Decorators;
using InvestCaixa.UnitTests.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

namespace InvestCaixa.UnitTests.Fixtures;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<InvestimentoDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            var distributedCacheDescriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(Microsoft.Extensions.Caching.Distributed.IDistributedCache));
            if (distributedCacheDescriptor != null)
                services.Remove(distributedCacheDescriptor);

            var cacheServiceDescriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(ICacheService));
            if (cacheServiceDescriptor != null)
                services.Remove(cacheServiceDescriptor);

            // Banco de dados em memória para testes
            services.AddDbContext<InvestimentoDbContext>(options =>
            {
                options.UseInMemoryDatabase("InvestCaixaTests");
            });

            services.AddMemoryCache();

            services.AddSingleton<ICacheService, InMemoryCacheService>();

            // JWT simplificado para testes - autentica apenas SE houver token
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Só autentica se tiver Authorization header VÁLIDO
                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                        {
                            var token = authHeader.Substring("Bearer ".Length).Trim();
                            
                            // Só autentica se NÃO for um token explicitamente inválido
                            if (token != "invalid-token" && token != "token-invalido" && token != "invalid_token_123" && !string.IsNullOrEmpty(token))
                            {
                                var claims = new[]
                                {
                                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "test-user"),
                                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Test User")
                                };
                                
                                var identity = new System.Security.Claims.ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                                context.Principal = new System.Security.Claims.ClaimsPrincipal(identity);
                                context.Success();
                            }
                            // Se for token inválido, não autentica
                        }
                        // Se não tiver header, não faz nada (deixa não autenticado)
                        return Task.CompletedTask;
                    }
                };
            });

        });

        builder.UseEnvironment("Testing");
    }
}

/// <summary>
/// Cache em memória simples para testes (não usa Redis).
/// Implementa ICacheService com Dictionary thread-safe.
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        await Task.CompletedTask; // Simula async
        _cache.TryGetValue(key, out var value);
        return value as T;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        await Task.CompletedTask;
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);
        else
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // Default 30 min

        _cache.Set(key, value, options);
    }

    public async Task RemoveAsync(string key)
    {
        await Task.CompletedTask;
        _cache.Remove(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        await Task.CompletedTask;
        return _cache.TryGetValue(key, out _);
    }

    public async Task ClearAsync()
    {
        await Task.CompletedTask;
        _cache.Dispose();
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        var valorCache = await GetAsync<T>(key);
        if (valorCache != null)
        {
            return valorCache;
        }

        var valorExterno = await factory();
        await SetAsync(key, valorExterno, expiration);
        return valorExterno;
    }
}



/// <summary>
/// Extensão para decorar serviços de forma limpa.
/// Permite: services.Decorate<IProdutoRepository, CachingProdutoRepository>()
/// </summary>
public static class DecoratorExtensions
{
    public static IServiceCollection Decorate<TInterface, TDecorator>(
        this IServiceCollection services)
        where TInterface : class
        where TDecorator : class, TInterface
    {
        // Pega o provider temporário para resolver o inner repository
        var provider = services.BuildServiceProvider();
        var innerRepository = provider.GetRequiredService<TInterface>();

        // Remove o serviço original
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TInterface));
        if (descriptor != null)
            services.Remove(descriptor);

        // Registra novamente com o decorator
        services.AddScoped<TInterface>(sp =>
        {
            var cache = sp.GetRequiredService<ICacheService>();
            var logger = sp.GetRequiredService<ILogger<CachingProdutoRepository>>();

            // Para CachingProdutoRepository especificamente
            if (typeof(TDecorator) == typeof(CachingProdutoRepository))
            {
                var inner = sp.GetRequiredService<ProdutoRepository>();
                return (TInterface)(object)new CachingProdutoRepository(inner, cache, logger);
            }

            throw new InvalidOperationException($"Decorator {typeof(TDecorator).Name} não configurado");
        });

        return services;
    }
}

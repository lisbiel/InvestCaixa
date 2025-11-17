using InvestCaixa.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvestCaixa.Application.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CacheService(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            // Tenta obter do cache em memória primeiro
            if (_memoryCache.TryGetValue(key, out T? valorCache))
            {
                _logger.LogInformation("Cache hit (Memória): {Key}", key);
                return valorCache;
            }
            // Se não estiver no cache em memória, tenta obter do cache distribuído
            var valorRedis = await _distributedCache.GetStringAsync(key);
            if (valorRedis != null)
            {
                _logger.LogInformation("Cache hit (redis): {Key}", key);
                var value = JsonSerializer.Deserialize<T>(valorRedis, _jsonOptions);
                // Armazena no cache em memória para futuras requisições
                _memoryCache.Set(key, value, TimeSpan.FromMinutes(5)); // Exemplo: 5 minutos de expiração em memória
                return value;
            }

            _logger.LogInformation("Cache miss: {key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter do cache: {Key}", key);
            return default;
        }
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

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
            _memoryCache.Remove(key);
            _logger.LogDebug("Cache removido: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover o cache: {Key}", key);
        }
    }

    /* Pattern ficará para versão futura devido à necessidade de serviços adicionais
    public Task RemoveByPatternAsync(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Padrão do cache não pode estar vazio.", nameof(pattern));

        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());

            // SCAN com MATCH: itera sobre chaves sem travar
            var keysToDelete = new List<RedisKey>();
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                keysToDelete.Add(key);
            }

            if (keysToDelete.Count == 0)
            {
                _logger.LogDebug("Nenhuma chave encontrada para padrão: {Pattern}", pattern);
                return;
            }

            // Delete em lotes para não sobrecarregar
            const int batchSize = 100;
            var db = _redis.GetDatabase();

            for (int i = 0; i < keysToDelete.Count; i += batchSize)
            {
                var batch = keysToDelete.Skip(i).Take(batchSize).ToArray();
                await db.KeyDeleteAsync(batch);
            }

            _logger.LogInformation(
                "Cache removido por padrão: {Pattern} ({Count} chaves deletadas)",
                pattern,
                keysToDelete.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover cache por padrão: {Pattern}", pattern);
            // Falha aberta
        }
    }*/

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var valorSerializado = JsonSerializer.Serialize(value, _jsonOptions);
            //Cache distribuído com maior tempo de expiração para compartilhamento entre instâncias
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10) // Expiração padrão de 10 minutos
            };
            await _distributedCache.SetStringAsync(key, valorSerializado, options);

            //Cache em memória com tempo de expiração menor para acesso rápido
            _memoryCache.Set(key, value, expiration ?? TimeSpan.FromMinutes(5)); // Expiração padrão de 5 minutos

            _logger.LogDebug("Cache definido: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir o cache: {Key}", key);
        }
    }
}

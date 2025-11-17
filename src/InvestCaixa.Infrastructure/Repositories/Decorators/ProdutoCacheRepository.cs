using InvestCaixa.Application.Interfaces;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace InvestCaixa.Infrastructure.Repositories.Decorators;

public class CachingProdutoRepository : IProdutoRepository
{
    private readonly IProdutoRepository _innerRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<CachingProdutoRepository> _logger;

    public CachingProdutoRepository(
        IProdutoRepository innerRepository,
        ICacheService cache,
        ILogger<CachingProdutoRepository> logger)
    {
        _innerRepository = innerRepository;
        _cache = cache;
        _logger = logger;
    }

    #region Obter produto único
    public async Task<ProdutoInvestimento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKeyForId(id);

        // Tenta cache
        var cached = await _cache.GetAsync<ProdutoInvestimento>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit para produto {Id}", id);
            return cached;
        }

        // Cache miss: busca do repo original (DB)
        _logger.LogDebug("Cache miss para produto {Id}, buscando do DB", id);
        var produto = await _innerRepository.GetByIdAsync(id);

        // Armazena em cache
        if (produto != null)
        {
            await _cache.SetAsync(cacheKey, produto, TimeSpan.FromMinutes(30));
        }

        return produto;
    }

    public async Task<ProdutoInvestimento?> ObterPorTipoAsync(
        string tipo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            return null;

        var cacheKey = BuildCacheKeyForTipo(tipo);

        var cached = await _cache.GetAsync<ProdutoInvestimento>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit para produto tipo {Tipo}", tipo);
            return cached;
        }

        _logger.LogDebug("Cache miss para produto tipo {Tipo}", tipo);
        var produto = await _innerRepository.ObterPorTipoAsync(tipo, cancellationToken);

        if (produto != null)
        {
            await _cache.SetAsync(cacheKey, produto);
        }

        return produto;
    }
    #endregion

    #region Obter múltiplos produtos
    public async Task<IEnumerable<ProdutoInvestimento>> ObterPorPerfilAsync(
        PerfilInvestidor perfil,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKeyForPerfil(perfil);

        var cached = await _cache.GetAsync<List<ProdutoInvestimento>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit para produtos perfil {Perfil}", perfil);
            return cached;
        }

        _logger.LogDebug("Cache miss para produtos perfil {Perfil}", perfil);
        var produtos = (await _innerRepository.ObterPorPerfilAsync(perfil, cancellationToken))
            .ToList();

        if (produtos.Any())
        {
            await _cache.SetAsync(cacheKey, produtos);
        }

        return produtos;
    }

    public async Task<IEnumerable<ProdutoInvestimento>> ObterPorCriteriosAsync(
        decimal? valorMinimo = null,
        NivelRisco? nivelRisco = null,
        bool? permiteLiquidez = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKeyForCriterios(valorMinimo, nivelRisco, permiteLiquidez);

        var cached = await _cache.GetAsync<List<ProdutoInvestimento>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit para produtos critérios {Criterios}", cacheKey);
            return cached;
        }

        _logger.LogDebug("Cache miss para produtos critérios {Criterios}", cacheKey);
        var produtos = (await _innerRepository.ObterPorCriteriosAsync(
            valorMinimo, nivelRisco, permiteLiquidez, cancellationToken))
            .ToList();

        if (produtos.Any())
        {
            await _cache.SetAsync(cacheKey, produtos);
        }

        return produtos;
    }

    public async Task<IEnumerable<ProdutoInvestimento>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKeyForTodos();

        var cached = await _cache.GetAsync<IEnumerable<ProdutoInvestimento>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit para todos os produtos");
            return cached;
        }

        var produtos = await _innerRepository.GetAllAsync();

        await _cache.SetAsync(cacheKey, produtos, TimeSpan.FromMinutes(30));
        return produtos;
    }
    #endregion

    #region Write/Delete com invalidação de cache
    public async Task<ProdutoInvestimento> AddAsync(
        ProdutoInvestimento entity,
        CancellationToken cancellationToken = default)
    {
        await _innerRepository.AddAsync(entity, cancellationToken);

        // Invalida caches relevantes após inserção
        await InvalidarCacheAposMudancaAsync(entity);

        _logger.LogInformation("Produto {Id} adicionado, cache invalidado", entity.Id);
        return entity;
    }

    public async Task UpdateAsync(
        ProdutoInvestimento entity,
        CancellationToken cancellationToken = default)
    {
        await _innerRepository.UpdateAsync(entity, cancellationToken);

        // Invalida caches do produto específico e listas
        await InvalidarCacheAposMudancaAsync(entity);

        _logger.LogInformation("Produto {Id} atualizado, cache invalidado", entity.Id);
    }

    public async Task DeleteEntityAsync(
        ProdutoInvestimento entity,
        CancellationToken cancellationToken = default)
    {
        await _innerRepository.DeleteAsync(entity.Id, cancellationToken);

        // Invalida caches relacionados
        await InvalidarCacheProdutosAsync(entity);

        _logger.LogInformation("Produto {Id} removido, cache invalidado", entity.Id);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var produto = await GetByIdAsync(id, cancellationToken);
        await _innerRepository.DeleteAsync(id, cancellationToken);

        // Invalida caches do produto específico
        await InvalidarCacheAposMudancaAsync(produto);

        _logger.LogInformation("Produto {Id} removido, cache invalidado", id);
    }
    #endregion

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKeyForId(id);
        var cached = await _cache.GetAsync<ProdutoInvestimento>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit para existência do produto {Id}", id);
            return true;
        }

        return await _innerRepository.ExistsAsync(id, cancellationToken);
    }
    private async Task InvalidarCacheProdutosAsync(ProdutoInvestimento produto)
    {
        var keysToInvalidate = new[]
        {
            $"produto:id:{produto.Id}",
            $"produto:tipo:{produto.Tipo.ToString().ToLower()}",
        };

        foreach (var key in keysToInvalidate)
        {
            await _cache.RemoveAsync(key);
        }

        // Invalida todas as listas (perfil, critérios)
        // Usa padrão para remover todos os "produtos:*" em um comando
        //await _cache.RemoveByPatternAsync("produtos:*");
    }

    #region Cache Key Builders - Vamos evitar pattern por enquanto

    /// <summary>
    /// Constrói as chaves de forma centralizada e reutilizável.
    /// Mantém a "verdade" de como as chaves são montadas em um único lugar.
    /// </summary>

    private static string BuildCacheKeyForId(Guid id)
        => $"produto:id:{id}";

    private static string BuildCacheKeyForTipo(string tipo)
        => $"produto:tipo:{tipo.ToLower()}";

    private static string BuildCacheKeyForTodos()
        => "produtos:todos";

    private static string BuildCacheKeyForPerfil(PerfilInvestidor perfil)
        => $"produtos:perfil:{perfil}";

    private static string BuildCacheKeyForCriterios(decimal? valorMinimo, NivelRisco? nivelRisco, bool? permiteLiquidez)
    {
        var parts = new List<string>();

        if (valorMinimo.HasValue)
            parts.Add($"vm:{valorMinimo.Value}");

        if (nivelRisco.HasValue)
            parts.Add($"risco:{nivelRisco.Value}");

        if (permiteLiquidez.HasValue)
            parts.Add($"liq:{permiteLiquidez.Value}");

        var criterios = parts.Any() ? string.Join(":", parts) : "default";
        return $"produtos:criterios:{criterios}";
    }

    #endregion

    #region Invalidação de Cache - Vamos evitar pattern por enquanto

    /// <summary>
    /// Invalida todos os caches relacionados a um produto após mudança.
    /// Remove exatamente: ID, Tipo, e todas as listas.
    /// </summary>
    private async Task InvalidarCacheAposMudancaAsync(ProdutoInvestimento produto)
    {
        // Chaves exatas do produto específico
        var keysToRemove = new List<string>
        {
            BuildCacheKeyForId(produto.Id),
            BuildCacheKeyForTipo(produto.Tipo.ToString()),
        };

        // Remove chaves individuais
        foreach (var key in keysToRemove)
        {
            await _cache.RemoveAsync(key);
        }

        // Remove todas as listas (já que produto pode afetar qualquer uma)
        await InvalidarTodasAsListasAsync();

        _logger.LogDebug("Cache invalidado para produto {Id}: {Keys}", produto.Id, string.Join(", ", keysToRemove));
    }

    /// <summary>
    /// Remove TODAS as chaves de lista de uma vez.
    /// Sem padrão: exatamente as 4 chaves que você sabe que existem.
    /// </summary>
    private async Task InvalidarTodasAsListasAsync()
    {
        var listKeys = new[]
        {
            BuildCacheKeyForTodos(),
        };

        foreach (var key in listKeys)
        {
            await _cache.RemoveAsync(key);
        }

        // Para as listas por perfil, você remove todos os perfis conhecidos
        var perfils = Enum.GetValues(typeof(PerfilInvestidor))
            .Cast<PerfilInvestidor>()
            .ToList();

        foreach (var perfil in perfils)
        {
            var perfilKey = BuildCacheKeyForPerfil(perfil);
            await _cache.RemoveAsync(perfilKey);
        }

        // Nota: chaves de "criterios" dinâmicos são mais complexas.
        // Opção 1: Aceitar que podem ficar em cache (TTL curto = 15min)
        // Opção 2: Implementar um índice separado (mais complexo)
        // Por enquanto, deixamos criterios como está (expira naturalmente em 15min)

        _logger.LogDebug("Todas as listas de cache foram invalidadas");
    }
    #endregion
}

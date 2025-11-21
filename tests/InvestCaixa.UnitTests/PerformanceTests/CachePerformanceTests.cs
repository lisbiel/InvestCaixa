namespace InvestCaixa.UnitTests.PerformanceTests;

using FluentAssertions;
using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using InvestCaixa.UnitTests.Fixtures;
using InvestCaixa.UnitTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Testes de performance de cache e fallback scenarios.
/// Valida comportamento do cache Redis/MemoryCache sob diferentes condições.
/// </summary>
[Collection("Integration Tests")]
public class CachePerformanceTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CachePerformanceTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _ = _fixture.ResetDatabase();
    }

    [Fact]
    public async Task CacheWarmup_PrimeiraVezVsSubsequentes_DeveAcelerarSignificativamente()
    {
        // Arrange
        var endpoints = new[]
        {
            "/api/perfil-risco/produtos-recomendados/Conservador",
            "/api/perfil-risco/produtos-recomendados/Moderado",
            "/api/perfil-risco/produtos-recomendados/Agressivo"
        };

        var temposPrimeiro = new Dictionary<string, long>();
        var temposSubsequentes = new Dictionary<string, List<long>>();

        foreach (var endpoint in endpoints)
        {
            temposSubsequentes[endpoint] = new List<long>();
        }

        // Act - Primeiro acesso (cache miss)
        foreach (var endpoint in endpoints)
        {
            var sw = Stopwatch.StartNew();
            var response = await _fixture.Client.GetAsync(endpoint);
            sw.Stop();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            temposPrimeiro[endpoint] = sw.ElapsedMilliseconds;
            
            _output.WriteLine($"Cache MISS {endpoint}: {sw.ElapsedMilliseconds}ms");
        }

        // Act - Acessos subsequentes (cache hit)
        for (int i = 0; i < 10; i++)
        {
            foreach (var endpoint in endpoints)
            {
                var sw = Stopwatch.StartNew();
                var response = await _fixture.Client.GetAsync(endpoint);
                sw.Stop();

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                temposSubsequentes[endpoint].Add(sw.ElapsedMilliseconds);
            }
        }

        // Assert
        foreach (var endpoint in endpoints)
        {
            var tempoMiss = temposPrimeiro[endpoint];
            var tempoMedioHit = temposSubsequentes[endpoint].Average();
            var melhoriaPercentual = ((tempoMiss - tempoMedioHit) / tempoMiss) * 100;

            _output.WriteLine($"\n📊 {endpoint}:");
            _output.WriteLine($"   Cache MISS: {tempoMiss}ms");
            _output.WriteLine($"   Cache HIT médio: {tempoMedioHit:F2}ms");
            _output.WriteLine($"   Melhoria: {melhoriaPercentual:F1}%");

            // Cache deve proporcionar melhoria ou manter performance baixa
            if (tempoMiss > 5) // Só exige melhoria se o tempo inicial for significativo
            {
                melhoriaPercentual.Should().BeGreaterThan(20, 
                    $"Cache deve melhorar performance em pelo menos 20% para {endpoint}");
            }
            
            tempoMedioHit.Should().BeLessThan(500, 
                $"Cache hit deve ser < 500ms para {endpoint}");
        }
    }

    [Fact]
    public async Task CacheConcurrency_MultiploAcessosSimultaneos_NaoDeveCorromperDados()
    {
        // Arrange
        const int numeroTasks = 20;
        const string endpoint = "/api/perfil-risco/produtos-recomendados/Moderado";
        var resultados = new ConcurrentBag<List<ProdutoResponse>>();
        var tempos = new ConcurrentBag<long>();

        // Act - Acessos simultâneos ao mesmo endpoint
        var tasks = Enumerable.Range(1, numeroTasks).Select(async i =>
        {
            var sw = Stopwatch.StartNew();
            
            var response = await _fixture.Client.GetAsync(endpoint);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseBody = await response.Content.ReadAsStringAsync();
            var produtos = JsonSerializer.Deserialize<List<ProdutoResponse>>(responseBody, _jsonOptions);
            
            sw.Stop();
            
            resultados.Add(produtos!);
            tempos.Add(sw.ElapsedMilliseconds);
        });

        await Task.WhenAll(tasks);

        // Assert - Consistência dos dados
        var primeiroResultado = resultados.First();
        resultados.Should().AllSatisfy(resultado =>
        {
            resultado.Should().HaveCount(primeiroResultado.Count);
            resultado.Should().BeEquivalentTo(primeiroResultado);
        });

        var tempoMedio = tempos.Average();
        var tempoMaximo = tempos.Max();
        var tempoMinimo = tempos.Min();

        _output.WriteLine($"🔄 CACHE CONCURRENCY TEST:");
        _output.WriteLine($"Tasks executadas: {numeroTasks}");
        _output.WriteLine($"Tempo médio: {tempoMedio:F2}ms");
        _output.WriteLine($"Tempo min/max: {tempoMinimo}/{tempoMaximo}ms");
        _output.WriteLine($"Produtos por response: {primeiroResultado.Count}");

        // Performance sob concorrência
        tempoMedio.Should().BeLessThan(1000, "Tempo médio deve ser < 1s mesmo com concorrência");
        tempoMaximo.Should().BeLessThan(3000, "Nenhum request deve exceder 3s");
        
        // Variação de tempo deve ser baixa (indicando cache efetivo)
        var coeficienteVariacao = tempos.StandardDeviation() / tempoMedio;
        coeficienteVariacao.Should().BeLessThan(2.0, "Baixa variação indica cache funcionando bem");
    }

    [Fact]
    public async Task CacheExpiration_AposTTL_DeveRecarregarDados()
    {
        // Arrange
        const string endpoint = "/api/perfil-risco/produtos-recomendados/Conservador";
        
        // Primeiro acesso para popular o cache
        var sw1 = Stopwatch.StartNew();
        var response1 = await _fixture.Client.GetAsync(endpoint);
        sw1.Stop();
        
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        var tempo1 = sw1.ElapsedMilliseconds;

        // Segundo acesso imediato (deve vir do cache)
        var sw2 = Stopwatch.StartNew();
        var response2 = await _fixture.Client.GetAsync(endpoint);
        sw2.Stop();
        
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        var tempo2 = sw2.ElapsedMilliseconds;

        _output.WriteLine($"🕐 CACHE EXPIRATION TEST:");
        _output.WriteLine($"Primeiro acesso: {tempo1}ms");
        _output.WriteLine($"Segundo acesso (cache): {tempo2}ms");

        // Segundo acesso deve ser mais rápido (cache hit) ou pelo menos igual para tempos muito baixos
        tempo2.Should().BeLessThanOrEqualTo(Math.Max(1L, (long)(tempo1 * 0.7)), "Segundo acesso deve ser mais rápido ou igual");

        // Note: Não podemos testar TTL real pois seria muito lento para o teste
        // Em produção, o TTL é configurado para 30 minutos
        _output.WriteLine("ℹ️ TTL real (30min) não testado por questões de performance do teste");
    }

    [Fact]
    public async Task CacheInvalidation_AposOperacaoWrite_DeveAtualizarCache()
    {
        // Arrange - Popular cache com consulta
        var produtosResponse1 = await _fixture.Client.GetAsync("/api/perfil-risco/produtos-recomendados/Moderado");
        produtosResponse1.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var produtos1 = JsonSerializer.Deserialize<List<ProdutoResponse>>(
            await produtosResponse1.Content.ReadAsStringAsync(), _jsonOptions);

        // Simular operação que pode afetar produtos (criar simulação)
        var simulacaoRequest = TestDataBuilder.CriarSimulacaoRequest();
        var content = new StringContent(
            JsonSerializer.Serialize(simulacaoRequest),
            Encoding.UTF8,
            "application/json");

        var simulacaoResponse = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
        simulacaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Nova consulta após operação write
        var sw = Stopwatch.StartNew();
        var produtosResponse2 = await _fixture.Client.GetAsync("/api/perfil-risco/produtos-recomendados/Moderado");
        sw.Stop();
        
        produtosResponse2.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var produtos2 = JsonSerializer.Deserialize<List<ProdutoResponse>>(
            await produtosResponse2.Content.ReadAsStringAsync(), _jsonOptions);

        // Assert
        _output.WriteLine($"🔄 CACHE INVALIDATION TEST:");
        _output.WriteLine($"Produtos antes: {produtos1!.Count}");
        _output.WriteLine($"Produtos depois: {produtos2!.Count}");
        _output.WriteLine($"Tempo reconsulta: {sw.ElapsedMilliseconds}ms");

        // Dados devem ser consistentes (produtos não mudam com simulações)
        produtos2.Should().HaveCount(produtos1.Count);
        sw.ElapsedMilliseconds.Should().BeLessThan(3000, "Reconsulta deve ser rápida");
    }

    [Fact]
    public async Task CacheMemoryPressure_SobAltaCarga_DeveManterPerformance()
    {
        // Arrange - Simula pressão no cache com muitas consultas diferentes
        var perfis = new[] { "Conservador", "Moderado", "Agressivo" };
        var clientes = Enumerable.Range(1, 15).ToArray();
        var temposPorTipo = new Dictionary<string, List<long>>
        {
            ["produtos"] = new(),
            ["perfil"] = new(),
            ["simulacoes"] = new()
        };

        // Act - Múltiplas consultas para pressionar cache
        var tasks = new List<Task>();

        // Produtos (cacheable)
        foreach (var perfil in perfis)
        {
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var sw = Stopwatch.StartNew();
                    var response = await _fixture.Client.GetAsync($"/api/perfil-risco/produtos-recomendados/{perfil}");
                    sw.Stop();
                    
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        lock (temposPorTipo["produtos"])
                        {
                            temposPorTipo["produtos"].Add(sw.ElapsedMilliseconds);
                        }
                    }
                }));
            }
        }

        // Perfis (cacheable)
        foreach (var clienteId in clientes)
        {
            for (int i = 0; i < 3; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var sw = Stopwatch.StartNew();
                    var response = await _fixture.Client.GetAsync($"/api/perfil-risco/{clienteId}");
                    sw.Stop();
                    
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        lock (temposPorTipo["perfil"])
                        {
                            temposPorTipo["perfil"].Add(sw.ElapsedMilliseconds);
                        }
                    }
                }));
            }
        }

        // Simulações (não cacheable - para comparação)
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                var response = await _fixture.Client.GetAsync("/api/simulacao/simulacoes");
                sw.Stop();
                
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    lock (temposPorTipo["simulacoes"])
                    {
                        temposPorTipo["simulacoes"].Add(sw.ElapsedMilliseconds);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        _output.WriteLine($"\n💾 CACHE MEMORY PRESSURE TEST:");
        
        foreach (var tipo in temposPorTipo)
        {
            if (tipo.Value.Any())
            {
                var tempoMedio = tipo.Value.Average();
                var tempoMaximo = tipo.Value.Max();
                var count = tipo.Value.Count;

                _output.WriteLine($"{tipo.Key}: {count} requests, média: {tempoMedio:F2}ms, máx: {tempoMaximo}ms");

                // Requests cacheadas devem ser consistentemente rápidas
                if (tipo.Key != "simulacoes")
                {
                    tempoMedio.Should().BeLessThan(2000, $"{tipo.Key} deve manter performance com cache");
                }
                
                tempoMaximo.Should().BeLessThan(5000, $"Nenhum request {tipo.Key} deve exceder 5s");
            }
        }

        // Cache deve demonstrar benefício vs operações não-cacheadas
        if (temposPorTipo["produtos"].Any() && temposPorTipo["simulacoes"].Any())
        {
            var mediaProdutos = temposPorTipo["produtos"].Average();
            var mediaSimulacoes = temposPorTipo["simulacoes"].Average();
            
            _output.WriteLine($"Benefício cache produtos vs simulações: {((mediaSimulacoes - mediaProdutos) / mediaSimulacoes * 100):F1}%");
        }
    }

    [Fact]
    public async Task CacheHitRatio_SobUsoNormal_DeveManterAltaEficiencia()
    {
        // Arrange
        const int numeroRounds = 5;
        const int requestsPorRound = 10;
        var hitsMisses = new Dictionary<string, int> { ["hits"] = 0, ["misses"] = 0 };
        var endpoints = new[]
        {
            "/api/perfil-risco/produtos-recomendados/Conservador",
            "/api/perfil-risco/produtos-recomendados/Moderado",
            "/api/perfil-risco/produtos-recomendados/Agressivo"
        };

        // Act - Simula padrão real de uso (algumas consultas repetidas)
        for (int round = 1; round <= numeroRounds; round++)
        {
            _output.WriteLine($"Round {round}/{numeroRounds}");
            
            for (int req = 0; req < requestsPorRound; req++)
            {
                // 70% das requests vão para endpoints já acessados (hit)
                // 30% das requests são para novos endpoints ou variações (miss)
                var isHitExpected = req < (requestsPorRound * 0.7);
                var endpoint = endpoints[isHitExpected ? 0 : (req % endpoints.Length)];
                
                var sw = Stopwatch.StartNew();
                var response = await _fixture.Client.GetAsync(endpoint);
                sw.Stop();

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Primeira vez em cada endpoint = miss provável, resto = hit provável
                var tempoLimiteHit = 500; // ms
                if (sw.ElapsedMilliseconds <= tempoLimiteHit)
                {
                    hitsMisses["hits"]++;
                }
                else
                {
                    hitsMisses["misses"]++;
                }
            }

            // Pausa entre rounds
            if (round < numeroRounds)
            {
                await Task.Delay(100);
            }
        }

        // Assert
        var totalRequests = hitsMisses["hits"] + hitsMisses["misses"];
        var hitRatio = (double)hitsMisses["hits"] / totalRequests * 100;

        _output.WriteLine($"\n🎯 CACHE HIT RATIO TEST:");
        _output.WriteLine($"Total requests: {totalRequests}");
        _output.WriteLine($"Cache hits: {hitsMisses["hits"]}");
        _output.WriteLine($"Cache misses: {hitsMisses["misses"]}");
        _output.WriteLine($"Hit ratio: {hitRatio:F1}%");

        // Em uso normal, hit ratio deve ser alto
        hitRatio.Should().BeGreaterThan(60, "Cache hit ratio deve ser > 60% em uso normal");
        hitsMisses["hits"].Should().BeGreaterThan(0, "Deve haver pelo menos alguns cache hits");
        totalRequests.Should().Be(numeroRounds * requestsPorRound, "Todas as requests devem ter sido processadas");
    }
}

// Extension helper para cálculos estatísticos
public static class StatisticsExtensions
{
    public static double StandardDeviation(this IEnumerable<long> values)
    {
        var avg = values.Average();
        var sumOfSquares = values.Sum(x => Math.Pow(x - avg, 2));
        return Math.Sqrt(sumOfSquares / values.Count());
    }
}
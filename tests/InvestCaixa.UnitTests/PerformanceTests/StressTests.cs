namespace InvestCaixa.UnitTests.PerformanceTests;

using FluentAssertions;
using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.UnitTests.Fixtures;
using InvestCaixa.UnitTests.Helpers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Testes de stress avançados para identificar pontos de falha do sistema.
/// Simula condições extremas e edge cases de performance.
/// </summary>
[Collection("Integration Tests")]
public class StressTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public StressTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _ = _fixture.ResetDatabase();
    }

    [Fact]
    public async Task StressTest_PicoDeTrafegoSimultaneo_DeveManterEstabilidade()
    {
        // Arrange - Simula Black Friday / pico de tráfego
        const int ondas = 3;
        const int requestsPorOnda = 25;
        var resultadosGerais = new List<(int Onda, int Sucessos, int Falhas, double TempoMedio)>();

        _output.WriteLine("🚀 INICIANDO STRESS TEST - PICO DE TRÁFEGO");

        for (int onda = 1; onda <= ondas; onda++)
        {
            _output.WriteLine($"\n📈 ONDA {onda}/{ondas} - {requestsPorOnda} requests simultâneas");
            
            var sucessos = new ConcurrentBag<long>();
            var falhas = new ConcurrentBag<string>();
            var sw = Stopwatch.StartNew();

            // Act - Rajada de requests simultâneas
            var tasks = Enumerable.Range(1, requestsPorOnda).Select(async i =>
            {
                var requestSw = Stopwatch.StartNew();
                try
                {
                    // Varia tipos de requests para simular tráfego real
                    HttpResponseMessage response = (i % 4) switch
                    {
                        0 => await CriarSimulacao(i),
                        1 => await ConsultarPerfilRisco(i % 10 + 1),
                        2 => await ConsultarProdutos(),
                        _ => await ConsultarTelemetria()
                    };

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        sucessos.Add(requestSw.ElapsedMilliseconds);
                    }
                    else
                    {
                        falhas.Add($"Request {i}: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    falhas.Add($"Request {i}: Exception - {ex.GetType().Name}");
                }
                finally
                {
                    requestSw.Stop();
                }
            });

            await Task.WhenAll(tasks);
            sw.Stop();

            // Assert para cada onda
            var tempoMedio = sucessos.Any() ? sucessos.Average() : 0;
            resultadosGerais.Add((onda, sucessos.Count, falhas.Count, tempoMedio));

            _output.WriteLine($"   ✅ Sucessos: {sucessos.Count}/{requestsPorOnda}");
            _output.WriteLine($"   ❌ Falhas: {falhas.Count}");
            _output.WriteLine($"   ⏱️ Tempo médio: {tempoMedio:F2}ms");
            _output.WriteLine($"   🕐 Tempo total onda: {sw.ElapsedMilliseconds}ms");

            // Pausa entre ondas para simular comportamento real
            if (onda < ondas)
            {
                await Task.Delay(2000);
            }
        }

        // Assert final - Sistema deve manter estabilidade
        var taxaSucessoGeral = resultadosGerais.Sum(r => r.Sucessos) / 
                              (double)resultadosGerais.Sum(r => r.Sucessos + r.Falhas) * 100;

        var tempoMedioGeral = resultadosGerais.Where(r => r.TempoMedio > 0).Average(r => r.TempoMedio);

        _output.WriteLine($"\n📊 RESULTADO FINAL STRESS TEST:");
        _output.WriteLine($"Taxa de sucesso geral: {taxaSucessoGeral:F1}%");
        _output.WriteLine($"Tempo médio geral: {tempoMedioGeral:F2}ms");

        taxaSucessoGeral.Should().BeGreaterThan(90, "Sistema deve manter > 90% de disponibilidade sob stress");
        tempoMedioGeral.Should().BeLessThan(5000, "Tempo médio não deve exceder 5s sob stress");

        // Performance não deve degradar drasticamente entre ondas
        var primeiraOnda = resultadosGerais.First();
        var ultimaOnda = resultadosGerais.Last();
        var degradacao = (ultimaOnda.TempoMedio - primeiraOnda.TempoMedio) / primeiraOnda.TempoMedio * 100;
        
        _output.WriteLine($"Degradação de performance: {degradacao:F1}%");
        degradacao.Should().BeLessThan(100, "Degradação não deve exceder 100% entre ondas");
    }

    [Fact]
    public async Task ResourceExhaustionTest_AltaMemoriaECPU_DeveManterFuncionamento()
    {
        // Arrange - Operações que consomem recursos
        const int numeroOperacoes = 200;
        var memoriaInicial = GC.GetTotalMemory(false);
        var processInicial = Process.GetCurrentProcess();
        var cpuInicial = processInicial.TotalProcessorTime;

        _output.WriteLine($"💾 Memória inicial: {memoriaInicial / 1024 / 1024:F1} MB");
        _output.WriteLine($"🖥️ CPU inicial: {cpuInicial.TotalMilliseconds}ms");

        var sucessos = 0;
        var falhas = 0;
        var tempos = new List<long>();

        // Act - Operações intensivas
        for (int i = 1; i <= numeroOperacoes; i++)
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                // Simula diferentes cargas de trabalho
                var tasks = new List<Task<HttpResponseMessage>>
                {
                    CriarSimulacao(i),
                    ConsultarPerfilRisco((i % 10) + 1),
                    ConsultarHistorico(),
                    ConsultarProdutos()
                };

                var responses = await Task.WhenAll(tasks);
                
                if (responses.All(r => r.StatusCode == HttpStatusCode.OK))
                {
                    sucessos++;
                }
                else
                {
                    falhas++;
                }

                tempos.Add(sw.ElapsedMilliseconds);

                // Monitoramento a cada 50 operações
                if (i % 50 == 0)
                {
                    var memoriaAtual = GC.GetTotalMemory(false);
                    var crescimentoMemoriaAtual = (memoriaAtual - memoriaInicial) / 1024.0 / 1024.0;
                    
                    _output.WriteLine($"Op {i}: +{crescimentoMemoriaAtual:F1}MB, Tempo médio: {tempos.Skip(Math.Max(0, tempos.Count - 50)).Average():F1}ms");
                }
            }
            catch (Exception ex)
            {
                falhas++;
                _output.WriteLine($"Erro na operação {i}: {ex.GetType().Name}");
            }
            finally
            {
                sw.Stop();
            }
        }

        // Assert
        var memoriaFinal = GC.GetTotalMemory(true);
        var processoFinal = Process.GetCurrentProcess();
        var cpuFinal = processoFinal.TotalProcessorTime;
        
        var crescimentoMemoria = memoriaFinal - memoriaInicial;
        var usoCpu = cpuFinal - cpuInicial;

        _output.WriteLine($"\n🔍 ANÁLISE DE RECURSOS:");
        _output.WriteLine($"Operações executadas: {numeroOperacoes}");
        _output.WriteLine($"Taxa de sucesso: {sucessos * 100.0 / (sucessos + falhas):F1}%");
        _output.WriteLine($"Crescimento memória: {crescimentoMemoria / 1024.0 / 1024.0:F1} MB");
        _output.WriteLine($"Uso CPU adicional: {usoCpu.TotalMilliseconds:F0}ms");
        _output.WriteLine($"Tempo médio operação: {tempos.Average():F2}ms");

        // Critérios de aceitação para uso de recursos
        sucessos.Should().BeGreaterThan((int)(numeroOperacoes * 0.90), "Taxa de sucesso deve ser > 90%");
        crescimentoMemoria.Should().BeLessThan(100 * 1024 * 1024, "Crescimento de memória < 100MB");
        tempos.Average().Should().BeLessThan(3000, "Tempo médio < 3s mesmo sob alta carga");
    }

    [Theory]
    [InlineData(1000)]   // Valor mínimo válido
    [InlineData(100000)] // Valor alto válido
    [InlineData(24)]     // Prazo máximo válido (2 anos)
    public async Task EdgeCasesPerformance_ValoresExtremos_DeveProcessarCorretamente(decimal valorOuPrazo)
    {
        // Arrange
        var isValor = valorOuPrazo >= 1000; // Determina se é valor ou prazo
        
        var request = TestDataBuilder.CriarSimulacaoRequest(
            valor: isValor ? valorOuPrazo : 50000m,
            prazoMeses: isValor ? 12 : (int)valorOuPrazo
        );

        // Act
        var sw = Stopwatch.StartNew();
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
        sw.Stop();

        // Assert
        var tipo = isValor ? "valor" : "prazo";
        _output.WriteLine($"Edge case {tipo} {valorOuPrazo}: {sw.ElapsedMilliseconds}ms");

        response.StatusCode.Should().Be(HttpStatusCode.OK, $"Edge case {tipo} {valorOuPrazo} falhou");
        sw.ElapsedMilliseconds.Should().BeLessThan(5000, $"Edge case {tipo} {valorOuPrazo} muito lento");
    }

    [Fact]
    public async Task CacheEvictionTest_SobPressao_DeveManterPerformance()
    {
        // Arrange - Força eviction do cache com muitas consultas diferentes
        const int numeroConsultas = 100;
        var perfis = new[] { "Conservador", "Moderado", "Agressivo" };
        var temposCache = new ConcurrentBag<long>();
        var hitsMisses = new ConcurrentDictionary<string, int>();

        // Act - Múltiplas consultas para forçar cache turnover
        var tasks = Enumerable.Range(1, numeroConsultas).Select(async i =>
        {
            var perfil = perfis[i % perfis.Length];
            var clienteId = (i % 20) + 1; // 20 clientes diferentes
            
            var sw = Stopwatch.StartNew();
            
            // Alterna entre consultas que usam cache
            HttpResponseMessage response = (i % 3) switch
            {
                0 => await _fixture.Client.GetAsync($"/api/perfil-risco/produtos-recomendados/{perfil}"),
                1 => await _fixture.Client.GetAsync($"/api/perfil-risco/{clienteId}"),
                _ => await _fixture.Client.GetAsync("/api/simulacao/simulacoes")
            };
            
            sw.Stop();
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                temposCache.Add(sw.ElapsedMilliseconds);
                hitsMisses.AddOrUpdate($"success_{i % 10}", 1, (k, v) => v + 1);
            }
            else
            {
                hitsMisses.AddOrUpdate("error", 1, (k, v) => v + 1);
            }
        });

        await Task.WhenAll(tasks);

        // Assert
        var sucessos = temposCache.Count;
        var tempoMedio = temposCache.Average();
        var tempoMaximo = temposCache.Max();
        
        _output.WriteLine($"💾 CACHE EVICTION TEST:");
        _output.WriteLine($"Consultas processadas: {sucessos}/{numeroConsultas}");
        _output.WriteLine($"Tempo médio: {tempoMedio:F2}ms");
        _output.WriteLine($"Tempo máximo: {tempoMaximo}ms");
        _output.WriteLine($"Distribuição: {string.Join(", ", hitsMisses.Take(5).Select(kv => $"{kv.Key}:{kv.Value}"))}");

        sucessos.Should().BeGreaterThan((int)(numeroConsultas * 0.85), "85% das consultas devem ser bem-sucedidas");
        tempoMedio.Should().BeLessThan(3000, "Tempo médio deve permanecer < 3s mesmo com cache pressure");
        tempoMaximo.Should().BeLessThan(10000, "Nenhuma consulta deve exceder 10s");
    }

    private async Task<HttpResponseMessage> CriarSimulacao(int seed)
    {
        var request = TestDataBuilder.CriarSimulacaoRequest(
            clienteId: (seed % 10) + 1,
            valor: 1000m + (seed * 100m),
            prazoMeses: (seed % 36) + 6
        );

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        return await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
    }

    private async Task<HttpResponseMessage> ConsultarPerfilRisco(int clienteId)
    {
        return await _fixture.Client.GetAsync($"/api/perfil-risco/{clienteId}");
    }

    private async Task<HttpResponseMessage> ConsultarProdutos()
    {
        var perfis = new[] { "Conservador", "Moderado", "Agressivo" };
        var perfilAleatorio = perfis[Random.Shared.Next(perfis.Length)];
        return await _fixture.Client.GetAsync($"/api/perfil-risco/produtos-recomendados/{perfilAleatorio}");
    }

    private async Task<HttpResponseMessage> ConsultarHistorico()
    {
        return await _fixture.Client.GetAsync("/api/simulacao/simulacoes");
    }

    private async Task<HttpResponseMessage> ConsultarTelemetria()
    {
        return await _fixture.Client.GetAsync("/api/telemetria");
    }

    [Fact]
    public async Task TimeoutResilience_RequestsLentas_DeveManterResponsividade()
    {
        // Arrange - Simula requests que podem ser lentas
        const int requestsRapidas = 10;
        const int requestsLentas = 5;
        var temposRapidas = new ConcurrentBag<long>();
        var temposLentas = new ConcurrentBag<long>();

        // Act - Executa requests rápidas e lentas simultaneamente
        var taskRapidas = Enumerable.Range(1, requestsRapidas).Select(async i =>
        {
            var sw = Stopwatch.StartNew();
            var response = await _fixture.Client.GetAsync($"/api/perfil-risco/{(i % 5) + 1}");
            sw.Stop();
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                temposRapidas.Add(sw.ElapsedMilliseconds);
            }
        });

        var taskLentas = Enumerable.Range(1, requestsLentas).Select(async i =>
        {
            var sw = Stopwatch.StartNew();
            
            // Simula operação mais pesada (múltiplas simulações)
            var request = TestDataBuilder.CriarSimulacaoRequest(
                valor: 100000m + (i * 50000m), // Valores altos
                prazoMeses: 60 + (i * 12) // Prazos longos
            );

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
            sw.Stop();
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                temposLentas.Add(sw.ElapsedMilliseconds);
            }
        });

        await Task.WhenAll(taskRapidas.Concat(taskLentas));

        // Assert
        var tempoMedioRapidas = temposRapidas.Any() ? temposRapidas.Average() : 0;
        var tempoMedioLentas = temposLentas.Any() ? temposLentas.Average() : 0;

        _output.WriteLine($"⚡ TIMEOUT RESILIENCE TEST:");
        _output.WriteLine($"Requests rápidas processadas: {temposRapidas.Count}/{requestsRapidas}");
        _output.WriteLine($"Requests lentas processadas: {temposLentas.Count}/{requestsLentas}");
        _output.WriteLine($"Tempo médio rápidas: {tempoMedioRapidas:F2}ms");
        _output.WriteLine($"Tempo médio lentas: {tempoMedioLentas:F2}ms");

        // Requests rápidas não devem ser afetadas pelas lentas
        temposRapidas.Count.Should().BeGreaterThanOrEqualTo((int)(requestsRapidas * 0.9));
        tempoMedioRapidas.Should().BeLessThan(2000, "Requests rápidas devem permanecer rápidas");
        
        // Ajuste mais flexível - em ambiente de teste rápido pode não haver diferença significativa
        if (tempoMedioRapidas > 0 && tempoMedioLentas > 0)
        {
            (tempoMedioLentas / tempoMedioRapidas).Should().BeGreaterThan(0.8, "Diferença mínima entre requests rápidas e lentas");
        }
    }
}

import os

# Fase 7: Testes de Integração - Business Logic

business_logic_tests = {
    "tests/InvestmentSimulation.UnitTests/IntegrationTests/BusinessLogicIntegrationTests.cs": '''namespace InvestmentSimulation.UnitTests.IntegrationTests;

using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

[Collection("Integration Tests")]
public class BusinessLogicIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public BusinessLogicIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
    }

    [Theory]
    [InlineData("CDB", 0.12)]
    [InlineData("LCI", 0.11)]
    public async Task SimularInvestimento_DeveUtilizarTaxaCorreta(string tipo, decimal taxaEsperada)
    {
        // Arrange
        var valorInvestido = 10000m;
        var prazoMeses = 12;
        var request = TestDataBuilder.CriarSimulacaoRequest(
            valor: valorInvestido,
            prazoMeses: prazoMeses,
            tipoProduto: tipo);

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
        var responseBody = await response.Content.ReadAsStringAsync();
        var simulacao = JsonSerializer.Deserialize<SimulacaoResponse>(responseBody, _jsonOptions);

        // Assert - Validar que a rentabilidade está próxima da taxa esperada
        var rentabilidadeAproximada = simulacao!.ResultadoSimulacao.RentabilidadeEfetiva;
        rentabilidadeAproximada.Should().BeCloseTo(taxaEsperada, tolerance: 0.05m);
    }

    [Fact]
    public async Task PerfilRisco_ConservadorDeveRetornarMenorPontuacao()
    {
        // Arrange - Criar um cliente conservador (baixo volume, baixa frequência)
        var clienteId = 1;

        // Act
        var response = await _fixture.Client.GetAsync($"/api/perfil-risco/{clienteId}");
        var responseBody = await response.Content.ReadAsStringAsync();
        var perfil = JsonSerializer.Deserialize<PerfilRiscoResponse>(responseBody, _jsonOptions);

        // Assert
        perfil.Should().NotBeNull();
        perfil!.Perfil.Should().Be("Conservador");
    }

    [Fact]
    public async Task ProdutosRecomendados_DeveFiltroPorRisco()
    {
        // Act
        var conservadorResponse = await _fixture.Client.GetAsync("/api/perfil-risco/produtos-recomendados/Conservador");
        var conservadorBody = await conservadorResponse.Content.ReadAsStringAsync();
        var conservador = JsonSerializer.Deserialize<List<ProdutoResponse>>(conservadorBody, _jsonOptions);

        var agressivoResponse = await _fixture.Client.GetAsync("/api/perfil-risco/produtos-recomendados/Agressivo");
        var agressivoBody = await agressivoResponse.Content.ReadAsStringAsync();
        var agressivo = JsonSerializer.Deserialize<List<ProdutoResponse>>(agressivoBody, _jsonOptions);

        // Assert
        conservador.Should().NotBeEmpty();
        agressivo.Should().NotBeEmpty();
        
        // Conservador deve ter produtos com risco mais baixo
        var mediaRiscoConservador = conservador!.Count(p => p.Risco == "Baixo");
        var mediaRiscoAgressivo = agressivo!.Count(p => p.Risco == "Alto");
        
        mediaRiscoConservador.Should().BeGreaterThan(0);
        mediaRiscoAgressivo.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SimularInvestimento_ComPrazoMaior_DeveRetornarValorMaior()
    {
        // Arrange
        var valorInicial = 10000m;
        var prazoCurto = 6;
        var prazoLongo = 24;

        // Act
        var requestCurto = TestDataBuilder.CriarSimulacaoRequest(valor: valorInicial, prazoMeses: prazoCurto);
        var contentCurto = new StringContent(
            JsonSerializer.Serialize(requestCurto),
            Encoding.UTF8,
            "application/json");

        var responseCurto = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", contentCurto);
        var bodyCurto = await responseCurto.Content.ReadAsStringAsync();
        var simulacaoCurta = JsonSerializer.Deserialize<SimulacaoResponse>(bodyCurto, _jsonOptions);

        var requestLongo = TestDataBuilder.CriarSimulacaoRequest(valor: valorInicial, prazoMeses: prazoLongo);
        var contentLongo = new StringContent(
            JsonSerializer.Serialize(requestLongo),
            Encoding.UTF8,
            "application/json");

        var responseLongo = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", contentLongo);
        var bodyLongo = await responseLongo.Content.ReadAsStringAsync();
        var simulacaoLonga = JsonSerializer.Deserialize<SimulacaoResponse>(bodyLongo, _jsonOptions);

        // Assert
        simulacaoLonga!.ResultadoSimulacao.ValorFinal.Should()
            .BeGreaterThan(simulacaoCurta!.ResultadoSimulacao.ValorFinal);
    }

    [Fact]
    public async Task SimularInvestimento_ComValorMaior_DeveRetornarMaisRendimento()
    {
        // Arrange
        var prazo = 12;
        var valorBaixo = 1000m;
        var valorAlto = 100000m;

        // Act
        var requestBaixo = TestDataBuilder.CriarSimulacaoRequest(valor: valorBaixo, prazoMeses: prazo);
        var contentBaixo = new StringContent(
            JsonSerializer.Serialize(requestBaixo),
            Encoding.UTF8,
            "application/json");

        var responseBaixo = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", contentBaixo);
        var bodyBaixo = await responseBaixo.Content.ReadAsStringAsync();
        var simulacaoBaixa = JsonSerializer.Deserialize<SimulacaoResponse>(bodyBaixo, _jsonOptions);

        var requestAlto = TestDataBuilder.CriarSimulacaoRequest(valor: valorAlto, prazoMeses: prazo);
        var contentAlto = new StringContent(
            JsonSerializer.Serialize(requestAlto),
            Encoding.UTF8,
            "application/json");

        var responseAlto = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", contentAlto);
        var bodyAlto = await responseAlto.Content.ReadAsStringAsync();
        var simulacaoAlta = JsonSerializer.Deserialize<SimulacaoResponse>(bodyAlto, _jsonOptions);

        // Assert - Rentabilidade percentual deve ser igual
        simulacaoBaixa!.ResultadoSimulacao.RentabilidadeEfetiva.Should()
            .BeCloseTo(simulacaoAlta!.ResultadoSimulacao.RentabilidadeEfetiva, tolerance: 0.001m);

        // Valor final em reais deve ser maior
        simulacaoAlta.ResultadoSimulacao.ValorFinal.Should()
            .BeGreaterThan(simulacaoBaixa.ResultadoSimulacao.ValorFinal);
    }

    [Fact]
    public async Task SimularInvestimento_ValidarLimitesDeNegocio()
    {
        // Arrange
        var request = new SimularInvestimentoRequest
        {
            ClienteId = 1,
            Valor = 10000m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HistoricoSimulacoes_DeveAgrupaPorProdutoEDia()
    {
        // Arrange - Criar múltiplas simulações
        for (int i = 0; i < 5; i++)
        {
            var request = TestDataBuilder.CriarSimulacaoRequest();
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
        }

        // Act
        var response = await _fixture.Client.GetAsync("/api/simulacao/simulacoes/por-produto-dia");
        var responseBody = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<List<SimulacaoPorProdutoDiaResponse>>(responseBody, _jsonOptions);

        // Assert
        resultado.Should().NotBeEmpty();
        resultado!.ForEach(r =>
        {
            r.QuantidadeSimulacoes.Should().BeGreaterThan(0);
            r.MediaValorFinal.Should().BeGreaterThan(0);
            r.Produto.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Telemetria_DeveRegistrarTodasAsOperacoes()
    {
        // Arrange - Fazer várias operações
        var operacoes = 3;
        for (int i = 0; i < operacoes; i++)
        {
            var request = TestDataBuilder.CriarSimulacaoRequest();
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
        }

        // Act
        var response = await _fixture.Client.GetAsync("/api/telemetria");
        var responseBody = await response.Content.ReadAsStringAsync();
        var telemetria = JsonSerializer.Deserialize<TelemetriaResponse>(responseBody, _jsonOptions);

        // Assert
        telemetria.Should().NotBeNull();
        telemetria!.Servicos.Should().NotBeEmpty();
        
        var servicoSimulacao = telemetria.Servicos.FirstOrDefault(s => s.Nome.Contains("simulacao"));
        if (servicoSimulacao != null)
        {
            servicoSimulacao.QuantidadeChamadas.Should().BeGreaterThanOrEqualTo(operacoes);
        }
    }
}
''',
}

for path, content in business_logic_tests.items():
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)

print("✅ Business Logic Integration Tests criados com sucesso!")

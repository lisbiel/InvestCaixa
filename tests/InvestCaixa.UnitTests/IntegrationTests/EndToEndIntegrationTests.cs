namespace InvestCaixa.UnitTests.IntegrationTests;

using FluentAssertions;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.UnitTests.Fixtures;
using InvestCaixa.UnitTests.Helpers;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

[Collection("Integration Tests")]
public class EndToEndIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public EndToEndIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
    }

    [Fact]
    public async Task FluxoCompleto_CriarSimulacaoEConsultarHistorico_DeveFuncionarComSucesso()
    {
        // Arrange
        var simulacoes = new List<SimulacaoResponse>();

        // Act - Passo 1: Criar múltiplas simulações
        for (int i = 0; i < 3; i++)
        {
            var request = TestDataBuilder.CriarSimulacaoRequest(
                valor: 10000m + (i * 1000m),
                tipoProduto: i % 2 == 0 ? "CDB" : "Fundo");

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseBody = await response.Content.ReadAsStringAsync();
            var simulacao = JsonSerializer.Deserialize<SimulacaoResponse>(responseBody, _jsonOptions);
            simulacoes.Add(simulacao!);
        }

        // Act - Passo 2: Consultar histórico
        var historicoResponse = await _fixture.Client.GetAsync("/api/simulacao/simulacoes");
        historicoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var historicoBody = await historicoResponse.Content.ReadAsStringAsync();
        var historico = JsonSerializer.Deserialize<List<SimulacaoHistoricoResponse>>(historicoBody, _jsonOptions);

        // Act - Passo 3: Consultar telemetria
        var telemetriaResponse = await _fixture.Client.GetAsync("/api/telemetria");
        telemetriaResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var telemetriaBody = await telemetriaResponse.Content.ReadAsStringAsync();
        var telemetria = JsonSerializer.Deserialize<TelemetriaResponse>(telemetriaBody, _jsonOptions);

        // Assert
        simulacoes.Should().HaveCount(3);
        historico.Should().NotBeEmpty();
        historico!.Count.Should().BeGreaterThanOrEqualTo(3);
        telemetria.Should().NotBeNull();
        telemetria!.Servicos.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FluxoCompleto_ObterPerfilEProdutosRecomendados_DeveFuncionarComSucesso()
    {
        // Arrange
        var clienteId = 1;

        // Act - Passo 1: Obter perfil de risco
        var perfilResponse = await _fixture.Client.GetAsync($"/api/perfil-risco/{clienteId}");
        perfilResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var perfilBody = await perfilResponse.Content.ReadAsStringAsync();
        var perfil = JsonSerializer.Deserialize<PerfilRiscoResponse>(perfilBody, _jsonOptions);

        // Act - Passo 2: Obter produtos recomendados para o perfil
        var produtosResponse = await _fixture.Client.GetAsync($"/api/perfil-risco/produtos-recomendados/{perfil!.Perfil}");
        produtosResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var produtosBody = await produtosResponse.Content.ReadAsStringAsync();
        var produtos = JsonSerializer.Deserialize<List<ProdutoResponse>>(produtosBody, _jsonOptions);

        // Assert
        perfil.Should().NotBeNull();
        perfil!.ClienteId.Should().Be(clienteId);
        produtos.Should().NotBeEmpty();
        produtos!.All(p => !string.IsNullOrEmpty(p.Nome)).Should().BeTrue();
    }

    [Fact]
    public async Task FluxoCompleto_SimularComDiferentesPrazos_DeveCalcularCorretamente()
    {
        // Arrange
        var prazos = new[] { 6, 12, 24, 36 };
        var resultados = new List<decimal>();

        // Act
        foreach (var prazo in prazos)
        {
            var request = TestDataBuilder.CriarSimulacaoRequest(prazoMeses: prazo);
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            var simulacao = JsonSerializer.Deserialize<SimulacaoResponse>(responseBody, _jsonOptions);

            resultados.Add(simulacao!.ResultadoSimulacao.ValorFinal);
        }

        // Assert
        resultados.Should().HaveCount(4);

        // Verificar que prazos maiores geram valores maiores
        for (int i = 1; i < resultados.Count; i++)
        {
            resultados[i].Should().BeGreaterThanOrEqualTo(resultados[i - 1]);
        }
    }

    [Fact]
    public async Task FluxoCompleto_SimularComValoresVariaveis_DeveRetornarResultadosVariados()
    {
        // Arrange
        var valores = new[] { 1000m, 5000m, 10000m, 50000m };
        var resultados = new List<SimulacaoResponse>();

        // Act
        foreach (var valor in valores)
        {
            var request = TestDataBuilder.CriarSimulacaoRequest(valor: valor);
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            var simulacao = JsonSerializer.Deserialize<SimulacaoResponse>(responseBody, _jsonOptions);

            resultados.Add(simulacao!);
        }

        // Assert
        resultados.Should().HaveCount(4);

        // Verificar que rentabilidade percentual é consistente
        foreach (var resultado in resultados)
        {
            resultado.ResultadoSimulacao.RentabilidadeEfetiva.Should().BeGreaterThan(0);
            resultado.ResultadoSimulacao.RentabilidadeEfetiva.Should().BeLessThan(1);
        }
    }

    [Fact]
    public async Task FluxoCompleto_PersistenciaEmBancoDados_DeveRetornarDados()
    {
        // Arrange
        var valorOriginal = 15000m;
        var request = TestDataBuilder.CriarSimulacaoRequest(valor: valorOriginal);
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act - Criar simulação
        var simulacaoResponse = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
        var simulacaoBody = await simulacaoResponse.Content.ReadAsStringAsync();
        var simulacao = JsonSerializer.Deserialize<SimulacaoResponse>(simulacaoBody, _jsonOptions);

        // Act - Recuperar do histórico
        var historicoResponse = await _fixture.Client.GetAsync("/api/simulacao/simulacoes");
        var historicoBody = await historicoResponse.Content.ReadAsStringAsync();
        var historico = JsonSerializer.Deserialize<List<SimulacaoHistoricoResponse>>(historicoBody, _jsonOptions);

        // Assert
        historico.Should().NotBeEmpty();

        var simulacaoPersistida = historico!.FirstOrDefault(s => 
            Math.Abs(s.ValorInvestido - valorOriginal) < 0.01m);

        simulacaoPersistida.Should().NotBeNull();
    }

    [Fact]
    public async Task FluxoCompleto_MultiplosCLientes_DeveMantenerDadosSeparados()
    {
        // Arrange - Criar simulações para diferentes clientes
        var clienteIds = new[] { 1, 2 };

        // Act
        foreach (var clienteId in clienteIds)
        {
            var request = TestDataBuilder.CriarSimulacaoRequest(clienteId: clienteId);
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert - Verificar que ambos foram salvos
        var historicoResponse = await _fixture.Client.GetAsync("/api/simulacao/simulacoes");
        var historicoBody = await historicoResponse.Content.ReadAsStringAsync();
        var historico = JsonSerializer.Deserialize<List<SimulacaoHistoricoResponse>>(historicoBody, _jsonOptions);

        historico.Should().NotBeEmpty();
        historico!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task FluxoCompleto_ValidarTodosCamposResponse_DevemEstarPopulados()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest();
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
        var responseBody = await response.Content.ReadAsStringAsync();
        var simulacao = JsonSerializer.Deserialize<SimulacaoResponse>(responseBody, _jsonOptions);

        // Assert - Validar todos os campos
        simulacao.Should().NotBeNull();
        simulacao!.ProdutoValidado.Should().NotBeNull();
        simulacao.ProdutoValidado.Id.Should().NotBe(Guid.Empty);
        simulacao.ProdutoValidado.Nome.Should().NotBeNullOrEmpty();
        simulacao.ProdutoValidado.Tipo.Should().NotBeNullOrEmpty();
        simulacao.ProdutoValidado.Rentabilidade.Should().BeGreaterThan(0);
        simulacao.ProdutoValidado.Risco.Should().NotBeNullOrEmpty();

        simulacao.ResultadoSimulacao.Should().NotBeNull();
        simulacao.ResultadoSimulacao.ValorFinal.Should().BeGreaterThan(0);
        simulacao.ResultadoSimulacao.RentabilidadeEfetiva.Should().BeGreaterThan(0);
        simulacao.ResultadoSimulacao.PrazoMeses.Should().BeGreaterThan(0);

        simulacao.DataSimulacao.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
    }
}

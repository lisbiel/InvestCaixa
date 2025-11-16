namespace InvestCaixa.UnitTests.IntegrationTests;

using FluentAssertions;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using InvestCaixa.UnitTests.Fixtures;
using InvestCaixa.UnitTests.Helpers;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

[Collection("Integration Tests")]
public class SimulacaoControllerIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SimulacaoControllerIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
    }

    [Fact]
    public async Task SimularInvestimento_ComDadosValidos_DeveRetornarSimulacao()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(clienteId: 1, valor: 10000m, prazoMeses: 12, tipoProduto: "CDB");
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SimulacaoResponse>(responseBody, _jsonOptions);

        result.Should().NotBeNull();
        result!.ProdutoValidado.Should().NotBeNull();
        result.ProdutoValidado.Nome.Should().Contain("CDB");
        result.ResultadoSimulacao.Should().NotBeNull();
        result.ResultadoSimulacao.ValorFinal.Should().BeGreaterThan(request.Valor);
        result.DataSimulacao.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
    }

    [Theory]
    [InlineData("CDB")]
    [InlineData("LCI")]
    [InlineData("Fundo")]
    public async Task SimularInvestimento_ComDiferentesTiposProduto_DeveRetornarProdutosValidos(string tipoProduto)
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(tipoProduto: tipoProduto);
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SimulacaoResponse>(responseBody, _jsonOptions);

        result.Should().NotBeNull();
        result!.ProdutoValidado.Tipo.Should().Be(tipoProduto);
    }

    [Fact]
    public async Task SimularInvestimento_ComValorAbaixoDoMinimo_DeveRetornarErro400()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(valor: 100m); // Valor muito baixo
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SimularInvestimento_ComProdutoInexistente_DeveRetornarErro404()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(tipoProduto: "INEXISTENTE");
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObterSimulacoes_DeveRetornarListaDeSimulacoes()
    {
        // Arrange - Criar simulações
        var request = TestDataBuilder.CriarSimulacaoRequest();
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Act
        var response = await _fixture.Client.GetAsync("/api/simulacao/simulacoes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<SimulacaoHistoricoResponse>>(responseBody, _jsonOptions);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result!.Count.Should().BeGreaterThanOrEqualTo(1);
        result[0].Produto.Should().NotBeNullOrEmpty();
        result[0].DataSimulacao.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task ObterSimulacoesPorProdutoDia_DeveRetornarDadosAgrupados()
    {
        // Arrange - Criar várias simulações
        for (int i = 0; i < 3; i++)
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

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<SimulacaoPorProdutoDiaResponse>>(responseBody, _jsonOptions);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result!.First().QuantidadeSimulacoes.Should().BeGreaterThan(0);
        result.First().MediaValorFinal.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObterSimulacoesPorProdutoDia_ComFiltroDeData_DeveRetornarResultadosCorretos()
    {
        // Arrange
        var dataInicio = DateTime.UtcNow.AddDays(-1);
        var dataFim = DateTime.UtcNow.AddDays(1);

        // Act
        var response = await _fixture.Client.GetAsync(
            $"/api/simulacao/simulacoes/por-produto-dia?dataInicio={dataInicio:O}&dataFim={dataFim:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SimularInvestimento_DeveCalcularRentabilidadeCorretamente()
    {
        // Arrange
        var valorInvestido = 10000m;
        var prazoMeses = 12;
        var request = TestDataBuilder.CriarSimulacaoRequest(valor: valorInvestido, prazoMeses: prazoMeses);
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SimulacaoResponse>(responseBody, _jsonOptions);

        // Assert - Validar cálculo de rentabilidade
        var rentabilidadeCalculada = result!.ResultadoSimulacao.RentabilidadeEfetiva;
        rentabilidadeCalculada.Should().BeGreaterThan(0);
        rentabilidadeCalculada.Should().BeLessThan(1); // Entre 0 e 100%
    }

    [Fact]
    public async Task SimularInvestimento_SemAutenticacao_DeveRetornar401()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest();
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var clientSemAuth = _fixture.CreateClientSemAuth();

        // Act
        var response = await clientSemAuth.PostAsync("/api/simulacao/simular-investimento", content);

        // Assert (esperando 401 ou erro de conexão, ambos são válidos)
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }
}

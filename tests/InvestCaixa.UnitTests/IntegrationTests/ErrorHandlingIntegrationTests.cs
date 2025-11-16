namespace InvestCaixa.UnitTests.IntegrationTests;

using FluentAssertions;
using InvestCaixa.UnitTests.Fixtures;
using InvestCaixa.UnitTests.Helpers;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

[Collection("Integration Tests")]
public class ErrorHandlingIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ErrorHandlingIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
    }

    [Fact]
    public async Task SimularInvestimento_ComValorZero_DeveRetornarErro400()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(valor: 0m);
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
    public async Task SimularInvestimento_ComValorNegativo_DeveRetornarErro400()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(valor: -5000m);
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
    public async Task SimularInvestimento_ComPrazoZero_DeveRetornarErro400()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(prazoMeses: 0);
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
    public async Task SimularInvestimento_ComClienteInvalido_DeveRetornarErro400()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(clienteId: -1);
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
    public async Task SimularInvestimento_ComJsonInvalido_DeveRetornarErro400()
    {
        // Arrange
        var jsonInvalido = "{ json invalido }";
        var content = new StringContent(jsonInvalido, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObterPerfilRisco_ComClienteInexistente_DeveRetornarErro404()
    {
        // Arrange
        var clienteInexistente = 999999;

        // Act
        var response = await _fixture.Client.GetAsync($"/api/perfil-risco/{clienteInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Endpoint_SemAutenticacao_DeveRetornarErro401()
    {
        // Arrange
        var clientSemAuth = _fixture.CreateClientSemAuth();

        // Act
        var response = await clientSemAuth.GetAsync("/api/simulacao/simulacoes");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_ComAutenticacaoInvalida_DeveRetornarErro401()
    {
        // Arrange
        var client = _fixture.CreateClientSemAuth();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid_token_123");

        // Act
        var response = await client.GetAsync("/api/simulacao/simulacoes");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_InvalidoDeveRetornarErro404()
    {
        // Act
        var response = await _fixture.Client.GetAsync("/api/endpoint-inexistente/nao-existe");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SimularInvestimento_DeveRetornarProblemDetailsComErro()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(tipoProduto: "TIPO_INEXISTENTE");
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        responseBody.Should().ContainAny("type", "title", "detail");
    }

    [Fact]
    public async Task SimularInvestimento_ComPrazoMuitoAlto_DeveRetornarErro400()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(prazoMeses: 500); // Acima do máximo
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
    public async Task SimularInvestimento_ComValorMuitoAlto_DeveRetornarErro400()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(valor: 100_000_000_000m); // Muito alto
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
    public async Task ErrorResponse_DeveConterInformacoesUteis()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest(valor: 0m);
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        responseBody.Should().NotBeEmpty();
    }
}

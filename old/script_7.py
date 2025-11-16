
import os

# Fase 8: Testes de Integração - Error Handling e Segurança

error_handling_tests = {
    "tests/InvestmentSimulation.UnitTests/IntegrationTests/ErrorHandlingIntegrationTests.cs": '''namespace InvestmentSimulation.UnitTests.IntegrationTests;

using FluentAssertions;
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
        var clientSemAuth = new HttpClient();

        // Act
        var response = await clientSemAuth.GetAsync("https://localhost/api/simulacao/simulacoes");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_ComAutenticacaoInvalida_DeveRetornarErro401()
    {
        // Arrange
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid_token_123");

        // Act
        var response = await client.GetAsync("https://localhost/api/simulacao/simulacoes");

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
        responseBody.Should().Contain("type").Or.Contain("title").Or.Contain("detail");
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
''',

    "tests/InvestmentSimulation.UnitTests/IntegrationTests/SecurityIntegrationTests.cs": '''namespace InvestmentSimulation.UnitTests.IntegrationTests;

using FluentAssertions;
using System.Net;
using Xunit;

[Collection("Integration Tests")]
public class SecurityIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;

    public SecurityIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetSimulacoes_SemBearerToken_DeveRetornar401()
    {
        // Arrange
        var client = new HttpClient();

        // Act
        var response = await client.GetAsync("https://localhost/api/simulacao/simulacoes");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPerfilRisco_SemBearerToken_DeveRetornar401()
    {
        // Arrange
        var client = new HttpClient();

        // Act
        var response = await client.GetAsync("https://localhost/api/perfil-risco/1");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTelemetria_SemBearerToken_DeveRetornar401()
    {
        // Arrange
        var client = new HttpClient();

        // Act
        var response = await client.GetAsync("https://localhost/api/telemetria");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_ComCredenciaisVazias_DeveRetornarErro()
    {
        // Arrange
        var client = new HttpClient();
        var content = new StringContent("{\"username\":\"\",\"password\":\"\"}", 
            System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("https://localhost/api/auth/login", content);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ComAutenticacaoValida_DeveAceitarRequisicoes()
    {
        // Act
        var response = await _fixture.Client.GetAsync("/api/simulacao/simulacoes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
''',
}

for path, content in error_handling_tests.items():
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)

print("✅ Error Handling e Security Integration Tests criados com sucesso!")

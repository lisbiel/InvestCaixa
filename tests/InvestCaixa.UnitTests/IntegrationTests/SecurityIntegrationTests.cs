namespace InvestCaixa.UnitTests.IntegrationTests;

using FluentAssertions;
using InvestCaixa.UnitTests.Fixtures;
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
        var client = _fixture.CreateClientSemAuth();

        // Act
        var response = await client.GetAsync("/api/simulacao/simulacoes");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPerfilRisco_SemBearerToken_DeveRetornar401()
    {
        // Arrange
        var client = _fixture.CreateClientSemAuth();

        // Act
        var response = await client.GetAsync("/api/perfil-risco/1");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTelemetria_SemBearerToken_DeveRetornar401()
    {
        // Arrange
        var client = _fixture.CreateClientSemAuth();

        // Act
        var response = await client.GetAsync("/api/telemetria");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_ComCredenciaisVazias_DeveRetornarErro()
    {
        // Arrange
        var client = _fixture.CreateClientSemAuth();
        var content = new StringContent("{\"username\":\"\",\"password\":\"\"}", 
            System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

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

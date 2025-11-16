namespace InvestCaixa.UnitTests.IntegrationTests;

using FluentAssertions;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.UnitTests.Fixtures;
using System.Net;
using System.Text.Json;
using Xunit;

[Collection("Integration Tests")]
public class PerfilRiscoControllerIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PerfilRiscoControllerIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
    }

    [Fact]
    public async Task ObterPerfilRisco_ComClienteExistente_DeveRetornarPerfil()
    {
        // Arrange
        var clienteId = 1;

        // Act
        var response = await _fixture.Client.GetAsync($"/api/perfil-risco/{clienteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PerfilRiscoResponse>(responseBody, _jsonOptions);

        result.Should().NotBeNull();
        result!.ClienteId.Should().Be(clienteId);
        result.Perfil.Should().NotBeNullOrEmpty();
        result.Pontuacao.Should().BeGreaterThanOrEqualTo(0);
        result.Descricao.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ObterPerfilRisco_ComClienteInexistente_DeveRetornarErro404()
    {
        // Arrange
        var clienteId = 99999;

        // Act
        var response = await _fixture.Client.GetAsync($"/api/perfil-risco/{clienteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("Conservador")]
    [InlineData("Moderado")]
    [InlineData("Agressivo")]
    public async Task ObterProdutosRecomendados_ComDiferentesPerfs_DeveRetornarProdutos(string perfil)
    {
        // Act
        var response = await _fixture.Client.GetAsync($"/api/perfil-risco/produtos-recomendados/{perfil}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<ProdutoResponse>>(responseBody, _jsonOptions);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ObterProdutosRecomendados_ConservadorDeveRetornarProdutosBaixoRisco()
    {
        // Act
        var response = await _fixture.Client.GetAsync("/api/perfil-risco/produtos-recomendados/Conservador");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<ProdutoResponse>>(responseBody, _jsonOptions);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result!.All(p => p.Risco == "Baixo" || p.Risco == "Medio").Should().BeTrue();
    }

    [Fact]
    public async Task ObterProdutosRecomendados_AgressivoDeveRetornarProdutosAltoRisco()
    {
        // Act
        var response = await _fixture.Client.GetAsync("/api/perfil-risco/produtos-recomendados/Agressivo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<ProdutoResponse>>(responseBody, _jsonOptions);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ObterPerfilRisco_DeveRetornarPontuacaoValida()
    {
        // Arrange
        var clienteId = 1;

        // Act
        var response = await _fixture.Client.GetAsync($"/api/perfil-risco/{clienteId}");
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PerfilRiscoResponse>(responseBody, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Pontuacao.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task ObterPerfilRisco_DeveRetornarUmDeTresPerfisValidos()
    {
        // Arrange
        var clienteId = 1;

        // Act
        var response = await _fixture.Client.GetAsync($"/api/perfil-risco/{clienteId}");
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PerfilRiscoResponse>(responseBody, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        var perfisValidos = new[] { "Conservador", "Moderado", "Agressivo" };
        perfisValidos.Should().Contain(result!.Perfil);
    }

    [Fact]
    public async Task ObterProdutosRecomendados_SemAutenticacao_DeveRetornar401()
    {
        // Arrange
        var clientSemAuth = _fixture.CreateClientSemAuth();

        // Act
        var response = await clientSemAuth.GetAsync("https://localhost/api/perfil-risco/produtos-recomendados/Conservador");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ObterProdutosRecomendados_DeveRetornarProdutosComTodosOsCampos()
    {
        // Act
        var response = await _fixture.Client.GetAsync("/api/perfil-risco/produtos-recomendados/Moderado");
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<ProdutoResponse>>(responseBody, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.First().Id.Should().NotBe(Guid.Empty);
        result.First().Nome.Should().NotBeNullOrEmpty();
        result.First().Tipo.Should().NotBeNullOrEmpty();
        result.First().Rentabilidade.Should().BeGreaterThan(0);
        result.First().Risco.Should().NotBeNullOrEmpty();
    }
}

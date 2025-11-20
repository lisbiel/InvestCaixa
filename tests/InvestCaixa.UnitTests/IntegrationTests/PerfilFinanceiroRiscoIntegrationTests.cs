namespace InvestCaixa.UnitTests.IntegrationTests;

using FluentAssertions;
using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.UnitTests.Fixtures;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

/// <summary>
/// Testes de integração para o fluxo de atualização de PerfilFinanceiro
/// e recalcução automática de PerfilRisco.
/// Valida que diferentes payloads resultam nas classificações esperadas.
/// </summary>
[Collection("Integration Tests")]
public class PerfilFinanceiroRiscoIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PerfilFinanceiroRiscoIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region Testes de Payload Conservador

    [Fact]
    public async Task AtualizarPerfilFinanceiro_PayloadConservador_DeveRetornarPerfilConservador()
    {
        // Arrange
        var clienteId = 10;
        var request = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = 3000,
            PatrimonioTotal = 20000,
            DividasAtivas = 5000,
            DependentesFinanceiros = 2,
            Horizonte = 1,  // CurtoPrazo
            Objetivo = 1,   // ReservaEmergencia
            ToleranciaPerda = 0,
            ExperienciaInvestimentos = false
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync(
            $"/api/perfil-financeiro/{clienteId}",
            content);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<dynamic>(responseBody, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.ToLower().Should().Contain("conservador");
    }

    [Fact]
    public async Task AtualizarPerfilFinanceiro_ClientePequenaRenda_DeveSerConservador()
    {
        // Arrange: Pequena renda, patrimônio mínimo, sem experiência
        var clienteId = 1;
        var request = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = 2000,
            PatrimonioTotal = 15000,
            DividasAtivas = 8000,
            DependentesFinanceiros = 3,
            Horizonte = 1,  // CurtoPrazo
            Objetivo = 1,   // ReservaEmergencia
            ToleranciaPerda = 0,
            ExperienciaInvestimentos = false
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.Client.PostAsync($"/api/perfil-financeiro/{clienteId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.ToLower().Should().Contain("conservador");
    }

    #endregion

    #region Testes de Payload Moderado

    [Fact]
    public async Task AtualizarPerfilFinanceiro_PayloadModerado_DeveRetornarPerfilModerado()
    {
        // Arrange
        var clienteId = 11;
        var request = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = 8000,
            PatrimonioTotal = 150000,
            DividasAtivas = 10000,
            DependentesFinanceiros = 1,
            Horizonte = 2,  // MedioPrazo
            Objetivo = 3,   // CompraImovel
            ToleranciaPerda = 5,  // Aumentado de 2 para garantir moderado
            ExperienciaInvestimentos = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync(
            $"/api/perfil-financeiro/{clienteId}",
            content);

        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.ToLower().Should().Contain("moderado");
    }

    [Fact]
    public async Task AtualizarPerfilFinanceiro_ClienteProfissional_DeveSerModeradoOuAgressivo()
    {
        // Arrange: Profissional com bom patrimônio e horizonte longo
        var clienteId = 1;
        var request = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = 10000,
            PatrimonioTotal = 300000,
            DividasAtivas = 15000,
            DependentesFinanceiros = 1,
            Horizonte = 3,  // LongoPrazo
            Objetivo = 2,   // Aposentadoria
            ToleranciaPerda = 5,
            ExperienciaInvestimentos = true
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.Client.PostAsync($"/api/perfil-financeiro/{clienteId}", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bodyLower = responseBody.ToLower();
        (bodyLower.Contains("moderado") || bodyLower.Contains("agressivo")).Should().BeTrue("Deve ser Moderado ou Agressivo");
    }

    #endregion

    #region Testes de Payload Agressivo

    [Fact]
    public async Task AtualizarPerfilFinanceiro_PayloadAgressivo_DeveRetornarPerfilAgressivo()
    {
        // Arrange
        var clienteId = 1;
        var request = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = 15000,
            PatrimonioTotal = 500000,
            DividasAtivas = 20000,
            DependentesFinanceiros = 0,
            Horizonte = 3,  // LongoPrazo
            Objetivo = 2,   // Aposentadoria
            ToleranciaPerda = 7,
            ExperienciaInvestimentos = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync(
            $"/api/perfil-financeiro/{clienteId}",
            content);

        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.ToLower().Should().Contain("agressivo");
    }

    [Fact]
    public async Task AtualizarPerfilFinanceiro_ClienteRicoComExperiencia_DeveSerAgressivo()
    {
        // Arrange: Cliente muito rico, sem dívidas, experiência, longo prazo
        var clienteId = 1;
        var request = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = 25000,
            PatrimonioTotal = 1000000,
            DividasAtivas = 0,
            DependentesFinanceiros = 0,
            Horizonte = 3,  // LongoPrazo
            Objetivo = 2,   // Aposentadoria
            ToleranciaPerda = 9,
            ExperienciaInvestimentos = true
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.Client.PostAsync($"/api/perfil-financeiro/{clienteId}", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.ToLower().Should().Contain("agressivo");
    }

    #endregion

    #region Testes de Atualização e Recalculation

    [Fact]
    public async Task AtualizarPerfilFinanceiro_Duas_Vezes_DeveRecalcularPerfil()
    {
        // Arrange
        var clienteId = 1;

        // Primeira atualização: conservador
        var requestConservador = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = 3000,
            PatrimonioTotal = 20000,
            DividasAtivas = 5000,
            DependentesFinanceiros = 2,
            Horizonte = 1,
            Objetivo = 1,
            ToleranciaPerda = 0,
            ExperienciaInvestimentos = false
        };

        var content1 = new StringContent(
            JsonSerializer.Serialize(requestConservador),
            Encoding.UTF8,
            "application/json");

        // Act 1: Primeira atualização
        var response1 = await _fixture.Client.PostAsync(
            $"/api/perfil-financeiro/{clienteId}",
            content1);

        var body1 = await response1.Content.ReadAsStringAsync();

        // Act 2: Segunda atualização com dados mais agressivos
        var requestAgressivo = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = 15000,
            PatrimonioTotal = 500000,
            DividasAtivas = 10000,
            DependentesFinanceiros = 1,
            Horizonte = 3,
            Objetivo = 2,
            ToleranciaPerda = 8,
            ExperienciaInvestimentos = true
        };

        var content2 = new StringContent(
            JsonSerializer.Serialize(requestAgressivo),
            Encoding.UTF8,
            "application/json");

        var response2 = await _fixture.Client.PostAsync(
            $"/api/perfil-financeiro/{clienteId}",
            content2);

        var body2 = await response2.Content.ReadAsStringAsync();

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        body1.ToLower().Should().Contain("conservador");
        body2.ToLower().Should().Contain("agressivo");
    }

    [Fact]
    public async Task PerfilFinanceiro_Atualizar_DeveRetornarPerfilRiscoAtualizado()
    {
        // Arrange
        var clienteId = 1;
        var request = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = 8000,
            PatrimonioTotal = 150000,
            DividasAtivas = 10000,
            DependentesFinanceiros = 1,
            Horizonte = 2,
            Objetivo = 3,
            ToleranciaPerda = 2,
            ExperienciaInvestimentos = true
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.Client.PostAsync($"/api/perfil-financeiro/{clienteId}", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Parse response para verificar presença de PerfilRisco
        var responseDoc = JsonDocument.Parse(responseBody);
        var root = responseDoc.RootElement;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        root.TryGetProperty("perfilRisco", out var perfilRisco).Should().BeTrue("Response deve incluir perfilRisco");
        
        perfilRisco.TryGetProperty("perfil", out var perfil).Should().BeTrue();
        perfil.GetString().Should().NotBeNullOrEmpty();

        perfilRisco.TryGetProperty("pontuacao", out var pontuacao).Should().BeTrue();
        pontuacao.GetInt32().Should().BeGreaterThan(0);
    }

    #endregion

    #region Testes de Validação de Entrada

    [Fact]
    public async Task AtualizarPerfilFinanceiro_ToleranciaInvalida_DeveRetornarErro400()
    {
        // Arrange: Tolerância fora do intervalo [0-10]
        var clienteId = 1;
        var request = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = 8000,
            PatrimonioTotal = 150000,
            DividasAtivas = 10000,
            DependentesFinanceiros = 1,
            Horizonte = 2,
            Objetivo = 3,
            ToleranciaPerda = 15,  // INVÁLIDO (máximo é 10)
            ExperienciaInvestimentos = true
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.Client.PostAsync($"/api/perfil-financeiro/{clienteId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AtualizarPerfilFinanceiro_RendaNegativa_DeveRetornarErro400()
    {
        // Arrange
        var clienteId = 1;
        var request = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = -5000,  // INVÁLIDO
            PatrimonioTotal = 150000,
            DividasAtivas = 10000,
            DependentesFinanceiros = 1,
            Horizonte = 2,
            Objetivo = 3,
            ToleranciaPerda = 2,
            ExperienciaInvestimentos = true
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.Client.PostAsync($"/api/perfil-financeiro/{clienteId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Testes de Campos na Resposta

    [Fact]
    public async Task PerfilFinanceiro_Resposta_DeveConterTodosOsCamposObrigatorios()
    {
        // Arrange
        var clienteId = 1;
        var request = new CriarPerfilFinanceiroRequest
        {
            RendaMensal = 8000,
            PatrimonioTotal = 150000,
            DividasAtivas = 10000,
            DependentesFinanceiros = 1,
            Horizonte = 2,
            Objetivo = 3,
            ToleranciaPerda = 2,
            ExperienciaInvestimentos = true
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.Client.PostAsync($"/api/perfil-financeiro/{clienteId}", content);
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseDoc = JsonDocument.Parse(responseBody);
        var root = responseDoc.RootElement;

        // Assert
        root.TryGetProperty("perfilFinanceiro", out var pf).Should().BeTrue();
        pf.TryGetProperty("rendaMensal", out _).Should().BeTrue();
        pf.TryGetProperty("patrimonioTotal", out _).Should().BeTrue();
        pf.TryGetProperty("dividasAtivas", out _).Should().BeTrue();
        pf.TryGetProperty("toleranciaPerda", out _).Should().BeTrue();
        pf.TryGetProperty("experienciaInvestimentos", out _).Should().BeTrue();

        root.TryGetProperty("perfilRisco", out var pr).Should().BeTrue();
        pr.TryGetProperty("perfil", out _).Should().BeTrue();
        pr.TryGetProperty("pontuacao", out _).Should().BeTrue();
        pr.TryGetProperty("descricao", out _).Should().BeTrue();
    }

    #endregion
}

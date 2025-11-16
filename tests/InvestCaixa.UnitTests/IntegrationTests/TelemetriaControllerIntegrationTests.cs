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
public class TelemetriaControllerIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TelemetriaControllerIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
    }

    [Fact]
    public async Task ObterTelemetria_DeveRetornarDadosValidos()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest();
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Fazer uma chamada para gerar telemetria
        await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Act
        var response = await _fixture.Client.GetAsync("/api/telemetria");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TelemetriaResponse>(responseBody, _jsonOptions);

        result.Should().NotBeNull();
        result!.Servicos.Should().NotBeNull();
        result.Periodo.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterTelemetria_DeveRetornarPeriodo()
    {
        // Act
        var response = await _fixture.Client.GetAsync("/api/telemetria");
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TelemetriaResponse>(responseBody, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Periodo.Inicio.Should().BeBefore(result.Periodo.Fim);
    }

    [Fact]
    public async Task ObterTelemetria_ComMultiplasRequisicoes_DeveRetornarEstatisticas()
    {
        // Arrange - Fazer múltiplas requisições
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
        var response = await _fixture.Client.GetAsync("/api/telemetria");
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TelemetriaResponse>(responseBody, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Servicos.Should().NotBeEmpty();

        var simulacaoService = result.Servicos.FirstOrDefault(s => s.Nome.Contains("simulacao"));
        if (simulacaoService != null)
        {
            simulacaoService.QuantidadeChamadas.Should().BeGreaterThan(0);
            simulacaoService.MediaTempoRespostaMs.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task ObterTelemetria_ComFiltroDeData_DeveRetornarResultadosCorretos()
    {
        // Arrange
        var dataInicio = DateTime.UtcNow.AddDays(-30);
        var dataFim = DateTime.UtcNow.AddDays(1);

        // Fazer uma chamada para gerar telemetria
        var simulacaoRequest = TestDataBuilder.CriarSimulacaoRequest();
        var content = new StringContent(
            JsonSerializer.Serialize(simulacaoRequest),
            Encoding.UTF8,
            "application/json");

        await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Act
        var response = await _fixture.Client.GetAsync(
            $"/api/telemetria?dataInicio={dataInicio:O}&dataFim={dataFim:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TelemetriaResponse>(responseBody, _jsonOptions);

        result.Should().NotBeNull();
        result!.Periodo.Inicio.Should().BeOnOrBefore(DateTime.UtcNow);
        result.Periodo.Fim.Should().BeOnOrAfter(DateTime.UtcNow.AddDays(-30));
    }

    [Fact]
    public async Task ObterTelemetria_DeveRetornarTempoMedioPositivo()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest();
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Act
        var response = await _fixture.Client.GetAsync("/api/telemetria");
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TelemetriaResponse>(responseBody, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Servicos.Should().NotBeEmpty();
        result.Servicos.All(s => s.MediaTempoRespostaMs >= 0).Should().BeTrue();
    }

    [Fact]
    public async Task ObterTelemetria_SemAutenticacao_DeveRetornar401()
    {
        // Arrange
        var clientSemAuth = _fixture.CreateClientSemAuth();

        // Act
        var response = await clientSemAuth.GetAsync("/api/telemetria");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ObterTelemetria_DeveRetornarServicoComNome()
    {
        // Arrange
        var request = TestDataBuilder.CriarSimulacaoRequest();
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Act
        var response = await _fixture.Client.GetAsync("/api/telemetria");
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TelemetriaResponse>(responseBody, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Servicos.All(s => !string.IsNullOrEmpty(s.Nome)).Should().BeTrue();
    }
}

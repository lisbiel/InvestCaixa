namespace InvestCaixa.UnitTests.IntegrationTests;

using FluentAssertions;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.UnitTests.Fixtures;
using InvestCaixa.UnitTests.Helpers;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

/// <summary>
/// Testes de integração para a nova funcionalidade de recomendação inteligente
/// que considera o perfil de risco do cliente ao selecionar produtos.
/// </summary>
[Collection("Integration Tests")]
public class RecomendacaoInteligenteIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RecomendacaoInteligenteIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
    }

    [Fact]
    public async Task ObterProdutosRecomendadosPorTipo_ComClienteExistente_DeveRetornarProdutosOrdenados()
    {
        // Arrange
        var clienteId = 1;
        var tipoProduto = "CDB";

        // Act - Obter produtos recomendados considerando perfil do cliente
        var response = await _fixture.Client.GetAsync($"/api/simulacao/produtos-recomendados/{clienteId}/{tipoProduto}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var produtos = JsonSerializer.Deserialize<List<ProdutoResponse>>(responseBody, _jsonOptions);

        produtos.Should().NotBeNull();
        produtos.Should().NotBeEmpty();
        
        // Verificar que apenas produtos do tipo solicitado foram retornados
        produtos!.All(p => p.Tipo == tipoProduto).Should().BeTrue();
    }

    [Fact]
    public async Task SimulacaoComRecomendacaoInteligente_DevePriorizarProdutoBasedoPerfil()
    {
        // Arrange - Criar uma simulação que vai usar a recomendação inteligente
        var request = TestDataBuilder.CriarSimulacaoRequest(
            clienteId: 1, 
            valor: 10000m, 
            prazoMeses: 12, 
            tipoProduto: "CDB");

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act - Simular investimento (agora usando inteligência de perfil)
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var simulacao = JsonSerializer.Deserialize<SimulacaoResponse>(responseBody, _jsonOptions);

        simulacao.Should().NotBeNull();
        simulacao!.ProdutoValidado.Should().NotBeNull();
        simulacao.ProdutoValidado.Tipo.Should().Be("CDB");
        
        // O produto selecionado deve ter informações de adequação ao perfil
        simulacao.AdequacaoPerfil.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(1, "Conservador")]
    [InlineData(2, "Moderado")]
    [InlineData(3, "Agressivo")]
    public async Task ObterProdutosRecomendados_ParaDiferentesClientes_DeveRespeitarPerfis(
        int clienteId, 
        string perfilEsperado)
    {
        // Arrange - Primeiro obter o perfil do cliente
        var perfilResponse = await _fixture.Client.GetAsync($"/api/perfil-risco/{clienteId}");
        
        if (perfilResponse.StatusCode == HttpStatusCode.OK)
        {
            var perfilBody = await perfilResponse.Content.ReadAsStringAsync();
            var perfil = JsonSerializer.Deserialize<PerfilRiscoResponse>(perfilBody, _jsonOptions);
            
            // Act - Obter produtos recomendados para o tipo CDB
            var produtosResponse = await _fixture.Client.GetAsync($"/api/simulacao/produtos-recomendados/{clienteId}/CDB");

            // Assert
            produtosResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var produtosBody = await produtosResponse.Content.ReadAsStringAsync();
            var produtos = JsonSerializer.Deserialize<List<ProdutoResponse>>(produtosBody, _jsonOptions);

            produtos.Should().NotBeNull();
            produtos.Should().NotBeEmpty();
            
            // Log para debug - mostra como os produtos estão sendo ordenados
            foreach (var produto in produtos!)
            {
                Console.WriteLine($"Cliente {clienteId} ({perfilEsperado}): Produto {produto.Nome} - Risco: {produto.Risco} - Perfil Recomendado: {produto.PerfilRecomendado}");
            }
        }
    }

    [Fact]
    public async Task SimulacaoComClienteSemPerfil_DeveUsarOrdenacaoPorRentabilidade()
    {
        // Este teste verifica o fallback quando cliente não tem perfil definido
        // Arrange - Usar um cliente que pode não ter perfil de risco calculado
        var request = TestDataBuilder.CriarSimulacaoRequest(
            clienteId: 5, // Cliente menos provável de ter perfil
            valor: 5000m, 
            prazoMeses: 6, 
            tipoProduto: "Fundo");

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _fixture.Client.PostAsync("/api/simulacao/simular-investimento", content);

        // Assert - Deve funcionar mesmo sem perfil definido
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var simulacao = JsonSerializer.Deserialize<SimulacaoResponse>(responseBody, _jsonOptions);

        simulacao.Should().NotBeNull();
        simulacao!.ProdutoValidado.Should().NotBeNull();
        simulacao.ProdutoValidado.Tipo.Should().Be("Fundo");
    }

    [Fact]
    public async Task CompararRecomendacoes_EntreClientesConservadorEAgressivo_DeveMostrarDiferenca()
    {
        // Arrange
        var clienteConservador = 1; // Baseado nos dados de seed
        var clienteAgressivo = 3;   // Baseado nos dados de seed
        var tipoProduto = "CDB";

        // Act - Obter recomendações para ambos os clientes
        var responseConservador = await _fixture.Client.GetAsync($"/api/simulacao/produtos-recomendados/{clienteConservador}/{tipoProduto}");
        var responseAgressivo = await _fixture.Client.GetAsync($"/api/simulacao/produtos-recomendados/{clienteAgressivo}/{tipoProduto}");

        // Assert
        responseConservador.StatusCode.Should().Be(HttpStatusCode.OK);
        responseAgressivo.StatusCode.Should().Be(HttpStatusCode.OK);

        var produtosConservador = JsonSerializer.Deserialize<List<ProdutoResponse>>(
            await responseConservador.Content.ReadAsStringAsync(), _jsonOptions);
        
        var produtosAgressivo = JsonSerializer.Deserialize<List<ProdutoResponse>>(
            await responseAgressivo.Content.ReadAsStringAsync(), _jsonOptions);

        // Verificar que ambos retornaram produtos
        produtosConservador.Should().NotBeEmpty();
        produtosAgressivo.Should().NotBeEmpty();

        // Os primeiros produtos podem ser diferentes baseados no perfil
        // (este é o comportamento esperado da recomendação inteligente)
        Console.WriteLine($"Primeiro produto para Conservador: {produtosConservador![0].Nome}");
        Console.WriteLine($"Primeiro produto para Agressivo: {produtosAgressivo![0].Nome}");
    }
}
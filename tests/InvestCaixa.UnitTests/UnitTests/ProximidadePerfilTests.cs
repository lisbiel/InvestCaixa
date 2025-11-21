namespace InvestCaixa.UnitTests.UnitTests;

using FluentAssertions;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Infrastructure.Data;
using InvestCaixa.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Teste específico para validar a lógica de proximidade de perfis
/// quando não há produto exato para o perfil do cliente.
/// </summary>
public class ProximidadePerfilTests
{
    [Fact]
    public async Task ObterPorTipoEPerfil_ClienteConservadorSemProdutoConservador_DeveRetornarModeradoMaisProximo()
    {
        // Arrange - Setup banco em memória
        var options = new DbContextOptionsBuilder<InvestimentoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new InvestimentoDbContext(options);
        
        // Criar produtos CDB SEM opção Conservadora
        var produtos = new[]
        {
            new ProdutoInvestimento("CDB Premium", TipoProduto.CDB, 0.13m, NivelRisco.Medio, 180, 5000m, true, PerfilInvestidor.Moderado),
            new ProdutoInvestimento("CDB High Yield", TipoProduto.CDB, 0.15m, NivelRisco.Alto, 360, 25000m, false, PerfilInvestidor.Agressivo),
            new ProdutoInvestimento("CDB Básico", TipoProduto.CDB, 0.12m, NivelRisco.Medio, 90, 1000m, true, PerfilInvestidor.Moderado),
            new ProdutoInvestimento("Fundo Arrojado", TipoProduto.Fundo, 0.18m, NivelRisco.Alto, 0, 500m, true, PerfilInvestidor.Agressivo) // Diferente tipo
        };

        context.Produtos.AddRange(produtos);
        await context.SaveChangesAsync();

        var repository = new ProdutoRepository(context);

        // Act - Cliente CONSERVADOR solicita CDB (mas não há CDB conservador)
        var resultado = await repository.ObterPorTipoEPerfilAsync("CDB", PerfilInvestidor.Conservador);
        var produtosOrdenados = resultado.ToList();

        // Assert
        produtosOrdenados.Should().HaveCount(3); // Apenas os CDBs
        
        // Primeiro produto deve ser Moderado (mais próximo ao Conservador)
        var primeiroProduto = produtosOrdenados.First();
        primeiroProduto.PerfilRecomendado.Should().Be(PerfilInvestidor.Moderado);
        
        // Entre os Moderados, deve ser o de maior rentabilidade
        primeiroProduto.Nome.Should().Be("CDB Premium"); // 13% > 12%
        primeiroProduto.Rentabilidade.Should().Be(0.13m);
        
        // Produto Agressivo deve ficar por último (mais distante)
        var ultimoProduto = produtosOrdenados.Last();
        ultimoProduto.PerfilRecomendado.Should().Be(PerfilInvestidor.Agressivo);
        ultimoProduto.Nome.Should().Be("CDB High Yield");
        
        // Verificar a ordem completa
        var ordensEsperadas = new[]
        {
            ("CDB Premium", PerfilInvestidor.Moderado, 0.13m),    // Proximidade 1, maior rentabilidade
            ("CDB Básico", PerfilInvestidor.Moderado, 0.12m),     // Proximidade 1, menor rentabilidade
            ("CDB High Yield", PerfilInvestidor.Agressivo, 0.15m) // Proximidade 2
        };

        for (int i = 0; i < ordensEsperadas.Length; i++)
        {
            var (nomeEsperado, perfilEsperado, rentabilidadeEsperada) = ordensEsperadas[i];
            var produto = produtosOrdenados[i];
            
            produto.Nome.Should().Be(nomeEsperado);
            produto.PerfilRecomendado.Should().Be(perfilEsperado);
            produto.Rentabilidade.Should().Be(rentabilidadeEsperada);
        }
    }

    [Fact]
    public async Task ObterPorTipoEPerfil_ClienteAgressivoSemProdutoAgressivo_DeveRetornarModeradoMaisProximo()
    {
        // Arrange - Setup banco em memória
        var options = new DbContextOptionsBuilder<InvestimentoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new InvestimentoDbContext(options);
        
        // Criar produtos CDB SEM opção Agressiva
        var produtos = new[]
        {
            new ProdutoInvestimento("CDB Conservador", TipoProduto.CDB, 0.10m, NivelRisco.Baixo, 90, 1000m, true, PerfilInvestidor.Conservador),
            new ProdutoInvestimento("CDB Premium", TipoProduto.CDB, 0.13m, NivelRisco.Medio, 180, 5000m, true, PerfilInvestidor.Moderado),
            new ProdutoInvestimento("CDB Básico", TipoProduto.CDB, 0.12m, NivelRisco.Medio, 90, 1000m, true, PerfilInvestidor.Moderado)
        };

        context.Produtos.AddRange(produtos);
        await context.SaveChangesAsync();

        var repository = new ProdutoRepository(context);

        // Act - Cliente AGRESSIVO solicita CDB (mas não há CDB agressivo)
        var resultado = await repository.ObterPorTipoEPerfilAsync("CDB", PerfilInvestidor.Agressivo);
        var produtosOrdenados = resultado.ToList();

        // Assert
        produtosOrdenados.Should().HaveCount(3);
        
        // Primeiro produto deve ser Moderado (mais próximo ao Agressivo)
        // Agressivo=3, Moderado=2, Conservador=1 → |3-2|=1, |3-1|=2
        var primeiroProduto = produtosOrdenados.First();
        primeiroProduto.PerfilRecomendado.Should().Be(PerfilInvestidor.Moderado);
        
        // Entre os Moderados, deve ser o de maior rentabilidade
        primeiroProduto.Nome.Should().Be("CDB Premium"); // 13% > 12%
        
        // Produto Conservador deve ficar por último (mais distante)
        var ultimoProduto = produtosOrdenados.Last();
        ultimoProduto.PerfilRecomendado.Should().Be(PerfilInvestidor.Conservador);
        ultimoProduto.Nome.Should().Be("CDB Conservador");
    }

    [Theory]
    [InlineData(PerfilInvestidor.Conservador, PerfilInvestidor.Moderado, 1)] // |1-2| = 1
    [InlineData(PerfilInvestidor.Conservador, PerfilInvestidor.Agressivo, 2)] // |1-3| = 2
    [InlineData(PerfilInvestidor.Moderado, PerfilInvestidor.Conservador, 1)]   // |2-1| = 1
    [InlineData(PerfilInvestidor.Moderado, PerfilInvestidor.Agressivo, 1)]     // |2-3| = 1
    [InlineData(PerfilInvestidor.Agressivo, PerfilInvestidor.Moderado, 1)]     // |3-2| = 1
    [InlineData(PerfilInvestidor.Agressivo, PerfilInvestidor.Conservador, 2)]  // |3-1| = 2
    public void CalcularProximidade_DeveCalcularDistanciaCorreta(
        PerfilInvestidor perfilCliente, 
        PerfilInvestidor perfilProduto, 
        int distanciaEsperada)
    {
        // Act - Simular o cálculo que o algoritmo faz
        var distanciaCalculada = Math.Abs((int)perfilProduto - (int)perfilCliente);

        // Assert
        distanciaCalculada.Should().Be(distanciaEsperada);
    }
}
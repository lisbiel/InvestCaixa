namespace InvestCaixa.UnitTests.Repositories;

using FluentAssertions;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Infrastructure.Data;
using InvestCaixa.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class SimulacaoRepositoryTests
{
    [Fact]
    public async Task ObterPorClienteAsync_ComClienteExistente_DeveRetornarSimulacoes()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<InvestimentoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using (var context = new InvestimentoDbContext(options))
        {
            var produto = new ProdutoInvestimento("CDB Caixa 2026", TipoProduto.CDB, 0.12m, NivelRisco.Baixo, 180, 1000m, true, PerfilInvestidor.Conservador);
            context.Produtos.Add(produto);
            await context.SaveChangesAsync();

            var cliente = new Cliente("Teste", "teste@test.com", "12345678901", DateTime.Now.AddYears(-30));
            var simulacao = new Simulacao(1, "CDB Caixa 2026", produto.Id,  10000m, 11200m, 12, DateTime.UtcNow);

            context.Clientes.Add(cliente);
            context.Simulacoes.Add(simulacao);
            await context.SaveChangesAsync();

            var repository = new SimulacaoRepository(context);

            // Act
            var resultado = await repository.ObterPorClienteAsync(1);

            // Assert
            resultado.Should().NotBeEmpty();
            resultado.Count().Should().Be(1);
        }
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarTodasAsSimulacoes()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<InvestimentoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using (var context = new InvestimentoDbContext(options))
        {
            var produtoId = Guid.NewGuid();
            var simulacao1 = new Simulacao(1, "CDB Caixa 2026", produtoId, 10000m, 11200m, 12, DateTime.UtcNow);
            var simulacao2 = new Simulacao(2, "CDB Caixa 2026", produtoId, 5000m, 5600m, 12, DateTime.UtcNow);

            context.Simulacoes.Add(simulacao1);
            context.Simulacoes.Add(simulacao2);
            await context.SaveChangesAsync();

            var repository = new SimulacaoRepository(context);

            // Act
            var resultado = await repository.GetAllAsync();

            // Assert
            resultado.Count().Should().Be(2);
        }
    }
}

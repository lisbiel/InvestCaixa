using FluentAssertions;
using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Infrastructure.Data;
using InvestCaixa.UnitTests.Fixtures;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace InvestCaixa.UnitTests.IntegrationTests;

[Collection("Integration Tests")]
public class InvestimentosIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;

    public InvestimentosIntegrationTests(IntegrationTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Finalizar_Investimento_ComDadosValidos_DeveRetornar201()
    {
        var options = new DbContextOptionsBuilder<InvestimentoDbContext>()
            .UseInMemoryDatabase("InvestCaixaTests")
            .Options;

        using (var context = new InvestimentoDbContext(options))
        {
            //Necessário um produto id válido para confirmar o funcionamen
            var produto = new ProdutoInvestimento("CDB Caixa 2026", TipoProduto.CDB, 0.12m, NivelRisco.Baixo, 180, 1000m, true, PerfilInvestidor.Conservador);
            context.Produtos.Add(produto);
            await context.SaveChangesAsync();
            var request = new FinalizarInvestimentoRequest { ClienteId = 1, ProdutoId = produto.Id, ValorAplicado = 5000m, PrazoMeses = 12 };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");
            var response = await _fixture.Client.PostAsync("/api/investimentos/finalizar", content);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        }
    }

    [Fact]
    public async Task ObterHistoricoCompleto_DeveRetornarSimulacoesEInvestimentos()
    {
        var response = await _fixture.Client.GetAsync("/api/investimentos/historico/1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("totalInvestido");
    }
}

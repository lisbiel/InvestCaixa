using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;

namespace InvestCaixa.UnitTests.Helpers;

public class TestDataBuilder
{
    public static SimularInvestimentoRequest CriarSimulacaoRequest(
        int clienteId = 1,
        decimal valor = 10000m,
        int prazoMeses = 12,
        string tipoProduto = "CDB")
    {
        return new SimularInvestimentoRequest
        {
            ClienteId = clienteId,
            Valor = valor,
            PrazoMeses = prazoMeses,
            TipoProduto = tipoProduto
        };
    }

    public static LoginRequest CriarLoginRequest(
        string username = "admin",
        string password = "Admin@123")
    {
        return new LoginRequest
        {
            Usuario = username,
            Senha = password
        };
    }

    public static ProdutoInvestimento CriarProduto(
        string nome = "Teste Produto",
        TipoProduto tipo = TipoProduto.CDB,
        decimal rentabilidade = 0.12m,
        NivelRisco risco = NivelRisco.Baixo,
        int prazoMinimoDias = 180,
        decimal valorMinimo = 1000m,
        bool permiteLiquidez = true,
        PerfilInvestidor perfilRecomendado = PerfilInvestidor.Conservador)
    {
        return new ProdutoInvestimento(
            nome, tipo, rentabilidade, risco, 
            prazoMinimoDias, valorMinimo, permiteLiquidez, perfilRecomendado);
    }

    public static Cliente CriarCliente(
        string nome = "Teste Cliente",
        string email = "teste@test.com",
        string cpf = "12345678901",
        DateTime? dataNascimento = null)
    {
        return new Cliente(
            nome, email, cpf, 
            dataNascimento ?? DateTime.Now.AddYears(-30));
    }
}

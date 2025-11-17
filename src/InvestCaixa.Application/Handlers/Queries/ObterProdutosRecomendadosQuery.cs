namespace InvestCaixa.Application.Handlers.Queries;
using InvestCaixa.Application.DTOs.Response;
using MediatR;

public class ObterProdutosRecomendadosQuery : IRequest<IEnumerable<ProdutoResponse>>
{
    public int Perfil { get; set; }

    public ObterProdutosRecomendadosQuery(int perfil) => Perfil = perfil;
}
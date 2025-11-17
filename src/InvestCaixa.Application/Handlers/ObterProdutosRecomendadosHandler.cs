using AutoMapper;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Handlers.Queries;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InvestCaixa.Application.Handlers;

public class ObterProdutosRecomendadosQueryHandler : IRequestHandler<ObterProdutosRecomendadosQuery, IEnumerable<ProdutoResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ObterProdutosRecomendadosQueryHandler> _logger;

    public ObterProdutosRecomendadosQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ObterProdutosRecomendadosQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProdutoResponse>> Handle(
        ObterProdutosRecomendadosQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando produtos recomendados para perfil {Perfil}", request.Perfil);

        var perfil = (PerfilInvestidor)request.Perfil;
        var produtos = await _unitOfWork.ProdutoRepository
            .ObterPorPerfilAsync(perfil, cancellationToken);

        return _mapper.Map<IEnumerable<ProdutoResponse>>(produtos);
    }
}

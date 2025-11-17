using InvestCaixa.Application.Handlers.Commands;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Exceptions;
using InvestCaixa.Domain.Interfaces;
using MediatR;

namespace InvestCaixa.Application.Handlers;

public class FinalizarInvestimentoCommandHandler : IRequestHandler<FinalizarInvestimentoCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public FinalizarInvestimentoCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(FinalizarInvestimentoCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _unitOfWork.ClienteRepository.GetByIdAsync(request.ClienteId, cancellationToken);
        if (cliente is null) throw new NotFoundException("Cliente não encontrado");

        var produto = await _unitOfWork.ProdutoRepository.GetByIdAsync(request.ProdutoId, cancellationToken);
        if (produto is null) throw new NotFoundException("Produto não encontrado");

        var investimento = new InvestimentoFinalizado(request.ClienteId, request.ProdutoId, request.ValorAplicado, DateTime.UtcNow);
        await _unitOfWork.InvestimentoFinalizadoRepository.AddAsync(investimento, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return investimento.Id;
    }
}


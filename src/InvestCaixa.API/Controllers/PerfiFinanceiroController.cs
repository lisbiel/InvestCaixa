using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.Interfaces;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestCaixa.API.Controllers;

// <summary>
// Controla o perfil financeiro mais completo do cliente/ estilo suitability
// </summary>
[ApiController]
[Route("api/perfil-financeiro")]
[Authorize]
public class PerfilFinanceiroController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPerfilRiscoService _perfilRiscoService;

    public PerfilFinanceiroController(IUnitOfWork unitOfWork, IPerfilRiscoService perfilRiscoService)
    {
        _unitOfWork = unitOfWork;
        _perfilRiscoService = perfilRiscoService;
    }

    /// <summary>
    /// Cria ou atualiza perfil financeiro do cliente
    /// </summary>
    [HttpPost("{clienteId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CriarPerfilFinanceiro(
        int clienteId,
        [FromBody] CriarPerfilFinanceiroRequest request,
        CancellationToken cancellationToken)
    {
        var cliente = await _unitOfWork.ClienteRepository.GetByIdAsync(clienteId, cancellationToken);
        if (cliente is null) return NotFound("Cliente não encontrado");

        var perfilFinanceiroExistente = await _unitOfWork.PerfilFinanceiroRepository.ObterPorClienteAsync(clienteId, cancellationToken);

        var perfilRiscoExistente = await _unitOfWork.ClienteRepository.ObterPerfilRiscoAsync(clienteId, cancellationToken);

        PerfilFinanceiro perfilFinanceiro;
        var isNovoPerfil = false;

        if (perfilFinanceiroExistente != null)
        {
            perfilFinanceiroExistente.AtualizarDados(
                request.RendaMensal,
                request.PatrimonioTotal,
                request.DividasAtivas,
                request.DependentesFinanceiros,
                (HorizonteInvestimento)request.Horizonte,
                (ObjetivoInvestimento)request.Objetivo,
                request.ToleranciaPerda,
                request.ExperienciaInvestimentos);

            perfilFinanceiro = perfilFinanceiroExistente;
            await _unitOfWork.PerfilFinanceiroRepository.UpdateAsync(perfilFinanceiro, cancellationToken);
        }
        else
        {
            perfilFinanceiro = new PerfilFinanceiro(
                clienteId,
                request.RendaMensal,
                request.PatrimonioTotal,
                request.DividasAtivas,
                request.DependentesFinanceiros,
                (HorizonteInvestimento)request.Horizonte,
                (ObjetivoInvestimento)request.Objetivo,
                request.ToleranciaPerda,
                request.ExperienciaInvestimentos);
            await _unitOfWork.PerfilFinanceiroRepository.AddAsync(perfilFinanceiro, cancellationToken);
            isNovoPerfil = true;

            await _unitOfWork.PerfilFinanceiroRepository.AddAsync(perfilFinanceiro, cancellationToken);
            isNovoPerfil = true;
        }

        if (perfilRiscoExistente != null)
        {
            perfilRiscoExistente.AtualizarPerfil(perfilFinanceiro);
            await _unitOfWork.ClienteRepository.AtualizarPerfilRiscoAsync(perfilRiscoExistente, cancellationToken);
        }
        else
        {
            var novoPerfilRisco = new PerfilRisco(
                clienteId,
                volumeInvestimentos: 0,
                frequenciaMovimentacoes: 0,
                prefereLiquidez: true);

            novoPerfilRisco.AtualizarPerfil(perfilFinanceiro);
            await _unitOfWork.ClienteRepository.AdicionarPerfilRiscoAsync(novoPerfilRisco, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return isNovoPerfil ? CreatedAtAction(nameof(CriarPerfilFinanceiro), new { id = perfilFinanceiro.Id }) : Ok(new {message = "Perfil financeiro e de risco atualizados com sucesso", perfilFinanceiro});
    }

    /// <summary>
    /// Obter as opções de preenchimento do perfil financeiro
    /// </summary>
    [HttpGet("opcoes")]
    [AllowAnonymous]
    public ActionResult<object> ObterOpcoesPerfilFinanceiro()
    {
        var opcoes = new
        {
            Horizontes = new
            {
                _1 = "Curto prazo (até 1 ano e meio) - Foco em Liquidez",
                _2 = "Médio prazo (de 1 ano e meio até 4 anos) - Foco em Crescimento",
                _3 = "Longo prazo (acima de 4 anos) - Foco em Rentabilidade"
            },
            Objetivos = new
            {
                _1 = "Reserva de Emergência - Máxima Segurança",
                _2 = "Aposentadoria - Crescimento Sustentável e Longo Prazo",
                _3 = "Compra de Imóvel - Equilíbrio entre Segurança e Rentabilidade",
                _4 = "Educação dos Filhos - Planejamento e Crescimento",
                _5 = "Outros objetivos"
            },
            ToleranciaPerda = new
            {
                _0 = "Nenhuma (0% de perda)",
                _1 = "Até 2% de perda",
                _2 = "Até 5% de perda",
                _3 = "Até 9% de perda",
                _4 = "Até 12% de perda",
                _5 = "Até 15% de perda",
                _6 = "Até 18% de perda",
                _7 = "Até 20% de perda",
                _8 = "Até 23% de perda",
                _9 = "Até 25% de perda",
                _10 = "Até 30% de perda"
            }
        };
        return Ok(opcoes);
    }

    /// <summary>
    /// Obter exemplos de preenchimento do perfil financeiro
    /// </summary>
    [HttpGet("exemplos")]
    [AllowAnonymous]
    public ActionResult<object> ObterExemplosPerfilFinanceiro()
    {
        var exemplos = new
        {
            Conservador = new
            {
                RendaMensal = 3000,
                PatrimonioTotal = 20000,
                DividasAtivas = 5000,
                DependentesFinanceiros = 2,
                Horizonte = 1,
                Objetivo = 1,
                ToleranciaPerda = 0,
                ExperienciaInvestimentos = false
            },
            Moderado = new
            {
                RendaMensal = 8000,
                PatrimonioTotal = 150000,
                DividasAtivas = 10000,
                DependentesFinanceiros = 1,
                Horizonte = 2,
                Objetivo = 3,
                ToleranciaPerda = 2,
                ExperienciaInvestimentos = true
            },
            Agressivo = new
            {
                RendaMensal = 15000,
                PatrimonioTotal = 500000,
                DividasAtivas = 20000,
                DependentesFinanceiros = 0,
                Horizonte = 3,
                Objetivo = 2,
                ToleranciaPerda = 7,
                ExperienciaInvestimentos = true
            }
        };
        return Ok(exemplos);
    }
}

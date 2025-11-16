namespace InvestCaixa.Application.Validators;

using FluentValidation;
using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Domain.Enums;

public class SimularInvestimentoValidator : AbstractValidator<SimularInvestimentoRequest>
{
    public SimularInvestimentoValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0)
            .WithMessage("ClienteId deve ser maior que zero");

        RuleFor(x => x.Valor)
            .GreaterThan(0)
            .WithMessage("Valor deve ser maior que zero")
            .LessThanOrEqualTo(10_000_000)
            .WithMessage("Valor não pode exceder R$ 10.000.000,00");

        RuleFor(x => x.PrazoMeses)
            .InclusiveBetween(1, 360)
            .WithMessage("Prazo deve estar entre 1 e 360 meses");

        RuleFor(x => x.TipoProduto)
            .NotEmpty()
            .WithMessage("Tipo de produto é obrigatório")
            .Must(BeValidTipoProduto)
            .WithMessage("Tipo de produto inválido");
    }

    private static bool BeValidTipoProduto(string tipo)
    {
        return Enum.TryParse<TipoProduto>(tipo, true, out _);
    }
}

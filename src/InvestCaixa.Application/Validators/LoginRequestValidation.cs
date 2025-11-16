using FluentValidation;
using InvestCaixa.Application.DTOs.Request;

namespace InvestCaixa.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Usuario)
            .NotEmpty()
            .WithMessage("Usuário é obrigatório")
            .MaximumLength(100)
            .WithMessage("Usuário não pode exceder 100 caracteres");

        RuleFor(x => x.Senha)
            .NotEmpty()
            .WithMessage("Senha é obrigatória")
            .MinimumLength(6)
            .WithMessage("Senha deve ter pelo menos 6 caracteres")
            .MaximumLength(100)
            .WithMessage("Senha não pode exceder 100 caracteres");
    }
}

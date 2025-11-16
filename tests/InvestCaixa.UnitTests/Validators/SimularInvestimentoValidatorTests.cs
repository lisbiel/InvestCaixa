namespace InvestCaixa.UnitTests.Validators;

using FluentAssertions;
using FluentValidation.TestHelper;
using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.Validators;
using Xunit;

public class SimularInvestimentoValidatorTests
{
    private readonly SimularInvestimentoValidator _validator;

    public SimularInvestimentoValidatorTests()
    {
        _validator = new SimularInvestimentoValidator();
    }

    [Fact]
    public void Validate_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var request = new SimularInvestimentoRequest
        {
            ClienteId = 123,
            Valor = 10000m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ComClienteIdZero_DeveRetornarErro()
    {
        // Arrange
        var request = new SimularInvestimentoRequest
        {
            ClienteId = 0,
            Valor = 10000m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ClienteId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Validate_ComValorNegativoOuZero_DeveRetornarErro(decimal valor)
    {
        // Arrange
        var request = new SimularInvestimentoRequest
        {
            ClienteId = 123,
            Valor = valor,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Valor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(361)]
    public void Validate_ComPrazoForaDoIntervalo_DeveRetornarErro(int prazo)
    {
        // Arrange
        var request = new SimularInvestimentoRequest
        {
            ClienteId = 123,
            Valor = 10000m,
            PrazoMeses = prazo,
            TipoProduto = "CDB"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PrazoMeses);
    }
}

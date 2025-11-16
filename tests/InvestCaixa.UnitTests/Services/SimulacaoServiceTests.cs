namespace InvestCaixa.UnitTests.Services;

using AutoMapper;
using FluentAssertions;
using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Services;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Domain.Exceptions;
using InvestCaixa.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SimulacaoServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<SimulacaoService>> _loggerMock;
    private readonly SimulacaoService _service;

    public SimulacaoServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<SimulacaoService>>();

        _service = new SimulacaoService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SimularInvestimento_ComDadosValidos_DeveRetornarSimulacao()
    {
        // Arrange
        var produtoId = Guid.NewGuid();
        var request = new SimularInvestimentoRequest
        {
            ClienteId = 123,
            Valor = 10000m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };

        var produto = new ProdutoInvestimento(
            "CDB Teste",
            TipoProduto.CDB,
            0.12m,
            NivelRisco.Baixo,
            180,
            1000m,
            true,
            PerfilInvestidor.Conservador);

        _unitOfWorkMock
            .Setup(x => x.ProdutoRepository.ObterPorTipoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto);

        _unitOfWorkMock
            .Setup(x => x.SimulacaoRepository.AddAsync(It.IsAny<Simulacao>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Simulacao s, CancellationToken ct) => s);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<ProdutoValidadoDto>(It.IsAny<ProdutoInvestimento>()))
            .Returns(new ProdutoValidadoDto
            {
                Id = produto.Id,
                Nome = "CDB Teste",
                Tipo = "CDB",
                Rentabilidade = 0.12m,
                Risco = "Baixo"
            });

        // Act
        var resultado = await _service.SimularInvestimentoAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.ProdutoValidado.Should().NotBeNull();
        resultado.ResultadoSimulacao.Should().NotBeNull();
        resultado.ResultadoSimulacao.ValorFinal.Should().BeGreaterThan(request.Valor);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task SimularInvestimento_ComProdutoInexistente_DeveLancarNotFoundException()
    {
        // Arrange
        var request = new SimularInvestimentoRequest
        {
            ClienteId = 123,
            Valor = 10000m,
            PrazoMeses = 12,
            TipoProduto = "INEXISTENTE"
        };

        _unitOfWorkMock
            .Setup(x => x.ProdutoRepository.ObterPorTipoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProdutoInvestimento?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.SimularInvestimentoAsync(request));
    }

    [Theory]
    [InlineData(500, 1000)]
    [InlineData(100, 1000)]
    public async Task SimularInvestimento_ComValorAbaixoDoMinimo_DeveLancarValidationException(
        decimal valor, 
        decimal valorMinimo)
    {
        // Arrange
        var request = new SimularInvestimentoRequest
        {
            ClienteId = 123,
            Valor = valor,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };

        var produto = new ProdutoInvestimento(
            "CDB Teste",
            TipoProduto.CDB,
            0.12m,
            NivelRisco.Baixo,
            180,
            valorMinimo,
            true,
            PerfilInvestidor.Conservador);

        _unitOfWorkMock
            .Setup(x => x.ProdutoRepository.ObterPorTipoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.SimularInvestimentoAsync(request));
    }
}

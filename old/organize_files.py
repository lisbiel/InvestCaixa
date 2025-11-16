#!/usr/bin/env python3
"""
Script para organizar arquivos em suas pastas corretas
Você já tem os arquivos .cs, este script os move para os diretórios corretos
"""

import os
import shutil

# Mapeamento: arquivo.cs -> pasta_destino
ARQUIVO_PARA_PASTA = {
    # Domain - Entities
    "BaseEntity.cs": "src/InvestCaixa.Domain/Entities",
    "Cliente.cs": "src/InvestCaixa.Domain/Entities",
    "ProdutoInvestimento.cs": "src/InvestCaixa.Domain/Entities",
    "Simulacao.cs": "src/InvestCaixa.Domain/Entities",
    "PerfilRisco.cs": "src/InvestCaixa.Domain/Entities",

    # Domain - Enums
    "TipoProduto.cs": "src/InvestCaixa.Domain/Enums",
    "NivelRisco.cs": "src/InvestCaixa.Domain/Enums",
    "PerfilInvestidor.cs": "src/InvestCaixa.Domain/Enums",

    # Domain - Exceptions
    "DomainException.cs": "src/InvestCaixa.Domain/Exceptions",
    "NotFoundException.cs": "src/InvestCaixa.Domain/Exceptions",
    "ValidationException.cs": "src/InvestCaixa.Domain/Exceptions",

    # Domain - Interfaces
    "IRepository.cs": "src/InvestCaixa.Domain/Interfaces",
    "ISimulacaoRepository.cs": "src/InvestCaixa.Domain/Interfaces",
    "IProdutoRepository.cs": "src/InvestCaixa.Domain/Interfaces",
    "IClienteRepository.cs": "src/InvestCaixa.Domain/Interfaces",
    "IUnitOfWork.cs": "src/InvestCaixa.Domain/Interfaces",

    # Application - DTOs Request
    "SimularInvestimentoRequest.cs": "src/InvestCaixa.Application/DTOs/Request",
    "LoginRequest.cs": "src/InvestCaixa.Application/DTOs/Request",
    "RefreshTokenRequest.cs": "src/InvestCaixa.Application/DTOs/Request",

    # Application - DTOs Response
    "SimulacaoResponse.cs": "src/InvestCaixa.Application/DTOs/Response",
    "SimulacaoHistoricoResponse.cs": "src/InvestCaixa.Application/DTOs/Response",
    "PerfilRiscoResponse.cs": "src/InvestCaixa.Application/DTOs/Response",
    "ProdutoResponse.cs": "src/InvestCaixa.Application/DTOs/Response",
    "TelemetriaResponse.cs": "src/InvestCaixa.Application/DTOs/Response",
    "LoginResponse.cs": "src/InvestCaixa.Application/DTOs/Response",

    # Application - Interfaces
    "ISimulacaoService.cs": "src/InvestCaixa.Application/Interfaces",
    "IPerfilRiscoService.cs": "src/InvestCaixa.Application/Interfaces",
    "ITelemetriaService.cs": "src/InvestCaixa.Application/Interfaces",
    "IAuthService.cs": "src/InvestCaixa.Application/Interfaces",

    # Application - Services
    "SimulacaoService.cs": "src/InvestCaixa.Application/Services",
    "PerfilRiscoService.cs": "src/InvestCaixa.Application/Services",
    "TelemetriaService.cs": "src/InvestCaixa.Application/Services",
    "AuthService.cs": "src/InvestCaixa.Application/Services",

    # Application - Validators
    "SimularInvestimentoValidator.cs": "src/InvestCaixa.Application/Validators",

    # Application - Mappings
    "MappingProfile.cs": "src/InvestCaixa.Application/Mappings",

    # Application - Common
    "Result.cs": "src/InvestCaixa.Application/Common",

    # Infrastructure - Data
    "ApplicationDbContext.cs": "src/InvestCaixa.Infrastructure/Data",

    # Infrastructure - Configurations
    "ClienteConfiguration.cs": "src/InvestCaixa.Infrastructure/Configurations",
    "ProdutoConfiguration.cs": "src/InvestCaixa.Infrastructure/Configurations",
    "SimulacaoConfiguration.cs": "src/InvestCaixa.Infrastructure/Configurations",
    "PerfilRiscoConfiguration.cs": "src/InvestCaixa.Infrastructure/Configurations",

    # Infrastructure - Repositories
    "Repository.cs": "src/InvestCaixa.Infrastructure/Repositories",
    "SimulacaoRepository.cs": "src/InvestCaixa.Infrastructure/Repositories",
    "ProdutoRepository.cs": "src/InvestCaixa.Infrastructure/Repositories",
    "ClienteRepository.cs": "src/InvestCaixa.Infrastructure/Repositories",
    "UnitOfWork.cs": "src/InvestCaixa.Infrastructure/Repositories",

    # Infrastructure - Services
    "JwtTokenService.cs": "src/InvestCaixa.Infrastructure/Services",
    "DateTimeService.cs": "src/InvestCaixa.Infrastructure/Services",

    # API - Controllers
    "AuthController.cs": "src/InvestCaixa.API/Controllers",
    "SimulacaoController.cs": "src/InvestCaixa.API/Controllers",
    "PerfilRiscoController.cs": "src/InvestCaixa.API/Controllers",
    "TelemetriaController.cs": "src/InvestCaixa.API/Controllers",

    # API - Middlewares
    "GlobalExceptionHandler.cs": "src/InvestCaixa.API/Middlewares",
    "TelemetriaMiddleware.cs": "src/InvestCaixa.API/Middlewares",

    # API - Extensions
    "ServiceCollectionExtensions.cs": "src/InvestCaixa.API/Extensions",
    "ApplicationBuilderExtensions.cs": "src/InvestCaixa.API/Extensions",

    # API - Program
    "Program.cs": "src/InvestCaixa.API",

    # Tests
    "SimulacaoServiceTests.cs": "tests/InvestCaixa.UnitTests/Services",
    "SimularInvestimentoValidatorTests.cs": "tests/InvestCaixa.UnitTests/Validators",
    "SimulacaoRepositoryTests.cs": "tests/InvestCaixa.UnitTests/Repositories",

    # Integration Tests
    "IntegrationTestFixture.cs": "tests/InvestCaixa.UnitTests/Fixtures",
    "TestDataBuilder.cs": "tests/InvestCaixa.UnitTests/Helpers",
    "AuthControllerIntegrationTests.cs": "tests/InvestCaixa.UnitTests/IntegrationTests",
    "SimulacaoControllerIntegrationTests.cs": "tests/InvestCaixa.UnitTests/IntegrationTests",
    "PerfilRiscoControllerIntegrationTests.cs": "tests/InvestCaixa.UnitTests/IntegrationTests",
    "TelemetriaControllerIntegrationTests.cs": "tests/InvestCaixa.UnitTests/IntegrationTests",
    "EndToEndIntegrationTests.cs": "tests/InvestCaixa.UnitTests/IntegrationTests",
    "BusinessLogicIntegrationTests.cs": "tests/InvestCaixa.UnitTests/IntegrationTests",
    "ErrorHandlingIntegrationTests.cs": "tests/InvestCaixa.UnitTests/IntegrationTests",
    "SecurityIntegrationTests.cs": "tests/InvestCaixa.UnitTests/IntegrationTests",
}

def organize_files():
    """Move arquivos para suas pastas corretas"""

    print("\n" + "="*70)
    print("Organizando arquivos para suas pastas")
    print("="*70 + "\n")

    moved = 0
    not_found = []

    for arquivo, pasta_destino in ARQUIVO_PARA_PASTA.items():
        # Procurar arquivo na pasta atual
        if os.path.exists(arquivo):
            # Criar pasta destino se não existir
            os.makedirs(pasta_destino, exist_ok=True)

            # Mover arquivo
            destino_final = os.path.join(pasta_destino, arquivo)
            shutil.move(arquivo, destino_final)

            print(f"✓ {arquivo:45} -> {pasta_destino}")
            moved += 1
        else:
            not_found.append(arquivo)

    print("\n" + "="*70)
    print(f"✅ {moved} arquivos movidos com sucesso!")

    if not_found:
        print(f"\n⚠️  {len(not_found)} arquivos não encontrados:")
        for f in not_found:
            print(f"   - {f}")

    print("="*70 + "\n")

if __name__ == "__main__":
    organize_files()

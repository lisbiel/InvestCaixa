# Suite Completa de Testes de Integração - Investment Simulation API

## 📋 Visão Geral

Este documento descreve a suite completa de testes de integração para a API de Simulação de Investimentos. Os testes cobrem todos os endpoints, cenários de negócio, tratamento de erros e segurança.

## 🏗️ Estrutura de Testes

### 1. **Fixtures e Helpers** (`Fixtures/` e `Helpers/`)

#### IntegrationTestFixture.cs
- Configura WebApplicationFactory do .NET 8
- Usa InMemoryDatabase para testes isolados
- Fornece HttpClient pré-autenticado
- Seed de dados inicial automático
- Reset de banco de dados entre testes

#### TestDataBuilder.cs
- Builder para criar dados de teste
- Métodos convenientes para requests, entities e clientes
- Valores padrão realistas

### 2. **Testes de Autenticação** (`AuthControllerIntegrationTests.cs`)

| Teste | Cenário | Esperado |
|-------|---------|----------|
| `Login_ComCredenciaisValidas_DeveRetornarToken` | Login com credenciais corretas | Token JWT retornado |
| `Login_ComCredenciaisInvalidas_DeveRetornar401` | Login com senha errada | Status 401 |
| `RefreshToken_ComTokenValido_DeveRetornarNovoToken` | Renovar token expirado | Novo token retornado |
| `Login_SemCredenciais_DeveRetornarErroDeValidacao` | Login com campos vazios | Status 400 |

### 3. **Testes de Simulação** (`SimulacaoControllerIntegrationTests.cs`)

| Teste | Cenário | Esperado |
|-------|---------|----------|
| `SimularInvestimento_ComDadosValidos` | Simulação com dados corretos | Response 200 com cálculos corretos |
| `SimularInvestimento_ComDiferentesTiposProduto` | Testar cada tipo de produto | Produtos retornados corretamente |
| `SimularInvestimento_ComValorAbaixoDoMinimo` | Valor menor que o mínimo | Status 400 |
| `SimularInvestimento_ComProdutoInexistente` | Produto não existe | Status 404 |
| `ObterSimulacoes_DeveRetornarListaDeSimulacoes` | Buscar histórico completo | Lista de simulações |
| `ObterSimulacoesPorProdutoDia_DeveRetornarDadosAgrupados` | Agrupamento por produto/dia | Dados agregados corretos |
| `SimularInvestimento_DeveCalcularRentabilidadeCorretamente` | Verificar cálculos | Rentabilidade positiva e coerente |

### 4. **Testes de Perfil de Risco** (`PerfilRiscoControllerIntegrationTests.cs`)

| Teste | Cenário | Esperado |
|-------|---------|----------|
| `ObterPerfilRisco_ComClienteExistente` | Buscar perfil de cliente existente | Perfil retornado com dados |
| `ObterPerfilRisco_ComClienteInexistente` | Cliente não existe | Status 404 |
| `ObterProdutosRecomendados_ComDiferentesPerfs` | Recomendações por perfil | Produtos filtrados por perfil |
| `ObterProdutosRecomendados_ConservadorDeveRetornarProdutosBaixoRisco` | Perfil conservador | Apenas produtos baixo risco |
| `ObterProdutosRecomendados_AgressivoDeveRetornarProdutosAltoRisco` | Perfil agressivo | Produtos com rentabilidade maior |
| `ObterPerfilRisco_DeveRetornarPontuacaoValida` | Validar pontuação | Entre 0 e 100 |

### 5. **Testes de Telemetria** (`TelemetriaControllerIntegrationTests.cs`)

| Teste | Cenário | Esperado |
|-------|---------|----------|
| `ObterTelemetria_DeveRetornarDadosValidos` | Buscar telemetria | Serviços e período retornados |
| `ObterTelemetria_ComMultiplasRequisicoes_DeveRetornarEstatisticas` | Múltiplas chamadas | Contadores e médias calculadas |
| `ObterTelemetria_ComFiltroDeData_DeveRetornarResultadosCorretos` | Filtro de período | Dados dentro da data |
| `ObterTelemetria_DeveRetornarTempoMedioPositivo` | Validar tempos | Valores >= 0 |

### 6. **Testes End-to-End** (`EndToEndIntegrationTests.cs`)

Fluxos completos que cobrem múltiplos endpoints:

| Teste | Cenário |
|-------|---------|
| `FluxoCompleto_CriarSimulacaoEConsultarHistorico` | Criar 3 simulações → consultar histórico → telemetria |
| `FluxoCompleto_ObterPerfilEProdutosRecomendados` | Obter perfil → produtos recomendados |
| `FluxoCompleto_SimularComDiferentesPrazos` | Simular com 4 prazos diferentes → validar rentabilidades |
| `FluxoCompleto_SimularComValoresVariaveis` | Valores de 1K a 50K → validar consistência |
| `FluxoCompleto_PersistenciaEmBancoDados` | Criar simulação → recuperar do histórico |
| `FluxoCompleto_MultiplosCLientes_DeveMantenerDadosSeparados` | 2 clientes → dados isolados |

### 7. **Testes de Lógica de Negócio** (`BusinessLogicIntegrationTests.cs`)

| Teste | Validação |
|-------|-----------|
| `SimularInvestimento_DeveUtilizarTaxaCorreta` | Taxa de rentabilidade por produto |
| `PerfilRisco_ConservadorDeveRetornarMenorPontuacao` | Classificação de perfil |
| `ProdutosRecomendados_DeveFiltroPorRisco` | Filtragem por nível de risco |
| `SimularInvestimento_ComPrazoMaior_DeveRetornarValorMaior` | Relação prazo × rendimento |
| `SimularInvestimento_ComValorMaior_DeveRetornarMaisRendimento` | Consistência de cálculos |
| `HistoricoSimulacoes_DeveAgrupaPorProdutoEDia` | Agregação correta |
| `Telemetria_DeveRegistrarTodasAsOperacoes` | Rastreamento de chamadas |

### 8. **Testes de Tratamento de Erros** (`ErrorHandlingIntegrationTests.cs`)

| Teste | Erro Esperado |
|-------|---------------|
| `SimularInvestimento_ComValorZero` | 400 Bad Request |
| `SimularInvestimento_ComValorNegativo` | 400 Bad Request |
| `SimularInvestimento_ComPrazoZero` | 400 Bad Request |
| `SimularInvestimento_ComClienteInvalido` | 400 Bad Request |
| `SimularInvestimento_ComJsonInvalido` | 400 Bad Request |
| `ObterPerfilRisco_ComClienteInexistente` | 404 Not Found |
| `Endpoint_SemAutenticacao` | 401 Unauthorized |
| `Endpoint_ComAutenticacaoInvalida` | 401 Unauthorized |
| `Endpoint_Invalido` | 404 Not Found |

### 9. **Testes de Segurança** (`SecurityIntegrationTests.cs`)

| Teste | Validação |
|-------|-----------|
| `GetSimulacoes_SemBearerToken_DeveRetornar401` | JWT obrigatório |
| `GetPerfilRisco_SemBearerToken_DeveRetornar401` | JWT obrigatório |
| `GetTelemetria_SemBearerToken_DeveRetornar401` | JWT obrigatório |
| `Login_ComCredenciaisVazias_DeveRetornarErro` | Validação de credenciais |
| `ComAutenticacaoValida_DeveAceitarRequisicoes` | Autorização funciona |

## 🚀 Executar Testes

### Todos os testes
```bash
dotnet test
```

### Apenas testes de integração
```bash
dotnet test --filter "Category=Integration" 2>/dev/null || dotnet test tests/InvestmentSimulation.UnitTests/IntegrationTests/
```

### Testes específicos
```bash
dotnet test --filter "SimulacaoControllerIntegrationTests"
```

### Com cobertura
```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
```

### Watch mode (reexecutar ao salvar)
```bash
dotnet watch test
```

## 📊 Cobertura de Testes

### Endpoints cobertos
- ✅ `POST /api/auth/login` - Autenticação
- ✅ `POST /api/auth/refresh` - Renovação de token
- ✅ `POST /api/simulacao/simular-investimento` - Simulação
- ✅ `GET /api/simulacao/simulacoes` - Histórico
- ✅ `GET /api/simulacao/simulacoes/por-produto-dia` - Agregação
- ✅ `GET /api/perfil-risco/{clienteId}` - Perfil de risco
- ✅ `GET /api/perfil-risco/produtos-recomendados/{perfil}` - Recomendações
- ✅ `GET /api/telemetria` - Telemetria

### Cenários cobertos
- ✅ Fluxos felizes (happy path)
- ✅ Validações de entrada
- ✅ Tratamento de erros
- ✅ Segurança (autenticação/autorização)
- ✅ Lógica de negócio
- ✅ Persistência em banco de dados
- ✅ Cálculos matemáticos
- ✅ Agregações e groupings
- ✅ Filtros e queries

## 🔧 Configuração de Testes

### Database para testes
- **Tipo**: InMemoryDatabase
- **Limpeza**: Automática entre testes
- **Seed**: Dados iniciais automáticos

### Autenticação para testes
- **Tipo**: Bearer Token JWT válido
- **Validade**: Configurada sem expiração
- **Setup**: Automático no fixture

### Timeout
- **Padrão**: 30 segundos por teste
- **Ajustável**: Via `[Timeout(ms)]`

## 📝 Boas Práticas Aplicadas

1. **Padrão AAA**: Arrange, Act, Assert
2. **Fixtures**: Reutilização de setup/teardown
3. **Builders**: Factory pattern para dados
4. **Assertions**: FluentAssertions para legibilidade
5. **Isolamento**: Testes independentes
6. **Nomes Descritivos**: Clareza do objetivo
7. **Single Responsibility**: Um teste, uma responsabilidade
8. **No Magic Numbers**: Valores nomeados e claros

## 🐛 Troubleshooting

### Testes falhando por timeout
```bash
dotnet test --logger "console;verbosity=detailed" --no-build
```

### Testes com database
- Verificar se InMemory database está sendo usada
- Confirmar seed de dados no fixture

### Problemas de autenticação
- Validar token JWT no fixture
- Verificar expiração de token

## 📈 Próximos Passos

1. Executar suite de testes
2. Validar cobertura (objetivo: 80%+)
3. Implementar testes de carga (se necessário)
4. Adicionar testes de API versioning
5. Incluir testes de performance

## 📚 Recursos

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

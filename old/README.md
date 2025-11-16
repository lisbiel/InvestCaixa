# Investment Simulation API - Clean Architecture

API completa de simulação de investimentos desenvolvida com .NET 8, seguindo Clean Architecture e boas práticas de desenvolvimento.

## 🏗️ Arquitetura

Este projeto segue os princípios de **Clean Architecture** (também conhecida como Onion Architecture), garantindo:

- ✅ Separação de responsabilidades
- ✅ Independência de frameworks
- ✅ Testabilidade
- ✅ Independência de UI e banco de dados
- ✅ Princípios SOLID, KISS e DRY

## 🚀 Funcionalidades

- **Simulação de Investimentos**: CDB, LCI, LCA, Tesouro Direto, Fundos
- **Perfil de Risco Dinâmico**: Algoritmo baseado em volume, frequência e preferências
- **Histórico de Simulações**: Rastreamento completo
- **Telemetria**: Monitoramento de performance
- **Autenticação JWT**: Segurança robusta
- **Logging Estruturado**: Serilog com múltiplos sinks
- **Tratamento Global de Exceções**: ProblemDetails

## 📋 Pré-requisitos

- .NET 8.0 SDK
- Docker & Docker Compose
- SQL Server (ou use o container)
- Visual Studio 2022 / VS Code / Rider

## 🔧 Instalação e Configuração

### 1. Setup Automatizado

**Windows (PowerShell):**
```bash
.\setup.ps1
```

**Linux/Mac (Bash):**
```bash
chmod +x setup.sh
./setup.sh
```

### 2. Restaurar Dependências

```bash
dotnet restore
```

### 3. Aplicar Migrations

```bash
cd src/InvestmentSimulation.API
dotnet ef migrations add InitialCreate --project ../InvestmentSimulation.Infrastructure
dotnet ef database update --project ../InvestmentSimulation.Infrastructure
```

### 4. Executar a Aplicação

**Desenvolvimento local:**
```bash
dotnet run --project src/InvestmentSimulation.API
```

**Com Docker Compose:**
```bash
docker-compose up --build
```

A API estará disponível em:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5001`
- Swagger UI: `https://localhost:7001/swagger`

## 🐳 Docker

### Build da Imagem

```bash
docker build -t investment-simulation-api:latest -f src/InvestmentSimulation.API/Dockerfile .
```

### Executar com Docker Compose

```bash
docker-compose up --build
```

Serviços disponíveis:
- **API**: `http://localhost:8080`
- **SQL Server**: `localhost:1433`

## 🧪 Testes

### Executar Todos os Testes

```bash
dotnet test
```

### Testes com Cobertura

```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
```

## 📚 Endpoints Principais

### Autenticação
- `POST /api/auth/login` - Login e geração de token JWT
- `POST /api/auth/refresh` - Renovação de token

### Simulações
- `POST /api/simular-investimento` - Simular investimento
- `GET /api/simulacoes` - Listar todas as simulações
- `GET /api/simulacoes/por-produto-dia` - Simulações por produto/dia

### Perfil de Risco
- `GET /api/perfil-risco/{clienteId}` - Obter perfil de risco
- `GET /api/perfil-risco/produtos-recomendados/{perfil}` - Produtos recomendados

### Telemetria
- `GET /api/telemetria` - Dados de telemetria

## 🔐 Autenticação

1. Obter token:
```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "Admin@123"}'
```

2. Usar token nas requisições:
```bash
curl -X GET https://localhost:7001/api/simulacoes \
  -H "Authorization: Bearer {seu-token}"
```

## 🛠️ Tecnologias Utilizadas

- **.NET 8** - Framework principal
- **Entity Framework Core 8** - ORM
- **Serilog** - Logging estruturado
- **AutoMapper** - Mapeamento de objetos
- **FluentValidation** - Validação de DTOs
- **xUnit** - Framework de testes
- **Moq** - Mocking para testes
- **Swagger/OpenAPI** - Documentação
- **Docker** - Containerização

## 📂 Estrutura do Projeto

```
InvestmentSimulation/
├── src/
│   ├── InvestmentSimulation.Domain/
│   ├── InvestmentSimulation.Application/
│   ├── InvestmentSimulation.Infrastructure/
│   └── InvestmentSimulation.API/
├── tests/
│   └── InvestmentSimulation.UnitTests/
├── docker-compose.yml
├── Dockerfile
├── .gitignore
└── README.md
```

## 📋 Checklist de Implementação

- [x] Clean Architecture / Onion Architecture
- [x] SOLID, KISS, DRY principles
- [x] Domain Layer (Entities, Enums, Interfaces, Exceptions)
- [x] Application Layer (DTOs, Services, Validators, Mappings)
- [x] Infrastructure Layer (DbContext, Repositories, Migrations)
- [x] API Layer (Controllers, Middlewares, Extensions)
- [x] Authentication JWT
- [x] Logging com Serilog
- [x] Global Exception Handling
- [x] Unit Tests com xUnit e Moq
- [x] Docker e Docker Compose
- [x] Swagger/OpenAPI

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT.

## 🙏 Agradecimentos

- Clean Architecture por Robert C. Martin
- Comunidade .NET
- CAIXA - Desafio de Investimentos

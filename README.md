# InvestCaixa - API de Simulação de Investimentos

Uma API de simulação de investimentos de nível produção construída com .NET 8, implementando princípios de **Arquitetura Limpa** com recursos avançados de segurança incluindo **JWT com Refresh Tokens**, **Cache Redis** e telemetria abrangente.

## 🏗️ Visão Geral da Arquitetura

Este projeto segue os princípios de **Arquitetura Limpa** (Onion Architecture), garantindo:

- ✅ **Separação de Responsabilidades**: Limites claros entre camadas
- ✅ **Inversão de Dependências**: Dependências fluem para o interior
- ✅ **Independência de Framework**: Lógica de negócio isolada de frameworks
- ✅ **Testabilidade**: Todos os componentes completamente testáveis
- ✅ **Princípios SOLID**: Responsabilidade Única, Aberto/Fechado, Substituição de Liskov, Segregação de Interface, Inversão de Dependência
- ✅ **DRY & KISS**: Não se Repita, Mantenha Simples

### 🧅 Arquitetura em Camadas

```
┌─────────────────────────────────────────────────┐
│    Camada de API (Apresentação)                 │
│  Controllers, Middlewares, Tratamento Exceções  │
├─────────────────────────────────────────────────┤
│  Camada de Aplicação (Lógica de Negócio)        │
│  DTOs, Services, Validadores, Casos de Uso    │
├─────────────────────────────────────────────────┤
│    Camada de Domínio (Núcleo do Negócio)       │
│  Entities, Interfaces, Enums, Exceções         │
├─────────────────────────────────────────────────┤
│ Camada de Infraestrutura (Serviços Externos)   │
│  Banco de Dados, Cache, Auth, Logging, Email   │
└─────────────────────────────────────────────────┘
```

## 🚀 Funcionalidades Principais

### 🔐 Autenticação & Autorização
- **Tokens JWT**: Autenticação segura baseada em tokens
- **Suporte a Refresh Token**: Sessões de longa duração sem re-autenticação
- **Validação de Token**: Validação abrangente de claims e assinatura
- **Controle de Acesso Baseado em Funções**: Gerenciamento granular de permissões

### ⚡ Performance & Cache
- **Integração Redis**: Cache distribuído para melhor performance
- **Cache Inteligente**: Invalidação estratégica de cache e gerenciamento TTL
- **Connection Pooling**: Conexões otimizadas com banco de dados
- **Async/Await**: E/S não-bloqueante em toda aplicação

### 💼 Funcionalidades de Negócio
- **Simulação de Investimentos**: CDB, LCI, LCA, Tesouro Direto, Fundos
- **Perfil de Risco Dinâmico**: Algoritmo inteligente baseado em volume, frequência e preferências
- **Histórico de Simulações**: Trilha de auditoria completa e rastreamento histórico
- **Motor de Recomendações**: Recomendações de produtos baseadas no perfil de risco

### 📊 Monitoramento & Observabilidade
- **Logging Estruturado**: Serilog com múltiplos sinks
- **Health Checks**: Monitoramento de saúde do serviço e dependências
- **Telemetria**: Métricas de performance e rastreamento
- **Tratamento Global de Exceções**: Respostas de erro consistentes

## 📋 Pré-requisitos

- **.NET 8.0 SDK** ou superior
- **Docker & Docker Compose** (para setup containerizado)
- **SQL Server 2019+** (ou use o container Docker fornecido)
- **Redis 7.0+** (ou use o container Docker fornecido)
- **Visual Studio 2022**, **VS Code** ou **Rider**

## 🔧 Instalação & Configuração

### Opção 1: Início Rápido com Docker Compose (Recomendado)

```bash
# Navegue até a raiz do projeto
cd c:\Desenvolvimento\CSharp\InvesteCaixa

# Inicie todos os serviços (API, SQL Server, Redis)
docker-compose up --build

# A API estará disponível em:
# HTTP:  http://localhost:7148
# Swagger UI: http://localhost:7148/swagger
```

### Opção 2: Setup de Desenvolvimento Local

#### 1. Pré-requisitos
```bash
# Verifique a instalação do .NET
dotnet --version

# Instale EF CLI (se não estiver instalado)
dotnet tool install --global dotnet-ef
```

#### 2. Restaurar Dependências
```bash
dotnet restore
```

#### 3. Configurar Serviços
Crie `appsettings.Development.json` ou defina variáveis de ambiente:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost;Initial Catalog=InvestCaixaDB;Integrated Security=true;TrustServerCertificate=true",
    "Redis": "localhost:6379,abortConnect=false"
  },
  "Jwt": {
    "Secret": "SuaChaveSuperSeguraAqui!",
    "Issuer": "https://localhost:7001",
    "Audience": "InvestCaixa-usoGeral",
    "ExpirationMinutes": 60
  }
}
```

#### 4. Migrations de Banco de Dados
```bash
# Aplique as migrations para criar o schema do banco
dotnet ef database update --project src/InvestCaixa.Infrastructure

# Ou no Console do Gerenciador de Pacotes
Update-Database
```

#### 5. Inicie a API
```bash
cd src/InvestCaixa.API
dotnet run

# A API estará disponível em:
# HTTPS: https://localhost:7148
# HTTP:  http://localhost:5148
# Swagger: https://localhost:7148/swagger
```

#### 6. Inicie o Redis (se usando instância local)
```bash
# Usando Docker
docker run -d -p 6379:6379 redis:7-alpine

# Ou use Windows Subsystem for Linux (WSL)
# Acesso CLI Redis em localhost:6379
```

## 🐳 Deployment Docker

### Visão Geral dos Serviços

O `docker-compose.yml` orquestra três serviços:

| Serviço | Container | Porta | Propósito |
|---------|-----------|-------|----------|
| **API** | investcaixa-api | 7148:8080 | Servidor de Aplicação .NET 8 |
| **SQL Server** | investcaixa-sqlserver | 1431:1433 | Servidor de Banco de Dados |
| **Redis** | investcaixa-redis | 6379 | Cache Distribuído |

### Health Checks

Todos os serviços incluem health checks:

```bash
# Verifique saúde da API
curl http://localhost:7148/health

# Verifique SQL Server
docker exec investcaixa-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P C@ix4V3rs0!

# Verifique Redis
docker exec investcaixa-redis redis-cli ping
```

### Parando Serviços

```bash
# Pare todos os containers
docker-compose down

# Pare e remova volumes (cuidado com os dados!)
docker-compose down -v
```

## 🔐 Fluxo de Autenticação

### Arquitetura JWT & Refresh Token

```
CLIENTE                         SERVIDOR API
  │
  ├─── 1. Login (usuário/senha) ───→
  │                              ├─ Validar credenciais
  │    ← 2. JWT + Refresh Token ──┤
  │                              └─ Cachear refresh token em Redis
  │
  ├─── 3. Requisição API + Bearer JWT ───→
  │                              ├─ Validar assinatura JWT
  │    ← 4. Recurso ──────────────┤
  │                              └─ Conceder acesso
  │
  │ (JWT expira após 60 minutos)
  │
  ├─── 5. Requisição Refresh Token ───→
  │                              ├─ Verificar Redis para token válido
  │    ← 6. Novo JWT + Refresh Token ──┤
  │                              └─ Atualizar cache Redis
```

### Endpoint de Login

**Requisição:**
```bash
curl -X POST http://localhost:7148/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usuario": "admin",
    "senha": "Admin@123"
  }'
```

**Resposta (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "c3RyaW5nLmNvbS4uLg==",
  "expiresIn": 3600,
  "usuario": "admin"
}
```

### Endpoint de Refresh Token

**Requisição:**
```bash
curl -X POST http://localhost:7148/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "c3RyaW5nLmNvbS4uLg=="
  }'
```

**Resposta (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "bmV3LXJlZnJlc2gtdG9rZW4=",
  "expiresIn": 3600,
  "usuario": "admin"
}
```

### Usando JWT nas Requisições

```bash
# Endpoint protegido - deve incluir cabeçalho Authorization
curl -X GET http://localhost:7148/api/simulacoes \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## ⚡ Estratégia de Cache Redis

### Configuração de Cache

**String de Conexão:**
```
redis:6379,abortConnect=false,connectTimeout=5000,syncTimeout=5000
```

**Funcionalidades:**
- Cache automático de validação de token
- Armazenamento de refresh token com expiração
- Cache de recomendações de produtos
- Cache de perfil de risco
- Expiração automática baseada em TTL

### Chaves de Cache

| Padrão de Chave | TTL | Propósito |
|-----------------|-----|----------|
| `refresh_token:{userId}` | 7 dias | Armazenamento de refresh token |
| `jwt_cache:{userId}` | 1 hora | Claims JWT validados |
| `perfil_risco:{clientId}` | 24 horas | Dados do perfil de risco |
| `produtos:{perfil}` | 12 horas | Recomendações de produtos |
| `simulacao:{id}` | 1 hora | Resultados de simulação |

### Operações Manuais de Cache

```csharp
// Injete IDistributedCache
public class MyService 
{
    private readonly IDistributedCache _cache;
    
    public MyService(IDistributedCache cache)
    {
        _cache = cache;
    }
    
    // Definir cache
    await _cache.SetStringAsync("key", "value", 
        new DistributedCacheEntryOptions 
        { 
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) 
        });
    
    // Obter cache
    var value = await _cache.GetStringAsync("key");
    
    // Remover cache
    await _cache.RemoveAsync("key");
}
```

## 📚 Endpoints da API

### Endpoints de Autenticação

| Método | Endpoint | Auth | Descrição |
|--------|----------|------|----------|
| POST | `/api/auth/login` | ❌ Não | Gerar JWT & Refresh Token |
| POST | `/api/auth/refresh` | ❌ Não | Renovar JWT usando Refresh Token |

### Endpoints de Simulação de Investimentos

| Método | Endpoint | Auth | Descrição |
|--------|----------|------|----------|
| POST | `/api/simulacoes` | ✅ JWT | Criar nova simulação |
| GET | `/api/simulacoes` | ✅ JWT | Listar todas as simulações |
| GET | `/api/simulacoes/{id}` | ✅ JWT | Obter detalhes da simulação |
| GET | `/api/simulacoes/por-produto-dia` | ✅ JWT | Obter simulações por produto/dia |

### Endpoints de Perfil de Risco

| Método | Endpoint | Auth | Descrição |
|--------|----------|------|----------|
| GET | `/api/perfil-risco/{clienteId}` | ✅ JWT | Obter perfil de risco do cliente |
| GET | `/api/perfil-risco/produtos-recomendados/{perfil}` | ✅ JWT | Obter produtos recomendados |
| POST | `/api/perfil-risco` | ✅ JWT | Criar novo perfil de risco |
| PUT | `/api/perfil-risco/{clienteId}` | ✅ JWT | Atualizar perfil de risco |

### Endpoints de Perfil Financeiro

| Método | Endpoint | Auth | Descrição |
|--------|----------|------|----------|
| POST | `/api/perfil-financeiro` | ✅ JWT | Criar perfil financeiro |
| GET | `/api/perfil-financeiro/{clienteId}` | ✅ JWT | Obter perfil financeiro |

### Endpoints de Investimentos

| Método | Endpoint | Auth | Descrição |
|--------|----------|------|----------|
| POST | `/api/investimentos/finalizar` | ✅ JWT | Finalizar investimento |
| GET | `/api/investimentos/historico/{clienteId}` | ✅ JWT | Obter histórico de investimentos |

### Endpoints de Telemetria

| Método | Endpoint | Auth | Descrição |
|--------|----------|------|----------|
| GET | `/api/telemetria` | ✅ JWT | Obter métricas de telemetria |
| GET | `/health` | ❌ Não | Verificação de saúde do serviço |

## 🧪 Testing

### Running All Tests

```bash
# Run all unit tests
dotnet test

# Run with detailed output
dotnet test -v d

# Run specific test project
dotnet test tests/InvestCaixa.UnitTests/InvestCaixa.UnitTests.csproj
```

### Code Coverage Report

```bash
# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover

# View HTML report
start coverage/index.html
```

### Test Structure

```
tests/
└── InvestCaixa.UnitTests/
    ├── Controllers/
    ├── Services/
    ├── Validators/
    ├── Fixtures/
    └── README.md
```

## 🛠️ Technologies & Dependencies

### Core Framework
- **.NET 8.0** - Latest LTS framework
- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core 8** - ORM for data access

### Authentication & Security
- **System.IdentityModel.Tokens.Jwt** - JWT token generation/validation
- **Microsoft.IdentityModel.Tokens** - Token validation parameters
- **BCrypt.Net-Next** - Secure password hashing

### Caching & Performance
- **StackExchange.Redis** - Redis client library
- **Microsoft.Extensions.Caching.StackExchangeRedis** - Redis distributed cache provider

### Data Validation
- **FluentValidation** - Model validation with fluent API
- **System.ComponentModel.DataAnnotations** - Attribute-based validation

### Logging & Monitoring
- **Serilog** - Structured logging
- **Serilog.Sinks.File** - File sink
- **Serilog.Sinks.Console** - Console sink
- **Serilog.Enrichers.Environment** - Environment data enrichment

### Mapping
- **AutoMapper** - Object-to-object mapping
- **AutoMapper.Extensions.Microsoft.DependencyInjection** - DI integration

### Testing
- **xUnit** - Testing framework
- **Moq** - Mocking library
- **FluentAssertions** - Fluent assertion library

### API Documentation
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI implementation

### Database
- **SQL Server** - Primary database (or SQLite for local development)
- **Microsoft.Data.SqlClient** - SQL Server data provider

## 📂 Project Structure

```
InvesteCaixa/
├── src/
│   ├── InvestCaixa.Domain/
│   │   ├── Entities/           # Core business entities
│   │   ├── Enums/              # Business enumerations
│   │   ├── Interfaces/         # Domain contracts
│   │   └── Exceptions/         # Custom exceptions
│   │
│   ├── InvestCaixa.Application/
│   │   ├── DTOs/
│   │   │   ├── Request/        # Input models
│   │   │   └── Response/       # Output models
│   │   ├── Services/           # Business logic
│   │   ├── Validators/         # FluentValidation rules
│   │   ├── Mappings/           # AutoMapper profiles
│   │   ├── Interfaces/         # Application contracts
│   │   └── Exceptions/         # Application exceptions
│   │
│   ├── InvestCaixa.Infrastructure/
│   │   ├── Data/
│   │   │   ├── Context.cs      # DbContext
│   │   │   └── Migrations/     # EF Migrations
│   │   ├── Repositories/       # Data access
│   │   ├── Services/
│   │   │   ├── JwtTokenService.cs
│   │   │   └── AuthService.cs
│   │   ├── Configurations/     # Entity configurations
│   │   └── HealthChecks/       # Custom health checks
│   │
│   └── InvestCaixa.API/
│       ├── Controllers/        # HTTP endpoints
│       ├── Middlewares/        # Custom middleware
│       ├── Extensions/         # Extension methods
│       ├── Program.cs          # Startup configuration
│       └── appsettings*.json   # Configuration files
│
├── tests/
│   └── InvestCaixa.UnitTests/
│       ├── Controllers/
│       ├── Services/
│       ├── Validators/
│       ├── Fixtures/
│       └── *.cs                # Test files
│
├── docker-compose.yml          # Docker orchestration
├── Dockerfile                  # Container image definition
├── .env                        # Environment variables
├── .env.example               # Environment template
└── README.md                  # This file
```

## 🔑 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=InvestCaixa.db",
    "Redis": "localhost:6379,abortConnect=false"
  },
  "Jwt": {
    "Secret": "Your-Super-Secret-Key-Min-32-Chars",
    "Issuer": "https://localhost:7148",
    "Audience": "InvestCaixa-usoGeral",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Environment Variables (.env)
```
JWT_SECRET=Your-Super-Secret-Key-Min-32-Chars
SA_PASSWORD=YourSqlServerPassword
ASPNETCORE_ENVIRONMENT=Development
```

## 🚀 Deployment

### Production Checklist

- [ ] Update JWT secret in environment variables
- [ ] Configure strong database password
- [ ] Enable HTTPS only
- [ ] Configure CORS for frontend domain
- [ ] Set up structured logging to persistent storage
- [ ] Configure health check monitoring
- [ ] Enable database backups
- [ ] Set up Redis persistence
- [ ] Configure API rate limiting
- [ ] Enable API versioning
- [ ] Set up CI/CD pipeline

### Docker Production Build

```bash
# Build production image
docker build -t investcaixa-api:1.0.0 -f Dockerfile .

# Tag and push to registry
docker tag investcaixa-api:1.0.0 registry.example.com/investcaixa-api:1.0.0
docker push registry.example.com/investcaixa-api:1.0.0
```

### Kubernetes Deployment (Optional)

```bash
# Deploy using Kubernetes
kubectl apply -f k8s/

# Check deployment status
kubectl get pods
kubectl logs -f deployment/investcaixa-api
```

## 🐛 Troubleshooting

### Common Issues

**1. Redis Connection Failing**
```bash
# Check Redis is running
docker ps | grep redis

# Restart Redis
docker-compose restart redis

# Test connection
redis-cli ping
```

**2. SQL Server Connection Issues**
```bash
# Verify SQL Server container
docker ps | grep sqlserver

# Check logs
docker logs investcaixa-sqlserver

# Reset database
docker-compose down -v
docker-compose up -d
```

**3. JWT Token Validation Errors**
- Verify JWT_SECRET is set correctly
- Check token hasn't expired
- Validate Authorization header format: `Bearer <token>`

**4. Cache Not Working**
- Verify Redis connection string
- Check Redis is running: `redis-cli ping`
- Review Redis logs: `docker logs investcaixa-redis`

## 📖 Documentation

### API Documentation
- Swagger UI: `http://localhost:7148/swagger`
- ReDoc (alternative): `http://localhost:7148/api-docs`

### Additional Resources
- [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [Redis Documentation](https://redis.io/docs/)
- [Entity Framework Core Docs](https://docs.microsoft.com/en-us/ef/core/)

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Code Standards
- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use async/await patterns
- Add XML documentation to public members
- Write unit tests for new features
- Ensure code coverage > 80%

## 📋 Checklist

- [x] Clean Architecture / Onion Architecture
- [x] SOLID & DRY principles
- [x] Domain Layer (Entities, Enums, Interfaces, Exceptions)
- [x] Application Layer (DTOs, Services, Validators, Mappings)
- [x] Infrastructure Layer (DbContext, Repositories, Migrations)
- [x] API Layer (Controllers, Middlewares, Extensions)
- [x] **JWT Authentication with Access Tokens**
- [x] **Refresh Token Support**
- [x] **Token Storage in Redis Cache**
- [x] **Distributed Caching Strategy**
- [x] Structured Logging (Serilog)
- [x] Global Exception Handling
- [x] Health Checks
- [x] Unit Tests (xUnit + Moq)
- [x] API Documentation (Swagger)
- [x] Docker & Docker Compose
- [x] Database Migrations
- [x] Input Validation (FluentValidation)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## 🙏 Acknowledgments

- [Robert C. Martin](https://en.wikipedia.org/wiki/Robert_C._Martin) - Clean Architecture concepts
- [Microsoft .NET Team](https://github.com/dotnet) - Excellent framework and tooling
- [Redis Labs](https://redis.io/) - Outstanding caching platform
- Community contributors and testers

## 📞 Support

For questions or issues:
- 📧 Email: support@investcaixa.example.com
- 🐛 Issues: [GitHub Issues](https://github.com/lisbiel/InvesteCaixa/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/lisbiel/InvesteCaixa/discussions)

---

**Last Updated:** November 20, 2025  
**Version:** 2.0.0  
**Status:** ✅ Production Ready

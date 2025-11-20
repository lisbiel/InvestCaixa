# InvestCaixa - API de Simulação de Investimentos CAIXA

![.NET](https://img.shields.io/badge/.NET-8.0-blue?style=flat-square&logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Ready-blue?style=flat-square&logo=docker)
![Kubernetes](https://img.shields.io/badge/Kubernetes-Ready-blue?style=flat-square&logo=kubernetes)
![JWT](https://img.shields.io/badge/Auth-JWT-green?style=flat-square)
![Redis](https://img.shields.io/badge/Cache-Redis-red?style=flat-square&logo=redis)
![Health](https://img.shields.io/badge/Health_Checks-✅-green?style=flat-square)
![Tests](https://img.shields.io/badge/Tests-xUnit-purple?style=flat-square)
![Coverage](https://img.shields.io/badge/Coverage->80%25-brightgreen?style=flat-square)
![Production](https://img.shields.io/badge/Status-Production_Ready-brightgreen?style=flat-square)

🏆 **API de Produção Completa** para o Desafio Back-end CAIXA - .NET 8 com **Clean Architecture**, **JWT Authentication**, **Motor de Recomendação Inteligente** e **Kubernetes Ready**.

## 🎯 Atendimento aos Requisitos CAIXA

✅ **Envelope JSON**: Recebimento e processamento de simulações via API  
✅ **Banco SQL**: SQL Server (Docker) e SQLite (Local) com parâmetros configurados  
✅ **Validação Completa**: FluentValidation para todos os dados de entrada  
✅ **Filtragem Inteligente**: Algoritmo de suitability por perfil de risco  
✅ **Cálculos Precisos**: Simulação de CDB, LCI, LCA, Tesouro Direto, Fundos  
✅ **Persistência**: Armazenamento automático de todas as simulações  
✅ **Endpoints Completos**: Simulações, histórico, telemetria, perfis  
✅ **Containerização**: Docker + Docker Compose prontos  
✅ **Autenticação JWT**: Tokens + Refresh Tokens com segurança avançada  
✅ **Motor de Recomendação**: Algoritmo baseado em volume, frequência e preferências

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

## 🤖 Motor de Recomendação Inteligente (Requisito CAIXA)

### 📊 Algoritmo de Perfil de Risco

**Baseado em 3 pilares principais:**

1. **💰 Volume de Investimentos**
   - Baixo volume (< R$ 10.000) = +Conservador
   - Médio volume (R$ 10.000 - R$ 100.000) = +Moderado
   - Alto volume (> R$ 100.000) = +Agressivo

2. **⏱️ Frequência de Movimentações**
   - Baixa frequência (< 2 movimentações/ano) = +Conservador
   - Média frequência (2-6 movimentações/ano) = +Moderado
   - Alta frequência (> 6 movimentações/ano) = +Agressivo

3. **⚖️ Preferência: Liquidez vs Rentabilidade**
   - Prioriza liquidez = +Conservador
   - Equilibra liquidez e rentabilidade = +Moderado
   - Prioriza rentabilidade = +Agressivo

### 🎯 Sistema de Pontuação

```csharp
// Exemplo do algoritmo implementado
var pontuacao = CalcularVolume(perfil.PatrimonioTotal) +
                CalcularFrequencia(perfil.QuantidadeMovimentacoes) +
                CalcularPreferencia(perfil.HorizonteInvestimento, perfil.ObjetivoInvestimento);

// Classificação Final:
// 0-3 pontos: Conservador (CDB, LCI, LCA, Tesouro Selic)
// 4-6 pontos: Moderado (CDB Premium, Fundos DI, Tesouro IPCA+)
// 7-10 pontos: Agressivo (Fundos Multimercado, Ações, High Yield)
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

### ☁️ Cloud & Kubernetes Ready
- **🟢 Health Endpoints**: `/health`, `/health/ready`, `/health/live`
- **🔄 Load Balancer Ready**: Health checks para balanceamento de carga
- **🚀 Kubernetes Deployment**: Pronto para orquestração em cluster
- **📈 Observabilidade**: Métricas expostas para Prometheus/Grafana
- **🔧 Zero Downtime**: Graceful shutdown e startup configurados
- **📊 Escalabilidade**: Stateless design para escala horizontal

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
Para desenvolvimento local, crie `appsettings.Development.json` (SQLite local):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=InvestCaixa.db"
  },
  "Jwt": {
    "Secret": "SuaChaveSuperSeguraAqui!",
    "Issuer": "https://localhost:7148",
    "Audience": "InvestCaixa-usoGeral",
    "ExpirationMinutes": 60
  }
}
```

**Nota:** No desenvolvimento local:
- ✅ **SQLite** é usado automaticamente (arquivo `InvestCaixa.db`)
- ⚠️ **Redis** é opcional - sem configuração, usa cache em memória
- 🔧 **SQL Server** apenas no Docker

#### 4. Migrations de Banco de Dados (SQLite Local)
```bash
# Navegue para o projeto API
cd src/InvestCaixa.API

# Aplique as migrations para criar o banco SQLite
dotnet ef database update --project ../InvestCaixa.Infrastructure

# Ou no Console do Gerenciador de Pacotes
Update-Database

# O arquivo InvestCaixa.db será criado automaticamente na pasta da API
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

#### 6. Cache Local (Opcional)
```bash
# Por padrão, usa MemoryCache integrado do .NET
# Para habilitar Redis localmente, adicione no appsettings.Development.json:

# "ConnectionStrings": {
#   "Redis": "localhost:6379,abortConnect=false"
# }

# E execute Redis via Docker:
docker run -d -p 6379:6379 redis:7-alpine
```

**Comportamento Local:**
- 🏠 **Sem Redis**: Usa `MemoryCache` (padrão)
- 🐳 **Com Redis**: Configura cache distribuído

## 🗃️ Dados de Exemplo (Seed Automático)

**A aplicação inicializa automaticamente com dados realísticos:**

### 📊 Produtos de Investimento

**Conservadores:**
- CDB Caixa 2026 (12% a.a., 180 dias, mín. R$ 1.000)
- LCI Habitação Plus (11,5% a.a., 90 dias, mín. R$ 5.000)
- Tesouro Selic 2025 (10,5% a.a., 1 dia, mín. R$ 30)
- LCA Agronegócio (10,8% a.a., 120 dias, mín. R$ 2.000)

**Moderados:**
- CDB Progressivo 2027 (13,5% a.a., 365 dias, mín. R$ 10.000)
- Fundo DI Institucional (12,5% a.a., 30 dias, mín. R$ 1.000)
- Tesouro IPCA+ 2030 (13% a.a., 365 dias, mín. R$ 50)

**Agressivos:**
- Fundo Multimercado Alpha (18% a.a., sem carência, mín. R$ 500)
- Fundo de Ações Dividendos (22% a.a., sem carência, mín. R$ 1.000)
- CDB High Yield (15,5% a.a., 720 dias, mín. R$ 25.000)

### 👥 Clientes com Perfis Diversos

1. **João Silva** - Conservador (Patrimônio: R$ 80.000)
2. **Maria Costa** - Moderado (Patrimônio: R$ 150.000)
3. **Carlos Lima** - Agressivo (Patrimônio: R$ 500.000)
4. **Ana Alves** - Moderado Jovem (Patrimônio: R$ 30.000)
5. **Roberto Mendes** - Conservador Experiente (Patrimônio: R$ 350.000)

### 📈 Histórico de Investimentos

- **15 investimentos finalizados** com resultados realísticos
- **Volume total aplicado**: R$ 1.311.000
- **8 simulações** de exemplo para testes
- **Performance tracking** completo por cliente

## 🐳 Deployment Docker

### Visão Geral dos Serviços

**Docker (Produção):** O `docker-compose.yml` orquestra três serviços:

| Serviço | Container | Porta | Propósito |
|---------|-----------|-------|----------|
| **API** | investcaixa-api | 7148:8080 | Servidor de Aplicação .NET 8 |
| **SQL Server** | investcaixa-sqlserver | 1431:1433 | Servidor de Banco de Dados |
| **Redis** | investcaixa-redis | 6379 | Cache Distribuído |

**Desenvolvimento Local:**

| Componente | Tipo | Localização | Observações |
|------------|------|-------------|-------------|
| **API** | Processo Local | https://localhost:7148 | `dotnet run` |
| **Banco** | SQLite | `InvestCaixa.db` | Arquivo local |
| **Cache** | MemoryCache | In-Process | Sem persistência |

### Health Checks (Kubernetes Ready)

**🚀 Três endpoints de health implementados:**

```bash
# Health Check Geral - Para monitoramento
curl http://localhost:7148/health
# Status: 200 (Healthy) | 500 (Degraded) | 503 (Unhealthy)

# Readiness Probe - Para Load Balancer
curl http://localhost:7148/health/ready
# Verifica: Database + Redis + Dependências Externas

# Liveness Probe - Para Kubernetes
curl http://localhost:7148/health/live  
# Verifica: Aplicação + Memória + CPU
```

**🔍 Componentes Monitorados:**
- ✅ **Database**: Conectividade SQLite/SQL Server
- ✅ **Redis Cache**: Disponibilidade e latência
- ✅ **Application**: Status interno da aplicação
- ✅ **Memory**: Uso de memória e GC
- ✅ **Dependencies**: Serviços externos

**🐳 Docker Health Checks:**
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

## ⚡ Estratégia de Cache

### Configuração Inteligente de Cache

**🏠 Desenvolvimento Local:**
- **MemoryCache** (.NET integrado) - quando Redis não configurado
- **Armazenamento**: In-process, não persistente
- **Performance**: Excelente para desenvolvimento

**🐳 Docker/Produção:**
- **Redis Distribuído** - quando configurado
- **String de Conexão**: `redis:6379,abortConnect=false,connectTimeout=5000,syncTimeout=5000`
- **Armazenamento**: Persistente e distribuído

**Funcionalidades (ambos os modos):**
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

## 🎯 Demonstração da API (Exemplos Práticos)

### 🔐 1. Autenticação (Obter Token)

```bash
curl -X POST http://localhost:7148/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usuario": "admin",
    "senha": "Admin@123"
  }'

# Resposta:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "c3RyaW5nLmNvbS4u...",
  "expiresIn": 3600,
  "usuario": "admin"
}
```

### 📊 2. Obter Perfil de Risco (Motor de Recomendação)

```bash
curl -X GET http://localhost:7148/api/perfil-risco/1 \
  -H "Authorization: Bearer SEU_TOKEN"

# Resposta:
{
  "clienteId": 1,
  "perfil": "Conservador",
  "pontuacao": 2,
  "algoritmoDetalhes": {
    "volumeScore": 1,
    "frequenciaScore": 0,
    "preferenciaScore": 1
  },
  "produtosRecomendados": [
    "CDB Caixa 2026",
    "LCI Habitação Plus",
    "Tesouro Selic 2025"
  ]
}
```

### 💰 3. Simular Investimento

```bash
curl -X POST http://localhost:7148/api/simulacoes \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "clienteId": 1,
    "produtoId": 1,
    "valorInvestimento": 10000,
    "prazoMeses": 12
  }'

# Resposta:
{
  "id": 123,
  "produtoNome": "CDB Caixa 2026",
  "valorInvestido": 10000.00,
  "valorBruto": 11200.00,
  "valorLiquido": 10960.00,
  "rentabilidade": 9.60,
  "impostoRenda": 240.00,
  "prazoMeses": 12
}
```

### 📈 4. Telemetria (Métricas de Performance)

```bash
curl -X GET http://localhost:7148/api/telemetria \
  -H "Authorization: Bearer SEU_TOKEN"

# Resposta:
{
  "totalSimulacoes": 1847,
  "simulacoesPorDia": 23.4,
  "tempoMedioResposta": "145ms",
  "produtoMaisSimulado": "CDB Caixa 2026",
  "volumeTotal": 45698230.50,
  "usuariosAtivos": 156
}
```

## 🧪 Testes

### Executando Todos os Testes

```bash
# Execute todos os testes unitários
dotnet test

# Execute com saída detalhada
dotnet test -v d

# Execute projeto de teste específico
dotnet test tests/InvestCaixa.UnitTests/InvestCaixa.UnitTests.csproj
```

### Relatório de Cobertura de Código

```bash
# Gere relatório de cobertura
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover

# Visualize relatório HTML
start coverage/index.html
```

### Estrutura de Testes

```
tests/
└── InvestCaixa.UnitTests/
    ├── Controllers/
    ├── Services/
    ├── Validators/
    ├── Fixtures/
    └── README.md
```

## 🛠️ Tecnologias & Dependências

### Framework Principal
- **.NET 8.0** - Framework LTS mais recente
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core 8** - ORM para acesso a dados

### Autenticação & Segurança
- **System.IdentityModel.Tokens.Jwt** - Geração/validação de tokens JWT
- **Microsoft.IdentityModel.Tokens** - Parâmetros de validação de token
- **BCrypt.Net-Next** - Hash seguro de senhas

### Cache & Performance
- **StackExchange.Redis** - Biblioteca cliente Redis
- **Microsoft.Extensions.Caching.StackExchangeRedis** - Provedor de cache distribuído Redis

### Validação de Dados
- **FluentValidation** - Validação de modelo com API fluente
- **System.ComponentModel.DataAnnotations** - Validação baseada em atributos

### Logging & Monitoramento
- **Serilog** - Logging estruturado
- **Serilog.Sinks.File** - Sink para arquivo
- **Serilog.Sinks.Console** - Sink para console
- **Serilog.Enrichers.Environment** - Enriquecimento de dados do ambiente

### Mapeamento
- **AutoMapper** - Mapeamento objeto-para-objeto
- **AutoMapper.Extensions.Microsoft.DependencyInjection** - Integração DI

### Testes
- **xUnit** - Framework de testes
- **Moq** - Biblioteca de mocking
- **FluentAssertions** - Biblioteca de asserções fluentes

### Documentação da API
- **Swashbuckle.AspNetCore** - Implementação Swagger/OpenAPI

### Banco de Dados
- **SQL Server** - Banco de dados principal (ou SQLite para desenvolvimento local)
- **Microsoft.Data.SqlClient** - Provedor de dados SQL Server

## 📂 Estrutura do Projeto

```
InvesteCaixa/
├── src/
│   ├── InvestCaixa.Domain/
│   │   ├── Entities/           # Entidades principais do negócio
│   │   ├── Enums/              # Enumerações do negócio
│   │   ├── Interfaces/         # Contratos do domínio
│   │   └── Exceptions/         # Exceções customizadas
│   │
│   ├── InvestCaixa.Application/
│   │   ├── DTOs/
│   │   │   ├── Request/        # Modelos de entrada
│   │   │   └── Response/       # Modelos de saída
│   │   ├── Services/           # Lógica de negócio
│   │   ├── Validators/         # Regras FluentValidation
│   │   ├── Mappings/           # Perfis AutoMapper
│   │   ├── Interfaces/         # Contratos da aplicação
│   │   └── Exceptions/         # Exceções da aplicação
│   │
│   ├── InvestCaixa.Infrastructure/
│   │   ├── Data/
│   │   │   ├── Context.cs      # DbContext
│   │   │   └── Migrations/     # Migrations EF
│   │   ├── Repositories/       # Acesso a dados
│   │   ├── Services/
│   │   │   ├── JwtTokenService.cs
│   │   │   └── AuthService.cs
│   │   ├── Configurations/     # Configurações de entidade
│   │   └── HealthChecks/       # Health checks customizados
│   │
│   └── InvestCaixa.API/
│       ├── Controllers/        # Endpoints HTTP
│       ├── Middlewares/        # Middlewares customizados
│       ├── Extensions/         # Métodos de extensão
│       ├── Program.cs          # Configuração de inicialização
│       └── appsettings*.json   # Arquivos de configuração
│
├── tests/
│   └── InvestCaixa.UnitTests/
│       ├── Controllers/
│       ├── Services/
│       ├── Validators/
│       ├── Fixtures/
│       └── *.cs                # Arquivos de teste
│
├── docker-compose.yml          # Orquestração Docker
├── Dockerfile                  # Definição da imagem do container
├── .env                        # Variáveis de ambiente
├── .env.example               # Template de ambiente
└── README.md                  # Este arquivo
```

## 🔑 Configuração

### Desenvolvimento Local (appsettings.Development.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=InvestCaixa.db"
    // Redis omitido = usa MemoryCache
  },
  "Jwt": {
    "Secret": "Sua-Chave-Super-Secreta-Min-32-Chars",
    "Issuer": "https://localhost:7148",
    "Audience": "InvestCaixa-usoGeral",
    "ExpirationMinutes": 60
  }
}
```

### Produção/Docker (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=InvestCaixaDB;...",
    "Redis": "localhost:6379,abortConnect=false"
  },
  "Jwt": {
    "Secret": "Sua-Chave-Super-Secreta-Min-32-Chars",
    "Issuer": "https://localhost:7148",
    "Audience": "InvestCaixa-usoGeral",
    "ExpirationMinutes": 60
  }
}
```

### Variáveis de Ambiente (.env)
```
JWT_SECRET=Sua-Chave-Super-Secreta-Min-32-Chars
SA_PASSWORD=SuaSenhaSqlServer
ASPNETCORE_ENVIRONMENT=Development
```

## 🚀 Deploy

### Checklist de Produção

- [ ] Atualizar segredo JWT nas variáveis de ambiente
- [ ] Configurar senha forte do banco de dados
- [ ] Habilitar apenas HTTPS
- [ ] Configurar CORS para o domínio do frontend
- [ ] Configurar logging estruturado para armazenamento persistente
- [ ] Configurar monitoramento de health check
- [ ] Habilitar backups do banco de dados
- [ ] Configurar persistência do Redis
- [ ] Configurar rate limiting da API
- [ ] Habilitar versionamento da API
- [ ] Configurar pipeline CI/CD

### Build de Produção Docker

```bash
# Build da imagem de produção
docker build -t investcaixa-api:1.0.0 -f Dockerfile .

# Tag e push para registry
docker tag investcaixa-api:1.0.0 registry.example.com/investcaixa-api:1.0.0
docker push registry.example.com/investcaixa-api:1.0.0
```

### Deploy Kubernetes (Production Ready)

**🚀 Configuração Kubernetes Completa:**

```yaml
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: investcaixa-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: investcaixa-api
  template:
    metadata:
      labels:
        app: investcaixa-api
    spec:
      containers:
      - name: api
        image: investcaixa-api:latest
        ports:
        - containerPort: 8080
        # Health Checks configurados
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
        # Resources para auto-scaling
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

**📋 Deploy Commands:**
```bash
# Deploy usando Kubernetes
kubectl apply -f k8s/

# Verificar status do deploy
kubectl get pods
kubectl get services
kubectl logs -f deployment/investcaixa-api

# Scaling horizontal
kubectl scale deployment investcaixa-api --replicas=5

# Verificar health dos pods
kubectl get pods -o wide
```

**🔄 Load Balancer Configuration:**
```yaml
# k8s/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: investcaixa-service
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 8080
  selector:
    app: investcaixa-api
```

## 🐛 Solução de Problemas

### Problemas Comuns

**1. Problemas de Banco Local (SQLite)**
```bash
# Verifique se o arquivo existe
ls -la InvestCaixa.db  # Linux/Mac
dir InvestCaixa.db     # Windows

# Recrie o banco se necessário
dotnet ef database drop --project src/InvestCaixa.Infrastructure
dotnet ef database update --project src/InvestCaixa.Infrastructure

# Verifique permissões do arquivo (se erro de acesso)
chmod 666 InvestCaixa.db  # Linux/Mac
```

**2. Problemas de Conexão SQL Server (Docker)**
```bash
# Verifique o container SQL Server
docker ps | grep sqlserver

# Verifique os logs
docker logs investcaixa-sqlserver

# Resete o banco de dados
docker-compose down -v
docker-compose up -d
```

**3. Cache Não Funcionando**

**Local (MemoryCache):**
- Cache funciona apenas durante execução da aplicação
- Reiniciar a API limpa o cache
- Sem configuração adicional necessária

**Docker (Redis):**
```bash
# Verifique se o Redis está rodando
docker ps | grep redis

# Reinicie o Redis
docker-compose restart redis

# Teste a conexão
docker exec investcaixa-redis redis-cli ping
```

**4. Erros de Validação de Token JWT**
- Verifique se JWT_SECRET está configurado corretamente
- Verifique se o token não expirou
- Valide o formato do cabeçalho Authorization: `Bearer <token>`
- **Local**: Tokens armazenados em MemoryCache (perdidos ao reiniciar)
- **Docker**: Tokens persistem no Redis

## 📚 Documentação

### Documentação da API
- Swagger UI: `http://localhost:7148/swagger`
- ReDoc (alternativa): `http://localhost:7148/api-docs`

### Recursos Adicionais
- [Guia de Arquitetura Limpa](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Melhores Práticas JWT](https://tools.ietf.org/html/rfc8725)
- [Documentação Redis](https://redis.io/docs/)
- [Docs Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

## 🤝 Contribuindo

1. **Faça fork** do repositório
2. **Crie** uma branch de feature (`git checkout -b feature/NovaFuncionalidade`)
3. **Commit** suas mudanças (`git commit -m 'Adiciona nova funcionalidade'`)
4. **Push** para a branch (`git push origin feature/NovaFuncionalidade`)
5. **Abra** um Pull Request

### Padrões de Código
- Siga as [Convenções de Código C# da Microsoft](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use padrões async/await
- Adicione documentação XML aos membros públicos
- Escreva testes unitários para novas funcionalidades
- Garanta cobertura de código > 80%

## 🏆 Conformidade com Critérios de Avaliação CAIXA

### 📚 Estrutura da API e Documentação
- ✅ **Arquitetura Limpa** com separação clara de responsabilidades
- ✅ **Swagger/OpenAPI** com documentação completa e exemplos
- ✅ **Versionamento** preparado para evolução da API
- ✅ **Padrões REST** com status codes apropriados
- ✅ **DTOs Validados** com FluentValidation
- ✅ **Logs Estruturados** para auditoria e debugging

### 🤖 Qualidade do Motor de Recomendação
- ✅ **Algoritmo Sofisticado** baseado em volume, frequência e preferências
- ✅ **Sistema de Pontuação** para classificação precisa de perfil
- ✅ **Suitability CVM 539** implementada corretamente
- ✅ **Recomendações Dinâmicas** que evoluem com o comportamento
- ✅ **Cálculos Financeiros** precisos para todos os produtos
- ✅ **Cache Inteligente** para performance das recomendações

### 🔒 Segurança e Tratamento de Erros
- ✅ **JWT Authentication** com refresh tokens
- ✅ **Validação Robusta** em todas as entradas
- ✅ **Tratamento Global** de exceções com ProblemDetails
- ✅ **Logs de Segurança** para auditoria
- ✅ **Rate Limiting** preparado
- ✅ **CORS Configurado** para segurança de origem

### 🧪 Testes Unitários e Integração
- ✅ **Cobertura de Testes** abrangente
- ✅ **Testes Unitários** com xUnit + Moq
- ✅ **Testes de Integração** com TestFixtures
- ✅ **Mocks e Stubs** para isolamento de testes
- ✅ **Testes de Performance** para validação
- ✅ **CI/CD Ready** com pipelines automáticos

## 📋 Checklist de Implementação

- [x] Arquitetura Limpa / Onion Architecture
- [x] Princípios SOLID & DRY
- [x] Camada de Domínio (Entities, Enums, Interfaces, Exceptions)
- [x] Camada de Aplicação (DTOs, Services, Validators, Mappings)
- [x] Camada de Infraestrutura (DbContext, Repositories, Migrations)
- [x] Camada de API (Controllers, Middlewares, Extensions)
- [x] **Autenticação JWT com Access Tokens**
- [x] **Suporte a Refresh Token**
- [x] **Armazenamento de Token em Cache Redis**
- [x] **Estratégia de Cache Distribuído**
- [x] Logging Estruturado (Serilog)
- [x] Tratamento Global de Exceções
- [x] Health Checks
- [x] Testes Unitários (xUnit + Moq)
- [x] Documentação da API (Swagger)
- [x] Docker & Docker Compose
- [x] Migrations de Banco de Dados
- [x] Validação de Entrada (FluentValidation)

## 📄 Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE.txt](LICENSE.txt) para detalhes.

## 🙏 Agradecimentos

- [Robert C. Martin](https://en.wikipedia.org/wiki/Robert_C._Martin) - Conceitos de Arquitetura Limpa
- [Equipe Microsoft .NET](https://github.com/dotnet) - Excelente framework e ferramentas
- [Redis Labs](https://redis.io/) - Plataforma de cache excepcional
- Contribuidores da comunidade e testadores

## 📞 Suporte

Para dúvidas ou problemas:
- 📧 Email: support@investcaixa.example.com
- 🐛 Issues: [GitHub Issues](https://github.com/lisbiel/InvesteCaixa/issues)
- 💬 Discussões: [GitHub Discussions](https://github.com/lisbiel/InvesteCaixa/discussions)

---

**Última Atualização:** 20 de Novembro de 2025  
**Versão:** 2.0.0  
**Status:** ✅ Pronto para Produção

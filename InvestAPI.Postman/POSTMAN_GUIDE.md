# 📮 Postman Collection - InvesteCaixa API

**Arquivo:** `InvestCaixa-API.postman_collection.json`

Esta é uma collection completa e funcional para testar todos os endpoints da API de Investimentos.

---

## 🚀 Quick Start (5 minutos)

### 1. Importar a Collection

```bash
1. Abrir Postman
2. Click em "Import" (canto superior esquerdo)
3. Selecionar: InvestCaixa-API.postman_collection.json
4. Click em "Import"
```

### 2. Configurar Variáveis de Ambiente

As variáveis já estão pré-configuradas, mas você pode editar:

```
base_url     → http://localhost:7148  (ou seu host)
jwt_token    → (auto-preenchido após login)
refresh_token → (auto-preenchido após login)
```

### 3. Executar Login (OBRIGATÓRIO)

```
1. Ir para: 🔐 Autenticação → 1. Login - Obter JWT Token
2. Click em "Send"
3. Resposta esperada: 200 OK com token JWT
4. ✅ Token é automaticamente salvo na variável jwt_token
```

### 4. Usar Qualquer Endpoint

Todos os endpoints estão organizados em pastas:
- 🔐 Autenticação
- 👥 Perfil Financeiro
- 📊 Perfil de Risco
- 💰 Simulações de Investimento
- 💎 Investimentos Finalizados
- 📈 Telemetria & Monitoramento

**Exemplo:**
```
1. Click em: 💰 Simulações → 2. Simular Investimento
2. Click em "Send"
3. Resposta: Simulação com produto recomendado por perfil
```

---

## 📚 Estrutura da Collection

### 🔐 Autenticação (2 endpoints)

| # | Endpoint | Método | Descrição |
|----|----------|--------|----------|
| 1 | `/api/auth/login` | POST | Fazer login e obter JWT token |
| 2 | `/api/auth/refresh` | POST | Renovar JWT usando refresh token |

**Fluxo de Uso:**
1. Faça login (1) e obtenha token
2. Use token em todas as requisições subsequentes
3. Quando token expirar, use refresh (2) para renovar

---

### 👥 Perfil Financeiro (3 endpoints)

| # | Endpoint | Método | Descrição | Auth |
|----|----------|--------|----------|------|
| 1 | `/api/perfil-financeiro/opcoes` | GET | Ver opções de preenchimento | ❌ Não |
| 2 | `/api/perfil-financeiro/exemplos` | GET | Ver exemplos de perfis | ❌ Não |
| 3 | `/api/perfil-financeiro/{id}` | POST | Criar/atualizar perfil | ✅ Sim |

**Exemplo de Uso:**
```bash
# 1. Ver opções disponíveis
GET /api/perfil-financeiro/opcoes
# Retorna: Horizontes, Objetivos, Tolerância à Perda

# 2. Ver exemplos (Conservador, Moderado, Agressivo)
GET /api/perfil-financeiro/exemplos
# Útil para entender o que preencher

# 3. Criar perfil para cliente ID 1
POST /api/perfil-financeiro/1
Body: {
  "rendaMensal": 8000,
  "patrimonioTotal": 150000,
  "dividasAtivas": 10000,
  "dependentesFinanceiros": 1,
  "horizonte": 2,
  "objetivo": 3,
  "toleranciaPerda": 5,
  "experienciaInvestimentos": true
}
```

---

### 📊 Perfil de Risco (2 endpoints)

| # | Endpoint | Método | Descrição |
|----|----------|--------|----------|
| 1 | `/api/perfil-risco/{clienteId}` | GET | Obter perfil de risco calculado |
| 2 | `/api/perfil-risco/produtos-recomendados/{perfil}` | GET | Produtos para perfil (1=Conservador, 2=Moderado, 3=Agressivo) |

**Exemplo:**
```bash
# Perfil de risco do cliente 1
GET /api/perfil-risco/1
# Retorna: Perfil (Conservador/Moderado/Agressivo), Pontuação, Descrição

# Produtos para perfil Conservador
GET /api/perfil-risco/produtos-recomendados/1
# Retorna: CDB, LCI, LCA, Tesouro Selic
```

---

### 💰 Simulações de Investimento (5 endpoints)

| # | Endpoint | Método | Descrição |
|----|----------|--------|----------|
| 1 | `/api/simulacao/produtos-disponiveis` | GET | Listar todos os produtos |
| 2 | `/api/simulacao/simular-investimento` | POST | **🧠 Simular com inteligência de proximidade** |
| 3 | `/api/simulacao/simulacoes` | GET | Histórico de todas as simulações |
| 4 | `/api/simulacao/simulacoes/por-produto-dia` | GET | Agrupar simulações por produto/dia |
| 5 | `/api/simulacao/produtos-recomendados/{clienteId}/{tipo}` | GET | Produtos de um tipo para cliente |

**Exemplo Principal - Simulação com Inteligência:**
```bash
POST /api/simulacao/simular-investimento
Body: {
  "clienteId": 1,
  "valor": 10000,
  "prazoMeses": 12,
  "tipoProduto": "CDB"
}

# Resposta:
# ✅ Produto selecionado é COMPATÍVEL com perfil do cliente
# ✅ Se houver múltiplos CDBs, retorna o mais próximo do perfil
# ✅ Inclui cálculos financeiros completos
```

---

### 💎 Investimentos Finalizados (2 endpoints)

| # | Endpoint | Método | Descrição |
|----|----------|--------|----------|
| 1 | `/api/investimentos/finalizar` | POST | Converter simulação em investimento real |
| 2 | `/api/investimentos/historico/{clienteId}` | GET | Histórico completo do cliente |

**Exemplo:**
```bash
# Finalizar simulação como investimento real
POST /api/investimentos/finalizar
Body: {
  "clienteId": 1,
  "produtoId": 1,
  "valorAplicado": 10000,
  "prazoMeses": 12
}
# Resposta: 201 Created com ID do investimento

# Ver histórico: simulações + investimentos finalizados
GET /api/investimentos/historico/1
# Retorna: Simulações não realizadas + Investimentos (ativos/finalizados/resgatados)
```

---

### 📈 Telemetria & Monitoramento (4 endpoints)

| # | Endpoint | Método | Descrição | Auth |
|----|----------|--------|----------|------|
| 1 | `/api/telemetria` | GET | Dados agregados da API | ✅ Sim |
| 2 | `/health` | GET | Health check geral | ❌ Não |
| 3 | `/health/ready` | GET | Readiness probe (DB, Redis) | ❌ Não |
| 4 | `/health/live` | GET | Liveness probe (app alive) | ❌ Não |

**Exemplo:**
```bash
# Telemetria de uso
GET /api/telemetria?dataInicio=2025-11-01&dataFim=2025-11-30
# Retorna: Total de simulações, produto mais usado, volume, usuários ativos

# Health checks (sem autenticação)
GET /health        → Status geral (200/500/503)
GET /health/ready  → Pronto? (verifica DB, Redis)
GET /health/live   → Vivo? (processo, memória, CPU)
```

---

## 🎯 Fluxo Recomendado de Testes

### Cenário 1: Conhecer Opções
```
1. GET /perfil-financeiro/opcoes
2. GET /perfil-financeiro/exemplos
3. POST /auth/login
4. GET /perfil-risco/1 (verificar perfil pré-existente)
```

### Cenário 2: Simular Investimento
```
1. POST /auth/login
2. GET /simulacao/produtos-disponiveis
3. POST /simulacao/simular-investimento (cliente 1, CDB, R$10k)
4. GET /simulacao/simulacoes (ver histórico)
5. GET /perfil-risco/1 (verificar compatibilidade)
```

### Cenário 3: Finalizar Investimento
```
1. POST /auth/login
2. POST /simulacao/simular-investimento (para obter produtoId)
3. POST /investimentos/finalizar
4. GET /investimentos/historico/1
```

### Cenário 4: Monitoramento
```
1. GET /health
2. GET /health/ready
3. GET /health/live
4. GET /api/telemetria
```

---

## 🔑 Variáveis de Ambiente

A collection usa 3 variáveis:

### `base_url`
- **Default:** `http://localhost:7148`
- **Usar quando:** API está em outro servidor
- **Exemplo:** `https://api.investcaixa.com.br`

### `jwt_token`
- **Auto-preenchido:** Após executar /api/auth/login
- **Usa:** Autenticação em todos os endpoints protegidos
- **Duração:** 1 hora (configurável)
- **Expirou?** Execute `/api/auth/refresh` para renovar

### `refresh_token`
- **Auto-preenchido:** Após executar /api/auth/login
- **Usa:** Renovar JWT sem fazer login novamente
- **Duração:** 7 dias

---

## 🔐 Credenciais de Teste

**Para MVP/Demo:**
```
Usuário: Caixa
Senha:   Caixa@Verso
```

⚠️ **IMPORTANTE:** Estas são credenciais de **DEMONSTRAÇÃO APENAS**. Em produção, integre com sistema real de autenticação.

---

## 📊 Dados de Teste Disponíveis

A API vem com dados de seed automático:

### Clientes
1. **João Silva** (ID: 1) - Conservador
2. **Maria Costa** (ID: 2) - Moderado
3. **Carlos Lima** (ID: 3) - Agressivo
4. **Ana Alves** (ID: 4) - Moderado Jovem
5. **Roberto Mendes** (ID: 5) - Conservador Experiente

### Produtos (10 tipos)
- **Conservadores:** CDB Caixa, LCI, Tesouro Selic, LCA
- **Moderados:** CDB Progressivo, Fundo DI, Tesouro IPCA+
- **Agressivos:** Fundo Multimercado, Fundo Ações, CDB High Yield

### Simulações Pré-Existentes
- 8 simulações de exemplo
- 15 investimentos finalizados
- Volume total: R$ 1.311.000

---

## 🛠️ Troubleshooting

### ❌ "Unauthorized" (401)
**Solução:**
1. Execute `/api/auth/login` primeiro
2. Verifique se `jwt_token` foi salvo
3. Valide o Authorization header

### ❌ "Not Found" (404)
**Solução:**
1. Verifique se `clienteId` existe (1-5)
2. Verifique se `produtoId` existe
3. Use `/simulacao/produtos-disponiveis` para listar

### ❌ "Bad Request" (400)
**Solução:**
1. Valide o corpo JSON da requisição
2. Verifique tipos de dados (int, string, bool)
3. Use `/perfil-financeiro/opcoes` para valores válidos

### ❌ "Connection Refused"
**Solução:**
1. Verifique se API está rodando: `dotnet run`
2. Verifique `base_url` correto (http://localhost:7148)
3. Verifique firewall/proxy

---

## 💡 Dicas & Tricks

### 1. Copiar Response para Nova Requisição
```
1. Executar requisição
2. Copy resultado (ex: produtoId)
3. Usar em requisição seguinte
4. Ou usar Postman Variables para automatizar
```

### 2. Executar Múltiplas Requisições (Runner)
```
1. Click em "Runner" (canto superior esquerdo)
2. Selecionar collection
3. Definir iterações
4. Click em "Run"
```

### 3. Validar Respostas Automaticamente
```javascript
// Já implementado no script de teste:
// Salva jwt_token automaticamente após login
// Você pode adicionar mais validações em "Tests"
```

### 4. Exportar Resultados
```
1. Executar testes via Runner
2. Click em "Export Results"
3. Salvar como JSON/HTML
```

---

## 📖 Documentação Adicional

- **Swagger/OpenAPI:** http://localhost:7148/swagger
- **README Principal:** `README.md` (análise completa do projeto)
- **Análise de Polimento:** `ANALISE_POLIMENTO.md` (pontos a melhorar)
- **Testes:** `tests/` (50+ testes automáticos)

---

## ✅ Checklist de Teste Manual

- [ ] Abrir Postman
- [ ] Importar collection
- [ ] Executar `/api/auth/login`
- [ ] Verificar `jwt_token` preenchido
- [ ] Executar `/api/simulacao/simular-investimento`
- [ ] Verificar resposta com produto compatível
- [ ] Executar `/api/health`
- [ ] Verificar status 200 Healthy
- [ ] Todos os endpoints funcionais ✅

---

**Última Atualização:** 21 de Novembro de 2025  
**Status:** ✅ Collection Completa e Funcional

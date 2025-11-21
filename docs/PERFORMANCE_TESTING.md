# 🚀 Testes de Performance e Load - InvestCaixa

Suíte abrangente de testes de performance, concorrência, stress e cache para validar o comportamento do sistema sob diferentes cargas e condições.

## 📋 Visão Geral

### 🎯 **Objetivos dos Testes**
- **Performance Baseline**: Estabelecer métricas de performance aceitáveis
- **Concorrência**: Validar thread-safety e comportamento multi-usuário
- **Stress Testing**: Identificar pontos de falha sob carga extrema
- **Cache Performance**: Verificar eficiência e consistência do cache
- **Escalabilidade**: Avaliar comportamento com crescimento de carga

### 📊 **Métricas Monitoradas**
- Tempo de resposta (média, percentil 95, máximo)
- Throughput (requests por segundo)
- Taxa de sucesso (disponibilidade)
- Uso de memória e detecção de vazamentos
- Hit ratio do cache
- Degradação de performance ao longo do tempo

## 🧪 Suites de Testes

### 1. **PerformanceTests** 📈
**Objetivo**: Benchmarks e métricas base de performance

#### Cenários Cobertos:
- **Benchmark Completo**: Simulações com diferentes valores e prazos
- **Stress Test Sequencial**: 100 simulações seguidas
- **Memory Leak Detection**: 1000 operações de cache
- **Throughput Testing**: Diferentes cargas (5, 15, 30 requests)
- **Database Performance**: Consultas complexas
- **SLA Testing**: Carga sustentada por tempo definido

#### Critérios de Aceitação:
```
✅ Tempo médio simulação: < 1,5s
✅ Taxa de sucesso: > 95%
✅ Vazamento memória: < 50MB
✅ Degradação performance: < 50%
✅ Tempo percentil 95: < 2s
```

### 2. **ConcurrencyTests** ⚡
**Objetivo**: Validar comportamento multi-usuário e thread-safety

#### Cenários Cobertos:
- **50 Requests Simultâneas**: Simulações paralelas
- **Perfil Risco Concorrente**: Múltiplos acessos ao mesmo cliente
- **Cache Sob Concorrência**: Validação de consistência
- **Telemetria High-Frequency**: Registro de alta frequência
- **Histórico com Carga**: Performance com muitos dados
- **Auth Tokens Simultâneos**: Geração paralela de tokens

#### Critérios de Aceitação:
```
✅ Taxa sucesso concorrente: > 95%
✅ Tempo máximo request: < 5s
✅ Consistência dados: 100%
✅ Cache hit melhoria: > 20%
✅ Tempo médio auth: < 2s
```

### 3. **StressTests** 🔥
**Objetivo**: Comportamento sob condições extremas

#### Cenários Cobertos:
- **Pico de Tráfego**: 3 ondas de 25 requests simultâneas
- **Resource Exhaustion**: 200 operações intensivas
- **Edge Cases**: Valores extremos (mínimo/máximo)
- **Cache Eviction**: Pressão no cache com turnover
- **Timeout Resilience**: Requests lentas vs rápidas

#### Critérios de Aceitação:
```
✅ Disponibilidade sob stress: > 90%
✅ Tempo médio stress: < 5s
✅ Degradação entre ondas: < 100%
✅ Cache pressure handling: < 2s média
✅ Resource growth: < 100MB
```

### 4. **CachePerformanceTests** 💾
**Objetivo**: Performance e comportamento do sistema de cache

#### Cenários Cobertos:
- **Cache Warmup**: Primeira vez vs subsequentes
- **Concorrência Cache**: Thread-safety e consistência
- **TTL Behavior**: Expiração e recarregamento
- **Cache Invalidation**: Atualização após writes
- **Memory Pressure**: Alta carga no cache
- **Hit Ratio Analysis**: Eficiência em uso normal

#### Critérios de Aceitação:
```
✅ Melhoria cache: > 30%
✅ Cache hit time: < 500ms
✅ Hit ratio normal: > 60%
✅ Consistência concorrente: 100%
✅ Variação baixa: CV < 1.0
```

## 🚀 Como Executar

### **Windows (PowerShell)**
```powershell
# Todos os testes
.\scripts\run-performance-tests.ps1

# Testes específicos
.\scripts\run-performance-tests.ps1 -TestType Performance
.\scripts\run-performance-tests.ps1 -TestType Concurrency -Detailed
.\scripts\run-performance-tests.ps1 -TestType Stress -Report
.\scripts\run-performance-tests.ps1 -TestType Cache -Output "Results"
```

### **Linux/macOS (Bash)**
```bash
# Dar permissão de execução
chmod +x scripts/run-performance-tests.sh

# Todos os testes
./scripts/run-performance-tests.sh

# Testes específicos
./scripts/run-performance-tests.sh -t Performance
./scripts/run-performance-tests.sh -t Concurrency --detailed
./scripts/run-performance-tests.sh -t Stress --report
./scripts/run-performance-tests.sh -t Cache -o "Results"
```

### **Manual (dotnet test)**
```bash
# Suite específica
dotnet test tests/InvestCaixa.UnitTests --filter "FullyQualifiedName~PerformanceTests"

# Teste específico
dotnet test tests/InvestCaixa.UnitTests --filter "StressTest_PicoDeTrafegoSimultaneo_DeveManterEstabilidade"

# Com relatórios
dotnet test tests/InvestCaixa.UnitTests --filter "FullyQualifiedName~CachePerformanceTests" --collect:"XPlat Code Coverage"
```

## 📊 Interpretando Resultados

### **✅ Resultados Saudáveis**
```
🚀 INVESTECAIXA - PERFORMANCE TESTING SUITE
==================================================

✅ Performance concluído com sucesso em 45,23s
✅ Concurrency concluído com sucesso em 32,67s
✅ Cache concluído com sucesso em 28,91s
✅ Stress concluído com sucesso em 89,12s

📊 RELATÓRIO FINAL:
Tempo total de execução: 195,93s
Sucessos: 4
Falhas: 0

🎉 TODOS OS TESTES DE PERFORMANCE PASSARAM!
```

### **⚠️ Problemas de Performance**
```
❌ Stress falhou (Exit Code: 1) após 156,78s

🔍 PRÓXIMOS PASSOS:
1. Analise os testes que falharam
2. Verifique recursos do sistema (CPU, memória, disco)
3. Execute testes individuais para diagnosticar problemas
```

## 🔍 Troubleshooting

### **Problemas Comuns**

#### **Testes Muito Lentos**
```bash
# Sintomas: Testes levando > 5 minutos
# Soluções:
- Execute suites específicas: -TestType Performance
- Feche outras aplicações
- Verifique uso de CPU/memória
- Execute em horário de menor carga
```

#### **Falhas de Concorrência**
```bash
# Sintomas: Timeouts ou inconsistências
# Soluções:
- Verifique se API está rodando
- Reduza número de requests simultâneas
- Verifique logs para deadlocks
- Valide configuração de connection pool
```

#### **Memory Leaks Detectados**
```bash
# Sintomas: Crescimento excessivo de memória
# Soluções:
- Force garbage collection: GC.Collect()
- Verifique disposal de resources
- Analise com profilers (.NET Diagnostic Tools)
- Revise implementação de cache
```

#### **Cache Performance Ruim**
```bash
# Sintomas: Hit ratio baixo ou tempos altos
# Soluções:
- Verifique configuração Redis
- Analise TTL settings
- Revise chaves de cache
- Monitore eviction policies
```

## 📈 Métricas de Referência

### **Performance Baseline (Sistema Médio)**
| Métrica | Target | Alerta | Crítico |
|---------|--------|--------|---------|
| Simulação Individual | < 1s | < 2s | > 3s |
| Throughput | > 10 req/s | > 5 req/s | < 3 req/s |
| Taxa Sucesso | > 99% | > 95% | < 90% |
| Cache Hit Ratio | > 80% | > 60% | < 40% |
| Memory Growth | < 20MB | < 50MB | > 100MB |
| P95 Response Time | < 1.5s | < 3s | > 5s |

### **Hardware Recomendado para Testes**
```
Mínimo:
- CPU: 2 cores
- RAM: 4GB
- SSD: 50GB livre

Recomendado:
- CPU: 4+ cores
- RAM: 8GB+
- SSD: 100GB+ livre
- Rede: 100Mbps+
```

## 🎯 Cenários de Uso

### **CI/CD Pipeline**
```yaml
# Azure DevOps / GitHub Actions
- name: Performance Tests
  run: |
    ./scripts/run-performance-tests.sh -t Performance
    if [ $? -ne 0 ]; then
      echo "Performance regression detected!"
      exit 1
    fi
```

### **Desenvolvimento Local**
```bash
# Teste rápido após mudanças
./scripts/run-performance-tests.sh -t Cache

# Validação completa pré-commit
./scripts/run-performance-tests.sh -t All --detailed
```

### **Teste de Capacidade**
```bash
# Simular Black Friday / pico de carga
./scripts/run-performance-tests.sh -t Stress

# Análise de escalabilidade
./scripts/run-performance-tests.sh -t Concurrency --report
```

## 📚 Recursos Adicionais

### **Ferramentas de Monitoramento**
- **Application Insights**: Telemetria em produção
- **PerfView**: Análise detalhada de performance .NET
- **dotTrace**: Profiler de performance
- **BenchmarkDotNet**: Micro-benchmarks precisos

### **Leituras Recomendadas**
- [ASP.NET Core Performance Best Practices](https://docs.microsoft.com/aspnet/core/performance/performance-best-practices)
- [.NET Memory Management](https://docs.microsoft.com/dotnet/standard/garbage-collection/)
- [Redis Performance Tuning](https://redis.io/topics/benchmarks)
- [Load Testing Best Practices](https://docs.microsoft.com/azure/architecture/checklist/dev-ops#testing)

---

**💡 Dica**: Execute os testes de performance regularmente para detectar regressões antes que cheguem à produção!
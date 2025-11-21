#!/bin/bash

# 🚀 Script de Execução dos Testes de Performance (Linux/macOS)
# InvestCaixa - Performance & Load Testing Suite

# Configuração padrão
TEST_TYPE="All"
DETAILED=false
REPORT=false
OUTPUT="TestResults"

# Parse argumentos
while [[ $# -gt 0 ]]; do
    case $1 in
        -t|--test-type)
            TEST_TYPE="$2"
            shift 2
            ;;
        -d|--detailed)
            DETAILED=true
            shift
            ;;
        -r|--report)
            REPORT=true
            shift
            ;;
        -o|--output)
            OUTPUT="$2"
            shift 2
            ;;
        -h|--help)
            echo "🚀 InvestCaixa Performance Testing Suite"
            echo ""
            echo "Uso: $0 [opções]"
            echo ""
            echo "Opções:"
            echo "  -t, --test-type TYPE    Tipo de teste (All, Performance, Concurrency, Stress, Cache)"
            echo "  -d, --detailed          Logs detalhados com arquivos TRX"
            echo "  -r, --report           Gerar relatório de cobertura"
            echo "  -o, --output DIR       Diretório de saída (padrão: TestResults)"
            echo "  -h, --help             Mostrar esta ajuda"
            echo ""
            echo "Exemplos:"
            echo "  $0                                    # Todos os testes"
            echo "  $0 -t Performance                     # Apenas testes de performance"
            echo "  $0 -t Concurrency -d                  # Testes de concorrência com logs detalhados"
            exit 0
            ;;
        *)
            echo "❌ Argumento desconhecido: $1"
            echo "Use -h ou --help para ver as opções disponíveis"
            exit 1
            ;;
    esac
done

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

echo -e "${CYAN}🚀 INVESTECAIXA - PERFORMANCE TESTING SUITE${NC}"
echo -e "${GRAY}==================================================${NC}"

# Configuração de caminhos
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
TEST_PROJECT="$PROJECT_ROOT/tests/InvestCaixa.UnitTests"
OUTPUT_DIR="$PROJECT_ROOT/$OUTPUT"

# Criar diretório de saída
mkdir -p "$OUTPUT_DIR"

# Função para executar testes
execute_performance_test() {
    local test_filter="$1"
    local test_name="$2"
    
    echo -e "\n${YELLOW}🧪 Executando: $test_name${NC}"
    echo -e "${GRAY}------------------------------${NC}"
    
    local start_time=$(date +%s.%N)
    
    local test_command="dotnet test \"$TEST_PROJECT\" --filter \"$test_filter\" --logger console --verbosity normal"
    
    if [ "$DETAILED" = true ]; then
        test_command+=" --logger \"trx;LogFileName=$test_name.trx\""
    fi
    
    if [ "$REPORT" = true ]; then
        test_command+=" --collect:\"XPlat Code Coverage\""
    fi
    
    echo -e "${GRAY}Comando: $test_command${NC}"
    
    eval $test_command
    local exit_code=$?
    
    local end_time=$(date +%s.%N)
    local duration=$(echo "$end_time - $start_time" | bc -l)
    
    if [ $exit_code -eq 0 ]; then
        printf "${GREEN}✅ %s concluído com sucesso em %.2fs${NC}\n" "$test_name" "$duration"
    else
        printf "${RED}❌ %s falhou (Exit Code: %d) após %.2fs${NC}\n" "$test_name" "$exit_code" "$duration"
    fi
    
    echo "$test_name,$exit_code,$duration"
}

# Definir conjuntos de testes
declare -A test_suites
test_suites["Performance"]="FullyQualifiedName~InvestCaixa.UnitTests.PerformanceTests.PerformanceTests|Testes de Performance e Benchmarks"
test_suites["Concurrency"]="FullyQualifiedName~InvestCaixa.UnitTests.PerformanceTests.ConcurrencyTests|Testes de Concorrência e Paralelismo"
test_suites["Stress"]="FullyQualifiedName~InvestCaixa.UnitTests.PerformanceTests.StressTests|Testes de Stress e Carga Extrema"
test_suites["Cache"]="FullyQualifiedName~InvestCaixa.UnitTests.PerformanceTests.CachePerformanceTests|Testes de Performance de Cache"
test_suites["All"]="FullyQualifiedName~InvestCaixa.UnitTests.PerformanceTests|Todos os Testes de Performance"

# Verificar se o tipo de teste é válido
if [[ ! ${test_suites[$TEST_TYPE]+_} ]]; then
    echo -e "${RED}❌ Tipo de teste inválido: $TEST_TYPE${NC}"
    echo -e "${YELLOW}Tipos disponíveis: $(IFS=', '; echo "${!test_suites[*]}")${NC}"
    exit 1
fi

# Informações do ambiente
echo -e "\n${CYAN}📋 INFORMAÇÕES DO AMBIENTE:${NC}"
echo "Sistema Operacional: $(uname -s) $(uname -r)"
echo "Versão .NET: $(dotnet --version)"
echo "Processador: $(uname -m)"
if command -v free >/dev/null 2>&1; then
    echo "Memória Total: $(free -h | awk 'NR==2{print $2}')"
elif command -v sysctl >/dev/null 2>&1; then
    echo "Memória Total: $(sysctl -n hw.memsize | awk '{print $1/1024/1024/1024 " GB"}')"
fi
echo "Projeto: $TEST_PROJECT"
IFS='|' read -r filter description <<< "${test_suites[$TEST_TYPE]}"
echo "Tipo de Teste: $TEST_TYPE - $description"

# Verificar se o projeto existe
if [ ! -d "$TEST_PROJECT" ]; then
    echo -e "${RED}❌ Projeto de teste não encontrado: $TEST_PROJECT${NC}"
    exit 1
fi

# Preparar ambiente
echo -e "\n${CYAN}🔧 PREPARANDO AMBIENTE...${NC}"
echo -e "${GRAY}Executando dotnet restore...${NC}"
dotnet restore "$TEST_PROJECT" --verbosity quiet

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Falha no restore do projeto${NC}"
    exit 1
fi

echo -e "${GRAY}Executando dotnet build...${NC}"
dotnet build "$TEST_PROJECT" --no-restore --verbosity quiet

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Falha no build do projeto${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Ambiente preparado com sucesso${NC}"

# Executar testes
echo -e "\n${CYAN}🧪 EXECUTANDO TESTES DE PERFORMANCE...${NC}"
overall_start=$(date +%s.%N)

results=()

if [ "$TEST_TYPE" = "All" ]; then
    # Executar todos os tipos sequencialmente
    for suite in "Performance" "Concurrency" "Cache" "Stress"; do
        IFS='|' read -r filter description <<< "${test_suites[$suite]}"
        result=$(execute_performance_test "$filter" "$suite")
        results+=("$result")
    done
else
    # Executar tipo específico
    IFS='|' read -r filter description <<< "${test_suites[$TEST_TYPE]}"
    result=$(execute_performance_test "$filter" "$TEST_TYPE")
    results+=("$result")
fi

overall_end=$(date +%s.%N)
total_duration=$(echo "$overall_end - $overall_start" | bc -l)

# Relatório final
echo -e "\n${CYAN}📊 RELATÓRIO FINAL:${NC}"
echo -e "${GRAY}==================================================${NC}"

success_count=0
failure_count=0

for result in "${results[@]}"; do
    IFS=',' read -r name exit_code duration <<< "$result"
    if [ "$exit_code" -eq 0 ]; then
        ((success_count++))
    else
        ((failure_count++))
    fi
done

printf "Tempo total de execução: %.2fs\n" "$total_duration"
echo "Testes executados: ${#results[@]}"
echo -e "${GREEN}Sucessos: $success_count${NC}"
if [ $failure_count -gt 0 ]; then
    echo -e "${RED}Falhas: $failure_count${NC}"
else
    echo -e "${GREEN}Falhas: $failure_count${NC}"
fi

if [ ${#results[@]} -gt 1 ]; then
    echo -e "\n${YELLOW}📈 DETALHES POR SUITE:${NC}"
    for result in "${results[@]}"; do
        IFS=',' read -r name exit_code duration <<< "$result"
        if [ "$exit_code" -eq 0 ]; then
            printf "${GREEN}✅ %s: %.2fs${NC}\n" "$name" "$duration"
        else
            printf "${RED}❌ %s: %.2fs${NC}\n" "$name" "$duration"
        fi
    done
fi

# Verificar falhas
if [ $failure_count -gt 0 ]; then
    echo -e "\n${RED}⚠️  ATENÇÃO: Alguns testes falharam!${NC}"
    echo -e "${YELLOW}Verifique os logs acima para detalhes dos problemas.${NC}"
fi

# Dicas
echo -e "\n${CYAN}💡 DICAS DE PERFORMANCE:${NC}"
echo -e "${GRAY}• Para testes mais rápidos, execute suites individuais: -t Performance${NC}"
echo -e "${GRAY}• Use -d para logs detalhados com arquivos TRX${NC}"
echo -e "${GRAY}• Use -r para relatórios de cobertura${NC}"
echo -e "${GRAY}• Feche outras aplicações para resultados mais consistentes${NC}"

# Recomendações
if (( $(echo "$total_duration > 300" | bc -l) )); then
    echo -e "\n${YELLOW}⏱️  Os testes levaram mais de 5 minutos. Considere:${NC}"
    echo -e "${GRAY}  - Executar suites específicas em vez de 'All'${NC}"
    echo -e "${GRAY}  - Verificar se há outros processos consumindo recursos${NC}"
fi

if [ $success_count -eq ${#results[@]} ]; then
    echo -e "\n${GREEN}🎉 TODOS OS TESTES DE PERFORMANCE PASSARAM!${NC}"
    echo -e "${GREEN}Seu sistema está performando dentro dos parâmetros esperados.${NC}"
else
    echo -e "\n${YELLOW}🔍 PRÓXIMOS PASSOS:${NC}"
    echo -e "${GRAY}1. Analise os testes que falharam${NC}"
    echo -e "${GRAY}2. Verifique recursos do sistema (CPU, memória, disco)${NC}"
    echo -e "${GRAY}3. Execute testes individuais para diagnosticar problemas específicos${NC}"
fi

echo -e "\n${CYAN}🏁 Execução concluída!${NC}"

# Exit code baseado nos resultados
exit $failure_count
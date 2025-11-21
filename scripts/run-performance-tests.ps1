param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("All", "Performance", "Concurrency", "Stress", "CachePerformance")]
    [string]$TestType = "All",
    
    [Parameter(Mandatory=$false)]
    [switch]$Detailed
)

# Configuracao inicial
$ErrorActionPreference = "Stop"

# Funcao para executar teste
function Invoke-PerformanceTest {
    param(
        [string]$TestName,
        [string]$Filter
    )
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    
    try {
        Write-Host "Executando: $TestName..." -ForegroundColor Yellow
        
        $arguments = @(
            "test",
            "tests\InvestCaixa.UnitTests",
            "--filter", $Filter,
            "--verbosity", "minimal"
        )
        
        $process = Start-Process -FilePath "dotnet" -ArgumentList $arguments -Wait -PassThru -NoNewWindow
        
        $stopwatch.Stop()
        $duration = $stopwatch.Elapsed.TotalSeconds
        $exitCode = $process.ExitCode
        
        if ($exitCode -eq 0) {
            Write-Host "[OK] $TestName concluido em $("{0:F2}" -f $duration)s" -ForegroundColor Green
        } else {
            Write-Host "[FAIL] $TestName falhou em $("{0:F2}" -f $duration)s" -ForegroundColor Red
        }
        
        return @{
            Name = $TestName
            Success = ($exitCode -eq 0)
            Duration = $duration
            ExitCode = $exitCode
        }
    }
    catch {
        $stopwatch.Stop()
        Write-Host "[ERROR] Erro ao executar $TestName : $_" -ForegroundColor Red
        return @{
            Name = $TestName
            Success = $false
            Duration = $stopwatch.Elapsed.TotalSeconds
            ExitCode = -1
        }
    }
}

# Funcao para obter filtros de teste
function Get-TestFilter {
    param([string]$Type)
    
    switch ($Type) {
        "Performance" { return "FullyQualifiedName~PerformanceTests" }
        "Concurrency" { return "FullyQualifiedName~ConcurrencyTests" }
        "Stress" { return "FullyQualifiedName~StressTests" }
        "CachePerformance" { return "FullyQualifiedName~CachePerformanceTests" }
        "All" { return "FullyQualifiedName~PerformanceTests" }
        default { throw "Tipo de teste invalido: $Type" }
    }
}

# Cabecalho
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  TESTES DE PERFORMANCE - InvestCaixa" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Verificacao inicial
if (-not (Test-Path "tests\InvestCaixa.UnitTests")) {
    Write-Host "Erro: Diretorio de testes nao encontrado!" -ForegroundColor Red
    exit 1
}

# Build do projeto
Write-Host "Compilando projeto..." -ForegroundColor Yellow
$buildResult = Start-Process -FilePath "dotnet" -ArgumentList @("build", "--verbosity", "quiet") -Wait -PassThru -NoNewWindow
if ($buildResult.ExitCode -ne 0) {
    Write-Host "Erro na compilacao do projeto!" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Compilacao concluida com sucesso" -ForegroundColor Green
Write-Host ""

# Execucao dos testes
$overallStopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$results = @()

if ($TestType -eq "All") {
    $testTypes = @("Performance", "Concurrency", "Stress", "CachePerformance")
    foreach ($type in $testTypes) {
        $filter = Get-TestFilter $type
        $result = Invoke-PerformanceTest -TestName "$type Tests" -Filter $filter
        $results += $result
    }
} else {
    $filter = Get-TestFilter $TestType
    $result = Invoke-PerformanceTest -TestName "$TestType Tests" -Filter $filter
    $results += $result
}

$overallStopwatch.Stop()

# Relatorio final
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "           RELATORIO FINAL" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

$successCount = ($results | Where-Object { $_.Success }).Count
$failureCount = ($results | Where-Object { -not $_.Success }).Count
$totalDuration = $overallStopwatch.Elapsed.TotalSeconds

Write-Host "Tempo total: $("{0:F2}" -f $totalDuration)s" -ForegroundColor White
Write-Host "Testes executados: $($results.Count)" -ForegroundColor White
Write-Host "Sucessos: $successCount" -ForegroundColor Green
Write-Host "Falhas: $failureCount" -ForegroundColor $(if ($failureCount -gt 0) { "Red" } else { "Green" })

if ($results.Count -gt 1) {
    Write-Host ""
    Write-Host "DETALHES POR SUITE:" -ForegroundColor Yellow
    foreach ($result in $results) {
        $status = if ($result.Success) { "[OK]" } else { "[FAIL]" }
        $duration = "{0:F2}s" -f $result.Duration
        $color = if ($result.Success) { "Green" } else { "Red" }
        Write-Host "$status $($result.Name): $duration" -ForegroundColor $color
    }
}

if ($failureCount -gt 0) {
    Write-Host ""
    Write-Host "ATENCAO: Alguns testes falharam!" -ForegroundColor Red
    Write-Host "Verifique os logs acima para detalhes." -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "TODOS OS TESTES PASSARAM!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Execucao concluida!" -ForegroundColor Cyan

# Exit code baseado nos resultados
exit $failureCount
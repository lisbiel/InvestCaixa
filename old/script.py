
import os

# Gerar os 19 arquivos que faltam

missing_files = {}

# ============================================================
# DTOs - REQUEST (3 arquivos)
# ============================================================
missing_files["SimularInvestimentoRequest.cs"] = '''namespace InvestCaixa.Application.DTOs.Request;

public record SimularInvestimentoRequest
{
    public int ClienteId { get; init; }
    public decimal Valor { get; init; }
    public int PrazoMeses { get; init; }
    public string TipoProduto { get; init; } = string.Empty;
}
'''

missing_files["LoginRequest.cs"] = '''namespace InvestCaixa.Application.DTOs.Request;

public record LoginRequest
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
'''

missing_files["RefreshTokenRequest.cs"] = '''namespace InvestCaixa.Application.DTOs.Request;

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
'''

# ============================================================
# DTOs - RESPONSE (6 arquivos)
# ============================================================
missing_files["SimulacaoResponse.cs"] = '''namespace InvestCaixa.Application.DTOs.Response;

public record SimulacaoResponse
{
    public Guid Id { get; init; }
    public ProdutoValidadoDto ProdutoValidado { get; init; } = null!;
    public ResultadoSimulacaoDto ResultadoSimulacao { get; init; } = null!;
    public DateTime DataSimulacao { get; init; }
}

public record ProdutoValidadoDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public decimal Rentabilidade { get; init; }
    public string Risco { get; init; } = string.Empty;
}

public record ResultadoSimulacaoDto
{
    public decimal ValorFinal { get; init; }
    public decimal RentabilidadeEfetiva { get; init; }
    public int PrazoMeses { get; init; }
}
'''

missing_files["SimulacaoHistoricoResponse.cs"] = '''namespace InvestCaixa.Application.DTOs.Response;

public record SimulacaoHistoricoResponse
{
    public Guid Id { get; init; }
    public int ClienteId { get; init; }
    public string Produto { get; init; } = string.Empty;
    public decimal ValorInvestido { get; init; }
    public decimal ValorFinal { get; init; }
    public int PrazoMeses { get; init; }
    public DateTime DataSimulacao { get; init; }
    public decimal Rentabilidade { get; init; }
}
'''

missing_files["PerfilRiscoResponse.cs"] = '''namespace InvestCaixa.Application.DTOs.Response;

public record PerfilRiscoResponse
{
    public Guid Id { get; init; }
    public int ClienteId { get; init; }
    public string Perfil { get; init; } = string.Empty;
    public int Pontuacao { get; init; }
    public string Descricao { get; init; } = string.Empty;
}
'''

missing_files["ProdutoResponse.cs"] = '''namespace InvestCaixa.Application.DTOs.Response;

public record ProdutoResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public decimal Rentabilidade { get; init; }
    public string Risco { get; init; } = string.Empty;
    public int PrazoMinimoDias { get; init; }
    public decimal ValorMinimoAplicacao { get; init; }
    public bool PermiteLiquidez { get; init; }
}
'''

missing_files["TelemetriaResponse.cs"] = '''namespace InvestCaixa.Application.DTOs.Response;

public record TelemetriaResponse
{
    public PeriodoTelemetriaDto Periodo { get; init; } = null!;
    public List<ServicoTelemetriaDto> Servicos { get; init; } = new();
}

public record PeriodoTelemetriaDto
{
    public DateTime Inicio { get; init; }
    public DateTime Fim { get; init; }
}

public record ServicoTelemetriaDto
{
    public string Nome { get; init; } = string.Empty;
    public int QuantidadeChamadas { get; init; }
    public decimal MediaTempoRespostaMs { get; init; }
    public decimal MinTempoRespostaMs { get; init; }
    public decimal MaxTempoRespostaMs { get; init; }
}
'''

missing_files["LoginResponse.cs"] = '''namespace InvestCaixa.Application.DTOs.Response;

public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string TokenType { get; init; } = "Bearer";
}
'''

# Criar os arquivos
created = 0
for filename, content in missing_files.items():
    with open(filename, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"✓ {filename}")
    created += 1

print(f"\n✅ {created} arquivos criados com sucesso!")
print("\nAgora execute novamente:")
print("  python organize_files.py")

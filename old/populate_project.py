#!/usr/bin/env python3
"""
Script para popular projeto .NET 8 Clean Architecture com todos os arquivos de código
Copia arquivos para as pastas corretas do projeto
"""

import os
import sys

# Nome do projeto - ALTERE AQUI se necessário
PROJECT_NAME = "InvestCaixa"

# Dicionário com TODOS os arquivos de código
FILES_TO_CREATE = {
    # ===== DOMAIN LAYER =====
    f"src/{PROJECT_NAME}.Domain/Entities/BaseEntity.cs": """namespace {PROJECT_NAME}.Domain.Entities;

public abstract class BaseEntity
{{
    public Guid Id {{ get; protected set; }} = Guid.NewGuid();
    public DateTime CreatedAt {{ get; protected set; }} = DateTime.UtcNow;
    public DateTime? UpdatedAt {{ get; protected set; }}
    public bool IsDeleted {{ get; protected set; }} = false;

    public void MarkAsDeleted()
    {{
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }}
}}
""",

    f"src/{PROJECT_NAME}.Domain/Entities/Cliente.cs": """namespace {PROJECT_NAME}.Domain.Entities;

public class Cliente : BaseEntity
{{
    public string Nome {{ get; private set; }}
    public string Email {{ get; private set; }}
    public string CPF {{ get; private set; }}
    public DateTime DataNascimento {{ get; private set; }}
    public PerfilInvestidor PerfilAtual {{ get; private set; }} = PerfilInvestidor.Conservador;

    public virtual ICollection<Simulacao> Simulacoes {{ get; private set; }} = new List<Simulacao>();
    public virtual PerfilRisco? PerfilRisco {{ get; private set; }}

    private Cliente() {{ }}

    public Cliente(string nome, string email, string cpf, DateTime dataNascimento)
    {{
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do cliente não pode estar vazio");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email do cliente não pode estar vazio");
        if (string.IsNullOrWhiteSpace(cpf))
            throw new DomainException("CPF do cliente não pode estar vazio");

        Nome = nome;
        Email = email;
        CPF = cpf;
        DataNascimento = dataNascimento;
    }}

    public void AtualizarPerfil(PerfilInvestidor novoPerfil)
    {{
        PerfilAtual = novoPerfil;
        UpdatedAt = DateTime.UtcNow;
    }}
}}
""",

    f"src/{PROJECT_NAME}.Domain/Entities/ProdutoInvestimento.cs": """namespace {PROJECT_NAME}.Domain.Entities;

public class ProdutoInvestimento : BaseEntity
{{
    public string Nome {{ get; private set; }}
    public TipoProduto Tipo {{ get; private set; }}
    public decimal Rentabilidade {{ get; private set; }}
    public NivelRisco Risco {{ get; private set; }}
    public int PrazoMinimoDias {{ get; private set; }}
    public decimal ValorMinimoAplicacao {{ get; private set; }}
    public bool PermiteLiquidez {{ get; private set; }}
    public PerfilInvestidor PerfilRecomendado {{ get; private set; }}

    private ProdutoInvestimento() {{ }}

    public ProdutoInvestimento(
        string nome,
        TipoProduto tipo,
        decimal rentabilidade,
        NivelRisco risco,
        int prazoMinimoDias,
        decimal valorMinimoAplicacao,
        bool permiteLiquidez,
        PerfilInvestidor perfilRecomendado)
    {{
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do produto não pode estar vazio");
        if (rentabilidade < 0)
            throw new DomainException("Rentabilidade não pode ser negativa");
        if (valorMinimoAplicacao <= 0)
            throw new DomainException("Valor mínimo de aplicação deve ser positivo");

        Nome = nome;
        Tipo = tipo;
        Rentabilidade = rentabilidade;
        Risco = risco;
        PrazoMinimoDias = prazoMinimoDias;
        ValorMinimoAplicacao = valorMinimoAplicacao;
        PermiteLiquidez = permiteLiquidez;
        PerfilRecomendado = perfilRecomendado;
    }}

    public void AtualizarRentabilidade(decimal novaRentabilidade)
    {{
        if (novaRentabilidade < 0)
            throw new DomainException("Rentabilidade não pode ser negativa");

        Rentabilidade = novaRentabilidade;
        UpdatedAt = DateTime.UtcNow;
    }}
}}
""",

    f"src/{PROJECT_NAME}.Domain/Entities/Simulacao.cs": """namespace {PROJECT_NAME}.Domain.Entities;

public class Simulacao : BaseEntity
{{
    public int ClienteId {{ get; private set; }}
    public Guid ProdutoId {{ get; private set; }}
    public decimal ValorInvestido {{ get; private set; }}
    public decimal ValorFinal {{ get; private set; }}
    public int PrazoMeses {{ get; private set; }}
    public DateTime DataSimulacao {{ get; private set; }}
    public decimal RentabilidadeCalculada {{ get; private set; }}

    public virtual Cliente Cliente {{ get; private set; }} = null!;
    public virtual ProdutoInvestimento Produto {{ get; private set; }} = null!;

    private Simulacao() {{ }}

    public Simulacao(
        int clienteId,
        Guid produtoId,
        decimal valorInvestido,
        decimal valorFinal,
        int prazoMeses,
        DateTime dataSimulacao)
    {{
        if (valorInvestido <= 0)
            throw new DomainException("Valor investido deve ser maior que zero");
        if (valorFinal < valorInvestido)
            throw new DomainException("Valor final não pode ser menor que valor investido");
        if (prazoMeses <= 0)
            throw new DomainException("Prazo deve ser maior que zero");

        ClienteId = clienteId;
        ProdutoId = produtoId;
        ValorInvestido = valorInvestido;
        ValorFinal = valorFinal;
        PrazoMeses = prazoMeses;
        DataSimulacao = dataSimulacao;
        RentabilidadeCalculada = CalcularRentabilidade();
    }}

    private decimal CalcularRentabilidade()
    {{
        return (ValorFinal - ValorInvestido) / ValorInvestido;
    }}
}}
""",

    f"src/{PROJECT_NAME}.Domain/Entities/PerfilRisco.cs": """namespace {PROJECT_NAME}.Domain.Entities;

public class PerfilRisco : BaseEntity
{{
    public int ClienteId {{ get; private set; }}
    public PerfilInvestidor Perfil {{ get; private set; }}
    public int Pontuacao {{ get; private set; }}
    public string Descricao {{ get; private set; }}
    public decimal VolumeInvestimentos {{ get; private set; }}
    public int FrequenciaMovimentacoes {{ get; private set; }}
    public bool PrefereLiquidez {{ get; private set; }}

    public virtual Cliente Cliente {{ get; private set; }} = null!;

    private PerfilRisco() 
    {{
        Descricao = string.Empty;
    }}

    public PerfilRisco(
        int clienteId,
        decimal volumeInvestimentos,
        int frequenciaMovimentacoes,
        bool prefereLiquidez)
    {{
        ClienteId = clienteId;
        VolumeInvestimentos = volumeInvestimentos;
        FrequenciaMovimentacoes = frequenciaMovimentacoes;
        PrefereLiquidez = prefereLiquidez;

        CalcularPerfil();
    }}

    public void AtualizarDados(
        decimal volumeInvestimentos,
        int frequenciaMovimentacoes,
        bool prefereLiquidez)
    {{
        VolumeInvestimentos = volumeInvestimentos;
        FrequenciaMovimentacoes = frequenciaMovimentacoes;
        PrefereLiquidez = prefereLiquidez;
        CalcularPerfil();
        UpdatedAt = DateTime.UtcNow;
    }}

    private void CalcularPerfil()
    {{
        int pontos = 0;

        if (VolumeInvestimentos < 10000) pontos += 10;
        else if (VolumeInvestimentos < 50000) pontos += 20;
        else if (VolumeInvestimentos < 100000) pontos += 30;
        else pontos += 40;

        if (FrequenciaMovimentacoes < 3) pontos += 10;
        else if (FrequenciaMovimentacoes < 10) pontos += 20;
        else pontos += 30;

        pontos += PrefereLiquidez ? 10 : 30;

        Pontuacao = pontos;

        if (pontos <= 40)
        {{
            Perfil = PerfilInvestidor.Conservador;
            Descricao = "Perfil conservador: baixa movimentação, foco em segurança e liquidez";
        }}
        else if (pontos <= 70)
        {{
            Perfil = PerfilInvestidor.Moderado;
            Descricao = "Perfil moderado: equilíbrio entre segurança e rentabilidade";
        }}
        else
        {{
            Perfil = PerfilInvestidor.Agressivo;
            Descricao = "Perfil agressivo: busca por alta rentabilidade, aceita maior risco";
        }}
    }}
}}
""",

    f"src/{PROJECT_NAME}.Domain/Enums/TipoProduto.cs": """namespace {PROJECT_NAME}.Domain.Enums;

public enum TipoProduto
{{
    CDB = 1,
    LCI = 2,
    LCA = 3,
    TesouroDireto = 4,
    Fundo = 5
}}
""",

    f"src/{PROJECT_NAME}.Domain/Enums/NivelRisco.cs": """namespace {PROJECT_NAME}.Domain.Enums;

public enum NivelRisco
{{
    Baixo = 1,
    Medio = 2,
    Alto = 3
}}
""",

    f"src/{PROJECT_NAME}.Domain/Enums/PerfilInvestidor.cs": """namespace {PROJECT_NAME}.Domain.Enums;

public enum PerfilInvestidor
{{
    Conservador = 1,
    Moderado = 2,
    Agressivo = 3
}}
""",

    f"src/{PROJECT_NAME}.Domain/Exceptions/DomainException.cs": """namespace {PROJECT_NAME}.Domain.Exceptions;

public class DomainException : Exception
{{
    public DomainException(string message) : base(message) {{ }}
    public DomainException(string message, Exception innerException) : base(message, innerException) {{ }}
}}
""",

    f"src/{PROJECT_NAME}.Domain/Exceptions/NotFoundException.cs": """namespace {PROJECT_NAME}.Domain.Exceptions;

public class NotFoundException : Exception
{{
    public NotFoundException(string message) : base(message) {{ }}
    public NotFoundException(string message, Exception innerException) : base(message, innerException) {{ }}
}}
""",

    f"src/{PROJECT_NAME}.Domain/Exceptions/ValidationException.cs": """namespace {PROJECT_NAME}.Domain.Exceptions;

public class ValidationException : Exception
{{
    public ValidationException(string message) : base(message) {{ }}
    public ValidationException(string message, Exception innerException) : base(message, innerException) {{ }}
}}
""",

    f"src/{PROJECT_NAME}.Domain/Interfaces/IRepository.cs": """namespace {PROJECT_NAME}.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}}
""",

    f"src/{PROJECT_NAME}.Domain/Interfaces/ISimulacaoRepository.cs": """namespace {PROJECT_NAME}.Domain.Interfaces;

public interface ISimulacaoRepository : IRepository<Simulacao>
{{
    Task<IEnumerable<Simulacao>> ObterPorClienteAsync(int clienteId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SimulacaoPorProdutoDia>> ObterSimulacoesPorProdutoDiaAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken cancellationToken = default);
}}

public record SimulacaoPorProdutoDia
{{
    public string Produto {{ get; set; }} = string.Empty;
    public DateTime Data {{ get; set; }}
    public int QuantidadeSimulacoes {{ get; set; }}
    public decimal MediaValorFinal {{ get; set; }}
}}
""",

    f"src/{PROJECT_NAME}.Domain/Interfaces/IProdutoRepository.cs": """namespace {PROJECT_NAME}.Domain.Interfaces;

public interface IProdutoRepository : IRepository<ProdutoInvestimento>
{{
    Task<ProdutoInvestimento?> ObterPorTipoAsync(string tipo, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProdutoInvestimento>> ObterPorPerfilAsync(PerfilInvestidor perfil, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProdutoInvestimento>> ObterPorCriteriosAsync(decimal? valorMinimo = null, NivelRisco? nivelRisco = null, bool? permiteLiquidez = null, CancellationToken cancellationToken = default);
}}
""",

    f"src/{PROJECT_NAME}.Domain/Interfaces/IClienteRepository.cs": """namespace {PROJECT_NAME}.Domain.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{{
    Task<Cliente?> ObterPorCPFAsync(string cpf, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<PerfilRisco?> ObterPerfilRiscoAsync(int clienteId, CancellationToken cancellationToken = default);
    Task AdicionarPerfilRiscoAsync(PerfilRisco perfilRisco, CancellationToken cancellationToken = default);
}}
""",

    f"src/{PROJECT_NAME}.Domain/Interfaces/IUnitOfWork.cs": """namespace {PROJECT_NAME}.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{{
    ISimulacaoRepository SimulacaoRepository {{ get; }}
    IProdutoRepository ProdutoRepository {{ get; }}
    IClienteRepository ClienteRepository {{ get; }}

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}}
""",

    f"src/{PROJECT_NAME}.Domain/GlobalUsings.cs": """global using {PROJECT_NAME}.Domain.Entities;
global using {PROJECT_NAME}.Domain.Enums;
global using {PROJECT_NAME}.Domain.Exceptions;
global using {PROJECT_NAME}.Domain.Interfaces;
""",
}

def create_files():
    """Cria todos os arquivos necessários"""

    print(f"\n{'='*60}")
    print(f"Populando projeto: {PROJECT_NAME}")
    print(f"{'='*60}\n")

    created = 0
    for file_path, content in FILES_TO_CREATE.items():
        # Substituir nome do projeto no conteúdo
        content = content.format(PROJECT_NAME=PROJECT_NAME)

        # Criar diretórios se não existirem
        os.makedirs(os.path.dirname(file_path), exist_ok=True)

        # Criar arquivo
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)

        print(f"✓ {file_path}")
        created += 1

    print(f"\n{'='*60}")
    print(f"✅ {created} arquivos criados com sucesso!")
    print(f"{'='*60}\n")

if __name__ == "__main__":
    create_files()

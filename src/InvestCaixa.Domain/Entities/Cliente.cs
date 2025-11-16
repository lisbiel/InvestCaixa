namespace InvestCaixa.Domain.Entities;
public class Cliente : BaseEntity<int>
{
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string CPF { get; private set; }
    public DateTime DataNascimento { get; private set; }
    public PerfilInvestidor PerfilAtual { get; private set; } = PerfilInvestidor.Conservador;

    public virtual ICollection<Simulacao> Simulacoes { get; private set; } = new List<Simulacao>();
    public virtual PerfilRisco? PerfilRisco { get; private set; }

    private Cliente() { }

    public Cliente(string nome, string email, string cpf, DateTime dataNascimento)
    {
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
    }

    public void AtualizarPerfil(PerfilInvestidor novoPerfil)
    {
        PerfilAtual = novoPerfil;
        UpdatedAt = DateTime.UtcNow;
    }
}
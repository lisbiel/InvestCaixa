using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestCaixa.Application.DTOs.Request;
public class CriarPerfilFinanceiroRequest
{
    public decimal RendaMensal { get; set; }
    public decimal PatrimonioTotal { get; set; }
    public decimal DividasAtivas { get; set; }
    public int DependentesFinanceiros { get; set; }

    /// <summary>
    /// Horizonte de Investimentos (USE /opcoes para ver valores)
    /// </summary>
    /// <example>2</example>
    [Range(1, 3, ErrorMessage = "1 = Curto, 2 = Médio, 3 = Longo")]
    public int Horizonte { get; set; }
    /// <summary>
    /// Objetivo de Investimentos (USE /opcoes para ver valores)
    /// </summary>
    /// <example>3</example>
    [Range(1, 5, ErrorMessage = "1 = Reserva, 2 = Aposentadoria, 3 = Imóvel, 4 = Educação, 5 = Outros")]
    public int Objetivo { get; set; }
    /// <summary>
    /// Tolerância à Perda de 1 a 10 (Use /opcoes para ver valores recomendados)
    /// </summary>
    /// <example>7</example>
    [Range(0, 10, ErrorMessage = "Valor deve estar entre 0 e 10")]
    public int ToleranciaPerda { get; set; }
    public bool ExperienciaInvestimentos { get; set; }
}
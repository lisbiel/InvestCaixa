using System;
using System.Collections.Generic;
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
    public int Horizonte { get; set; }
    public int Objetivo { get; set; }
    public int ToleranciaPerda { get; set; }
    public bool ExperienciaInvestimentos { get; set; }
}
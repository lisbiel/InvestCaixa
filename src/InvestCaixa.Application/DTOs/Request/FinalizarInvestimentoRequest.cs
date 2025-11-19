using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestCaixa.Application.DTOs.Request;

public class FinalizarInvestimentoRequest
{
    public int ClienteId { get; set; }
    public Guid ProdutoId { get; set; }
    public decimal ValorAplicado { get; set; }
    public int PrazoMeses { get; set; }
}
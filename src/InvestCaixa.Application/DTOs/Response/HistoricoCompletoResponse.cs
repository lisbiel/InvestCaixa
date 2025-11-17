using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestCaixa.Application.DTOs.Response;

public class HistoricoCompletoResponse
{
    public IEnumerable<SimulacaoHistoricoResponse> Simulacoes { get; set; } = new List<SimulacaoHistoricoResponse>();
    public IEnumerable<InvestimentoFinalizadoResponse> Investimentos { get; set; } = new List<InvestimentoFinalizadoResponse>();
    public decimal TotalInvestido { get; set; }
    public decimal TotalResgatado { get; set; }
    public decimal RentabilidadeReal { get; set; }
    public int TotalOperacoes { get; set; }
}

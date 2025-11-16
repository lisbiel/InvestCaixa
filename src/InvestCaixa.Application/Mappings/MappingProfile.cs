namespace InvestCaixa.Application.Mappings;

using AutoMapper;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Domain.Entities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Produto -> ProdutoValidadoDto
        CreateMap<ProdutoInvestimento, ProdutoValidadoDto>()
            .ForMember(dest => dest.Tipo, 
                opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(dest => dest.Risco, 
                opt => opt.MapFrom(src => src.Risco.ToString()));

        // Produto -> ProdutoResponse
        CreateMap<ProdutoInvestimento, ProdutoResponse>()
            .ForMember(dest => dest.Tipo, 
                opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(dest => dest.Risco, 
                opt => opt.MapFrom(src => src.Risco.ToString()))
            .ForMember(dest => dest.PerfilRecomendado, 
                opt => opt.MapFrom(src => src.PerfilRecomendado.ToString()));

        // Simulacao -> SimulacaoHistoricoResponse
        CreateMap<Simulacao, SimulacaoHistoricoResponse>()
            .ForMember(dest => dest.Produto, 
                opt => opt.MapFrom(src => src.ProdutoSimulado));

        // PerfilRisco -> PerfilRiscoResponse
        CreateMap<PerfilRisco, PerfilRiscoResponse>()
            .ForMember(dest => dest.Perfil, 
                opt => opt.MapFrom(src => src.Perfil.ToString()));

        // Cliente -> ClienteResponse (será criado depois)
    }
}

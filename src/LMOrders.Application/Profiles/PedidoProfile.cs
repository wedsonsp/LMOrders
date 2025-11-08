using AutoMapper;
using LMOrders.Application.DTOs;
using LMOrders.Domain.Entities;

namespace LMOrders.Application.Profiles;

public class PedidoProfile : Profile
{
    public PedidoProfile()
    {
        CreateMap<PedidoItem, PedidoItemDto>();
        CreateMap<Pedido, PedidoResponse>()
            .ForMember(dest => dest.Itens, opt => opt.MapFrom(src => src.Itens))
            .ForMember(dest => dest.ValorTotal, opt => opt.MapFrom(src => src.ValorTotal))
            .ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.ClienteId))
            .ForMember(dest => dest.DataPedido, opt => opt.MapFrom(src => src.DataPedido));
    }
}


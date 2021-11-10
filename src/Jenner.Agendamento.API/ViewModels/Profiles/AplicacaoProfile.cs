using AutoMapper;
using Jenner.Agendamento.API.Services;
using Jenner.Comum.Models;

namespace Jenner.Agendamento.API.ViewModels.Profiles
{
    public class AplicacaoProfile : Profile
    {
        public AplicacaoProfile()
        {
            CreateMap<AplicacaoCreate, Aplicacao>();

            CreateMap<Aplicacao, AplicacaoView>();
        }
    }
}

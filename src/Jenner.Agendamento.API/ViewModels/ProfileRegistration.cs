using Jenner.Agendamento.API.ViewModels.Profiles;
using System;

namespace Jenner.Agendamento.API.ViewModels
{
    public class ProfileRegistration
    {
        public static Type[] GetProfiles() => new[]
        {
            typeof(AplicacaoProfile)
        };
    }
}

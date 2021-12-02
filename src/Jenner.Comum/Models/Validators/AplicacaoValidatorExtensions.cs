using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenner.Comum.Models.Validators
{
    public static class AplicacaoValidatorExtensions
    {
        public static void ValidaAgendamento(this IAplicacao aplicacao)
        {
            if (aplicacao.DataAgendamento < DateTime.Today)
            { 
                throw new ArgumentOutOfRangeException("Não é possível agendar uma aplicação para uma data anterior ao dia atual");
            }
        }
    }
}

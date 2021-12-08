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

        public static void ValidaAplicacao(this IAplicacao aplicacao)
        {
            if (!aplicacao.DataAplicacao.HasValue)
            {
                throw new ArgumentOutOfRangeException("Aplicação não possui data de aplicação");
            }

            if (aplicacao.DataAplicacao < DateTime.Today)
            {
                throw new ArgumentOutOfRangeException("Não é possível registrar uma aplicação para uma data anterior ao dia atual");
            }

            if (aplicacao.DataAplicacao < aplicacao.DataAgendamento)
            {
                throw new ArgumentOutOfRangeException("Não é possível realizar a aplicação antes da data agendada");
            }
        }
    }
}

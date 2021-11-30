using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jenner.Agendamento.API.ViewModels
{
    /// <summary>
    /// Objeto para visualização
    /// </summary>
    public class AplicacaoView
    {
        public Guid Id { get; set; }

        public string CPF { get; set; }

        public int IdVacina { get; set; }

        public int Dose { get; set; }

        public DateTime DataAgendamento { get; set; }
    }
}

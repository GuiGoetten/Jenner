using System;

namespace Jenner.Comum.Models
{
    class Aplicacao
    {
        public string CPF { get; set; }

        public Vacina Vacina { get; set; }

        public int Dose { get; set; }

        public DateTime DataAgendamento { get; set; }

        public DateTime? DataAplicacao { get; set; }

        public virtual Carteira Carteira { get; set; }
    }
}

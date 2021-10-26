using System;

namespace Jenner.Comum.Models
{
    class Vacinacao
    {
        public string Cpf { get; set; }

        public Vacina Vacina { get; set; }

        public int Dose { get; set; }

        public DateTime DataAgendada { get; set; }

        public DateTime DataAplicada { get; set; }

        public Status StatusVacina{ get; set; }

        public virtual Carteira Carteira { get; set; }
    }

    enum Status
    {
        Agendada,
        Aplicada,
        Pendente
    }
}

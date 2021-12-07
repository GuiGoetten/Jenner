using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Jenner.Comum.Models
{
    public interface IAplicacao
    {
        public string Cpf { get; }
        public string NomePessoa { get; }
        public string NomeVacina { get; }
        public int Dose { get; }
        public DateTime DataAgendamento { get; }
        public DateTime? DataAplicacao { get; }

    }

}

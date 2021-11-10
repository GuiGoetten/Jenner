using System;

namespace Jenner.Comum.Models
{
    /// <summary>
    /// Modelo de aplicação
    /// </summary>
    public class Aplicacao
    {
        public string CPF { get; set; }
        public Guid? Id { get; set; }
        public int IdVacina { get; set; }
        public virtual Vacina Vacina { get; set; }
        public int Dose { get; set; }
        public DateTime DataAgendamento { get; set; }
        public DateTime? DataAplicacao { get; set; }
        public virtual Carteira Carteira { get; set; }
    }
}

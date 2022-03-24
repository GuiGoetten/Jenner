using System;

namespace Jenner.Comum.Models
{
    public interface IVacina
    {
        public string NomeVacina { get; }
        public string Descricao { get; }
        public int Doses { get; }
        public int Intervalo { get; }
    }
}

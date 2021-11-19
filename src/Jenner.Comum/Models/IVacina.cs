using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenner.Comum.Models
{
    public interface IVacina
    {
        public string NomeVacina { get; }
        public string Descricao { get; }
        public int Doses { get; }
    }
}

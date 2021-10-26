using System.Collections.Generic;

namespace Jenner.Comum.Models
{
    class Carteira
    {

        public string Cpf { get; set; }

        public string NomePessoa { get; set; }

        public virtual ICollection<Vacinacao> Vacinas { get; set; }
    }
}

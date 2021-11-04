using System.Collections.Generic;

namespace Jenner.Comum.Models
{
    class Carteira
    {

        public string CPF { get; set; }

        public string NomePessoa { get; set; }

        public virtual ICollection<Aplicacao> Aplicacoes { get; set; }
    }
}

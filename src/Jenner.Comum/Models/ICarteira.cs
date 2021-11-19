using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenner.Comum.Models
{
    public interface ICarteira
    {
        public string Cpf { get; }
        public string NomePessoa { get; }
        public DateTime DataNascimento { get; }
        public IEnumerable<Aplicacao> Aplicacoes { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Jenner.Comum.Models
{
    public record Carteira(string Cpf, string NomePessoa, DateTime DataNascimento) : ICarteira
    {
        public IEnumerable<Aplicacao> Aplicacoes { get; init; } = Enumerable.Empty<Aplicacao>();

        public Carteira AddAplicacao(Aplicacao aplicacao)
        {
            return this with
            {
                Aplicacoes = new List<Aplicacao>(Aplicacoes)
                {
                    aplicacao
                },
            };
        }

        public Aplicacao GetLatestAplicacao() 
        {
            return Aplicacoes.Last();
        }

    }
}

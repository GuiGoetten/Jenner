using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Jenner.Comum.Models
{
    public record Carteira(Guid Id, string Cpf, string NomePessoa, DateTime DataNascimento) : ICarteira
    {
        public IEnumerable<Aplicacao> Aplicacoes { get; init; } = Enumerable.Empty<Aplicacao>();

        public Carteira AddAplicacao(Aplicacao aplicacao)
        {

            Carteira cr = this with
            {
                Aplicacoes = new List<Aplicacao>(Aplicacoes)
                {
                    aplicacao
                },
            };

            return cr;
        }

        public Aplicacao GetLatestAplicacao() 
        {
            return Aplicacoes.Last();
        }

    }
}

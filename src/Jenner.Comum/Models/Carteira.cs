using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Jenner.Comum.Models
{
    public record Carteira(Guid Id, string CPF, string NomePessoa)
    {
        public IEnumerable<Aplicacao> Aplicacoes { get; private set; }
        public Carteira(Guid id, string cpf, string nomePessoa, IEnumerable<Aplicacao> aplicacoes) : this(id, cpf, nomePessoa)
        {
            Aplicacoes = new HashSet<Aplicacao>(aplicacoes) ?? new HashSet<Aplicacao>();
        }

    }
}

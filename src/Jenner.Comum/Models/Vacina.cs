using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Jenner.Comum.Models
{
    public record Vacina(Guid Id, string Nome, string Descricao, int Doses)
    {
        public IEnumerable<Aplicacao> Aplicacoes { get; private set; }
        public Vacina(Guid id, string nome, string descricao, int doses, IEnumerable<Aplicacao> aplicacoes) : this(id, nome, descricao, doses)
        {
            Aplicacoes = new HashSet<Aplicacao>(aplicacoes) ?? new HashSet<Aplicacao>();
        }
    }
}
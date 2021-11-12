using Jenner.Comum.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jenner.Agendamento.API.Data
{
    public class VacinaPersistence
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int Doses { get; set; }
        public IEnumerable<AplicacaoPersistence> Aplicacoes { get; set; } = new HashSet<AplicacaoPersistence>();

        public Vacina ToVacina() => new (Id, Nome, Descricao, Doses, Aplicacoes.Select(a => a.ToAplicacao()));
    }

    public static class VacinaPersistenceExtensions
    {
        public static VacinaPersistence ToPersistence(this Vacina vacina)
        {
            return new()
            {
                Id = vacina.Id,
                Nome = vacina.Nome,
                Descricao = vacina.Descricao,
                Doses = vacina.Doses
            };
        }
    }
}

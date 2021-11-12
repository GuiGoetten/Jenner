using Jenner.Comum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jenner.Agendamento.API.Data
{
    public class CarteiraPersistence
    {
        public Guid Id { get; set; }
        public string CPF { get; set; }
        public string NomePessoa { get; set; }
        public IEnumerable<AplicacaoPersistence> Aplicacoes { get; set; } = new HashSet<AplicacaoPersistence>();
        public Carteira ToCarteira() => new Carteira(Id, CPF, NomePessoa, Aplicacoes.Select(a => a.ToAplicacao()));
    }

    public static class CarteiraPersistenceExtensions
    {
        public static CarteiraPersistence ToPersistence(this Carteira carteira)
        {
            return new()
            {
                Id = carteira.Id,
                CPF = carteira.CPF,
                NomePessoa = carteira.NomePessoa,
                Aplicacoes = carteira.Aplicacoes.Select(a => a.ToPersistence())
            };
        }
    }

}


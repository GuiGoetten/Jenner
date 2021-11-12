using Jenner.Comum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jenner.Agendamento.API.Data
{
    public class AplicacaoPersistence
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CPF { get; set; }
        public Guid IdVacina { get; set; }
        public int Dose { get; set; }
        public VacinaPersistence Vacina { get; set; }
        public DateTime DataAgendamento { get; set; }
        public DateTime? DataAplicacao { get; set; }
        public CarteiraPersistence Carteira { get; set; }  
        public Aplicacao ToAplicacao() => new(Id, CPF, IdVacina, Dose, DataAgendamento, DataAplicacao, Vacina.ToVacina(), Carteira.ToCarteira());

    }

    public static class AplicacaoPersistenceExtensions
    {
        public static AplicacaoPersistence ToPersistence(this Aplicacao aplicacao)
        {
            return new()
            {
                Id = aplicacao.Id,
                CPF = aplicacao.Cpf,
                IdVacina = aplicacao.idVacina,
                Dose = aplicacao.Dose,
                Vacina = aplicacao.Vacina.ToPersistence(),
                DataAgendamento = aplicacao.DataAgendamento,
                DataAplicacao = aplicacao.DataAplicacao,
                Carteira = aplicacao.Carteira.ToPersistence()
            };
        }
    }
}

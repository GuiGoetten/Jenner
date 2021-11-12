using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Jenner.Comum.Models
{
    /// <summary>
    /// Modelo de aplicação
    /// </summary>
    public record Aplicacao(Guid Id, string Cpf, Guid idVacina, int Dose, DateTime DataAgendamento, DateTime? DataAplicacao)
    {
        public Vacina Vacina { get; set; }
        public Carteira Carteira { get; set; }

        public Aplicacao(Guid id, string cpf, Guid idVacina, int dose, DateTime dataAgendamento, DateTime? dataAplicacao, Vacina vacina, Carteira carteira) : this(id, cpf, idVacina, dose, dataAgendamento, dataAplicacao)
        {
            Vacina = vacina;
            Carteira = carteira;
        }
    }
}

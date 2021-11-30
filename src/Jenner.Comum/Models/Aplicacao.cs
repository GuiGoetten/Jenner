using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Jenner.Comum.Models
{
    /// <summary>
    /// Modelo de aplicação
    /// </summary>
    public record Aplicacao(string Cpf, string NomePessoa, string NomeVacina, int Dose, DateTime DataAgendamento, DateTime? DataAplicacao) : IAplicacao
    {

    }
}

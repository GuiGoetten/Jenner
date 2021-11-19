using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Jenner.Comum.Models
{
    public record Vacina(string NomeVacina, string Descricao, int Doses) : IVacina
    {

    }
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Jenner.Comum.Models
{
    class Pessoa
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public int Idade { get; set; }
        public string Cpf { get; set; }
        public DateTime DataNascimento { get; set; }
    }
}

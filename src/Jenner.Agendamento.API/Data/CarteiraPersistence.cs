using Jenner.Comum.Models;
using Jenner.Comum.Models.Validators;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Agendamento.API.Data
{
    public class CarteiraPersistence : ICarteira
    {
        public Guid Id = Guid.NewGuid();
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public IEnumerable<Aplicacao> Aplicacoes { get; set; } = Enumerable.Empty<Aplicacao>();
        public Carteira ToCarteira() => new (Id, Cpf, NomePessoa, DataNascimento) { Aplicacoes = Aplicacoes ?? Enumerable.Empty<Aplicacao>() };
    }

    public static class CarteiraPersistenceExtensions
    {
        public static CarteiraPersistence ToPersistence(this Carteira carteira)
        {
            return new()
            {
                Id = carteira.Id,
                Cpf = carteira.Cpf,
                NomePessoa = carteira.NomePessoa,
                DataNascimento = carteira.DataNascimento,
                Aplicacoes = carteira.Aplicacoes ?? Enumerable.Empty<Aplicacao>()
            };
        }

        private const string AgendamentoCollection = "agendamento";
        public static IMongoCollection<CarteiraPersistence> GetCarteiraCollection(this IMongoDatabase mongo) => mongo.GetCollection<CarteiraPersistence>(AgendamentoCollection);
        public static async Task<Carteira> FetchAsync(this IMongoCollection<CarteiraPersistence> collection, string cpf, string nomePessoa , CancellationToken cancellationToken = default)
        {
            CarteiraPersistence mongoResult = await collection
                .Find(c => c.Cpf.Equals(cpf) && c.NomePessoa.Equals(nomePessoa))
                .FirstOrDefaultAsync(cancellationToken);
            return mongoResult?.ToCarteira() ?? null;
        }
        public static async Task<CarteiraPersistence> InsertNewAsync(this IMongoCollection<CarteiraPersistence> collection, CarteiraPersistence carteira, CancellationToken cancellationToken = default)
        {
            carteira.ValidaCarteira();

            await collection.InsertOneAsync(carteira, null, cancellationToken);
            return carteira;
        }

        public static async Task<Carteira> FindOrCreateAsync(this IMongoCollection<CarteiraPersistence> collection, string cpf, string nomePessoa, DateTime dataNascimento, Comum.Models.Aplicacao aplicacao, CancellationToken cancellationToken = default)
        {
            CarteiraPersistence novaCarteira = new CarteiraPersistence()
            {
                Cpf = cpf,
                NomePessoa = nomePessoa,
                DataNascimento = dataNascimento,
                Aplicacoes = new List<Comum.Models.Aplicacao>()
                {
                    aplicacao
                },
            };

            novaCarteira.ValidaCarteira();

            novaCarteira = await collection.InsertNewAsync(novaCarteira, cancellationToken);

            return novaCarteira?.ToCarteira() ?? null;
        }

        public static async Task<Carteira> UpdateAsync(this IMongoCollection<CarteiraPersistence> collection, CarteiraPersistence carteira, CancellationToken cancellationToken = default)
        {
            carteira.ValidaCarteira();

            _ = await collection
                .ReplaceOneAsync(s => s.Id.Equals(carteira.Id), carteira, new ReplaceOptions(), cancellationToken);
            return carteira.ToCarteira();
        }
    }

}


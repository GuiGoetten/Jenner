using Jenner.Comum.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
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
        public IEnumerable<Aplicacao> Aplicacoes { get; set; }
        public Carteira ToCarteira() => new (Cpf, NomePessoa, DataNascimento) { Aplicacoes = this.Aplicacoes};
    }

    public static class CarteiraPersistenceExtensions
    {
        public static CarteiraPersistence ToPersistence(this Carteira carteira)
        {
            return new()
            {
                Cpf = carteira.Cpf,
                NomePessoa = carteira.NomePessoa,
                DataNascimento = carteira.DataNascimento,
                Aplicacoes = carteira.Aplicacoes
            };
        }

        private const string AgendamentoCollection = "agendamento";
        public static IMongoCollection<CarteiraPersistence> GetCarteiraCollection(this IMongoDatabase mongo) => mongo.GetCollection<CarteiraPersistence>(AgendamentoCollection);
        public static async Task<Carteira> FetchAsync(this IMongoCollection<CarteiraPersistence> collection, string cpf, string nomePessoa , CancellationToken cancellationToken = default)
        {
            CarteiraPersistence mongoResult = await collection
                .Find(c => c.Cpf.Equals(cpf) && c.NomePessoa.Equals(nomePessoa))
                .SingleOrDefaultAsync(cancellationToken);
            return mongoResult?.ToCarteira() ?? null;
        }
        public static async Task<Carteira> InsertNewAsync(this IMongoCollection<CarteiraPersistence> collection, CarteiraPersistence carteira, CancellationToken cancellationToken = default)
        {
            await collection.InsertOneAsync(carteira, null, cancellationToken);
            return carteira.ToCarteira();
        }

        public static async Task<Carteira> FindOrCreateAsync(this IMongoCollection<CarteiraPersistence> collection, string cpf, string nomePessoa, DateTime dataNascimento, CancellationToken cancellationToken = default)
        {
            CarteiraPersistence mongoResult = await collection
                .Find(c => c.Cpf.Equals(cpf) && c.NomePessoa.Equals(nomePessoa))
                .SingleOrDefaultAsync(cancellationToken);

            return mongoResult?.ToCarteira() ?? 
                await collection.InsertNewAsync(
                    new CarteiraPersistence() 
                    { 
                        Cpf = cpf, 
                        NomePessoa = nomePessoa, 
                        DataNascimento = dataNascimento
                    }, cancellationToken);
        }

        public static async Task<Carteira> UpdateAsync(this IMongoCollection<CarteiraPersistence> collection, CarteiraPersistence carteira, CancellationToken cancellationToken = default)
        {
            _ = await collection
                .ReplaceOneAsync(s => s.Cpf.Equals(carteira.Cpf) && s.NomePessoa.Equals(carteira.NomePessoa), carteira, new ReplaceOptions(), cancellationToken);
            return carteira.ToCarteira();
        }
    }

}


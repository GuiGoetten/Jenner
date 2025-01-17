﻿using Jenner.Comum.Models;
using Jenner.Comum.Models.Validators;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Carteira.API.Data
{
    public class CarteiraPersistence : ICarteira
    {
        public Guid Id = Guid.NewGuid();
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public IEnumerable<Comum.Models.Aplicacao> Aplicacoes { get; set; } = Enumerable.Empty<Comum.Models.Aplicacao>();
        public Comum.Models.Carteira ToCarteira() => new (Id, Cpf, NomePessoa, DataNascimento) { Aplicacoes = Aplicacoes ?? Enumerable.Empty<Comum.Models.Aplicacao>() };
    }

    public static class CarteiraPersistenceExtensions
    {
        public static CarteiraPersistence ToPersistence(this Comum.Models.Carteira carteira)
        {
            return new()
            {
                Id = carteira.Id,
                Cpf = carteira.Cpf,
                NomePessoa = carteira.NomePessoa,
                DataNascimento = carteira.DataNascimento,
                Aplicacoes = carteira.Aplicacoes ?? Enumerable.Empty<Comum.Models.Aplicacao>()
            };
        }

        private const string AgendamentoCollection = "carteira";
        public static IMongoCollection<CarteiraPersistence> GetCarteiraCollection(this IMongoDatabase mongo) => mongo.GetCollection<CarteiraPersistence>(AgendamentoCollection);
        public static async Task<Comum.Models.Carteira> FetchAsync(this IMongoCollection<CarteiraPersistence> collection, string cpf, string nomePessoa , CancellationToken cancellationToken = default)
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

        public static async Task<Comum.Models.Carteira> CreateAsync(this IMongoCollection<CarteiraPersistence> collection, string cpf, string nomePessoa, DateTime dataNascimento, Comum.Models.Aplicacao aplicacao, CancellationToken cancellationToken = default)
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

            return novaCarteira?.ToCarteira();
        }

        public static async Task<Comum.Models.Carteira> UpdateAsync(this IMongoCollection<CarteiraPersistence> collection, CarteiraPersistence carteira, CancellationToken cancellationToken = default)
        {
            carteira.ValidaCarteira();

            _ = await collection
                .ReplaceOneAsync(s => s.Id.Equals(carteira.Id), carteira, new ReplaceOptions(), cancellationToken);
            return carteira.ToCarteira();
        }

        public static async Task<IEnumerable<Comum.Models.Carteira>> GetAllAsync(this IMongoCollection<CarteiraPersistence> collection, CancellationToken cancellationToken = default)
        {
            List<CarteiraPersistence> mongoResults = await collection.Find(_ => true).ToListAsync(cancellationToken);
            return mongoResults.Select(r => r.ToCarteira());
        }

        public static async Task<IEnumerable<Comum.Models.Carteira>> GetOneAsync(this IMongoCollection<CarteiraPersistence> collection, string cpf, string nomePessoa, CancellationToken cancellationToken = default)
        {
            List<CarteiraPersistence> mongoResults = await collection.Find(c => c.Cpf.Equals(cpf) && c.NomePessoa.Equals(nomePessoa)).ToListAsync(cancellationToken);
            return mongoResults.Select(r => r.ToCarteira());
        }
    }

}


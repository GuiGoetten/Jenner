using Jenner.Comum.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Agendamento.API.Data
{
    public class AplicacaoPersistence
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CPF { get; set; }
        public string NomeVacina { get; set; }
        public int Dose { get; set; }
        //public VacinaPersistence Vacina { get; set; }
        public DateTime DataAgendamento { get; set; }
        public DateTime? DataAplicacao { get; set; }
        //public CarteiraPersistence Carteira { get; set; }  
        public Aplicacao ToAplicacao() => new(Id, CPF, NomeVacina, Dose, DataAgendamento, DataAplicacao);//, Vacina.ToVacina(), Carteira.ToCarteira());

    }

    public static class AplicacaoPersistenceExtensions
    {
        public static AplicacaoPersistence ToPersistence(this Aplicacao aplicacao)
        {
            return new()
            {
                Id = aplicacao.Id,
                CPF = aplicacao.Cpf,
                NomeVacina = aplicacao.NomeVacina,
                Dose = aplicacao.Dose,
                //Vacina = aplicacao.Vacina.ToPersistence(),
                DataAgendamento = aplicacao.DataAgendamento,
                DataAplicacao = aplicacao.DataAplicacao
                //Carteira = aplicacao.Carteira.ToPersistence()
            };
        }

        private const string AplicacaoCollection = "agendamento";

        public static IMongoCollection<AplicacaoPersistence> GetAplicacaoCollection(this IMongoDatabase mongo) => mongo.GetCollection<AplicacaoPersistence>(AplicacaoCollection);

        public static async Task<Aplicacao> InsertNewAsync(this IMongoCollection<AplicacaoPersistence> collection, AplicacaoPersistence aplicacao, CancellationToken cancellationToken = default)
        {
            await collection.InsertOneAsync(aplicacao, null, cancellationToken);
            return aplicacao.ToAplicacao();
        }

        public static async Task<IEnumerable<Aplicacao>> GetAllAsync(this IMongoCollection<AplicacaoPersistence> collection, CancellationToken cancellationToken = default)
        {
            List<AplicacaoPersistence> mongoResults = await collection.Find(_ => true).ToListAsync(cancellationToken);
            return mongoResults.Select(r => r.ToAplicacao());
        }

        public static async Task<Aplicacao> UpdateAsync(this IMongoCollection<AplicacaoPersistence> collection, AplicacaoPersistence aplicacao, CancellationToken cancellationToken = default)
        {
            _ = await collection
                .ReplaceOneAsync(s => s.Id.Equals(aplicacao.Id), aplicacao, new ReplaceOptions(), cancellationToken);
            return aplicacao.ToAplicacao();
        }

        public static async Task<Aplicacao> FetchAsync(this IMongoCollection<AplicacaoPersistence> collection, Guid aplicacaoId, CancellationToken cancellationToken = default)
        {
            AplicacaoPersistence mongoResult = await collection
                .Find(s => s.Id.Equals(aplicacaoId))
                .SingleOrDefaultAsync(cancellationToken);
            return mongoResult?.ToAplicacao() ?? null;
        }
    }
}

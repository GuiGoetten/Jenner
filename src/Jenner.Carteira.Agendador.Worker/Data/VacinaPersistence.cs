using Jenner.Comum.Models;
using Jenner.Comum.Models.Validators;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Carteira.Agendador.Worker.Data
{
    public class VacinaPersistence : IVacina
    {
        public string NomeVacina { get; set; }

        public string Descricao { get; set; }

        public int Doses { get; set; }

        public int Intervalo { get; set; }

        public Vacina ToVacina() => new(NomeVacina, Descricao, Doses, Intervalo);
    }

    public static class VacinaPersistenceExtensions
    {
        public static VacinaPersistence ToPersistence(this Vacina vacina)
        {
            return new()
            {
                NomeVacina = vacina.NomeVacina,
                Descricao = vacina.Descricao,
                Doses = vacina.Doses,
                Intervalo = vacina.Intervalo
            };
        }

        private const string VacinaCollection = "vacina";
        public static IMongoCollection<VacinaPersistence> GetVacinaCollection(this IMongoDatabase mongo) => mongo.GetCollection<VacinaPersistence>(VacinaCollection);
        public static async Task<Vacina> FetchAsync(this IMongoCollection<VacinaPersistence> collection, string nomeVacina, CancellationToken cancellationToken = default)
        {
            VacinaPersistence mongoResult = await collection
                .Find(v => v.NomeVacina.Equals(nomeVacina))
                .SingleOrDefaultAsync(cancellationToken);
            return mongoResult?.ToVacina() ?? null;
        }

    }
}

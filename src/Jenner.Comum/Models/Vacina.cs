using System;

namespace Jenner.Comum.Models
{
    public record Vacina(Guid id, string NomeVacina, string Descricao, int Doses, int Intervalo) : IVacina
    {

    }
}
namespace Jenner.Comum.Models
{
    public record Vacina(string NomeVacina, string Descricao, int Doses, int Intervalo) : IVacina
    {

    }
}
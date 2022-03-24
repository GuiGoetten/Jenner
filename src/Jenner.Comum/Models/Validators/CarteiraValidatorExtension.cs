using System;
using System.Text.RegularExpressions;
using DanielMarques.Utilities;

namespace Jenner.Comum.Models.Validators
{
    public static class CarteiraValidatorExtension
    {
        public static void ValidaCarteira(this ICarteira carteira)
        {
            if (string.IsNullOrWhiteSpace(carteira.NomePessoa) || string.IsNullOrWhiteSpace(carteira.Cpf) || carteira.DataNascimento == DateTime.MinValue)
            {
                throw new ArgumentNullException("Uma carteira válida deve conter o Nome da Pessoa, CPF e Data de Nascimento");
            }

            ulong cpf = ulong.Parse(Regex.Replace(carteira.Cpf, "[^0-9]", ""));

            if (!CPF.IsValid(new CPF(cpf)))
            {
                throw new ArgumentException("O CPF informado não é válido");
            }
        }
    }
}

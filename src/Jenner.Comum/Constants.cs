namespace Jenner.Comum
{
    public static class Constants
    {

        public const string MongoConnectionString = @"MongoDb";

        public const string MongoAgendamentoDatabase = @"jenner";

        public const string MongoAplicacaoDatabase = @"jenner";

        public const string MongoCarteiraDatabase = @"jenner";

        public static class CloudEvents
        {
            public const string AgendadaTopic = "Vacinacao.Agendada";

            public const string AgendadaType = "Vacinacao.Type.Agendada";

            public const string AgendarTopic = "Vacinacao.Agendar";

            public const string AgendarType = "Vacinacao.Type.Agendar";

            public const string AplicadaTopic = "Vacinacao.Aplicada";

            public const string AplicadaType = "Vacinacao.Type.Aplicada";

            public const string AplicarTopic = "Vacinacao.Aplicar";

            public const string AplicarType = "Vacinacao.Type.Aplicar";
        }
    }
}

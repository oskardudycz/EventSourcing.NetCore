namespace Domain.Tests
{
    public static class Settings
    {
        public static string ConnectionString =
            "PORT = 5432; HOST = 127.0.0.1; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'EventSourcing'; PASSWORD = 'postgres'; USER ID = 'postgres'";
    }
}

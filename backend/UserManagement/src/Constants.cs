using System;
namespace UserManagement
{
    public static class Constants
    {
        public static readonly string SECRET_KEY = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        public static string TOKEN_KEY = "token";
        public static string DBHost = Environment.GetEnvironmentVariable("DB_HOST");
        public static string DBUser = Environment.GetEnvironmentVariable("DB_USER");
        public static string DBName = Environment.GetEnvironmentVariable("DB_NAME");
        public static string DBPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        public static string DBPort = Environment.GetEnvironmentVariable("DB_PORT");
        public static string LOGGING_URL = Environment.GetEnvironmentVariable("LOGGING_URL");
    }
}

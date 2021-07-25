using System;

namespace Authentication
{
    public static class Constants
    {
        public static readonly string SECRET_KEY = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        public static readonly string TOKEN_KEY = "token";
        public static string DBHost = Environment.GetEnvironmentVariable("DB_HOST");
        public static string DBUser = Environment.GetEnvironmentVariable("DB_USER");
        public static string DBName = Environment.GetEnvironmentVariable("DB_NAME");
        public static string DBPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        public static string DBPort = Environment.GetEnvironmentVariable("DB_PORT");
        public static string AES256_KEY = Environment.GetEnvironmentVariable("AUTH_AES256_KEY");
        public static string AES256_IV = Environment.GetEnvironmentVariable("AUTH_AES256_IV");
        public static string LOGGING_URL = "https://comp410s21loggingfunctionapp.azurewebsites.net/api/LoggingFunctionApp?code=lR00y8isqCaih0Lzf8awP7dXLJ3a7gDP4Zfkp18fqwdfaIFXn2KK3A==";
    }
}
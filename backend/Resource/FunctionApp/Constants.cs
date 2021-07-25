using System;

namespace FunctionApp
{
    public static class Constants
    {
        public static readonly string SECRET_KEY = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        public static string TOKEN_KEY = "token";
        public static string LOGGING_URL = Environment.GetEnvironmentVariable("LOGGING_URL");

        public static string getConnString()
        {
            return String.Format(
                    "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                    Environment.GetEnvironmentVariable("DB_HOST"),
                    Environment.GetEnvironmentVariable("DB_USER"),
                    Environment.GetEnvironmentVariable("DB_NAME"),
                    Environment.GetEnvironmentVariable("DB_PORT"),
                    Environment.GetEnvironmentVariable("DB_PASSWORD")
            );
        }
    }
}

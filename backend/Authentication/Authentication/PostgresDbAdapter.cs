using System;
using System.Threading.Tasks;
using Npgsql;

namespace Authentication
{
    public class PostgresDbAdapter : IDbAdapter
    {
        private string connString;

        public PostgresDbAdapter(string Host, string User, string DBname, string Password, string Port)
        {
            connString = String.Format(
                    "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                    Host,
                    User,
                    DBname,
                    Port,
                    Password);
        }

        public int GetIDByUsername(string username)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                // Find user id in database
                using (var command = new NpgsqlCommand("SELECT * FROM users WHERE username = @a1 LIMIT 1", conn))
                {
                    command.Parameters.AddWithValue("a1", username);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int user_id = reader.GetInt32(0);
                        return user_id;
                    }
                }
            }
            return -1;
        }

        public string GetStatusByID(int user_id)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                // Find user id in database
                using (var command = new NpgsqlCommand("SELECT user_status FROM users WHERE user_id = @a1 LIMIT 1", conn))
                {
                    command.Parameters.AddWithValue("a1", user_id);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string status = reader.GetString(0);
                        return status;
                    }
                }
            }
            return null;
        }

        public async Task<int> CreateCredentialsEntry(int userId, string password)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                // Store new entry in credentials
                await using (var command = new NpgsqlCommand("INSERT INTO credentials(user_id, password) VALUES(@v1, @v2);", conn))
                {
                    command.Parameters.AddWithValue("v1", userId);
                    command.Parameters.AddWithValue("v2", password);
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> UpdateCredentialsEntry(int userId, string password)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                // Store new entry in credentials
                await using (var command = new NpgsqlCommand("UPDATE credentials SET password = @v1 WHERE user_id = @v2;", conn))
                {
                    command.Parameters.AddWithValue("v1", password);
                    command.Parameters.AddWithValue("v2", userId);
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        public string LookupPasswordByUserId(int user_id)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                // Find user id in database
                using (var command = new NpgsqlCommand("SELECT password FROM credentials WHERE user_id = @a1 LIMIT 1", conn))
                {
                    command.Parameters.AddWithValue("a1", user_id);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var password = reader.GetString(0);
                        return password;
                    }
                }
            }
            return null;
        }

        public bool DoesIdExist(int userId)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                // Find user id in database
                using (var command = new NpgsqlCommand("SELECT * FROM credentials WHERE user_id = @v LIMIT 1", conn))
                {
                    command.Parameters.AddWithValue("v", userId);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

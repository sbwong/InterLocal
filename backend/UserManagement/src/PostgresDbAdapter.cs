using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace UserManagement
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

        public async Task<int> EditUserAsync(int user_id, dynamic data)
        {

            string username = null;
            string college = null;
            string email = null;
            string phone_number = null;
            string country = null;
            string first_name = null;
            string last_name = null;
            string year = null;
            string picture = null;
            string bio = null;
            string queryStr = "UPDATE users SET ";
            int counter = 0;
            string temp = null;
            string temp2 = null;
            List<string> items = new List<string>();
            if (data?.phone_number != null) {
                ++counter;
                temp = "v" + counter;
                queryStr += "phone_number=@" + temp + ", ";
                phone_number = data.phone_number;
                temp2 = temp + " " + phone_number;
                items.Add(temp2);
            }
            if (data?.username != null) {
                ++counter;
                temp = "v" + counter;
                queryStr += "username=@" + temp + ", ";
                username = data.username;
                temp2 = temp + " " + username;
                items.Add(temp2);
            }
            if (data?.college != null) {
                ++counter;
                temp = "v" + counter;
                queryStr += "college=@" + temp + ", ";
                college = data.college;
                temp2 = temp + " " + college;
                items.Add(temp2);
            }
            if (data?.email != null) {
                ++counter;
                temp = "v" + counter;
                queryStr += "email=@" + temp + ", ";
                email = data.email;
                temp2 = temp + " " + email;
                items.Add(temp2);
            }
            if (data?.country != null) {
                ++counter;
                temp = "v" + counter;
                queryStr += "country=@" + temp + ", ";
                country = data.country;
                temp2 = temp + " " + country;
                items.Add(temp2);
            }
            if (data?.first_name != null) {
                ++counter;
                temp = "v" + counter;
                queryStr += "first_name=@" + temp + ", ";
                first_name = data.first_name;
                temp2 = temp + " " + first_name;
                items.Add(temp2);
            }
            if (data?.last_name != null) {
                ++counter;
                temp = "v" + counter;
                queryStr += "last_name=@" + temp + ", ";
                last_name = data.last_name;
                temp2 = temp + " " + last_name;
                items.Add(temp2);
            }
            if (data?.year != null) {
                ++counter;
                temp = "v" + counter;
                queryStr += "year=@" + temp + ", ";
                year = data.year;
                temp2 = temp + " " + year;
                items.Add(temp2);                
            }
            if (data?.profile_pic_url != null) {
                ++counter;
                temp = "v" + counter;
                queryStr += "profile_pic_url=@" + temp + ", ";
                picture = data.profile_pic_url;
                temp2 = temp + " " + picture;
                items.Add(temp2);
            }
            if (data?.bio != null)
            {
                ++counter;
                temp = "v" + counter;
                queryStr += "bio=@" + temp + ", ";
                bio = data.bio;
                temp2 = temp + " " + bio;
                items.Add(temp2);
            }
            if (counter == 0) return -1;
            queryStr = queryStr.Substring(0, queryStr.Length - 2);
            queryStr += " ";
            queryStr += "WHERE user_id=@a1;";
            Console.WriteLine(queryStr);
            string msg = "Updated user";
            int nRows = 0;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                var transaction = await conn.BeginTransactionAsync();
                using (var command = new NpgsqlCommand(queryStr, conn))
                {
                    command.Parameters.AddWithValue("a1", user_id);
                    foreach(string receiver in items) {
                        string[] tempRec = receiver.Split(" ");
                        string tempRec2 = String.Join(" ", tempRec.Skip(1).ToArray());
                        command.Parameters.AddWithValue(tempRec[0], tempRec2);
                    }
                    nRows = command.ExecuteNonQuery();
                    if (nRows == 0) msg = "Could not find user";
                }
                await transaction.CommitAsync();
            }
            return nRows;
        }

        public int DeleteUser(int user_id)
        {
            using (var conn = new NpgsqlConnection(connString))
            {  
                conn.Open();

                // post id, author id, content body, timestamp 
                using (var command = new NpgsqlCommand("DELETE FROM users WHERE user_id = @a1;", conn))
                {
                    command.Parameters.AddWithValue("a1", user_id);
                    int nRows = command.ExecuteNonQuery();
                    return nRows;
                }
            }
        }

        public async Task<int> EditUserPrefsAsync(int user_id, dynamic data)
        {
            int nRows = 0;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                var transaction = await conn.BeginTransactionAsync();
                if (data.is_email_public != null)
                {
                    // log.LogInformation((string)String.Format("{0}",data.is_email_public.GetType()));
                    bool val = data.is_email_public;
                    using var command = new NpgsqlCommand("UPDATE user_preferences SET isemailpublic = @v1 WHERE user_id=@a1;", conn);
                    command.Parameters.AddWithValue("a1", user_id);
                    command.Parameters.AddWithValue("v1", val);
                    nRows = command.ExecuteNonQuery();
                    if (nRows == 0) return nRows;
                }
                if (data.is_phone_public != null)
                {
                    bool val = data.is_phone_public;
                    using var command = new NpgsqlCommand("UPDATE user_preferences SET isphonepublic = @v1 WHERE user_id=@a1;", conn);
                    command.Parameters.AddWithValue("a1", user_id);
                    command.Parameters.AddWithValue("v1", val);
                    nRows = command.ExecuteNonQuery();
                    if (nRows == 0) return nRows;
                }
                if (data.is_country_public != null)
                {
                    bool val = data.is_country_public;
                    using var command = new NpgsqlCommand("UPDATE user_preferences SET iscountrypublic = @v1 WHERE user_id=@a1;", conn);
                    command.Parameters.AddWithValue("a1", user_id);
                    command.Parameters.AddWithValue("v1", val);
                    nRows = command.ExecuteNonQuery();
                    if (nRows == 0) return nRows;
                }
                if (data.is_year_public != null)
                {
                    bool val = data.is_year_public;
                    using var command = new NpgsqlCommand("UPDATE user_preferences SET isyearpublic = @v1 WHERE user_id=@a1;", conn);
                    command.Parameters.AddWithValue("a1", user_id);
                    command.Parameters.AddWithValue("v1", val);
                    nRows = command.ExecuteNonQuery();
                    if (nRows == 0) return nRows;
                }
                if (data.is_residential_college_public != null)
                {
                    bool val = data.is_residential_college_public;
                    using var command = new NpgsqlCommand("UPDATE user_preferences SET isresidentialcollegepublic = @v1 WHERE user_id=@a1;", conn);
                    command.Parameters.AddWithValue("a1", user_id);
                    command.Parameters.AddWithValue("v1", val);
                    nRows = command.ExecuteNonQuery();
                    if (nRows == 0) return nRows;
                }
                await transaction.CommitAsync();
            }
            return nRows;
        }

        public UserProfile GetUser(int user_id)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                // Find user id in database
                using (var command = new NpgsqlCommand("SELECT *, getUserScore(user_id) FROM users WHERE user_id= @a1", conn))
                {
                    command.Parameters.AddWithValue("a1", user_id);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // Construct user to return to client
                        return new UserProfile
                        (
                            (int)reader.GetValue(0),
                            Convert.ToString(reader.GetValue(1)),
                            Convert.ToString(reader.GetValue(2)),
                            Convert.ToString(reader.GetValue(3)),
                            Convert.ToString(reader.GetValue(4)),
                            Convert.ToString(reader.GetValue(5)),
                            Convert.ToString(reader.GetValue(7)),
                            Convert.ToString(reader.GetValue(8)),
                            Convert.ToString(reader.GetValue(9)),
                            Convert.ToString(reader.GetValue(10)),
                            Convert.ToString(reader.GetValue(11)),
                            (int)reader.GetValue(12),
                            Convert.ToString(reader.GetValue(6)).Equals("admin")
                        );
                    }
                }
            }
            return null;
        }

        public UserProfile GetUser(string username)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                // Find user id in database
                using (var command = new NpgsqlCommand("SELECT *, getUserScore(user_id) FROM (SELECT * FROM users WHERE username= @a1) u", conn))
                {
                    command.Parameters.AddWithValue("a1", username);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        return new UserProfile
                        (
                            (int)reader.GetValue(0),
                            Convert.ToString(reader.GetValue(1)),
                            Convert.ToString(reader.GetValue(2)),
                            Convert.ToString(reader.GetValue(3)),
                            Convert.ToString(reader.GetValue(4)),
                            Convert.ToString(reader.GetValue(5)),
                            Convert.ToString(reader.GetValue(7)),
                            Convert.ToString(reader.GetValue(8)),
                            Convert.ToString(reader.GetValue(9)),
                            Convert.ToString(reader.GetValue(10)),
                            Convert.ToString(reader.GetValue(11)),
                            (int) reader.GetValue(12),
                            Convert.ToString(reader.GetValue(6)).Equals("admin")
                        );
                    };
                }
            }
            return null;
        }

        public UserPrefs GetUserPrefs(int user_id) 
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                // Find user id in database
                using (var command = new NpgsqlCommand("SELECT * FROM user_preferences WHERE user_id= @a1", conn))
                {
                    command.Parameters.AddWithValue("a1", user_id);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // Construct user to return to client
                        return new UserPrefs
                        (
                            (bool)reader.GetValue(1),
                            (bool)reader.GetValue(2),
                            (bool)reader.GetValue(3),
                            (bool)reader.GetValue(4),
                            (bool)reader.GetValue(5)
                        );
                    }
                }
            }
            return null;
        }

        public async Task<int> CreateUserAsync(dynamic data)
        {
            string username = data.username;
            string password = data.password;
            string college = data.college;
            string email = data.email;
            string phone_number = data.phone_number != null ?  data.phone_number : "";
            string country = data.country;
            string first_name = data.first_name;
            string last_name = data.last_name;
            string year = data.year;
            string profile_pic_url = data.profile_pic_url != null ? data.profile_pic_url : "";
            string bio = data.bio != null ? data.bio : "";
            int id = -1;
            await using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    /*Query the Database */
                    await using (var command = new NpgsqlCommand("INSERT INTO users(username, college, email, phone_number, country, user_status, first_name, last_name, year, profile_pic_url, bio) VALUES(@v2, @v3, @v4, @v5, @v6, 'user', @v8, @v9, @v10, @v11, @v12) RETURNING user_id;", conn))
                    {
                        command.Parameters.AddWithValue("v2", username);
                        command.Parameters.AddWithValue("v3", college);
                        command.Parameters.AddWithValue("v4", email);
                        command.Parameters.AddWithValue("v5", phone_number);
                        command.Parameters.AddWithValue("v6", country);
                        command.Parameters.AddWithValue("v8", first_name);
                        command.Parameters.AddWithValue("v9", last_name);
                        command.Parameters.AddWithValue("v10", year);
                        command.Parameters.AddWithValue("v11", profile_pic_url);
                        command.Parameters.AddWithValue("v12", bio);
                        id = (int)command.ExecuteScalar();
                    }
                }
            return id;
        }
        
        
    }
}

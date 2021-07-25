using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using System.Net.Http;

namespace FunctionApp
{
    public static class CreatePostFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("POST /CreatePostFunction");
        private static string purpose = "Post Creations";

        static Response res = new Response();

        [FunctionName("CreatePostFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("CreatePostFunction received a request.");

            // Build connection string using parameters from portal
            string connString = Constants.getConnString();

            string requestBody = "";
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string title = data?.title;

            // Confirm request has all required fields.
            if (title == null)
            {
                ResourceLogger.LogMissingFieldFailure(logger, purpose, "title");
                return (ActionResult)new BadRequestResult();
            }

            Claims claims = JwtDecoder.decodeString(data, req);
            int uid = claims.user_id;
            if (uid < 0 && uid != -200)
            {
                ResourceLogger.LogJwtDecodeFailure(logger, purpose);
                return new UnauthorizedResult();
            }

            // Extract required fields.
            int user_id = uid;
            DateTime time = DateTime.Now;
            int post_id = 0;
            long num_user_posts = 0;

            res.author_id = user_id;
            res.content = "";
            res.content_type = "";
            res.comments = new List<Comment>();

            await using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                await conn.OpenAsync();
                log.LogInformation("Opening connection using access token....");
                /*Query the Database */

                using (var command = new NpgsqlCommand("SELECT username FROM users WHERE user_id = @user_id;", conn))
                {
                    command.Parameters.AddWithValue("user_id", user_id);
                    res.username = (string)await command.ExecuteScalarAsync();
                }

                using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM post WHERE author_id = @user_id AND created_time > NOW() - INTERVAL '24 hours'", conn))
                {
                    command.Parameters.AddWithValue("user_id", user_id);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        num_user_posts = (long)reader.GetValue(0);
                    }
                    reader.Close();

                    // Spam prevention - max 30 posts a day
                    if (num_user_posts >= 30)
                    {
                        log.LogInformation("User maximum number of posts per day reached.");
                        return (ActionResult)new StatusCodeResult(429);
                    }
                }

                using (var command = new NpgsqlCommand("INSERT INTO post(author_id, title, created_time) VALUES(@v1, @v2, @v3) RETURNING post_id, title, created_time;", conn))
                {
                    command.Parameters.AddWithValue("v1", user_id);
                    command.Parameters.AddWithValue("v2", title);
                    command.Parameters.AddWithValue("v3", time);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        res.post_id = (int)reader.GetValue(0);
                        res.title = (string)reader.GetValue(1);
                        res.created_time = (DateTime)reader.GetValue(2);
                        post_id = res.post_id;
                    }
                    reader.Close();
                }

                if (data.note_body != null)
                {
                    string content = data.note_body;
                    using (var command = new NpgsqlCommand("INSERT INTO note_content(post_id, content_body) VALUES(@v1, @v2) RETURNING content_body;", conn))
                    {
                        command.Parameters.AddWithValue("v1", post_id);
                        command.Parameters.AddWithValue("v2", content);
                        var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            res.content = (string)reader.GetValue(0);
                            res.content_type = "note";
                        }
                        await reader.CloseAsync();
                    }
                }

                if (data.question_body != null)
                {
                    string content = data.question_body;
                    //string category = data.category_name;
                    using (var command = new NpgsqlCommand("INSERT INTO qa_content(post_id, question) VALUES(@v1, @v2) RETURNING question;", conn))
                    {
                        command.Parameters.AddWithValue("v1", post_id);
                        command.Parameters.AddWithValue("v2", content);
                        var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            res.content = (string)reader.GetValue(0);
                            res.content_type = "qa";
                        }
                        await reader.CloseAsync();
                    }
                }

                List<string> tags = new List<string>();
                if (data.tags != null)
                {
                    foreach (string tag in data.tags)
                    {
                        tags.Add(tag);
                        int? tag_id = null;
                        await using (var command = new NpgsqlCommand("SELECT tag_id FROM tag WHERE tag_name = @v LIMIT 1;", conn))
                        {
                            command.Parameters.AddWithValue("v", tag);
                            var reader = await command.ExecuteReaderAsync();
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                tag_id = (int)reader.GetValue(0);
                            }
                            await reader.CloseAsync();
                        }
                        if (tag_id == null)
                        {
                            await using (var command = new NpgsqlCommand("INSERT INTO tag(tag_name) VALUES(@v) RETURNING tag_id;", conn))
                            {
                                command.Parameters.AddWithValue("v", tag);
                                tag_id = (int)await command.ExecuteScalarAsync();
                            }
                        }
                        await using (var command = new NpgsqlCommand("INSERT INTO post_tag(post_id, tag_id) VALUES(@v1, @v2);", conn))
                        {
                            command.Parameters.AddWithValue("v1", post_id);
                            command.Parameters.AddWithValue("v2", tag_id);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                res.tags = tags;

                if (data.category != null)
                {
                    string category = data.category;
                    int? category_id = null;
                    await using (var command = new NpgsqlCommand("SELECT category_id FROM category WHERE category_name = @v LIMIT 1;", conn))
                    {
                        command.Parameters.AddWithValue("v", category);
                        var reader = await command.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();
                            category_id = (int)reader.GetValue(0);
                        }
                        await reader.CloseAsync();
                    }

                    if (category_id == null)
                    {
                        await using (var command = new NpgsqlCommand("INSERT INTO category(category_name) VALUES(@v) RETURNING category_id;", conn))
                        {
                            command.Parameters.AddWithValue("v", category);
                            category_id = (int)await command.ExecuteScalarAsync();
                        }
                    }
                    await using (var command = new NpgsqlCommand("INSERT INTO post_category(post_id, category_id) VALUES(@v1, @v2);", conn))
                    {
                        command.Parameters.AddWithValue("v1", post_id);
                        command.Parameters.AddWithValue("v2", category_id);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            // If the code reaches here, the operations on database has already been done.
            // We could index this new page.
            HttpResponseMessage index_response = await GoogleUtils.singleton.IndexPost(res.post_id.ToString(), context);

            ResourceLogger.LogSuccess(logger, purpose, $"User {user_id} successfully created post {post_id}", namespace_id: ResourceLogger.USER_JOURNEY_NAMESPACE);
            return new OkObjectResult(new { success = true, message = "Created database entry", res = res, isIndexSucceed = index_response });
        }
    }
}


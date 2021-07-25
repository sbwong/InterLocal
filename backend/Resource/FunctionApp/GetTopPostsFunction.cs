using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;

namespace FunctionApp
{
    public static class GetTopPostsFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("GET /GetTopPostsFunction");
        private static string purpose = "Top Post Retrievals";

        [FunctionName("GetTopPostsFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetTopPostsFunction received a request.");

            string connString = Constants.getConnString();

            // Extract required fields.
            int limit = 3;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string limit_str = req.Query["limit"];
            if (!String.IsNullOrEmpty(limit_str))
            {
                try
                {
                    limit = Int32.Parse(limit_str);
                }
                catch (FormatException)
                {
                    ResourceLogger.LogInvalidFieldFailure(logger, purpose, "limit", limit_str);
                    return (ActionResult)new BadRequestResult();
                }

            }
            else
            {
                limit = 3;
            }

            List<Response> responseList = new List<Response>();

            using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                log.LogInformation(connString);
                await conn.OpenAsync();
                log.LogInformation("Opening connection using access token....");

                /*Query the Database */
                await using (var command = new NpgsqlCommand("SELECT p.*, u.username, u.user_status FROM post p INNER JOIN USERS u ON p.author_id = u.user_id WHERE p.created_time > NOW() - INTERVAL '24 hours' ORDER BY up_count DESC LIMIT @limit", conn))
                {
                    command.Parameters.AddWithValue("limit", limit);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        Response cur_response = new Response();
                        cur_response.post_id = (int)reader.GetValue(0);
                        cur_response.author_id = (int)reader.GetValue(1);
                        cur_response.title = (string)reader.GetValue(2);
                        cur_response.created_time = (DateTime)reader.GetValue(3);
                        if (reader.GetValue(4) != System.DBNull.Value)
                        {
                            cur_response.last_edit_time = (DateTime)reader.GetValue(4);
                        }
                        cur_response.up_count = (int)reader.GetValue(5);
                        cur_response.down_count = (int)reader.GetValue(6);
                        cur_response.username = (string)reader.GetValue(7);
                        cur_response.is_admin = Convert.ToString(reader.GetValue(8)).Equals("admin");
                        cur_response.comments = new List<Comment>();
                        responseList.Add(cur_response);
                    }
                    await reader.CloseAsync();
                }

                for (int i = 0; i < responseList.Count; i++)
                {
                    Response cur_response = responseList[i];

                    // Get all the comments of the current post.
                    using (var command = new NpgsqlCommand("SELECT c.*, u.username, u.user_status FROM comment c INNER JOIN USERS u ON c.author_id = u.user_id WHERE post_id=@post_id", conn))
                    {
                        command.Parameters.AddWithValue("post_id", cur_response.post_id);
                        var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            Comment cur_comment = new Comment();
                            cur_comment.comment_id = (int)reader.GetValue(0);
                            cur_comment.post_id = (int)reader.GetValue(1);
                            cur_comment.author_id = (int)reader.GetValue(2);
                            if (System.DBNull.Value != reader.GetValue(3))
                            {
                                cur_comment.parent_id = (int)reader.GetValue(3);
                            }
                            cur_comment.content_body = (String)reader.GetValue(4);
                            if (reader.GetValue(5) != System.DBNull.Value)
                            {
                                cur_comment.created_time = (DateTime)reader.GetValue(5);
                            }
                            if (reader.GetValue(6) != System.DBNull.Value)
                            {
                                cur_comment.last_edit_time = (DateTime)reader.GetValue(6);
                            }
                            cur_comment.up_count = (int)reader.GetValue(7);
                            cur_comment.down_count = (int)reader.GetValue(8);
                            cur_comment.username = (string)reader.GetValue(9);
                            cur_comment.is_admin = Convert.ToString(reader.GetValue(10)).Equals("admin");
                            cur_response.comments.Add(cur_comment);
                        }
                        await reader.CloseAsync();
                    }

                    // Get the content of the current post.
                    using (var command = new NpgsqlCommand("SELECT * FROM getPostContentAndType(@post_id) LIMIT 1;", conn))
                    {
                        command.Parameters.AddWithValue("post_id", cur_response.post_id);
                        var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            cur_response.content = (string)reader.GetValue(0);
                            cur_response.content_type = (string)reader.GetValue(1);
                        }
                        await reader.CloseAsync();
                    }

                    // Get the tags of the current post.
                    List<string> tags = new List<string>();
                    using (var command = new NpgsqlCommand("SELECT tag_name FROM tag t INNER JOIN post_tag p ON t.tag_id = p.tag_id WHERE p.post_id = @post_id;", conn))
                    {
                        command.Parameters.AddWithValue("post_id", cur_response.post_id);
                        var reader = await command.ExecuteReaderAsync();
                        while (reader.Read())
                        {
                            tags.Add((string)reader.GetValue(0));
                        }
                        await reader.CloseAsync();
                    }
                    cur_response.tags = tags;
                }
            }

            ResourceLogger.LogSuccess(logger, purpose, $"Successfully retrieved top {limit} posts");
            return new OkObjectResult(new { success = true, posts = responseList.ToArray() });
        }
    }
}


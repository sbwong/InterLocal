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
using System.Net.Http;
using JWT.Builder;
using JWT.Algorithms;

namespace FunctionApp
{
    public static class ReadPostFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("GET /post/{post_id_str}");
        private static string purpose = "Post Retrievals";

        

        [FunctionName("ReadPostFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "post/{post_id_str}")] HttpRequest req,
            string post_id_str,
            ILogger log)
        {
            log.LogInformation("ReadPostFunction received a request.");

            string connString = Constants.getConnString();

            int post_id;
            int user_id;
            int count;
            bool? is_upvote;

            // get user id from request header JWT 
            Claims claims = JwtDecoder.decodeString(req);
            user_id = claims.user_id;
            if (user_id < 0 && user_id != -200)
            {
                ResourceLogger.LogJwtDecodeFailure(logger, purpose);
                return new UnauthorizedResult();
            }

            if (!String.IsNullOrEmpty(post_id_str))
            {
                try
                {
                    post_id = Int32.Parse(post_id_str);
                }
                catch (FormatException)
                {
                    ResourceLogger.LogInvalidFieldFailure(logger, purpose, "post_id", post_id_str);
                    return (ActionResult)new BadRequestResult();
                }
            }
            else
            {
                ResourceLogger.LogMissingFieldFailure(logger, purpose, "post_id");
                return (ActionResult)new BadRequestResult();
            }
            Response res = new Response();


            using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                await conn.OpenAsync();
                log.LogInformation("Opening connection using access token....");
                res.comments = new List<Comment>();
                /*Query the Database */


                ///////
                if (user_id != -200)
                {
                    //////
                    using (var command =
                        new NpgsqlCommand(
                            "SELECT p.*, u.username, u.user_status, pr.is_upvote FROM post p INNER JOIN USERS u ON p.author_id = u.user_id LEFT JOIN (SELECT * FROM Post_Rating WHERE user_id = @user_id AND post_id = @post_id) pr ON p.post_id = pr.post_id WHERE p.post_id = @post_id;",
                            conn))
                    {
                        command.Parameters.AddWithValue("post_id", post_id);
                        var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            res.post_id = (int) reader.GetValue(0);
                            res.author_id = (int) reader.GetValue(1);
                            res.title = (string) reader.GetValue(2);
                            res.created_time = (DateTime) reader.GetValue(3);
                            if (reader.GetValue(4) != System.DBNull.Value)
                            {
                                res.last_edit_time = (DateTime) reader.GetValue(4);
                            }

                            res.up_count = (int) reader.GetValue(5);
                            res.down_count = (int) reader.GetValue(6);
                            res.username = (string) reader.GetValue(7);
                            res.is_admin = Convert.ToString(reader.GetValue(8)).Equals("admin");
                            //res.is_upvote = reader.GetValue(8) == DBNull.Value ? null : (bool?) reader.GetValue(8);
                            //res.is_upvote = !Convert.IsDBNull(reader.GetValue(8))
                                //? (bool?)reader.GetValue(8)
                                //: null;
                        }

                        await reader.CloseAsync();
                    }
                }
                else
                {
                    using (var command =
                        new NpgsqlCommand(
                            "SELECT p.*, u.username, u.user_status, pr.is_upvote FROM post p INNER JOIN USERS u ON p.author_id = u.user_id LEFT JOIN (SELECT * FROM Post_Rating WHERE user_id = @user_id AND post_id = @post_id) pr ON p.post_id = pr.post_id WHERE p.post_id = @post_id;",
                            conn))
                    {
                        command.Parameters.AddWithValue("post_id", post_id);
                        var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            res.post_id = (int)reader.GetValue(0);
                            res.author_id = (int)reader.GetValue(1);
                            res.title = (string)reader.GetValue(2);
                            res.created_time = (DateTime)reader.GetValue(3);
                            if (reader.GetValue(4) != System.DBNull.Value)
                            {
                                res.last_edit_time = (DateTime)reader.GetValue(4);
                            }

                            res.up_count = (int)reader.GetValue(5);
                            res.down_count = (int)reader.GetValue(6);
                            res.username = (string)reader.GetValue(7);
                            res.is_admin = Convert.ToString(reader.GetValue(8)).Equals("admin");
                            //res.is_upvote = null;
                        }

                        await reader.CloseAsync();
                    }
                }

                // Get all the comments of the current post.
                using (var command =
                    new NpgsqlCommand(
                        "SELECT c.*, u.username, u.user_status FROM comment c INNER JOIN USERS u ON c.author_id = u.user_id WHERE post_id=@post_id",
                        conn))
                {
                    command.Parameters.AddWithValue("post_id", post_id);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        Comment current_comment = new Comment();
                        current_comment.comment_id = (int)reader.GetValue(0);
                        current_comment.post_id = (int)reader.GetValue(1);
                        current_comment.author_id = (int)reader.GetValue(2);
                        if (System.DBNull.Value != reader.GetValue(3))
                        {
                            current_comment.parent_id = (int)reader.GetValue(3);
                        }

                        current_comment.content_body = (String)reader.GetValue(4);
                        if (reader.GetValue(5) != System.DBNull.Value)
                        {
                            current_comment.created_time = (DateTime)reader.GetValue(5);
                        }

                        if (reader.GetValue(6) != System.DBNull.Value)
                        {
                            current_comment.last_edit_time = (DateTime)reader.GetValue(6);
                        }

                        current_comment.up_count = (int)reader.GetValue(7);
                        current_comment.down_count = (int)reader.GetValue(8);
                        current_comment.username = (string)reader.GetValue(9);
                        current_comment.is_admin = Convert.ToString(reader.GetValue(10)).Equals("admin");
                        res.comments.Add(current_comment);
                    }

                    await reader.CloseAsync();
                }

                // Get the content of the current post.
                using (var command = new NpgsqlCommand("SELECT * FROM getPostContentAndType(@post_id) LIMIT 1;", conn))
                {
                    command.Parameters.AddWithValue("post_id", post_id);
                    var reader = command.ExecuteReader();
                    while (await reader.ReadAsync())
                    {
                        res.content = (string)reader.GetValue(0);
                        res.content_type = (string)reader.GetValue(1);
                    }

                    await reader.CloseAsync();
                }

                // Get the tags of the current post.
                List<string> tags = new List<string>();
                using (var command =
                    new NpgsqlCommand(
                        "SELECT tag_name FROM tag t INNER JOIN post_tag p ON t.tag_id = p.tag_id WHERE p.post_id = @post_id;",
                        conn))
                {
                    command.Parameters.AddWithValue("post_id", post_id);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        tags.Add((string)reader.GetValue(0));
                    }

                    await reader.CloseAsync();
                }

                res.tags = tags;

                // get the is_upvote of the post 
                await using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM post_rating WHERE post_id = @a1 AND user_id = @b1;", conn))
                {
                    command.Parameters.AddWithValue("a1", post_id);
                    command.Parameters.AddWithValue("b1", user_id);
                    count = Convert.ToInt32(command.ExecuteScalar());
                }
                if (count > 0)
                {
                    using (var command = new NpgsqlCommand("SELECT is_upvote FROM post_rating WHERE post_id = @a1 AND user_id = @b1;", conn))
                    {
                        command.Parameters.AddWithValue("a1", post_id);
                        command.Parameters.AddWithValue("b1", user_id);
                        //is_upvote = Convert.ToBoolean(command.ExecuteScalar()); 
                        is_upvote = (bool?)command.ExecuteScalar();
                    }
                }
                else
                {
                    is_upvote = null;
                }
                res.is_upvote = is_upvote;
            }

            ResourceLogger.LogSuccess(logger, purpose, $"Successfully read post {post_id}");
            return new OkObjectResult(new { success = true, res = res });
        }
    }
}


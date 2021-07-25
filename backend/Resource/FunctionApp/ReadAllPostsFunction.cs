using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace FunctionApp
{
    public static class ReadAllPostsFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("GET /ReadAllPostsFunction");
        private static string purpose = "Post Filterings";

        [FunctionName("ReadAllPostsFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "post")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("ReadAllPostsFunction received a request.");

            string connString = Constants.getConnString();

            // Extract required fields.
            int offset = 0;
            int limit = 10;
            string author_filter = "";
            string tag_filter = "";
            string date_type_filter = "";
            string user_type_filter = "";

            string offset_str = req.Query["offset"];
            if (!String.IsNullOrEmpty(offset_str))
            {
                try
                {
                    offset = Int32.Parse(offset_str);
                }
                catch (FormatException)
                {
                    ResourceLogger.LogInvalidFieldFailure(logger, purpose, "offset", offset_str);
                    return (ActionResult)new BadRequestResult();
                }
            }

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

            string author_id_str = req.Query["author_id"];
            if (!String.IsNullOrEmpty(author_id_str))
            {
                try
                {
                    int author_id = Int32.Parse(author_id_str);
                    author_filter = " AND author_id = " + author_id;
                }
                catch (FormatException)
                {
                    ResourceLogger.LogInvalidFieldFailure(logger, purpose, "author_id", author_id_str);
                    return (ActionResult)new BadRequestResult();
                }

            }

            string tags_str = req.Query["tags"];
            if (!String.IsNullOrEmpty(tags_str))
            {
                try
                {
                    tag_filter += " AND t.tag_name IN (";
                    string[] tags = JArray.Parse(tags_str).ToObject<string[]>();
                    for (int i = 0; i < tags.Length; i++)
                    {
                        tags[i] = "\'" + tags[i] + "\'";
                    }
                    tag_filter += string.Join(",", tags) + " ) ";
                }
                catch (Exception)
                {
                    ResourceLogger.LogInvalidFieldFailure(logger, purpose, "tags", tags_str);
                    return (ActionResult)new BadRequestResult();
                }
            }
            string tag_join = tag_filter.Equals("") ? "" : " NATURAL JOIN Post_Tag NATURAL JOIN Tag t ";

            string date_type_str = req.Query["date_type"];
            if (!String.IsNullOrEmpty(date_type_str))
            {
                date_type_filter += " AND p.created_time > NOW() - INTERVAL \'1 " + date_type_str + "\'";
            }

            string user_type_str = req.Query["user_type"];
            if (!String.IsNullOrEmpty(user_type_str))
            {
                user_type_filter += " AND u.user_status = " + "\'" + user_type_str + "\'";
            }

            string content_type_join = "";
            switch (req.Query["content_type"])
            {
                case "note":
                    content_type_join = " NATURAL JOIN Note_Content ";
                    break;
                case "qa":
                    content_type_join = " NATURAL JOIN QA_Content ";
                    break;
                default:
                    break;
            }

            string isVoteOrdering_str = req.Query["isVoteOrdering"];
            string vote_order = "";
            if (!String.IsNullOrEmpty(isVoteOrdering_str))
            {
                try
                {
                    if (Boolean.Parse(isVoteOrdering_str))
                    {
                        vote_order = "up_count DESC, ";
                    }
                }
                catch (FormatException)
                {
                    ResourceLogger.LogInvalidFieldFailure(logger, purpose, "isVoteOrdering", isVoteOrdering_str);
                    return (ActionResult)new BadRequestResult();
                }
            }

            List<Response> responseList = new List<Response>();
            using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                await conn.OpenAsync();
                log.LogInformation("Opening connection using access token....");

                /*Query the Database */
                using (var command = new NpgsqlCommand("SELECT DISTINCT p.*, u.username, u.user_status FROM post p INNER JOIN USERS u ON p.author_id = u.user_id " + tag_join + content_type_join + " WHERE TRUE " + author_filter + tag_filter + user_type_filter + date_type_filter + " ORDER BY " + vote_order + " created_time DESC OFFSET @offset LIMIT @limit;", conn))
                {
                    command.Parameters.AddWithValue("offset", offset);
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
                        while (await reader.ReadAsync())
                        {
                            tags.Add((string)reader.GetValue(0));
                        }
                        await reader.CloseAsync();
                    }
                    cur_response.tags = tags;
                }

            }

            ResourceLogger.LogSuccess(logger, purpose, $"Successfully filtered {responseList.Count} posts");
            return new OkObjectResult(new { success = true, posts = responseList.ToArray() });
        }
    }
}


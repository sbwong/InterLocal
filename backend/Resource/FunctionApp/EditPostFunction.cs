using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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

    /**
     * This endpoint is for editing a post. Currently, it only supports editing the title
     * and content. After updating the title and/or content in the database The "last_edit_time"
     * field is automatically updated by a database trigger to the current timestamp.
     * 
     * Required path parameter:
     *     post_id: ID of the post to update
     * Required fields in request body:
     *     user_id: ID of the user making the request to update a post. This is checked against
     *              the ID of the author of the post.
     * Optional fields:
     *     title: New title.
     *     content: New content. Can either be a note or a question. The endpoint simply
     *              issues two queries to the database.
     * 
     * Returns an empty 200 response on success.
     * If the request body is malformed, returns a 400 response.
     * If the provided user_id does not match the ID of the author of the post,
     * returns a 401 response.
     */
    public static class EditPostFunction
    {
        private static LoggingAdapter logger = new LoggingAdapter("PUT /post/{post_id}");
        private static string purpose = "Post Editings";

        [FunctionName("EditPostFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "post/{post_id}")] HttpRequest req,
            string post_id,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("EditPostFunction received a request.");

            // Build connection string using parameters from portal
            string connString = Constants.getConnString();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Confirm request has all required fields.
            if (data == null || string.IsNullOrEmpty(post_id))
            {
                ResourceLogger.LogMissingFieldFailure(logger, purpose, "post_id");
                return (ActionResult)new BadRequestResult();
            }

            int pid;
            try
            {
                pid = Int32.Parse(post_id);
            }
            catch (FormatException)
            {
                ResourceLogger.LogInvalidFieldFailure(logger, purpose, "post_id", post_id);
                return (ActionResult)new BadRequestResult();
            }

            Claims claims = JwtDecoder.decodeString(data, req);
            int uid = claims.user_id;
            string role = claims.role;
            if (uid < 0 && uid != -200)
            {
                ResourceLogger.LogJwtDecodeFailure(logger, purpose);
                return new UnauthorizedResult();
            }

            string title = data.title;
            string content = data.content;
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(content) && data.tags == null)
            {
                return new OkObjectResult(new { });
            }

            await using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                conn.Open();
                log.LogInformation("Opening connection using access token....");

                var transaction = await conn.BeginTransactionAsync();

                if (uid != -1)
                {
                    using (var command = new NpgsqlCommand("SELECT author_id FROM post WHERE post_id = @pid", conn))
                    {
                        command.Parameters.AddWithValue("pid", pid);
                        int correct_uid = Convert.ToInt32(await command.ExecuteScalarAsync());
                        if (correct_uid != uid && role != "admin")
                        {
                            return (ActionResult)new UnauthorizedResult();
                        }
                    }
                    log.LogInformation("Checked user_id");
                }

                if (!string.IsNullOrEmpty(title))
                {
                    using (var command = new NpgsqlCommand("UPDATE post SET title = @title WHERE post_id = @pid", conn))
                    {
                        command.Parameters.AddWithValue("title", title);
                        command.Parameters.AddWithValue("pid", pid);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                if (!string.IsNullOrEmpty(content))
                {
                    using (var command = new NpgsqlCommand("UPDATE note_content SET content_body = @content WHERE post_id = @pid", conn))
                    {
                        command.Parameters.AddWithValue("content", content);
                        command.Parameters.AddWithValue("pid", pid);
                        await command.ExecuteNonQueryAsync();
                    }

                    using (var command = new NpgsqlCommand("UPDATE qa_content SET question = @content WHERE post_id = @pid", conn))
                    {
                        command.Parameters.AddWithValue("content", content);
                        command.Parameters.AddWithValue("pid", pid);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                List<int> new_tags = new List<int>();
                if (data.tags != null)
                {
                    foreach (string tag in data.tags)
                    {
                        int? tag_id = null;
                        await using (var command = new NpgsqlCommand("SELECT tag_id FROM tag WHERE tag_name = @v LIMIT 1;", conn))
                        {
                            command.Parameters.AddWithValue("v", tag);
                            var reader = await command.ExecuteReaderAsync();
                            if (reader.HasRows)
                            {
                                reader.Read();
                                tag_id = (int)reader.GetValue(0);
                            }
                            reader.Close();
                        }
                        if (tag_id == null)
                        {
                            await using (var command = new NpgsqlCommand("INSERT INTO tag(tag_name) VALUES(@v) RETURNING tag_id;", conn))
                            {
                                command.Parameters.AddWithValue("v", tag);
                                tag_id = (int)await command.ExecuteScalarAsync();
                            }
                        }
                        new_tags.Add((int)tag_id);
                    }

                    List<int> old_tags = new List<int>();
                    using (var command = new NpgsqlCommand("SELECT tag_id FROM Post_Tag WHERE post_id = @v;", conn))
                    {
                        command.Parameters.AddWithValue("v", pid);
                        var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            int tag_id = (int)reader.GetValue(0);
                            if (!new_tags.Remove(tag_id))
                            {
                                old_tags.Add(tag_id);
                            }
                        }
                        await reader.CloseAsync();
                    }

                    foreach (int tag_id in new_tags)
                    {
                        using (var command = new NpgsqlCommand("INSERT INTO post_tag(post_id, tag_id) VALUES(@v1, @v2);", conn))
                        {
                            command.Parameters.AddWithValue("v1", pid);
                            command.Parameters.AddWithValue("v2", tag_id);
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    foreach (int tag_id in old_tags)
                    {
                        using (var command = new NpgsqlCommand("DELETE FROM Post_Tag WHERE post_id = @v1 and tag_id = @v2;", conn))
                        {
                            command.Parameters.AddWithValue("v1", pid);
                            command.Parameters.AddWithValue("v2", tag_id);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }

                await transaction.CommitAsync();
            }
            // If the code reaches here, the post has already been updated.
            // Now we could send request to Google to reindex this post.
            HttpResponseMessage index_response = await GoogleUtils.singleton.IndexPost(post_id, context);

            ResourceLogger.LogSuccess(logger, purpose, $"User {uid} successfully edited post {pid}");
            return new OkObjectResult(new {isIndexSucceed = index_response.IsSuccessStatusCode});
        }
    }
}

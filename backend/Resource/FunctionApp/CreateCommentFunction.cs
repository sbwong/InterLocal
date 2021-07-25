using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using Npgsql;

namespace FunctionApp
{

    /**
     * Creates a new comment.
     * 
     * Required fields in request body:
     *     post_id: ID of the post this comment is for
     *     user_id: ID of the author of this comment
     *     content_body: Comment body
     * 
     * Returns a 200 response on successful insertion and a 400 response if
     * any required field is not present.
     */
    public static class CreateCommentFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("POST /CreateCommentFunction");
        private static string purpose = "Comment Creations";

        // read in stuff ; look at Chuck's group example and the linked website
        static Comment comment = new Comment();

        [FunctionName("CreateCommentFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("CreateCommentFunction received request");

            string connString = Constants.getConnString();

            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Confirm request has all required fields.
            if (data == null || data?.post_id == null || data?.content_body == null)
            {
                ResourceLogger.LogMissingFieldFailure(logger, purpose, "post_id", "content_body");
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
            int post_id = data.post_id;
            int author_id = uid;
            string content_body = data.content_body;
            long num_user_comments = 0;

            using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                await conn.OpenAsync();

                using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM comment WHERE author_id = @author_id AND created_time > NOW() - INTERVAL '24 hours'", conn))
                {
                    command.Parameters.AddWithValue("author_id", author_id);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        num_user_comments = (long)reader.GetValue(0);
                    }
                    reader.Close();

                    // Spam prevention - max 100 comments a day
                    if (num_user_comments >= 100)
                    {
                        log.LogInformation("User maximum number of comments per day reached.");
                        return (ActionResult) new StatusCodeResult(429);
                    }
                }  

                // post id, author id, content body, timestamp 
                using (var command = new NpgsqlCommand("INSERT INTO comment (post_id, author_id, content_body) VALUES (@a1, @b1, @c1) RETURNING comment_id, post_id, author_id, parent_id, content_body, created_time, last_edit_time, up_count, down_count;", conn))
                {
                    command.Parameters.AddWithValue("a1", post_id);
                    command.Parameters.AddWithValue("b1", author_id);
                    command.Parameters.AddWithValue("c1", content_body);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        comment.comment_id = (int)reader.GetValue(0);
                        comment.post_id = (int)reader.GetValue(1);
                        comment.author_id = (int)reader.GetValue(2);
                        if (reader.GetValue(3) != System.DBNull.Value)
                        {
                            comment.parent_id = (int)reader.GetValue(3);
                        }
                        comment.content_body = (string)reader.GetValue(4);
                        comment.created_time = (DateTime)reader.GetValue(5);
                        if (reader.GetValue(6) != System.DBNull.Value)
                        {
                            comment.last_edit_time = (DateTime)reader.GetValue(6);
                        }
                        comment.up_count = (int)reader.GetValue(7);
                        comment.down_count = (int)reader.GetValue(8);
                    }
                }

                log.LogInformation("Created comment");
            }

            ResourceLogger.LogSuccess(logger, purpose, $"User {uid} successfully created comment {comment}");
            return new OkObjectResult(new { success = true, message = "Created comment database entry", comment = comment });
        }
    }
}



using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Serialization;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FunctionApp
{
    public static class ReadCommentFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("GET /post/{post_id}/comments");
        private static string purpose = "Comment Retrievals";

        static ResponseComment res = new ResponseComment();

        /**
         * Path variable:
         *     post_id: ID of the post whose comments are to be retrieved
         * 
         * Returns a 200 response if successful. The "res" field contains an array
         * of comments. Returns a 400 response if the request is malformed, including
         * if the supplied `post_id` does not identify an existing post.
         */
        [FunctionName("ReadCommentFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "post/{post_id}/comments")] HttpRequest req,
            string post_id,
            ILogger log)
        {
            log.LogInformation("ReadCommentFunction received a request");

            string connString = Constants.getConnString();

            // Extract required fields.
            int pid;
            int user_id;
            try
            {
                pid = Int32.Parse(post_id);
            }
            catch (FormatException)
            {
                ResourceLogger.LogInvalidFieldFailure(logger, purpose, "post_id", post_id);
                return (ActionResult)new BadRequestResult();
            }

            // get user_id from JWT 
            Claims claims = JwtDecoder.decodeString(req);
            user_id = claims.user_id;
            if (user_id < 0 && user_id != -200)
            {
                ResourceLogger.LogJwtDecodeFailure(logger, "Comment Retrievals");
                return new UnauthorizedResult();
            }

            
            List<ResponseComment> commentList = new List<ResponseComment>();

            using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                log.LogInformation(connString);
                await conn.OpenAsync();

                /*Query the Database */

                var transaction = await conn.BeginTransactionAsync();
                log.LogInformation("Started transaction");

                using (var command = new NpgsqlCommand("SELECT COUNT(1) FROM post WHERE post_id = @pid", conn))
                {
                    command.Parameters.AddWithValue("pid", pid);
                    int numRows = Convert.ToInt32(await command.ExecuteScalarAsync());
                    if (numRows == 0)
                    {
                        ResourceLogger.LogNotExistFailure(logger, purpose, "Post", post_id);
                        return (ActionResult)new BadRequestResult();
                    }

                    log.LogInformation("Checked that post existed");
                }

                using (var command = new NpgsqlCommand("SELECT * FROM comment WHERE post_id = @a1", conn))
                {
                    command.Parameters.AddWithValue("a1", pid);
                    int nRows = await command.ExecuteNonQueryAsync();
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {

                        ResponseComment cur_comment = new ResponseComment();
                        cur_comment.comment_id = (int)reader.GetValue(0);
                        cur_comment.post_id = (int)reader.GetValue(1);
                        cur_comment.author_id = (int)reader.GetValue(2);
                        cur_comment.parent_id = !Convert.IsDBNull(reader.GetValue(3))
                            ? (int?)reader.GetValue(3)
                            : null;
                        cur_comment.content_body = (string)reader.GetValue(4);
                        cur_comment.created_time = (DateTime)reader.GetValue(5);
                        cur_comment.last_edit_time = !Convert.IsDBNull(reader.GetValue(6))
                            ? (DateTime?)reader.GetValue(6)
                            : null;
                        cur_comment.up_count = (int)reader.GetValue(7);
                        cur_comment.down_count = (int)reader.GetValue(8);


                        commentList.Add(cur_comment);
                    }

                    await reader.CloseAsync();
                    log.LogInformation("Retrieved all comments");
                }

                await transaction.CommitAsync();
                log.LogInformation("Commited transaction");

                // perform the query for each row to add on results of whether upvoted comment or not 

                for (int i = 0; i < commentList.Count; i++)
                {
                    ResponseComment current_response = commentList[i];

                    // get the result of whether the user has interacted with that comment or not for logged in users 
                    if (user_id != -200)
                    {
                        
                        using (var command = new NpgsqlCommand("SELECT u.username, u.user_status, cr.is_upvote FROM comment c INNER JOIN USERS u ON c.author_id = u.user_id LEFT JOIN (SELECT * FROM Comment_Rating WHERE user_id = @b1) cr ON c.comment_id = cr.comment_id WHERE c.comment_id = @a1;", conn))
                        {
                            command.Parameters.AddWithValue("a1", current_response.comment_id);
                            command.Parameters.AddWithValue("b1", user_id);

                            var reader = await command.ExecuteReaderAsync();
                            while (await reader.ReadAsync())
                            {

                                current_response.username = (string)reader.GetValue(0);
                                current_response.is_admin = Convert.ToString(reader.GetValue(1)).Equals("admin");
                                current_response.is_upvote = reader.GetValue(2) == DBNull.Value ? null : (bool?)reader.GetValue(2);

                            }

                            await reader.CloseAsync();

                            

                        }

                        
                    }
                    else
                    {

                        using (var command = new NpgsqlCommand("SELECT u.username, u.user_status, cr.is_upvote FROM comment c INNER JOIN USERS u ON c.author_id = u.user_id LEFT JOIN (SELECT * FROM Comment_Rating WHERE user_id = @b1) cr ON c.comment_id = cr.comment_id WHERE c.comment_id = @a1;", conn))
                        {
                            command.Parameters.AddWithValue("a1", current_response.comment_id);
                            command.Parameters.AddWithValue("b1", user_id);

                            var reader = await command.ExecuteReaderAsync();
                            while (await reader.ReadAsync())
                            {

                                
                                current_response.username = (string)reader.GetValue(0);
                                current_response.is_admin = Convert.ToString(reader.GetValue(1)).Equals("admin");
                                // have is_upvote be null for non logged in users 
                                current_response.is_upvote = null;

                            }

                            await reader.CloseAsync();



                        }
                       
                    }
                }
            } // close conn 

            ResourceLogger.LogSuccess(logger, purpose, $"Successfully read comments of post {pid}");
            return new OkObjectResult(new { success = true, res = commentList.ToArray() });
        }
    }
}

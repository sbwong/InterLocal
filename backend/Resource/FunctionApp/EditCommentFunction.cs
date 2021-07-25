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

//# Edit Comment

// start by grabbing the original comment 

//# Comment ID --> next id in database  // don't need to do here 
//# Post ID --> we need to grab this from parent post  // don't need to do here 
//# Author/username --> figure this out   // don't need to do here 
//# Content --> text input from the user  // need to do; we we will update the content 
//#Creation Date/Timestamp --> get date ()  // don't need to do here 
//#Last Edit Date/Timestamp --> update if we edit the comment --> get date()  // need to do 
//#Up count --> 1 by default ; stores number  // grab the current and keep
//#Down count --> 0 by default; stores number // grab the current and keep 

// package that all up into a 'comment' and update the comment associated with the comment id in the database  

namespace FunctionApp
{
    public static class EditCommentFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("PUT /comment/{comment_id}");
        private static string purpose = "Comment Editings";

        /**
         * Path variable:
         *     comment_id: ID of the comment to update
         * 
         * Required field in request body:
         *     content_body: New content body to replace the old content body.
         * 
         * Returns a 200 response if the update is successful and a 400 response if
         * the request is malformed, including if the supplied `comment_id` does not
         * identify an existing comment or if any required field is not present.
         */
        [FunctionName("EditCommentFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "comment/{comment_id}")] HttpRequest req,
            string comment_id,
            ILogger log)
        {
            log.LogInformation("EditCommentFunction received a request");

            if (string.IsNullOrEmpty(comment_id))
            {
                ResourceLogger.LogMissingFieldFailure(logger, purpose, "comment_id");
                return (ActionResult)new BadRequestResult();
            }
            int cid;
            try
            {
                cid = Int32.Parse(comment_id);
            }
            catch (FormatException)
            {
                ResourceLogger.LogInvalidFieldFailure(logger, purpose, "comment_id", comment_id);
                return (ActionResult)new BadRequestResult();
            }

            string connString = Constants.getConnString();

            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Confirm request has all required fields.
            if (data == null || data?.content_body == null)
            {
                ResourceLogger.LogMissingFieldFailure(logger, purpose, "content_body");
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

            string content_body = data.content_body;

            using (var conn = new NpgsqlConnection(connString))

            {
                log.LogInformation("Opening connection");
                await conn.OpenAsync();

                var transaction = await conn.BeginTransactionAsync();
                log.LogInformation("Started transaction");

                using (var command = new NpgsqlCommand("SELECT COUNT(1) FROM comment WHERE comment_id = @cid", conn))
                {
                    command.Parameters.AddWithValue("cid", cid);
                    int numRows = Convert.ToInt32(await command.ExecuteScalarAsync());
                    if (numRows == 0)
                    {
                        ResourceLogger.LogNotExistFailure(logger, purpose, "Comment", comment_id);
                        return (ActionResult)new BadRequestResult();
                    }
                    log.LogInformation("Checked that comment existed");
                }

                if (uid != -1)
                {
                    using (var command = new NpgsqlCommand("SELECT author_id FROM comment WHERE comment_id = @cid", conn))
                    {
                        command.Parameters.AddWithValue("cid", cid);
                        int correct_uid = Convert.ToInt32(await command.ExecuteScalarAsync());
                        if (correct_uid != uid && role != "admin")
                        {
                            return (ActionResult)new UnauthorizedResult();
                        }
                    }
                    log.LogInformation("Checked user_id");
                }

                // post id, author id, content body, timestamp 
                using (var command = new NpgsqlCommand("UPDATE comment SET content_body = @a1 WHERE comment_id = @b1;", conn))
                {
                    command.Parameters.AddWithValue("a1", content_body);
                    command.Parameters.AddWithValue("b1", cid);

                    int nRows = await command.ExecuteNonQueryAsync();
                    log.LogInformation(String.Format("Number of rows inserted={0}", nRows));
                    log.LogInformation("Updated comment");
                }

                await transaction.CommitAsync();

                log.LogInformation("Commited transaction");
            }

            ResourceLogger.LogSuccess(logger, purpose, $"User {uid} successfully edited comment {comment_id}");
            return new OkObjectResult(new { success = true, message = "edited comment database entry", comment_id = cid });
        }
    }
}
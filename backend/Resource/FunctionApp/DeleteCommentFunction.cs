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
    public static class DeleteCommentFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("DELETE /DeleteCommentFunction");
        private static string purpose = "Comment Deletions";

        /**
         * Deletes a comment.
         * 
         * Required field in request body:
         *     comment_id: ID of the comment to delete
         */
        [FunctionName("DeleteCommentFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("DeleteCommentFunction received a request");

            string connString = Constants.getConnString();

            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Confirm request has all required fields.
            if (data == null || data?.comment_id == null)
            {
                ResourceLogger.LogMissingFieldFailure(logger, purpose, "comment_id");
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

            // Extract required fields.
            int comment_id = data.comment_id;

            using (var conn = new NpgsqlConnection(connString))

            {
                log.LogInformation("Opening connection");
                conn.Open();

                if (uid != -1)
                {
                    using (var command = new NpgsqlCommand("SELECT author_id FROM comment WHERE comment_id = @cid", conn))
                    {
                        command.Parameters.AddWithValue("cid", comment_id);
                        int correct_uid = Convert.ToInt32(await command.ExecuteScalarAsync());
                        if (correct_uid != uid && role != "admin")
                        {
                            ResourceLogger.LogUnauthorizedRoleFailure(logger, purpose, uid, role);
                            return (ActionResult)new UnauthorizedResult();
                        }
                    }
                    log.LogInformation("Checked user_id");
                }

                // post id, author id, content body, timestamp 
                using (var command = new NpgsqlCommand("DELETE FROM comment WHERE comment_id = @a1;", conn))
                {
                    command.Parameters.AddWithValue("a1", comment_id);

                    int nRows = await command.ExecuteNonQueryAsync();
                    log.LogInformation(String.Format("Number of rows deleted = {0}", nRows));
                }
            }

            ResourceLogger.LogSuccess(logger, purpose, $"User {uid} successfully deleted comment {comment_id}");
            return new OkObjectResult(new { success = true, message = "Deleted comment database entry", comment_id });
        }
    }
}
using System.IO;
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
    public static class CancelCommentVoteFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("PUT /CancelCommentVoteFunction");

        private static string purpose = "Vote Cancellations on Comments";

        [FunctionName("CancelCommentVoteFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("CancelCommentVoteFunction received a request.");

            // Build connection string using parameters from portal
            string connString = Constants.getConnString();

            string requestBody = "";
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Confirm request has all required fields.
            if (data == null || data?.comment_id == null || data?.is_upvote == null)
            {
                ResourceLogger.LogMissingFieldFailure(logger, purpose, "comment_id", "is_upvote");
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
            int comment_id = data.comment_id;
            bool is_upvote = data.is_upvote;
            bool? has_upvote = null;

            using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                await conn.OpenAsync();
                log.LogInformation("Opening connection using access token....");

                /*Query the Database */
                await using (var command = new NpgsqlCommand("SELECT is_upvote FROM Comment_Rating WHERE comment_id = @v1 AND user_id = @v2 LIMIT 1;", conn))
                {
                    command.Parameters.AddWithValue("v1", comment_id);
                    command.Parameters.AddWithValue("v2", user_id);
                    var reader = await command.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        await reader.ReadAsync();
                        has_upvote = (bool)reader.GetValue(0);
                    }
                    reader.Close();
                }
                if (has_upvote != is_upvote)
                {
                    return new OkObjectResult(new { success = false, message = "Vote does not exist" });
                }
                using (var command = new NpgsqlCommand("DELETE FROM Comment_Rating WHERE comment_id = @v1 AND user_id = @v2;", conn))
                {
                    command.Parameters.AddWithValue("v1", comment_id);
                    command.Parameters.AddWithValue("v2", user_id);
                    await command.ExecuteNonQueryAsync();
                }
                string vote = is_upvote ? "up_count" : "down_count";
                using (var command = new NpgsqlCommand("UPDATE Comment SET " + vote + " = " + vote + " - 1 WHERE comment_id = @v;", conn))
                {
                    command.Parameters.AddWithValue("v", comment_id);
                    await command.ExecuteNonQueryAsync();
                }

                ResourceLogger.LogSuccess(logger, purpose, $"User {user_id} successfully cancelled vote on comment {comment_id}");
                return new OkObjectResult(new { success = true, message = "Cancelled Vote" });
            }
        }
    }
}


using System;
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
    public static class UpvotePostFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("POST /UpvotePostFunction");
        private static string purpose = "Votes on Posts";

        [FunctionName("UpvotePostFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("UpvotePostFunction received a request.");

            string connString = Constants.getConnString();

            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Confirm request has all required fields.
            if (data?.post_id == null || data?.is_upvote == null)
            {
                ResourceLogger.LogMissingFieldFailure(logger, purpose, "post_id", "is_upvote");
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
            int post_id = data.post_id;
            bool is_upvote = data.is_upvote;
            int count;

            await using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                await conn.OpenAsync();
                log.LogInformation("Opening connection using access token....");

                await using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM post_rating WHERE post_id = @a1 AND user_id = @b1;", conn))
                {
                    command.Parameters.AddWithValue("a1", post_id);
                    command.Parameters.AddWithValue("b1", user_id);

                    count = Convert.ToInt32(await command.ExecuteScalarAsync());
                }

                var format = count > 0 ?
                    "UPDATE post_rating SET is_upvote = @c1 WHERE post_id = @a1 AND user_id = @b1;" :
                    "INSERT INTO post_rating(post_id, user_id, is_upvote) VALUES(@a1, @b1, @c1);";
                using (var command = new NpgsqlCommand(format, conn))
                {
                    command.Parameters.AddWithValue("a1", post_id);
                    command.Parameters.AddWithValue("b1", user_id);
                    command.Parameters.AddWithValue("c1", is_upvote);
                    await command.ExecuteScalarAsync();
                }

                var upOrDown = is_upvote ? "up" : "down";
                ResourceLogger.LogSuccess(logger, is_upvote ? "Upvotes on Posts" : "Downvotes on Posts", $"User {uid} successfully {upOrDown}voted post {post_id}");
                var message = count > 0 ?
                    "Updated database entry" :
                    "Created database entry";
                return new OkObjectResult(new { success = true, message = message, post_id, user_id, is_upvote });
            }
        }
    }
}
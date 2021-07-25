using System;
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
    public static class DeletePostFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("DELETE /DeletePostFunction");
        private static string purpose = "Post Deletions";

        [FunctionName("DeletePostFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("DeletePostFunction received a request.");

            // Build connection string using parameters from portal
            string connString = Constants.getConnString();

            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Confirm request has all required fields.
            if (data == null || data?.post_id == null)
            {
                ResourceLogger.LogMissingFieldFailure(logger, purpose, "post_id");
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
            int post_id = data.post_id;

            await using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                await conn.OpenAsync();
                log.LogInformation("Opening connection using access token....");

                if (uid != -1)
                {
                    using (var command = new NpgsqlCommand("SELECT author_id FROM post WHERE post_id = @pid", conn))
                    {
                        command.Parameters.AddWithValue("pid", post_id);
                        int correct_uid = Convert.ToInt32(await command.ExecuteScalarAsync());
                        if (correct_uid != uid && role != "admin")
                        {
                            ResourceLogger.LogUnauthorizedRoleFailure(logger, purpose, uid, role);
                            return (ActionResult)new UnauthorizedResult();
                        }
                    }
                    log.LogInformation("Checked user_id");
                }

                /*Query the Database */
                using (var command = new NpgsqlCommand("DELETE FROM post WHERE post_id = @v1;", conn))
                {
                    command.Parameters.AddWithValue("v1", post_id);
                    await command.ExecuteReaderAsync();
                }
            }
            // If the code reaches here, we have already deleted the post.
            // Now we could tell the Google server to delete the post url.
            HttpResponseMessage delete_response = await GoogleUtils.singleton.DeletePost(post_id.ToString(), context);

            ResourceLogger.LogSuccess(logger, purpose, $"User {uid} successfully deleted post {post_id}", namespace_id: ResourceLogger.USER_JOURNEY_NAMESPACE);
            return new OkObjectResult(new { success = true, message = "Deleted database entry", post_id = post_id, isDeleteIndexSucceed = delete_response.IsSuccessStatusCode });
        }
    }
}


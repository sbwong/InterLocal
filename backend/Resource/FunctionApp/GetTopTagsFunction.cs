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

namespace FunctionApp
{
    public static class GetTopTagsFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("GET /GetTopTagsFunction");
        private static string purpose = "Top Tag Retrievals";

        [FunctionName("GetTopTagsFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetTopTagsFunction received a request.");

            string connString = Constants.getConnString();

            // Extract required fields.
            int limit = 5;
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

            List<int> tag_ids = new List<int>();
            List<string> tags = new List<string>();

            using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                await conn.OpenAsync();
                log.LogInformation("Opening connection using access token....");
                await using (var command = new NpgsqlCommand("SELECT count(p.post_id) as freq, t.tag_id FROM Post_Tag t left join Post p on t.post_id = p.post_id WHERE DATE_PART('day', NOW() - p.created_time) < 7 GROUP BY t.tag_id ORDER BY freq DESC LIMIT @v;", conn))
                {
                    command.Parameters.AddWithValue("v", limit);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int tag_id = (int)reader.GetValue(1);
                        tag_ids.Add(tag_id);
                    }
                    await reader.CloseAsync();
                }

                for (int i = 0; i < tag_ids.Count; i++)
                {
                    await using (var command = new NpgsqlCommand("SELECT tag_name FROM Tag WHERE tag_id = @v", conn))
                    {
                        command.Parameters.AddWithValue("v", tag_ids[i]);
                        var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            string tag = (string)reader.GetValue(0);
                            tags.Add(tag);
                        }
                        await reader.CloseAsync();
                    }
                }
            }

            ResourceLogger.LogSuccess(logger, purpose, $"Successfully retrieved top {limit} tags");
            return new OkObjectResult(new { success = true, tags = tags.ToArray() });
        }
    }
}


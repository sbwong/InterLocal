using System;
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
    public static class CountPostsFunction
    {
        private static ILoggingAdapter logger = new LoggingAdapter("GET /CountPostsFunction");
        private static string purpose = "Post Countings";

        [FunctionName("CountPostsFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("CountPostsFunction received a request.");

            string connString = Constants.getConnString();

            // Extract required fields.
            string author_filter = "";
            string tag_filter = "";
            string date_type_filter = "";
            string user_type_filter = "";

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

            long count = 0;

            using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening connection");
                await conn.OpenAsync();
                log.LogInformation("Opening connection using access token....");

                /*Query the Database */
                using (var command = new NpgsqlCommand("SELECT count(DISTINCT post_id) FROM post p INNER JOIN USERS u ON p.author_id = u.user_id " + tag_join + content_type_join + " WHERE TRUE " + author_filter + tag_filter + user_type_filter + date_type_filter + ";", conn))
                {
                    count = (long)await command.ExecuteScalarAsync();
                }
            }

            ResourceLogger.LogSuccess(logger, purpose, $"Successfully counted {count} posts with author filter = {author_id_str} and tag filter = {tags_str}");
            return new OkObjectResult(new { total_posts = count });
        }
    }
}


using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;

namespace UserManagement
{
    public static class CreateUserFunction
    {
        // Obtain connection string information from the portal
        //
        private static IDbAdapter db = new PostgresDbAdapter(
            Constants.DBHost, Constants.DBUser, Constants.DBName, Constants.DBPassword, Constants.DBPort
        );

        public static void setDBAdapter(IDbAdapter newdb) {
            CreateUserFunction.db = newdb;
        }
        public static ILoggingAdapter logger = new LoggingAdapter("POST /user");
        private static int MIN_PASSWORD_LENGTH = 8;

        [FunctionName("CreateUserFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Build connection string using parameters from portal
            //
            object res = new {};

            dynamic data = null;
            try {
                string requestBody = String.Empty;
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                log.LogInformation("Write HTTP trigger function processed a request.");
                data = JsonConvert.DeserializeObject(requestBody);
            } catch (Exception e) {
                string message = "Bad request syntax";
                logger.LogFailureMetric(message, "CreateUser Failures 400", e.ToString());
                return new BadRequestObjectResult(message);
            }
            
            if (data == null)
            {
                string message = "Empty request body";
                logger.LogFailureMetric(message, "CreateUser Failures 400");
                return new BadRequestObjectResult(message);
            }

            // Extract required fields.
            string username = data.username;
            string password = data.password;
            string college = data.college;
            string email = data.email;
            string phone_number = data.phone_number != null ?  data.phone_number : "";
            string country = data.country;
            string first_name = data.first_name;
            string last_name = data.last_name;
            string year = data.year;
            string profile_pic_url = data.profile_pic_url != null ? data.profile_pic_url : "";
            string bio = data.bio != null ? data.bio : "";
            int id = -1;

            // Confirm request has all required fields.
            if (username == null || password == null || college == null || email == null || country == null || first_name == null || last_name == null || year == null)
            {
                string message = "Missing fields in request body";
                logger.LogFailureMetric(message, "CreateUser Failures 400");
                return new BadRequestObjectResult(message);
            }

            int length = password.Length;
            bool isLetterPresent = password.Any(c => char.IsLetter(c));
            bool isNonLetterPresent = password.Any(c => !char.IsLetter(c));
            bool isWhiteSpacePresent = password.Any(c => char.IsWhiteSpace(c));
            if (length < MIN_PASSWORD_LENGTH || !isNonLetterPresent || !isLetterPresent || isWhiteSpacePresent)
            {
                string message = String.Format("Password failed to meet requirements");
                logger.LogFailureMetric(message, "CreateUser Failures 400");
                return new BadRequestObjectResult(message);
            }

            try
            {
                id = await db.CreateUserAsync(data);
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ErrorCode);
                string message = "Service unavailable";
                if (ex.Message.StartsWith("23505"))
                {
                    message = "Username already taken";
                    logger.LogFailureMetric($"{message} ({username})", "CreateUser Failures 409", ex.ToString());
                    return new ConflictObjectResult(message);
                }
                logger.LogFailureMetric(message, "CreateUser Failures 503", ex.ToString());
                return new StatusCodeResult(503);
            }

            if (id == -1) {
                return new StatusCodeResult(503);
            }

            logger.LogSuccessMetric(string.Format("User entry for {0} created.", username), "CreateUser Successes");
            return new OkObjectResult(new { username = username, password = password});
        }
    }
}


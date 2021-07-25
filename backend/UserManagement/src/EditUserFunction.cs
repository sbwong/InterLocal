using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Net.Http;
using JWT.Builder;
using JWT.Algorithms;

namespace UserManagement
{
    public static class EditUserFunction
    {
        private static IDbAdapter db = new PostgresDbAdapter(
            Constants.DBHost, Constants.DBUser, Constants.DBName, Constants.DBPassword, Constants.DBPort
        );

        public static void setDBAdapter(IDbAdapter newdb) {
            EditUserFunction.db = newdb;
        }
        public static ILoggingAdapter logger = new LoggingAdapter("PUT /user");

        [FunctionName("EditUserFunction")]
        public static async Task<IActionResult>  Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "user/{user_id:int?}")] HttpRequest req, int? user_id,
            ILogger log)
        {
            log.LogInformation("EditUserFunction HTTP trigger function received a request.");
            int jwt_id = -1;
            bool is_admin = false;
            if (((string)req.Headers[Constants.TOKEN_KEY]) == null || ((string)req.Headers[Constants.TOKEN_KEY]) == String.Empty)
            {
                logger.LogFailureMetric($"JWT not present", "EditUser Failures 401");
                return new StatusCodeResult(401);
            }
            else
            {
                string jwt_string = (string)req.Headers[Constants.TOKEN_KEY];
                try
                {
                    Claims claims = JwtDecoder.decodeString(jwt_string);
                    jwt_id = claims.user_id;
                    string role = claims.role;
                    is_admin = role.Equals("admin");
                    if (user_id == null) {
                        user_id = jwt_id;
                    }
                    if (user_id != jwt_id && !is_admin) {
                        logger.LogFailureMetric($"Forbidden attempt to edit user (user_id = {user_id})", "EditUser Failures 403");
                        return new StatusCodeResult(403);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    logger.LogFailureMetric($"JWT could not be decoded (user_id = {user_id})", "EditUser Failures 400", e.ToString());
                    return new BadRequestObjectResult(new { message = "JWT could not be decoded" });
                }
            }

            dynamic data = null;
            string requestBody = String.Empty;
            try
            {
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                data = JsonConvert.DeserializeObject(requestBody);
            }  catch (Exception e) {
                string message = "Bad request syntax";
                logger.LogFailureMetric($"{message} (user_id = {user_id})", "EditUser Failures 400", e.ToString());
                return new BadRequestObjectResult(message);
            }

            // Confirm request has all required fields.
            if (data == null)
            {
                logger.LogFailureMetric($"Empty request body (user_id = {user_id})", "EditUser Failures 400");
                return (ActionResult)new BadRequestObjectResult("Empty Request Body");
            }

            string msg = "Updated user";
            int nRows;
            try
            {
                nRows = await db.EditUserAsync((int)user_id, data);
            } catch (Exception e)
            {
                string message = "Service unavailable";
                logger.LogFailureMetric($"{message} (user_id = {user_id})", "EditUser Failures 503", e.ToString());
                return new StatusCodeResult(503);
            }
            if (nRows == -1)
            {
                logger.LogFailureMetric($"Request body does not contain correct fields (user_id = {user_id})", "EditUser Failures 400");
                return (ActionResult)new BadRequestObjectResult("Incorrect Request Body Fields");
            }
            if (nRows == 0)
            {
                msg = "Could not find user";
            }
            logger.LogSuccessMetric($"User (user_id = {user_id}) was updated", "EditUser Successes");
            return new OkObjectResult(new { message = msg});
        }
    }
}

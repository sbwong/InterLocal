using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Npgsql;
using JWT.Builder;
using JWT.Algorithms;

namespace UserManagement
{
    public static class DeleteUserFunction
    {

         private static IDbAdapter db = new PostgresDbAdapter(
            Constants.DBHost, Constants.DBUser, Constants.DBName, Constants.DBPassword, Constants.DBPort
        );

        public static void setDBAdapter(IDbAdapter newdb) {
            DeleteUserFunction.db = newdb;
        }

        public static ILoggingAdapter logger = new LoggingAdapter("DELETE /user");

        [FunctionName("DeleteUserFunction")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "user/{user_id:int?}")] HttpRequest req, int? user_id,
            ILogger log)
        {
            Console.WriteLine("DeleteUserFunction HTTP trigger function received a request.");
            if (((string)req.Headers[Constants.TOKEN_KEY]) == null || ((string)req.Headers[Constants.TOKEN_KEY]) == String.Empty) {
                logger.LogFailureMetric($"Missing JWT (user_id = {user_id})", "DeleteUser Failures 401");
                return new UnauthorizedResult();
            }
            string jwt_string = req.Headers[Constants.TOKEN_KEY];
            int jwt_user_id;
            bool is_admin;
            try {
                Claims claims = JwtDecoder.decodeString(jwt_string);
                jwt_user_id = claims.user_id;
                string role = claims.role;
                is_admin = role.Equals("admin");
                if (user_id == null) user_id = jwt_user_id;
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                logger.LogFailureMetric($"JWT could not be decoded (user_id = {user_id})", "DeleteUser Failures 400", e.ToString());
                return new BadRequestObjectResult(new {message = "JWT could not be decoded"});
            }
            log.LogInformation("DeleteUserFunction HTTP trigger function processed a request.");

            if (user_id != jwt_user_id && !is_admin) {
                logger.LogFailureMetric($"Forbidden attempt to delete user (user_id = {user_id})", "DeleteUser Failures 403");
                return new StatusCodeResult(403);
            }

            string msg = "Deleted users database entry";
            int nrows_deleted;
            try
            {
                nrows_deleted = db.DeleteUser((int)user_id);
            } catch (Exception e)
            {
                string message = "Service unavailable";
                logger.LogFailureMetric($"{message} (user_id = {user_id})", "DeleteUser Failures 503", e.ToString());
                return new StatusCodeResult(503);
            }

            if (nrows_deleted == 0) msg = "User already deleted";
            logger.LogSuccessMetric($"User entry (user_id = {user_id}) was deleted", "DeleteUser Successes");
            return new OkObjectResult(new {message = msg});
        }
    }
}
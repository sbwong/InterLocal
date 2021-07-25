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
    public static class GoogleIndexTestFunction
    {
        static GoogleUtils google_utils = new GoogleUtils();

        [FunctionName("GoogleIndexFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            string requestBody = "";
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = streamReader.ReadToEnd();
            }
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
            {
                return new BadRequestObjectResult(new { error = "Body is not provided" });
            }
            if (data?.url == null)
            {
                return new BadRequestObjectResult(new { error = "URL is not provided" });
            }
            if (data?.type == null)
            {
                return new BadRequestObjectResult(new { error = "Action type is not provided" });
            }
            string new_page_url = data?.url;
            string action_type = data?.type;
            if (action_type == "URL_UPDATED")
            {
                var res = await google_utils.UpdatePage(new_page_url, context);
                return new OkObjectResult(new { success = true, res = res, newUrl = new_page_url });
            }
            else if (action_type == "URL_DELETED")
            {
                var res = await google_utils.DeletePage(new_page_url, context);
                return new OkObjectResult(new { success = true, res = res, newUrl = new_page_url });
            }
            else
            {
                return new BadRequestObjectResult(new { error = "Invalid action type is provided." });
            }

        }
    }
}

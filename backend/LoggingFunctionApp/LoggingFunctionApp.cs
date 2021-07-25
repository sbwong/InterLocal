using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Newtonsoft.Json;

namespace LoggingFunctionApp
{
    public static class LoggingFunctionApp
    {
        //Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY")
        private static IConfiguration _configuration;
        private static string categoryName = "test category";
        private static TelemetryClient telemetryClient = new TelemetryClient(new TelemetryConfiguration(Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY")));
        private static IDictionary<string, SeverityLevel> sevLevels = new Dictionary<string, SeverityLevel>(){ 
            { "Verbose", SeverityLevel.Verbose }, 
            { "Information", SeverityLevel.Information }, 
            { "Warning", SeverityLevel.Warning }, 
            { "Critical", SeverityLevel.Critical }, 
            { "Error", SeverityLevel.Error } 
        };


        [FunctionName("LoggingFunctionApp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = streamReader.ReadToEnd();
            }
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data == null) { return new BadRequestObjectResult("Could not deserialize JSON body"); }

            string message;
            string log_type;
            string exception_message;
            bool exception_flag = false;
            try
            {
                message = data?.message;
            } catch (Exception e)
            {
                return new BadRequestObjectResult("Message is not valid");
            }

            try
            {
                log_type = data?.log_type;
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult("Log_type is not valid");
            }

            try
            {
                exception_message = data?.exception_message;
                exception_flag = true;
            }
            catch (Exception e)
            {
                exception_message = "";
            }






            if (!req.Headers.ContainsKey("Service_ID"))
            {
                return new BadRequestObjectResult("Header does not contain a Service ID");
            }

            string service_id = req.Headers["Service_ID"].ToString();
            if (log_type == null)
            {
                return new BadRequestObjectResult("log_type is null");
            }

            switch (log_type)
            {
                case "Debug":
                    if (message == null)
                    {
                        return new BadRequestObjectResult("message is null");
                    }
                    Debug(message, service_id, exception_message, exception_flag);
                    break;
                case "CustomMetric":
                    try
                    {
                        string key = data?.key;
                        double value = data?.value;
                        if (message == null)
                        {
                            message = "";
                        }
                        if (!req.Headers.ContainsKey("Namespace_ID")) { return new BadRequestObjectResult("Header does not contain a Namespace ID"); }
                        string namespace_id = req.Headers["Namespace_ID"].ToString();
                        CustomMetric(key, value, message, service_id, namespace_id, exception_message, exception_flag);
                        break;
                    } catch (Exception e)
                    {
                        return new BadRequestObjectResult("Key or value is not valid; both are required for Custom Metrics");
                    }
                    
                case "Critical":
                    if (message == null)
                    {
                        return new BadRequestObjectResult("message is null");
                    }
                    Critical(message, service_id, exception_message, exception_flag);
                    break;
                default:
                    return new BadRequestObjectResult("Log_type is not a valid input; please refer to the Logging Guide for valid strings");

            }

            return new OkObjectResult(new { success = true});
        }

        public static void Debug(string message, string service_id, string exception_message, bool exception_flag)
        {
            TraceTelemetry trace = new TraceTelemetry();
            trace.Timestamp = DateTime.Now;
            trace.SeverityLevel = sevLevels["Information"];
            trace.Message = message;
            trace.Properties["Service ID"] = service_id;
            if (exception_flag)
            {
                trace.Properties["Exception Message"] = exception_message;
            }
            telemetryClient.TrackTrace(trace);
        }

        public static void CustomMetric(string name, double value, string message, string service_id, string namespace_id, string exception_message, bool exception_flag)
        {
            MetricTelemetry response = new MetricTelemetry(name, value);
            response.Timestamp = DateTime.Now;
            response.MetricNamespace = namespace_id;
            response.Properties["Service ID"] = service_id;
            response.Properties["Message"] = message;
            if (exception_flag)
            {
                response.Properties["Exception Message"] = exception_message;
            }
            telemetryClient.TrackMetric(response);
        }

        public static void Critical(string message, string service_id, string exception_message, bool exception_flag)
        {
            TraceTelemetry trace = new TraceTelemetry();
            trace.Timestamp = DateTime.Now;
            trace.SeverityLevel = sevLevels["Critical"];
            trace.Message = message;
            trace.Properties["Service ID"] = service_id;
            if (exception_flag)
            {
                trace.Properties["Exception Message"] = exception_message;
            }
            telemetryClient.TrackTrace(trace);
        }
    }
}


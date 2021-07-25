/**
 * Copied from "backend/UserManagement/src/LoggingAdapter.cs".
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace FunctionApp
{

    public interface ILoggingAdapter
    {
        public void LogDebug(string service_id, string namespace_id, string message, string key, int value);
        public void LogMetric(string service_id, string namespace_id, string message, string key, int value);
    }

    public class LoggingAdapter : ILoggingAdapter
    {
        private HttpClient httpClient;
        private string functionName;
        private static string SERVICE_ID_HEADER = "Service_ID";
        private static string NAMESPACE_ID_HEADER = "Namespace_ID";

        public LoggingAdapter(string functionName)
        {
            this.functionName = functionName;
            httpClient = new HttpClient();
        }

        public async void LogDebug(string service_id, string namespace_id, string message, string key, int value)
        {
            Dictionary<string, Object> dictionary = new Dictionary<string, Object>();
            dictionary.Add("message", String.Format("[{0}]: {1}", functionName, message));
            dictionary.Add("log_type", "Debug");
            dictionary.Add("key", key);
            dictionary.Add("value", value);

            string json = JsonConvert.SerializeObject(dictionary);
            var requestData = new StringContent(json, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Remove(SERVICE_ID_HEADER);
            httpClient.DefaultRequestHeaders.Add(SERVICE_ID_HEADER, service_id);
            httpClient.DefaultRequestHeaders.Remove(NAMESPACE_ID_HEADER);
            httpClient.DefaultRequestHeaders.Add(NAMESPACE_ID_HEADER, namespace_id);
            await httpClient.PostAsync(Constants.LOGGING_URL, requestData);
        }

        public async void LogMetric(string service_id, string namespace_id, string message, string key, int value)
        {
            Dictionary<string, Object> dictionary = new Dictionary<string, Object>();
            dictionary.Add("message", String.Format("[{0}]: {1}", functionName, message));
            dictionary.Add("log_type", "CustomMetric");
            dictionary.Add("key", key);
            dictionary.Add("value", value);

            string json = JsonConvert.SerializeObject(dictionary);
            var requestData = new StringContent(json, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Remove(SERVICE_ID_HEADER);
            httpClient.DefaultRequestHeaders.Add(SERVICE_ID_HEADER, service_id);
            httpClient.DefaultRequestHeaders.Remove(NAMESPACE_ID_HEADER);
            httpClient.DefaultRequestHeaders.Add(NAMESPACE_ID_HEADER, namespace_id);
            await httpClient.PostAsync(Constants.LOGGING_URL, requestData);
        }
    }

    public class ResourceLogger
    {
        public static string SERVICE_ID = "Resource";
        public static string FAILURE_SERVICE_ID = "Resource.Failures";
        public static string USER_JOURNEY_NAMESPACE = "User Journey Centered Metrics";

        public static void LogJwtDecodeFailure(ILoggingAdapter logger, string endpoint_purpose, string namespace_id = "")
        {
            logger.LogMetric(
                ResourceLogger.FAILURE_SERVICE_ID,
                string.IsNullOrEmpty(namespace_id) ? ResourceLogger.FAILURE_SERVICE_ID : namespace_id,
                "Failed to decode uid from JWT",
                $"Failed {endpoint_purpose} 401",
                1
            );
        }

        public static void LogMissingFieldFailure(ILoggingAdapter logger, string endpoint_purpose, string namespace_id = "", params string[] fields)
        {
            logger.LogMetric(
                ResourceLogger.FAILURE_SERVICE_ID,
                string.IsNullOrEmpty(namespace_id) ? ResourceLogger.FAILURE_SERVICE_ID : namespace_id,
                $"{String.Join(" and/or ", fields)} field(s) missing",
                $"Failed {endpoint_purpose} 400",
                1
            );
        }

        public static void LogInvalidFieldFailure(ILoggingAdapter logger, string endpoint_purpose, string field, string value, string namespace_id = "")
        {
            logger.LogMetric(
                ResourceLogger.FAILURE_SERVICE_ID,
                string.IsNullOrEmpty(namespace_id) ? ResourceLogger.FAILURE_SERVICE_ID : namespace_id,
                $"Value {value} of field {field} is invalid",
                $"Failed {endpoint_purpose} 400",
                1
            );
        }

        public static void LogSuccess(ILoggingAdapter logger, string endpoint_purpose, string message, string namespace_id = "")
        {
            logger.LogMetric(
                ResourceLogger.SERVICE_ID,
                string.IsNullOrEmpty(namespace_id) ? ResourceLogger.SERVICE_ID : namespace_id,
                message,
                $"Successful {endpoint_purpose}",
                1
            );
        }

        public static void LogUnauthorizedRoleFailure(ILoggingAdapter logger, string endpoint_purpose, int uid, string role, string namespace_id = "")
        {
            logger.LogMetric(
                ResourceLogger.FAILURE_SERVICE_ID,
                string.IsNullOrEmpty(namespace_id) ? ResourceLogger.FAILURE_SERVICE_ID : namespace_id,
                $"User {uid} has role {role}, not admin",
                $"Failed {endpoint_purpose} 401",
                1
            );
        }

        public static void LogNotExistFailure(ILoggingAdapter logger, string endpoint_purpose, string type, string id, string namespace_id = "")
        {
            logger.LogMetric(
                ResourceLogger.FAILURE_SERVICE_ID,
                string.IsNullOrEmpty(namespace_id) ? ResourceLogger.FAILURE_SERVICE_ID : namespace_id,
                $"{type} {id} does not exist",
                $"Failed {endpoint_purpose} 400",
                1
            );
        }
    }
}

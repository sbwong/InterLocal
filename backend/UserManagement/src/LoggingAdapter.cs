using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace UserManagement
{

    public interface ILoggingAdapter
    {
        public void LogDebug(string message, string service_id);
        public void LogMetric(string message, string key, string service_id, string namespace_id, string error_msg = "", int value = 1);
        public void LogSuccessMetric(string message, string key, int value = 1);
        public void LogFailureMetric(string message, string key, string error_message = "", int value = 1);
    }

    public class LoggingAdapter : ILoggingAdapter
    {
        public static string SUCCESS_NAMESPACE_ID = "UserManagement.Successes";
        public static string FAILURE_NAMESPACE_ID = "UserManagement.Failures";
        public static string SERVICE_ID = "UserManagement";
        public static string SERVICE_ID_HEADER = "Service_ID";
        public static string NAMESPACE_ID_HEADER = "Namespace_ID";

        private HttpClient httpClient;
        private string functionName;

        public LoggingAdapter(string functionName)
        {
            this.functionName = functionName;
            httpClient = new HttpClient();
        }

        public async void LogDebug(string message, string service_id)
        {
            if (string.IsNullOrEmpty(service_id))
            {
                service_id = SERVICE_ID;
            }

            Dictionary<string, Object> dictionary = new Dictionary<string, Object>();
            dictionary.Add("message", String.Format("[{0}]: {1}", functionName, message));
            dictionary.Add("log_type", "Debug");

            string json = JsonConvert.SerializeObject(dictionary);
            var requestData = new StringContent(json, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Remove(SERVICE_ID_HEADER);
            httpClient.DefaultRequestHeaders.Add(SERVICE_ID_HEADER, service_id);
            await httpClient.PostAsync(Constants.LOGGING_URL, requestData);
        }

        public async void LogMetric(string message, string key, string service_id, string namespace_id, string error_msg="", int value=1)
        {
            if (string.IsNullOrEmpty(service_id))
            {
                service_id = SERVICE_ID;
            }
            if (string.IsNullOrEmpty(namespace_id))
            {
                namespace_id = SUCCESS_NAMESPACE_ID;
            }

            Dictionary<string, Object> dictionary = new Dictionary<string, Object>();
            dictionary.Add("message", String.Format("[{0}]: {1}", functionName, message));
            dictionary.Add("log_type", "CustomMetric");
            dictionary.Add("key", key);
            dictionary.Add("value", value);
            if (!string.IsNullOrEmpty(error_msg))
            {
                dictionary.Add("exception", error_msg);
            }

            string json = JsonConvert.SerializeObject(dictionary);
            var requestData = new StringContent(json, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Remove(SERVICE_ID_HEADER);
            httpClient.DefaultRequestHeaders.Add(SERVICE_ID_HEADER, service_id);
            httpClient.DefaultRequestHeaders.Remove(NAMESPACE_ID_HEADER);
            httpClient.DefaultRequestHeaders.Add(NAMESPACE_ID_HEADER, namespace_id);
            await httpClient.PostAsync(Constants.LOGGING_URL, requestData);
        }

        public void LogSuccessMetric(string message, string key, int value=1)
        {
            LogMetric(message, key, SERVICE_ID, SUCCESS_NAMESPACE_ID, "", value);
        }

        public void LogFailureMetric(string message, string key, string error_msg = "", int value = 1)
        {
            LogMetric(message, key, SERVICE_ID, FAILURE_NAMESPACE_ID, error_msg, value);
        }
    }
}

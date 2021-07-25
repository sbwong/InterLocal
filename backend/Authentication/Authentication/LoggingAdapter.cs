using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Authentication
{
    public class LoggingAdapter
    {
        private HttpClient httpClient;
        private string functionName;
        private static string SERVICE_ID_HEADER = "Service_ID";
        private static string SERVICE_ID = "Authentication";
        private static string NAMESPACE_ID_HEADER = "Namespace_ID";
        public static string SUCCESS_NAMESPACE_ID = "Authentication.Success";

        public LoggingAdapter(string functionName)
        {
            this.functionName = functionName;
            httpClient = new HttpClient();
        }

        public async void logDebug(string message)
        {
            Dictionary<string, Object> dictionary = new Dictionary<string, Object>();
            dictionary.Add("message", String.Format("[{0}]: {1}", functionName, message));
            dictionary.Add("log_type", "Debug");
            dictionary.Add("key", "Number of Requests");
            dictionary.Add("value", 1);

            string json = JsonConvert.SerializeObject(dictionary);
            var requestData = new StringContent(json, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Remove(SERVICE_ID_HEADER);
            httpClient.DefaultRequestHeaders.Add(SERVICE_ID_HEADER, SERVICE_ID);
            httpClient.DefaultRequestHeaders.Remove(NAMESPACE_ID_HEADER);
            await httpClient.PostAsync(Constants.LOGGING_URL, requestData);
        }

        public async void logMetric(string message, string key, Object value, string namespace_id = "Authentication.Failures")
        {
            Dictionary<string, Object> dictionary = new Dictionary<string, Object>();
            dictionary.Add("message", String.Format("[{0}]: {1}", functionName, message));
            dictionary.Add("log_type", "CustomMetric");
            dictionary.Add("key", key);
            dictionary.Add("value", value);

            string json = JsonConvert.SerializeObject(dictionary);
            var requestData = new StringContent(json, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Remove(SERVICE_ID_HEADER);
            httpClient.DefaultRequestHeaders.Add(SERVICE_ID_HEADER, SERVICE_ID);
            httpClient.DefaultRequestHeaders.Remove(NAMESPACE_ID_HEADER);
            httpClient.DefaultRequestHeaders.Add(NAMESPACE_ID_HEADER, namespace_id);
            await httpClient.PostAsync(Constants.LOGGING_URL, requestData);
        }
    }
}

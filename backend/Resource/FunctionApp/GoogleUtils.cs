using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Indexing.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.Reflection;
using Microsoft.Azure.WebJobs;

namespace FunctionApp
{
    public class GoogleUtils
    {
        private GoogleCredential _googleCredential;
        private string _googleApiKey;
        private string frontend_domain_name = "https://comp410-frontend-dev.azurewebsites.net";
        public static GoogleUtils singleton = new GoogleUtils();

        public GoogleUtils()
        {
            // _googleCredential = GetGoogleCredential();
            _googleApiKey = "AIzaSyC84AYhvA6xGsc-qh9tD-z8UF7twwGREUU";
        }

        public async Task<HttpResponseMessage> UpdatePage(string jobUrl, ExecutionContext context)
        {
            return await PostJobToGoogle(jobUrl, "URL_UPDATED", context);
        }

        public async Task<HttpResponseMessage> DeletePage(string jobUrl, ExecutionContext context)
        {
            return await PostJobToGoogle(jobUrl, "URL_DELETED", context);
        }

        public async Task<HttpResponseMessage> PostJobToGoogle(string jobUrl, string action, ExecutionContext context)
        {
            _googleCredential = GetGoogleCredential(context);

            var serviceAccountCredential = (ServiceAccountCredential)_googleCredential.UnderlyingCredential;

            string googleApiUrl = "https://indexing.googleapis.com/v3/urlNotifications:publish";

            var requestBody = new
            {
                url = jobUrl,
                type = action
            };

            var httpClientHandler = new HttpClientHandler();

            var configurableMessageHandler = new ConfigurableMessageHandler(httpClientHandler);

            var configurableHttpClient = new ConfigurableHttpClient(configurableMessageHandler);

            serviceAccountCredential.Initialize(configurableHttpClient);

            HttpContent content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await configurableHttpClient.PostAsync(new Uri(googleApiUrl), content);

            return response;
        }

        private GoogleCredential GetGoogleCredential(ExecutionContext context)
        {
            //var path = HostingEnvironment.MapPath("/hamidmosalla-28d08becf0a7.json");
            //var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory, ".."));
            //Console.WriteLine(rootDirectory);
            GoogleCredential credential;

            string path = Path.Combine(context.FunctionAppDirectory, "fine-eye-311021-fcdf95b5be16.json");
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(new[] { "https://www.googleapis.com/auth/indexing" });
            }

            return credential;
        }

        public async Task<HttpResponseMessage> IndexPost(string post_id, ExecutionContext context) {
            string new_post_url = $"{frontend_domain_name}/Post/{post_id}";
            HttpResponseMessage index_response = await UpdatePage(new_post_url, context);
            return index_response;
        }

        public async Task<HttpResponseMessage> DeletePost(string post_id, ExecutionContext context) {
            string deleted_post_url = $"{frontend_domain_name}/Post/{post_id}";
            HttpResponseMessage delete_response = await DeletePage(deleted_post_url, context);
            return delete_response;
        }


    }
}

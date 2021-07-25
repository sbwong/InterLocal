using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace FunctionApp.Tests
{
    public class EditPostFunctionTest
    {
        private static ILogger logger = new Mock<ILogger>().Object;

        private static ExecutionContext context = new Mock<ExecutionContext>().Object;

        private static int user_id = 107;
        private static string post_id = "284";

        private static string bad_post_id = "203";

        private HttpRequest getRequest()
        {
            List<string> tags = new List<string>();
            tags.Add("a");
            tags.Add("b");
            tags.Add("c");
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "content", "updated by EditPostFunctionTest.cs" },
                    { "tags", tags }
                },
                user_id: user_id
            );
        }

        private HttpRequest getAdminRequest()
        {
            List<string> tags = new List<string>();
            tags.Add("a");
            tags.Add("b");
            tags.Add("c");
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "user_id", TestUtils.ADMIN_USER_ID },
                    { "content", "updated by EditPostFunctionTest.cs" },
                    { "tags", tags }
                },
                user_id: TestUtils.ADMIN_USER_ID
            );
        }

        [Fact]
        public async System.Threading.Tasks.Task TestEditPost()
        {
            var response = (OkObjectResult)await EditPostFunction.Run(getRequest(), post_id, logger, context);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task TestAdminEditPost()
        {
            var response = (OkObjectResult)await EditPostFunction.Run(getAdminRequest(), post_id, logger, context);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task TestEditPostFail()
        {
            // Provided ID does not match author ID in database.
            var response3 = (UnauthorizedResult)await EditPostFunction.Run(getRequest(), bad_post_id, logger, context);
            Assert.Equal(401, response3.StatusCode);
        }

    }
}

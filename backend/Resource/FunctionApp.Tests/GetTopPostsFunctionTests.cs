using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace FunctionApp.Tests
{
    public class GetTopPostsFunctionTests
    {
        private static ILogger logger = new Mock<ILogger>().Object;

        private HttpRequest getRequest()
        {
            return TestUtils.CreateHttpRequest(
                queryParams: new Dictionary<string, StringValues> {
                    { "limit", new StringValues("10") }
                });
        }

        private HttpRequest getRequest2()
        {
            return TestUtils.CreateHttpRequest();
        }


        [Fact]
        public async System.Threading.Tasks.Task TestGetTopPosts()
        {
            var response = (OkObjectResult)await GetTopPostsFunction.Run(getRequest(), logger);
            object body = response.Value;
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(true, TestUtils.GetField(body, "success"));
            Response[] posts = (Response [])TestUtils.GetField(body, "posts");
            // Check the amount of responses is less than the limit
            Assert.True(posts.Length <= 10);

            for (int i = 0; i < posts.Length - 1; i++)
            {
                Assert.True(posts[i].up_count >= posts[i + 1].up_count);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task TestGetTopPosts2()
        {
            var response = (OkObjectResult)await GetTopPostsFunction.Run(getRequest2(), logger);
            object body = response.Value;
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(true, TestUtils.GetField(body, "success"));
            Response[] posts = (Response[])TestUtils.GetField(body, "posts");
            // Check the amount of responses is less than the limit
            Assert.True(posts.Length == 3);

            for (int i = 0; i < posts.Length - 1; i++)
            {
                Assert.True(posts[i].up_count >= posts[i + 1].up_count);
            }
        }
    }
}

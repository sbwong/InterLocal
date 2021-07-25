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
    public class ReadAllPostsFunctionTests
    {
        private static ILogger logger = new Mock<ILogger>().Object;

        int limit = 10;

        private HttpRequest getRequest()
        {
            return TestUtils.CreateHttpRequest(
                queryParams: new Dictionary<string, StringValues> {
                    { "author_id", new StringValues("7") }
                });
        }

        private HttpRequest getBadRequest()
        {
            return TestUtils.CreateHttpRequest(
                queryParams: new Dictionary<string, StringValues> {
                    { "author_id", new StringValues("asdf") }
                });
        }

        [Fact]
        public async System.Threading.Tasks.Task TestReadAllPost()
        {
            var response = (OkObjectResult)await ReadAllPostsFunction.Run(getRequest(), logger);
            object body = response.Value;
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(true, TestUtils.GetField(body, "success"));
            Response[] posts = (Response [])TestUtils.GetField(body, "posts");
            // Check the amount of responses is less than the limit
            Assert.True(posts.Length <= limit);

            // Check that all responses are from author_id
            Assert.Equal(7, posts[0].author_id);

            // Check that posts are ordered by created_time
            for (int i = 0; i < posts.Length - 1; i++)
            {
                Assert.True(DateTime.Compare(posts[i].created_time, posts[i + 1].created_time) > 0);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task TestReadAllPostFail()
        {
            // Malformed ID.
            var response2 = (BadRequestResult)await ReadAllPostsFunction.Run(getBadRequest(), logger);
            Assert.Equal(400, response2.StatusCode);
        }
    }
}

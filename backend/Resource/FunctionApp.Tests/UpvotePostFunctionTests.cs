using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace FunctionApp.Tests
{
    public class UpvotePostFunctionTests
    {
        private static ILogger logger = new Mock<ILogger>().Object;

        private static int post_id = 284;
        private static int user_id = 107;

        private HttpRequest getRequest()
        {
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "post_id", post_id },
                    { "user_id", user_id }, 
                    { "is_upvote", true}
                    
                },
                user_id: user_id
            );
        }

        private HttpRequest getRequest2()
        {
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "post_id", post_id },
                    { "user_id", user_id }, 
                    { "is_upvote", false}
                    
                },
                user_id: user_id
            );
        }

        
        [Fact]
        public async System.Threading.Tasks.Task TestUpvotePost()
        {
            // Insert/update to True 
            var response = (OkObjectResult)await UpvotePostFunction.Run(getRequest(), logger);
            object body = response.Value;
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(true, TestUtils.GetField(body, "success"));
            Assert.Equal(post_id, TestUtils.GetField(body, "post_id"));
            Assert.Equal(user_id, TestUtils.GetField(body, "user_id"));
            Assert.Equal(true, TestUtils.GetField(body, "is_upvote"));

            // Update previous to False 
            var response1 = (OkObjectResult)await UpvotePostFunction.Run(getRequest2(), logger);
            object body2 = response1.Value;
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(true, TestUtils.GetField(body2, "success"));
            Assert.Equal(post_id, TestUtils.GetField(body2, "post_id"));
            Assert.Equal(user_id, TestUtils.GetField(body2, "user_id"));
            Assert.Equal(false, TestUtils.GetField(body2, "is_upvote"));
        }

        // Cases of post_id or user_id not correct are handled by database automatically rejecting violation of foreign key constraints.  

        
    }
}

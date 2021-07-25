using System;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace FunctionApp.Tests
{
    public class ReadCommentFunctionTests
    {
        private static ILogger logger = new Mock<ILogger>().Object;

        private static string post_id = "284";

        [Fact]
        public async System.Threading.Tasks.Task TestGetPostComments()
        {
            var result = (OkObjectResult)await ReadCommentFunction.Run(
                TestUtils.CreateHttpRequest(), post_id, logger);
            Assert.Equal(200, result.StatusCode);

            object body = result.Value;
            Assert.Equal(true, TestUtils.GetField(body, "success"));

            var comments = TestUtils.GetField(body, "res");
            Assert.IsType<ResponseComment[]>(comments);
            object[] commentsArray = (object[])comments;
            if (commentsArray.Length == 0)
            {
                return;
            }
            object comment = commentsArray[0];
            var commentType = comment.GetType();
            Assert.IsType<int>(TestUtils.GetField(comment, "comment_id"));
            Assert.IsType<int>(TestUtils.GetField(comment, "post_id"));
            Assert.IsType<int>(TestUtils.GetField(comment, "author_id"));
            // Assert.IsType<int>(TestUtils.GetField(comment, "parent_id"));
            Assert.IsType<string>(TestUtils.GetField(comment, "content_body"));
            Assert.IsType<DateTime>(TestUtils.GetField(comment, "created_time"));
            // Assert.IsType<DateTime>(TestUtils.GetField(comment, "last_edit_time"));
            Assert.IsType<int>(TestUtils.GetField(comment, "up_count"));
            Assert.IsType<int>(TestUtils.GetField(comment, "down_count"));
            Assert.IsType<string>(TestUtils.GetField(comment, "username"));
            Assert.IsType<bool>(TestUtils.GetField(comment, "is_admin"));
        }
    }
}

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
    public class CreatePostFunctionTests
    {
        private static ILogger logger = new Mock<ILogger>().Object;

        private static ExecutionContext context = new Mock<ExecutionContext>().Object;

        private static int user_id = 33;

        private HttpRequest getQuestionRequest()
        {
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "user_id", user_id },
                    { "title", "this is a title for question_body" },
                    { "question_body", "this is a question_body"},
                    { "note_body", null }

                },
                user_id: user_id
            );
        }

        private HttpRequest getNoteRequest()
        {
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "user_id", user_id },
                    { "title", "this is a title for note_body" },
                    { "question_body", null},
                    { "note_body", "this is a note body" }

                },
                user_id: user_id
            );
        }

        private HttpRequest getTagRequest()
        {
            List<string> tags = new List<string>();
            tags.Add("tag1");
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "user_id", user_id },
                    { "title", "CreatePostTest.cs tagRequest" },
                    { "question_body", null},
                    { "note_body", "this is a note body" },
                    { "tags", tags }
                },
                user_id: user_id
            );
        }

        private HttpRequest getRequestbad()
        {
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {

                    { "title", "this is a title for note_body" },
                    { "question_body", null},
                    { "note_body", "this is a note body" }
                }
            );
        }

        private HttpRequest getRequestbad2()
        {
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {

                    { "user_id", user_id },
                    { "title", "this is a title for note_body" }
                },
                user_id: user_id
            );
        }

        private HttpRequest getRequestbad3()
        {
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {

                    { "user_id", user_id },
                    { "question_body", null},
                    { "note_body", "this is a note body" }
                },
                user_id: user_id
            );
        }

        [Fact]
        public async System.Threading.Tasks.Task TestCreateQuestionPost()
        {
            // Insert a question 
            var response = (OkObjectResult)await CreatePostFunction.Run(getQuestionRequest(), logger, context);
            object body = response.Value;
            Console.Write(body);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(true, TestUtils.GetField(body, "success"));
            //Assert.Equal(33, TestUtils.GetField(body, "res.user_id"));
            //Assert.Equal("b", TestUtils.GetField(body, "res.username"));
            Assert.Equal("this is a title for question_body", TestUtils.GetField(body, "res.title"));
            Assert.Equal("this is a question_body", TestUtils.GetField(body, "res.content"));
        }

        [Fact]
        public async System.Threading.Tasks.Task TestCreateNotePost()
        {
            // Insert a note 
            var response = (OkObjectResult)await CreatePostFunction.Run(getNoteRequest(), logger, context);
            object body = response.Value;
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(true, TestUtils.GetField(body, "success"));
            //Assert.Equal(33, TestUtils.GetField(body, "res.user_id"));
            //Assert.Equal("b", TestUtils.GetField(body, "res.username"));
            Assert.Equal("this is a title for note_body", TestUtils.GetField(body, "res.title"));
            Assert.Equal("this is a note body", TestUtils.GetField(body, "res.content"));
        }

        [Fact]
        public async System.Threading.Tasks.Task TestTagCreatePost()
        {
            var response = (OkObjectResult)await CreatePostFunction.Run(getTagRequest(), logger, context);
            object body = response.Value;
            List<string> tags = (List<string>)TestUtils.GetField(body, "res.tags");
            Assert.Equal("tag1", tags[0]);
        }

        [Fact]
        public async System.Threading.Tasks.Task TestCreatePostFail()
        {

            // user_id is null 
            var result = (UnauthorizedResult)await CreatePostFunction.Run(getRequestbad(), logger, context);
            Assert.Equal(401, result.StatusCode);

            // content_body is null 
            var result2 = (BadRequestResult)await CreateCommentFunction.Run(getRequestbad2(), logger);
            Assert.Equal(400, result2.StatusCode);

            // title is null
            var result3 = (BadRequestResult)await CreateCommentFunction.Run(getRequestbad3(), logger);
            Assert.Equal(400, result3.StatusCode);
        }
    }



}



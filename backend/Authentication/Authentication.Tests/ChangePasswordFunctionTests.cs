using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Authentication.Tests
{
    public class ChangePasswordFunctionTests
    {
        private Credentials user = new Credentials
        {
            status = "user",
            username = "Wexler",
            user_id = 1,
        };
        private string userJwt = "";
        private string oldPassword = "password";
        private string newPassword = "hellow0rld";

        public ChangePasswordFunctionTests()
        {
            // Create a valid JWT for the user.
            userJwt = new TokenIssuer().IssueTokenForUser(user);
            // Set up the mock database.
            var mockDB = new Mock<IDbAdapter>();
            var encryptedOldPassword = PasswordEncryption.Encrypt(oldPassword);
            mockDB.Setup(_ => _.LookupPasswordByUserId(user.user_id)).Returns(encryptedOldPassword);
            var encryptedNewPassword = PasswordEncryption.Encrypt(newPassword);
            mockDB.Setup(_ => _.UpdateCredentialsEntry(user.user_id, encryptedNewPassword));
            ChangePasswordFunction.db = mockDB.Object;

        }
        private readonly ILogger logger = TestFactory.CreateLogger();

        private HttpRequest createChangePasswordRequest(string currentPass, string newPass)
        {
            Dictionary<string, string> requestBody = new Dictionary<string, string>
            {
                ["currentPassword"] = currentPass,
                ["newPassword"] = newPass,
            };
            var request = TestFactory.CreateHttpPostRequestWithDictionary(requestBody);
            // Add the proper authorization to the request.
            request.Headers[Constants.TOKEN_KEY] = userJwt;
            return request;
        }

        /// <summary>
        /// This test verifies that a request with the correct current password successfully
        /// updates the user's password to the new password.
        /// </summary>
        [Fact]
        public async void Valid_request_should_return_OK()
        {
            var request = createChangePasswordRequest(oldPassword, newPassword);
            var response = (OkObjectResult)await ChangePasswordFunction.Run(request, logger);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("OK", response.Value);
        }


        /// <summary>
        /// This test verifies that requests that are missing required fields in the body
        /// return a Bad Request response.
        /// </summary>
        [Fact]
        public async void Missing_fields_should_return_bad_request()
        {
            // Create HTTP requests that are missing one field.
            var oldPassRequest = TestFactory.CreateHttpPostRequest("currentPassword", oldPassword);
            var newPassRequest = TestFactory.CreateHttpPostRequest("newPassword", newPassword);
            // Verify that both requests result in 400 status codes.
            var response = (BadRequestObjectResult)await ChangePasswordFunction.Run(oldPassRequest, logger);
            Assert.Equal(400, response.StatusCode);
            response = (BadRequestObjectResult)await ChangePasswordFunction.Run(newPassRequest, logger);
            Assert.Equal(400, response.StatusCode);
        }

        /// <summary>
        /// This test verifies that requests to change the user's password to an empty string
        /// returns a Bad Request response.
        /// </summary>
        [Fact]
        public async void Empty_new_password_should_return_bad_request()
        {
            var request = createChangePasswordRequest(oldPassword, "");
            var response = (BadRequestObjectResult)await ChangePasswordFunction.Run(request, logger);
            Assert.Equal(400, response.StatusCode);
        }

        /// <summary>
        /// This test verifies that requests to change the user's password to a string that doesn't meet
        /// the requirements returns a Bad Request response.
        /// </summary>
        [Fact]
        public async void Bad_new_password_should_return_bad_request_1()
        {
            var request = createChangePasswordRequest(oldPassword, "password");
            var response = (BadRequestObjectResult)await ChangePasswordFunction.Run(request, logger);
            Assert.Equal(400, response.StatusCode);
        }

        /// <summary>
        /// This test verifies that requests to change the user's password to a string that doesn't meet
        /// the requirements returns a Bad Request response.
        /// </summary>
        [Fact]
        public async void Bad_new_password_should_return_bad_request_2()
        {
            var request = createChangePasswordRequest(oldPassword, "12345678");
            var response = (BadRequestObjectResult)await ChangePasswordFunction.Run(request, logger);
            Assert.Equal(400, response.StatusCode);
        }

        /// <summary>
        /// This test verifies that requests to change the user's password to a string that doesn't meet
        /// the requirements returns a Bad Request response.
        /// </summary>
        [Fact]
        public async void Bad_new_password_should_return_bad_request_3()
        {
            var request = createChangePasswordRequest(oldPassword, "a123");
            var response = (BadRequestObjectResult)await ChangePasswordFunction.Run(request, logger);
            Assert.Equal(400, response.StatusCode);
        }

        /// <summary>
        /// This test verifies that requests to change the user's password to a string that doesn't meet
        /// the requirements returns a Bad Request response.
        /// </summary>
        [Fact]
        public async void Bad_new_password_should_return_bad_request_4()
        {
            var request = createChangePasswordRequest(oldPassword, "pass word");
            var response = (BadRequestObjectResult)await ChangePasswordFunction.Run(request, logger);
            Assert.Equal(400, response.StatusCode);
        }

        /// <summary>
        /// This test verifies that requests that do not have a JWT token return an
        /// unauthorized response.
        /// </summary>
        [Fact]
        public async void Missing_JWT_request_should_should_return_unauthorized()
        {
            var request = createChangePasswordRequest(oldPassword, newPassword);
            // Remove the token from the request headers.
            request.Headers.Remove(Constants.TOKEN_KEY);
            var response = (UnauthorizedResult)await ChangePasswordFunction.Run(request, logger);
            Assert.Equal(401, response.StatusCode);
        }

        /// <summary>
        /// This test verifies that requests with a bad JWT return an unauthorized resonse.
        /// </summary>
        [Fact]
        public async void Bad_JWT_request_should_should_return_unauthorized()
        {
            var request = createChangePasswordRequest(oldPassword, newPassword);
            // Overwrite the token header with a corrupted token.
            request.Headers[Constants.TOKEN_KEY] = userJwt + "abc";
            var response = (UnauthorizedResult)await ChangePasswordFunction.Run(request, logger);
            Assert.Equal(401, response.StatusCode);
        }

        // This test 
        [Fact]
        public async void Incorrect_current_password_should_return_unauthorized()
        {
            var request = createChangePasswordRequest(oldPassword + "abc", newPassword);
            var response = (UnauthorizedResult)await ChangePasswordFunction.Run(request, logger);
            Assert.Equal(401, response.StatusCode);
        }
    }
}

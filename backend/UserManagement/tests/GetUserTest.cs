using Xunit;
using UserManagement;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;

namespace UserManagementTest
{
    public class GetUserTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();
        public GetUserTest()
        {
            var mock = new Mock<IDbAdapter>();
            mock.Setup(_ => _.GetUser(It.IsAny<int>())).Returns(new UserProfile());
            mock.Setup(_ => _.GetUser(It.Is<string>(x => x.Equals("Loading")))).Returns(new UserProfile());
            mock.Setup(_ => _.GetUser(It.Is<string>(x => x.Equals("Loading2")))).Returns(() => {
                UserProfile user = new UserProfile();
                user.user_id = 1;
                user.username = "Loading2";
                return user;
            });
            mock.Setup(_=>_.GetUserPrefs(It.IsAny<int>())).Returns(new UserPrefs(true, true, false, false, false));
            GetUserFunction.setDBAdapter(mock.Object);
            GetUserFunction.logger = TestFactory.CreateLoggingAdapter();
        }

        [Fact]
        public void Get_User_No_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = GetUserFunction.Run(request, 0, logger);
            Assert.Equal(401, ((IStatusCodeActionResult)response).StatusCode);
            // object res = ((OkObjectResult)response).Value;
            // object user = res.GetType().GetProperty("res").GetValue(res, null);
            // Assert.Null(user?.GetType().GetProperty("country").GetValue(user, null));
            // Assert.Null(user?.GetType().GetProperty("year").GetValue(user, null));
            // Assert.Null(user?.GetType().GetProperty("college").GetValue(user, null));
            // Assert.Equal("Loading", user?.GetType().GetProperty("email").GetValue(user, null));
            // Assert.Equal("Loading", user?.GetType().GetProperty("phone_number").GetValue(user, null));
        }


        [Fact]
        public void Get_User_No_Route_Param_With_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);
            var response = GetUserFunction.Run(request, null, logger);
            Assert.Equal(200, ((IStatusCodeActionResult) response).StatusCode);
            object res = ((OkObjectResult)response).Value;
            object user = res.GetType().GetProperty("res").GetValue(res, null);
            Assert.Equal("Loading", user?.GetType().GetProperty("country").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("year").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("college").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("email").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("phone_number").GetValue(user, null));
        }

        [Fact]
        public void Get_User_Right_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);
            var response = GetUserFunction.Run(request, 0, logger);
            Assert.Equal(200, ((IStatusCodeActionResult) response).StatusCode);
            object res = ((OkObjectResult)response).Value;
            object user = res.GetType().GetProperty("res").GetValue(res, null);
            Assert.Equal("Loading", user?.GetType().GetProperty("country").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("year").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("college").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("email").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("phone_number").GetValue(user, null));
        }

        [Fact]
        public void Get_User_Wrong_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);
            var response = GetUserFunction.Run(request, 1, logger);
            Assert.Equal(200, ((IStatusCodeActionResult)response).StatusCode);
            object res = ((OkObjectResult)response).Value;
            object user = res.GetType().GetProperty("res").GetValue(res, null);
            Assert.Null(user?.GetType().GetProperty("country").GetValue(user, null));
            Assert.Null(user?.GetType().GetProperty("year").GetValue(user, null));
            Assert.Null(user?.GetType().GetProperty("college").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("email").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("phone_number").GetValue(user, null));
        }

        [Fact]
        public void Get_User_Wrong_JWT_But_Admin()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.ADMIN_JWT);
            var response = GetUserFunction.Run(request, 1, logger);
            Assert.Equal(200, ((IStatusCodeActionResult)response).StatusCode);
            object res = ((OkObjectResult)response).Value;
            object user = res.GetType().GetProperty("res").GetValue(res, null);
            Assert.Equal("Loading", user?.GetType().GetProperty("country").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("year").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("college").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("email").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("phone_number").GetValue(user, null));
        }

        [Fact]
        public void Get_User_Malformed_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.MALFORMED_JWT);
            var response = (IStatusCodeActionResult)GetUserFunction.Run(request, 0, logger);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public void Get_User_Expired_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.EXPIRED_JWT);
            var response = (IStatusCodeActionResult)GetUserFunction.Run(request, 0, logger);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public void Get_User_Wrong_Secret_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.WRONG_SECRET_JWT);
            var response = (IStatusCodeActionResult)GetUserFunction.Run(request, 0, logger);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public void Get_User_No_Route_Param_No_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = (IStatusCodeActionResult)GetUserFunction.Run(request, null, logger);
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public void Get_User_No_JWT_With_Username()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = GetUserFunction.Run2(request, "Loading", logger);
            Assert.Equal(401, ((IStatusCodeActionResult)response).StatusCode);
            // object res = ((OkObjectResult)response).Value;
            // object user = res.GetType().GetProperty("res").GetValue(res, null);
            // Assert.Null(user?.GetType().GetProperty("country").GetValue(user, null));
            // Assert.Null(user?.GetType().GetProperty("year").GetValue(user, null));
            // Assert.Null(user?.GetType().GetProperty("college").GetValue(user, null));
            // Assert.Equal("Loading", user?.GetType().GetProperty("email").GetValue(user, null));
            // Assert.Equal("Loading", user?.GetType().GetProperty("phone_number").GetValue(user, null));
        }

        [Fact]
        public void Get_User_Right_JWT_With_Username()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);
            var response = GetUserFunction.Run2(request, "Loading", logger);
            Assert.Equal(200, ((IStatusCodeActionResult)response).StatusCode);
            object res = ((OkObjectResult)response).Value;
            object user = res.GetType().GetProperty("res").GetValue(res, null);
            Assert.Equal("Loading", user?.GetType().GetProperty("country").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("year").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("college").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("email").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("phone_number").GetValue(user, null));
        }

        [Fact]
        public void Get_User_Wrong_JWT_With_Username()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);
            var response = GetUserFunction.Run2(request, "Loading2", logger);
            Assert.Equal(200, ((IStatusCodeActionResult)response).StatusCode);
            object res = ((OkObjectResult)response).Value;
            object user = res.GetType().GetProperty("res").GetValue(res, null);
            Assert.Null(user?.GetType().GetProperty("country").GetValue(user, null));
            Assert.Null(user?.GetType().GetProperty("year").GetValue(user, null));
            Assert.Null(user?.GetType().GetProperty("college").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("email").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("phone_number").GetValue(user, null));
        }

        [Fact]
        public void Get_User_Wrong_JWT_With_Username_But_Admin()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.ADMIN_JWT);
            var response = GetUserFunction.Run2(request, "Loading2", logger);
            Assert.Equal(200, ((IStatusCodeActionResult)response).StatusCode);
            object res = ((OkObjectResult)response).Value;
            object user = res.GetType().GetProperty("res").GetValue(res, null);
            Assert.Equal("Loading", user?.GetType().GetProperty("country").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("year").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("college").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("email").GetValue(user, null));
            Assert.Equal("Loading", user?.GetType().GetProperty("phone_number").GetValue(user, null));
        }

        [Fact]
        public void Get_User_With_Username_Malformed_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.MALFORMED_JWT);
            var response = (IStatusCodeActionResult)GetUserFunction.Run2(request, "Loading", logger);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public void Get_User_With_Username_Expired_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.EXPIRED_JWT);
            var response = (IStatusCodeActionResult)GetUserFunction.Run2(request, "Loading", logger);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public void Get_User_With_Username_Wrong_Secret_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.WRONG_SECRET_JWT);
            var response = (IStatusCodeActionResult)GetUserFunction.Run2(request, "Loading", logger);
            Assert.Equal(400, response.StatusCode);
        }

    }
}

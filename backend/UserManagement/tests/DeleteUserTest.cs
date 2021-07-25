using Xunit;
using UserManagement;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;

namespace UserManagementTest
{
    public class DeleteUserTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        public DeleteUserTest()
        {
            var mock = new Mock<IDbAdapter>();
            mock.Setup(_ => _.DeleteUser(It.IsAny<int>())).Returns(1);
            DeleteUserFunction.setDBAdapter(mock.Object);
            DeleteUserFunction.logger = TestFactory.CreateLoggingAdapter();
        }

        [Fact]
        public void No_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = (IStatusCodeActionResult) DeleteUserFunction.Run(request, 19, logger);
            Assert.Equal(401, response.StatusCode);
        }
        
        [Fact]
        public void Empty_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, "");
            var response = (IStatusCodeActionResult) DeleteUserFunction.Run(request, 19, logger);
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public void Malformed_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.MALFORMED_JWT);
            var response = (IStatusCodeActionResult) DeleteUserFunction.Run(request, 19, logger);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public void Expired_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.EXPIRED_JWT);
            var response = (IStatusCodeActionResult) DeleteUserFunction.Run(request, 19, logger);
            Assert.Equal(400, response.StatusCode);       
        }

        [Fact]
        public void Wrong_Secret_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.WRONG_SECRET_JWT);
            var response = (IStatusCodeActionResult) DeleteUserFunction.Run(request, 19, logger);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public void Wrong_User_JWT_User_Role()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);
            var response = (IStatusCodeActionResult) DeleteUserFunction.Run(request, 19, logger);
            Assert.Equal(403, response.StatusCode);
        }

        [Fact]
        public void Wrong_User_JWT_Admin_Role()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.ADMIN_JWT);
            var response = (IStatusCodeActionResult) DeleteUserFunction.Run(request, 92, logger);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public void Right_User_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.ADMIN_JWT);
            var response = (IStatusCodeActionResult) DeleteUserFunction.Run(request, 0, logger);
            Assert.Equal(200, response.StatusCode);
        }
        
        [Fact]
        public void Only_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.ADMIN_JWT);
            var response = (IStatusCodeActionResult) DeleteUserFunction.Run(request, null, logger);
            Assert.Equal(200, response.StatusCode);
        }
    }
}

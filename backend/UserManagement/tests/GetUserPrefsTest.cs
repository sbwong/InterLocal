using Xunit;
using UserManagement;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;

namespace UserManagementTest
{
    public class GetUserPrefsTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();
        private static Mock<IDbAdapter> mock = new Mock<IDbAdapter>();
        public GetUserPrefsTest()
        {
            mock.Setup(_ => _.GetUserPrefs(It.IsAny<int>())).Returns(new UserPrefs(false, false, false, false, false));
            GetUserPrefs.db = mock.Object;
            GetUserPrefs.logger = TestFactory.CreateLoggingAdapter();
        }

        [Fact]
        public void No_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            var response = (IStatusCodeActionResult) GetUserPrefs.Run(request, 19, logger);
            Assert.Equal(401, response.StatusCode);
        }
        
        [Fact]
        public void Empty_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, "");
            var response = (IStatusCodeActionResult) GetUserPrefs.Run(request, 19, logger);
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public void Malformed_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.MALFORMED_JWT);
            var response = (IStatusCodeActionResult) GetUserPrefs.Run(request, 19, logger);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public void Expired_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.EXPIRED_JWT);
            var response = (IStatusCodeActionResult) GetUserPrefs.Run(request, 19, logger);
            Assert.Equal(400, response.StatusCode);       
        }

        [Fact]
        public void Wrong_Secret_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.WRONG_SECRET_JWT);
            var response = (IStatusCodeActionResult) GetUserPrefs.Run(request, 19, logger);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public void Different_User_JWT_User_Role()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);
            var response = (IStatusCodeActionResult) GetUserPrefs.Run(request, 19, logger);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public void Different_User_JWT_Admin_Role()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.ADMIN_JWT);
            var response = (IStatusCodeActionResult) GetUserPrefs.Run(request, 92, logger);
            mock.Verify(_ => _.GetUserPrefs(92), Times.AtLeastOnce());
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public void Same_User_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);
            var response = (IStatusCodeActionResult) GetUserPrefs.Run(request, 0, logger);
            Assert.Equal(200, response.StatusCode);
        }
        
        [Fact]
        public void Only_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.ADMIN_JWT);
            var response = (IStatusCodeActionResult) GetUserPrefs.Run(request, null, logger);
            Assert.Equal(200, response.StatusCode);
        }
    }
}

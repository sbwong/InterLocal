using Authentication;
using JWT.Builder;
using JWT.Algorithms;
using Xunit;
using System.Collections.Generic;
using System;
using JWT.Exceptions;

namespace Authentication.Tests
{
    public class TokenIssuerTests
    {
        private static TokenIssuer tokenIssuer = new TokenIssuer();

        // Creates a JWT token using a TokenIssuer.
        private static string create_JWT(string username, int user_id, string status)
        {
            return tokenIssuer.IssueTokenForUser(new Credentials { username = username, user_id = user_id, status = status});
        }

        // Decodes a JWT token using the given secret for signature verification.
        private static IDictionary<string, object> decode_JWT(string jwt, string secret)
        {
            var json = JwtBuilder.Create()
                     .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                     .WithSecret(secret)
                     .MustVerifySignature()
                     .Decode<IDictionary<string, object>>(jwt);
            return json;
        }

        // This test checks that JWT tokens created by TokenIssuer contain the correct claims.
        [Fact]
        public void JWT_tokens_should_contain_correct_claims()
        {
            var username = "Jane Doe";
            var user_id = 1;
            var status = "user";
            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            var token = create_JWT(username, user_id, status);
            var decoded_token = decode_JWT(token, Constants.SECRET_KEY);
            // Check that the decoded token includes username and role claims.
            Assert.Equal(username, decoded_token["username"]);
            Assert.Equal(status, decoded_token["role"]);
            // Check that the exp in the decoded token is in the future.
            Assert.True((long)decoded_token["exp"] > timestamp);

        }

        // This test checks that JWT signature verification fails with incorrect secrets.
        [Fact]
        public void JWT_signature_verification_should_fail_with_wrong_signature()
        {
            var token = create_JWT("Amir", 2, "user");
            try
            {
                decode_JWT(token, Constants.SECRET_KEY + "abc");
                // xUnit doesn't include an Assert.Fail call, workaround from here:
                // https://github.com/xunit/xunit/issues/2027
                Assert.True(false, "Successfuly decoded JWT with incorrect secret.");
            }
            catch (SignatureVerificationException exp)
            {
                Assert.Equal("Invalid signature", exp.Message);
            }
        }
    }
}

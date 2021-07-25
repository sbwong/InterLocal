using System;
using System.Text;
using Xunit;

namespace Authentication.Tests
{
    public class PasswordEncryptionTests
    {

        [Fact]
        public void Decrypted_password_should_match_plaintext()
        {
            string password = "thisIsAPassword";
            string encrypted = PasswordEncryption.Encrypt(password);
            Assert.True(PasswordEncryption.Verify(encrypted, password));
        }

        [Fact]
        public void Wrong_plaintext_password_should_fail_verification()
        {
            string password = "thisIsAPassword";
            string badPassword = "thisIsTheWrongPassword";
            string encrypted = PasswordEncryption.Encrypt(password);
            Assert.False(PasswordEncryption.Verify(encrypted, badPassword));
        }

        [Fact]
        public void Wrong_ciphertext_password_should_fail_verification()
        {
            string password = "thisIsAPassword";
            string encrypted = PasswordEncryption.Encrypt(password);
            StringBuilder strB = new StringBuilder(encrypted);
            strB[0] = 'M';
            string badEncrypted = strB.ToString();
            // Verify fails silently with exception "BCrypt.Net.SaltParseException : Invalid salt version"
            Assert.False(PasswordEncryption.Verify(badEncrypted, password));
            Assert.False(PasswordEncryption.Verify("bogus", password));
        }
    }
}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BC = BCrypt.Net.BCrypt;

// Password encryption process is inspired by Dropbox:
//  https://dropbox.tech/security/how-dropbox-securely-stores-your-passwords 
//  https://crypto.stackexchange.com/questions/42415/dropbox-password-security 
// When to use Base64 vs UTF8 encoding/decoding: https://stackoverflow.com/a/26472848
namespace Authentication
{
    public static class PasswordEncryption
    {
        private static readonly byte[] KEY = Encoding.UTF8.GetBytes(Constants.AES256_KEY);
        private static readonly byte[] IV = Encoding.UTF8.GetBytes(Constants.AES256_IV);

        /// <summary>
        /// Hashes, then encrypts a user supplied password.
        /// </summary>
        /// <param name="plaintext">Password supplied by the user</param>
        public static string Encrypt(string plaintext)
        {
            // 1. Use SHA512 to transform the password into a hash value 512
            // bits long
            string sha512String = SHA512_ComputeHash(plaintext);

            // 2. Use bcrypt to hash the hash
            string bcryptHash = BCrypt_ComputeHash(sha512String);

            // 3. Use AES256 to encrypt the hash with pepper
            string encrypted = AES256_Encrypt(bcryptHash, KEY, IV);
            return encrypted;
        }

        /// <summary>
        /// Verifies the user supplied password against a ciphertext.
        /// </summary>
        /// <param name="cipher">Base64 encoded string, most likely from the DB</param>
        /// <param name="plaintext">Password supplied by the user</param>
        public static bool Verify(string cipher, string plaintext)
        {
            try
            {
                // 1. Use AES256 to decrypt
                string decrypted = AES256_Decrypt(cipher, KEY, IV);

                // 2. Transform the plaintext into a hash
                string sha512String = SHA512_ComputeHash(plaintext);

                // 3. Verify the hash against bcrypt
                bool verified = BCrypt_Verify(sha512String, decrypted);
                return verified;
            } catch {
                return false;
            }
        }

        // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha512managed?view=net-5.0
        /// <param name="plaintext">Text to hash (most likely the plain text password supplied by the user)</param>
        private static string SHA512_ComputeHash(string plaintext)
        {
            byte[] data = Encoding.UTF8.GetBytes(plaintext);
            SHA512 shaM = new SHA512Managed();
            byte[] sha512Hash = shaM.ComputeHash(data);
            string sha512String = Encoding.UTF8.GetString(sha512Hash);
            return sha512String;
        }

        // https://github.com/BcryptNet/bcrypt.net
        /// This library generates the salt for you
        /// <param name="text">Text to hash (most likely a SHA512 hash)</param>
        private static string BCrypt_ComputeHash(string text)
        {
            return BC.HashPassword(text);
        }

        /// <param name="text">Text to hash (most likely a BCrypt hash)</param>
        /// <param name="key">Secret key (most likely the pepper, a secret key common to all hashes)</param>
        /// <param name="iv">Initialization vector (most likely the pepper, a secret key common to all hashes)</param>
        private static string AES256_Encrypt(string text, byte[] key, byte[] iv)
        {
            byte[] encrypted;
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(text);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        /// <param name="plaintext">Text to hash (most likely a SHA512 hash of user supplied password)</param>
        /// <param name="hash">A BCrypt hash (most likely the result of decrypting the ciphertext stored in the DB)</param>
        private static bool BCrypt_Verify(string plaintext, string hash)
        {
            return BC.Verify(plaintext, hash);
        }

        /// <param name="cipherText">Text to decrypt (most likely the Base64 encoded string stored in the DB)</param>
        /// <param name="key">Secret key (most likely the pepper, a secret key common to all hashes)</param>
        /// <param name="iv">Initialization vector (most likely the pepper, a secret key common to all hashes)</param>
        private static string AES256_Decrypt(string cipherText, byte[] key, byte[] iv)
        {
            byte[] cipher = Convert.FromBase64String(cipherText);
            string decrypted;
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipher))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            decrypted = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return decrypted;
        }
    }
}
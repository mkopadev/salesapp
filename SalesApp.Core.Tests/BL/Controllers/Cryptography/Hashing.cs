using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SalesApp.Core.BL.Controllers.Security;

namespace SalesApp.Core.Tests.BL.Controllers.Cryptography
{
    public class Hashing : IHashing
    {
        public byte[] GetBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        /// <summary>
        ///     Hashes the user password.
        /// </summary>
        /// /// <param name="username">The user's username.</param>
        /// <param name="password">The user's plain text password.</param>
        /// <returns>The hashed passowrd</returns>
        public string HashPassword(string username, string password)
        {
            // ResponseText is salt + PIN
            string salt = this.Right(username, 5);
            string text = password + salt;

            string hash = this.HashSha256(text);
            return hash;
        }

        /// <summary>
        ///     Generates a hash string of text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The hash</returns>
        public string HashSha256(string text)
        {
            byte[] hash;
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using (var sha256 = new SHA256Managed())
            {
                hash = sha256.ComputeHash(bytes);
            }

            return hash.Aggregate(string.Empty, (current, x) => current + string.Format("{0:x2}", x));
        }

        /// <summary>
        ///    Extracts rightmost characters of a string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="length">The length.</param>
        /// <returns>right</returns>
        private string Right(string source, int length)
        {
            if (length >= source.Length)
            {
                return source;
            }

            return source.Substring(source.Length - length);
        }
    }
}
namespace SalesApp.Core.BL.Controllers.Security
{
    public interface IHashing
    {
        /// <summary>
        ///     Hashes the user password.
        /// </summary>
        /// /// <param name="username">The user's username.</param>
        /// <param name="password">The user's plain text password.</param>
        /// <returns>The hashed password</returns>
        string HashPassword(string username, string password);

        /// <summary>
        ///     Generates a hash string of text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The sha-256 encoding of the given string</returns>
        string HashSha256(string text);

        byte[] GetBytes(string str);
    }
}
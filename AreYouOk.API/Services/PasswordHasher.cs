namespace AreYouOk.API.Services;

/// <summary>
/// Service for hashing and verifying passwords
/// </summary>
public class PasswordHasher
{
    /// <summary>
    /// Hash a password using BCrypt
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verify a password against a hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Password hash</param>
    /// <returns>True if password matches hash</returns>
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

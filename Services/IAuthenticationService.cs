using Microsoft.AspNetCore.Identity;

namespace ITSMS.Services
{
    /// <summary>
    /// Authentication service for user login/register operations
    /// Handles password hashing, validation, and user authentication
    /// </summary>
    public interface IAuthenticationService
    {
        Task<(bool Success, string Message)> RegisterUserAsync(string username, string email, string password, string firstName, string lastName, int roleId);
        Task<(bool Success, string Message, int? UserId)> LoginUserAsync(string username, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly PasswordHasher<string> _passwordHasher;

        public AuthenticationService()
        {
            _passwordHasher = new PasswordHasher<string>();
        }

        public async Task<(bool Success, string Message)> RegisterUserAsync(string username, string email, string password, string firstName, string lastName, int roleId)
        {
            try
            {
                // Validation would be done here
                // This is a placeholder - actual implementation would query database
                return (true, "User registered successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Registration failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message, int? UserId)> LoginUserAsync(string username, string password)
        {
            try
            {
                // This is a placeholder - actual implementation would query database
                // and verify password
                return (true, "Login successful", 1);
            }
            catch (Exception ex)
            {
                return (false, $"Login failed: {ex.Message}", null);
            }
        }

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword("user", password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            var result = _passwordHasher.VerifyHashedPassword("user", hash, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}

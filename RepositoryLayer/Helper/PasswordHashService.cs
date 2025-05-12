using System.Security.Cryptography;

namespace RepositoryLayer.Helper
{
    public class PasswordHashService
    {
        private const int SaltSize = 16; // 128-bit salt
        private const int KeySize = 32; // 256-bit hash
        private const int Iterations = 10000;
        public string HashPassword(string userPassword)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);
            var key = new Rfc2898DeriveBytes(userPassword, salt, Iterations, HashAlgorithmName.SHA256).GetBytes(KeySize);
            var hashBytes = new byte[SaltSize + KeySize];
            Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltSize);
            Buffer.BlockCopy(key, 0, hashBytes, SaltSize, KeySize);
            return Convert.ToBase64String(hashBytes);
        }
        public bool VerifyPassword(string userPassword, string hashedPassword)
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);
            var salt = new byte[SaltSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);
            var key = new Rfc2898DeriveBytes(userPassword, salt, Iterations, HashAlgorithmName.SHA256).GetBytes(KeySize);
            for (int i = 0; i < KeySize; i++)
            {
                if (hashBytes[i + SaltSize] != key[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}

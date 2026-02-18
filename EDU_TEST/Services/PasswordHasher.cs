using System.Security.Cryptography;
using System.Text;

namespace EDU_TEST.Services
{
    public class PasswordHasher
    {
        public string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
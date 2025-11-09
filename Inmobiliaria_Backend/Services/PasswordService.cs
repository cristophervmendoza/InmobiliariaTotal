using BCryptNet = BCrypt.Net.BCrypt;

namespace Inmobiliaria_Backend.Services
{
    public interface IPasswordService
    {
        string Hash(string plain);
        bool Verify(string plain, string hash);
        bool NeedsRehash(string hash);
        bool LooksLikeBCrypt(string hash);
    }

    public class PasswordService : IPasswordService
    {
        private const int WorkFactor = 12;

        public string Hash(string plain)
            => BCryptNet.HashPassword(plain, workFactor: WorkFactor);

        public bool Verify(string plain, string hash)
        {
            if (!LooksLikeBCrypt(hash)) return false;
            return BCryptNet.Verify(plain, hash);
        }

        public bool NeedsRehash(string hash)
            => LooksLikeBCrypt(hash) && BCryptNet.PasswordNeedsRehash(hash, WorkFactor);

        public bool LooksLikeBCrypt(string hash)
            => !string.IsNullOrWhiteSpace(hash)
               && (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"))
               && hash.Length >= 60 && hash.Length <= 100;
    }
}

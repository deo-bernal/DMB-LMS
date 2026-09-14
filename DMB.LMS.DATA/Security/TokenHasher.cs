using System.Security.Cryptography;
using System.Text;

namespace Dmb.Lms.Data.Security;

public static class TokenHasher
{
    public static string Hash(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    public static string CreateRawToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }
}

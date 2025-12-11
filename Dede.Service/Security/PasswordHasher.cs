// Dede.Service/Security/PasswordHasher.cs

using System.Security.Cryptography;

namespace Dede.Service.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        using var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256);
        var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
        var salt = Convert.ToBase64String(algorithm.Salt);
        return $"{Iterations}.{salt}.{key}";
    }

    public static bool Check(string hash, string password)
    {
        var parts = hash.Split('.', 3);
        var iterations = Convert.ToInt32(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var key = Convert.FromBase64String(parts[2]);

        using var algorithm = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var keyToCheck = algorithm.GetBytes(KeySize);

        return CryptographicOperations.FixedTimeEquals(keyToCheck, key);
    }
}
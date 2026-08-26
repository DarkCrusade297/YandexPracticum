using System.Security.Cryptography;

namespace User.Application.Services;

public class PasswordService : IPasswordService
{
    private const string Scheme = "pbkdf2-sha256";
    private const int IterationCount = 600000;
    private const int MaximumAcceptedIterationCount = 1000000;
    private const int SaltSize = 16;
    private const int DerivedKeySize = 32;

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var derivedKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            IterationCount,
            HashAlgorithmName.SHA256,
            DerivedKeySize);

        return string.Join(
            '$',
            Scheme,
            IterationCount,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(derivedKey));
    }

    public bool Verify(string password, string passwordHash)
    {
        if (password is null || string.IsNullOrWhiteSpace(passwordHash)) return false;

        if (!TryParsePbkdf2Hash(passwordHash, out var iterations, out var salt, out var expectedKey))
            return false;

        var actualKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }

    private static bool TryParsePbkdf2Hash(
        string passwordHash,
        out int iterations,
        out byte[] salt,
        out byte[] derivedKey)
    {
        iterations = 0;
        salt = [];
        derivedKey = [];

        var parts = passwordHash.Split('$');
        if (parts.Length != 4 ||
            parts[0] != Scheme ||
            !int.TryParse(parts[1], out iterations) ||
            iterations <= 0 ||
            iterations > MaximumAcceptedIterationCount)
            return false;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            derivedKey = Convert.FromBase64String(parts[3]);
            return salt.Length == SaltSize && derivedKey.Length == DerivedKeySize;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

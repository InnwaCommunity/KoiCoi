using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Operational.Encrypt;


public class SaltedHash
{
    public const int SALT_BYTE_SIZE = 24;
    public const int HASH_BYTE_SIZE = 24;
    public const int PBKDF2_ITERATIONS = 1000;

    public string Hash { get; private set; }
    public string Salt { get; private set; }

    public SaltedHash(string password)
    {
        // var saltBytes = new byte[SALT_BYTE_SIZE];
        // using (var provider = new RNGCryptoServiceProvider())
        //     provider.GetNonZeroBytes(saltBytes);
        // Salt = Convert.ToBase64String(saltBytes);
        // Hash = ComputeHash(Salt, password);
        Salt = GenerateSalt();
        Hash = ComputeHash(Salt, password);

        /*
        "System.Runtime.Serialization.Primitives": "4.0.11-beta-23302",
        "System.Security.Cryptography.Algorithms": "4.0.0-beta-23225",
        "System.Security.Cryptography.RandomNumberGenerator": "4.0.0-beta-23225"
        */
        //  byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password + salt);
        // SHA256Managed sha256hashstring = new SHA256Managed();
        // byte[] hash = sha256hashstring.ComputeHash(bytes);
        // return Convert.ToBase64String(bytes);
    }

    public static string GenerateSalt()
    {
        // RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
        // byte[] salt2 = new byte[SALT_BYTE_SIZE];
        // csprng.GetBytes(salt2);
        // return Convert.ToBase64String(salt2);

        var rng = RandomNumberGenerator.Create();
        var buff = new byte[SALT_BYTE_SIZE];
        rng.GetBytes(buff);
        return Convert.ToBase64String(buff);
    }

    public static string ComputeHash(string salt, string password)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, PBKDF2_ITERATIONS))
            return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(HASH_BYTE_SIZE));
    }

    public static bool Verify(string salt, string hash, string password)
    {
        return hash == ComputeHash(salt, password);
    }
}



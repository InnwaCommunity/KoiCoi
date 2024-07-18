
using System;
using System.Security.Cryptography;
using System.Text;

namespace KoiCoi.Operational.Encrypt;


public class RandomPassword
{
    public string CreatePassword(int length, int numberOfNonAlphanumericCharacters)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        const string nonAlphaNumericChars = "!@#$%^&*()_-+=<>?";

        if (numberOfNonAlphanumericCharacters > length)
        {
            throw new ArgumentException("Number of non-alphanumeric characters cannot exceed the total length.");
        }

        StringBuilder res = new StringBuilder();
        byte[] buffer = new byte[4];

        // Add non-alphanumeric characters
        for (int i = 0; i < numberOfNonAlphanumericCharacters; i++)
        {
            RandomNumberGenerator.Fill(buffer);
            uint num = BitConverter.ToUInt32(buffer, 0);
            res.Append(nonAlphaNumericChars[(int)(num % nonAlphaNumericChars.Length)]);
        }

        // Add remaining alphanumeric characters
        for (int i = numberOfNonAlphanumericCharacters; i < length; i++)
        {
            RandomNumberGenerator.Fill(buffer);
            uint num = BitConverter.ToUInt32(buffer, 0);
            res.Append(validChars[(int)(num % validChars.Length)]);
        }

        // Shuffle the result to mix the characters
        return Shuffle(res.ToString());
    }

    private string Shuffle(string str)
    {
        char[] array = str.ToCharArray();
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        int n = array.Length;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do rng.GetBytes(box); while (!(box[0] < n * (byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            char value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
        return new string(array);
    }
}


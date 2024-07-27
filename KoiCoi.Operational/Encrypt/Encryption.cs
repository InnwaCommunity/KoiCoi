
using System.Security.Cryptography;
using System.Text;

namespace KoiCoi.Operational.Encrypt;


public class Encryption
{
    private static string _EncryptionKey = "LGdeb@lTraining21lob@lTrvaining";//Startup.Configuration.GetSection("Encryption:EncryptionKey").Value;
    private static string _SaltKey = "InnwaKoi@88";// Startup.Configuration.GetSection("Encryption:EncryptionSalt").Value;
    private static string _ClientEncryptionKey = "TTRAINING001224GGWTT";//Startup.Configuration.GetSection("Encryption:ClientEncryptionKey").Value;
    private static string _ClientEncryptionSalt = "VITRAINING001222987";//Startup.Configuration.GetSection("Encryption:ClientEncryptionSalt").Value;

    public static string EncryptFileName(string FileName)
    {
        return Encrypt_CBC_256(FileName);
    }
    public static string DecryptFileName(string EncFileName)
    {
        return Decrypt_CBC_256(EncFileName);
    }
    public static string DecryptID(string cipherText, string EncryptKey)
    {
        return MySQL_AES_Decrypt_ECB_128(cipherText, EncryptKey);
    }
    public static string EncryptID(string plainText, string EncryptKey)
    {
        return MySQL_AES_Encrypt_ECB_128(plainText, EncryptKey);
    }
    private static string MySQL_AES_Decrypt_ECB_128(string cipherText, string EncryptKey)
    {
        if (cipherText == "")
            return "";

        string plainText = "";
        try
        {
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.KeySize = 128;
                aesAlg.BlockSize = 128;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = mkey(EncryptKey);
                aesAlg.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plainText;
        }
        catch (Exception ex)
        {
            Globalfunction.WriteSystemLog("MySQL_AES_Decrypt: " + ex.Message);
        }
        return plainText;
    }
    private static string MySQL_AES_Encrypt_ECB_128(string plainText, string EncryptKey)
    {
        if (String.IsNullOrEmpty(plainText) || String.IsNullOrEmpty(EncryptKey))
            return "";
        string cipherText = "";
        try
        {
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.KeySize = 128;
                aesAlg.BlockSize = 128;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = mkey(EncryptKey); //Encoding.UTF8.GetBytes(Iat);
                aesAlg.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                byte[] clearBytes = Encoding.UTF8.GetBytes(plainText);

                // Create a encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(clearBytes, 0, clearBytes.Length);
                    }
                    cipherText = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            return cipherText;
        }
        catch (Exception ex)
        {
            Globalfunction.WriteSystemLog("MySQL_AES_Encrypt: " + ex.Message);
        }
        return cipherText;
    }

    public static string MySQL_AES_Decrypt_ECB_TokenData(string cipherText, string EncryptKey)
    {
        if (cipherText == "")
            return "";

        string plainText = "";
        try
        {
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.KeySize = 128;
                aesAlg.BlockSize = 128;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = mkey(EncryptKey);
                aesAlg.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plainText;
        }
        catch (Exception ex)
        {
            Globalfunction.WriteSystemLog("MySQL_AES_Decrypt_ECB_TokenData: " + ex.Message);
        }
        return plainText;
    }

    public static string EncryptToken(string PlainText, string EncryptionKey, string _SaltKey = "")//Encrypt_CBC_256_Token
    {
        return Encrypt_CBC_256(PlainText, EncryptionKey, _SaltKey);
    }
    public static string DecryptToken(string cipherText, string EncryptionKey = "", string _SaltKey = "")
    {
        return Decrypt_CBC_256(cipherText, EncryptionKey, _SaltKey);
    }

    public static string EncryptPayslipPassword(string PlainText, string EncryptionKey, string _SaltKey = "")//Encrypt_CBC_256_Token
    {
        return Encrypt_CBC_256(PlainText, EncryptionKey, _SaltKey);
    }
    public static string DecryptPayslipPassword(string cipherText, string EncryptionKey = "", string _SaltKey = "")
    {
        return Decrypt_CBC_256(cipherText, EncryptionKey, _SaltKey);
    }

    public static bool VerifyPayslipPassword(string cipherText, string EncryptionKey = "", string _SaltKey = "", string password = "")
    {
        return password == Decrypt_CBC_256(cipherText, EncryptionKey, _SaltKey);
    }

    public static string Encrypt_CBC_256_URL(string PlainText, string EncryptionKey, string _SaltKey = "")
    {
        return Encrypt_CBC_256(PlainText, EncryptionKey, _SaltKey);
    }
    public static string Decrypt_CBC_256_URL(string cipherText, string EncryptionKey = "", string _SaltKey = "")
    {
        return Decrypt_CBC_256(cipherText, EncryptionKey, _SaltKey);
    }

    private static string Encrypt_CBC_256(string PlainText, string EncryptionKey = "", string _SaltKey = "")
    {

        if (PlainText == "")
            return "";
        if (EncryptionKey == null || EncryptionKey == "")
            return "";

        string encryptString = "";
        try
        {
            if (EncryptionKey.Trim() == "") EncryptionKey = _EncryptionKey;  // You can overwrite default enc key
            var bsaltkey = Encoding.UTF8.GetBytes(_SaltKey);
            byte[] clearBytes = Encoding.UTF8.GetBytes(PlainText);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, bsaltkey, 1000, HashAlgorithmName.SHA256);
                encryptor.Key = pdb.GetBytes(32);  //256 bit Key
                encryptor.IV = GenerateRandomBytes(16);
                encryptor.Mode = CipherMode.CBC;
                encryptor.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                    }
                    byte[] result = MergeArrays(encryptor.IV, ms.ToArray());  //append IV to cipher, so cipher length will longer
                    encryptString = Convert.ToBase64String(result);

                }
            }
            return encryptString;
        }
        catch (Exception ex)
        {
            Globalfunction.WriteSystemLog("Encrypt_CBC_256: " + ex.Message);
        }
        return encryptString;
    }

    private static string Decrypt_CBC_256(string cipherText, string EncryptionKey = "", string _SaltKey = "")
    {
        if (cipherText == "")
            return "";
        if (EncryptionKey == null || EncryptionKey == "")
            return "";
        string plainText = "";
        try
        {
            if (EncryptionKey.Trim() == "") EncryptionKey = _EncryptionKey;
            var bsaltkey = Encoding.UTF8.GetBytes(_SaltKey);

            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {

                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, bsaltkey, 1000, HashAlgorithmName.SHA256);
                encryptor.Mode = CipherMode.CBC;
                encryptor.Padding = PaddingMode.PKCS7;
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = cipherBytes.Take(16).ToArray();
                cipherBytes = cipherBytes.Skip(16).ToArray();

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    }
                    plainText = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            return plainText;
        }
        catch (Exception ex)
        {
            Globalfunction.WriteSystemLog("Decrypt_CBC_256: " + ex.Message + " Inputtext :" + cipherText);
        }
        return plainText;
    }

    private static byte[] GenerateRandomBytes(int numberOfBytes)
    {
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        var randomBytes = new byte[numberOfBytes];
        rng.GetBytes(randomBytes);
        return randomBytes;
    }


    private static byte[] MergeArrays(params byte[][] arrays)
    {
        var merged = new byte[arrays.Sum(a => a.Length)];
        var mergeIndex = 0;
        for (int i = 0; i < arrays.GetLength(0); i++)
        {
            arrays[i].CopyTo(merged, mergeIndex);
            mergeIndex += arrays[i].Length;
        }

        return merged;
    }




    private static byte[] mkey(string skey)
    {

        byte[] key = Encoding.UTF8.GetBytes(skey);
        byte[] k = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        for (int i = 0; i < key.Length; i++)
        {
            k[i % 16] = (byte)(k[i % 16] ^ key[i]);
        }

        return k;
    }

    public static string Encrypt_PDFKey(string PlainText, string EncryptionKey, string _SaltKey = "")
    {
        return Encrypt_CBC_256(PlainText, EncryptionKey, _SaltKey);
    }
    public static string Decrypt_PDFKey(string cipherText, string EncryptionKey = "", string _SaltKey = "")
    {
        return Decrypt_CBC_256(cipherText, EncryptionKey, _SaltKey);
    }

    public static string EncryptClientString(string PlainText, string EncryptionKey, string _SaltKey = "")//Encrypt_CBC_256_Token
    {
        return Encrypt_CBC_256(PlainText, EncryptionKey, _SaltKey);
    }
    public static string DecryptClientString(string cipherText, string EncryptionKey = "", string _SaltKey = "")
    {
        return DecryptClient_CBC_256(cipherText, EncryptionKey, _SaltKey);
    }
    private static string DecryptClient_CBC_256(string cipherText, string _ClientEncryptionKey = "", string _ClientEncryptionSalt = "")
    {
        if (cipherText == "")
            return "";

        string plainText = "";
        try
        {

            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                //byte[] ClientKey = Encoding.UTF8.GetBytes(_ClientEncryptionKey);
                byte[] ClientSalt = Encoding.UTF8.GetBytes(_ClientEncryptionSalt);

                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(_ClientEncryptionKey, ClientSalt, 1000, HashAlgorithmName.SHA256);
                encryptor.Mode = CipherMode.CBC;
                encryptor.Padding = PaddingMode.PKCS7;
                //encryptor.Padding = PaddingMode.None;
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = cipherBytes.Take(16).ToArray();
                cipherBytes = cipherBytes.Skip(16).ToArray();

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    }
                    plainText = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            return plainText;
        }
        catch (Exception ex)
        {
            Globalfunction.WriteSystemLog("DecryptClient_CBC_256: " + ex.Message);
        }
        return plainText;
    }
}


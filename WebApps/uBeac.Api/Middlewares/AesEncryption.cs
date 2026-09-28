using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;

namespace uBeac.Api.Middlewares
{
    public class AesEncryption
    {
        private readonly IConfiguration _configuration;
        public AesEncryption(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Encrypt(string plainText)
        {
            try
            {
                var key = Convert.FromBase64String(_configuration.GetValue<string>("AESKey"));
                var iv = Convert.FromBase64String(_configuration.GetValue<string>("AESIV"));

                if (string.IsNullOrEmpty(plainText))
                    return string.Empty;

                byte[] encrypted;

                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }

                return Convert.ToBase64String(encrypted);
            }
            catch (Exception)
            {
                return string.Empty;
            }            

        }

        public string Decrypt(string encodedText)
        {
            var cipherText = Convert.FromBase64String(encodedText);
            string plaintext = null;

            var key = Convert.FromBase64String(_configuration.GetValue<string>("AESKey"));
            var iv = Convert.FromBase64String(_configuration.GetValue<string>("AESIV"));

            if (string.IsNullOrEmpty(encodedText))
                return string.Empty;
            try
            {
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                }
            }
            catch (Exception)
            {                
            }
            
            return plaintext;
        }
    }
}

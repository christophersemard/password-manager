using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Web.Client.Services
{
    public class EncryptionService
    {
        private static readonly int KeySize = 32; // 256-bit AES key
        private static readonly int IvSize = 16; // 128-bit IV for AES

        public static string Encrypt(string plainText, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV(); 
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText.Normalize(NormalizationForm.FormC));

            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            byte[] result = aes.IV.Concat(encryptedBytes).ToArray();
            string base64Result = Convert.ToBase64String(result);

            return base64Result;  // Retourne directement un format prêt à être stocké
        }



        public static string Decrypt(byte[] encryptedData, byte[] key)
        {
            try
            {
                if (encryptedData.Length < 16)
                {
                    Console.WriteLine($"❌ Erreur : Données trop courtes pour un déchiffrement AES.");
                    return string.Empty;
                }

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = encryptedData.Take(16).ToArray();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] encryptedBytes = encryptedData.Skip(16).ToArray();
  
                using var decryptor = aes.CreateDecryptor();
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                string decryptedText = Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');
                return decryptedText;
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"❌ Erreur de déchiffrement AES : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur inattendue dans Decrypt() : {ex.Message}");
            }

            return string.Empty;
        }

    }
}

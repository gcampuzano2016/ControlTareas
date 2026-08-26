using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CapaDato
{
    public class FileEncryptionService
    {
        private readonly byte[] _key;

        public FileEncryptionService()
        {
            string password = ConfigurationManager.AppSettings["EncryptionPassword"];
            string saltStr = ConfigurationManager.AppSettings["EncryptionSalt"];
            byte[] salt = Encoding.UTF8.GetBytes(saltStr);

            using (var keyGen = new Rfc2898DeriveBytes(password, salt, 100000))
            {
                _key = keyGen.GetBytes(32);
            }
        }

        public void EncryptFile(string inputPath, string outputPath)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();

                using (var outputStream = File.Create(outputPath))
                {
                    outputStream.Write(aes.IV, 0, aes.IV.Length);

                    using (var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (var inputStream = File.OpenRead(inputPath))
                        {
                            inputStream.CopyTo(cryptoStream);
                            cryptoStream.FlushFinalBlock();
                        }
                    }
                }
            }
        }

        public MemoryStream DecryptToMemory(string encryptedPath)
        {
            using (var inputStream = File.OpenRead(encryptedPath))
            {
                var iv = new byte[16];
                inputStream.Read(iv, 0, 16);

                using (var aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = iv;

                    using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        var memoryStream = new MemoryStream();
                        cryptoStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        return memoryStream;
                    }
                }
            }
        }
    }
}
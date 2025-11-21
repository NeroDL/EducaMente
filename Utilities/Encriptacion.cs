using Isopoh.Cryptography.Argon2;
using System.Security.Cryptography;
using System.Text;

namespace EducaMente.Utilities
{
    public class Encriptacion
    {
        private static string key;
        private static string iv;
        public static void CargarConfiguracion(IConfiguration configuration)
        {
            key = configuration.GetValue<string>("CrytographySettings:Key");
            iv = configuration.GetValue<string>("CrytographySettings:IV");
        }

        public static string Hashear(string password)
        {
            var salt = GenerateSalt(16);

            var config = new Argon2Config
            {
                Version = Argon2Version.Nineteen,
                TimeCost = 4,
                MemoryCost = 65536, // 64 MB
                Lanes = 2,
                Threads = Environment.ProcessorCount,
                Password = Encoding.UTF8.GetBytes(password),
                Salt = salt,
                HashLength = 32
            };

            using (var argon2 = new Argon2(config))
            using (var hash = argon2.Hash())
            {
                return config.EncodeString(hash.Buffer); // Usamos `.Buffer` para obtener los bytes
            }
        }

        public static bool VerificarHash(string password, string storedHash)
        {
            return Argon2.Verify(storedHash, password);
        }


        //Verifica si un valor es null antes de Hashear
        public static string HashearDifNull(string valor)
        {
            return string.IsNullOrEmpty(valor) ? valor : Hashear(valor);
        }

        private static byte[] GenerateSalt(int length)
        {
            var salt = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        // Método para encriptar un texto plano
        public static string Encriptar(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentNullException(nameof(plainText), "El texto no puede ser nulo o vacío.");
            }

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
            {
                throw new InvalidOperationException("La clave o el IV no están configurados.");
            }

            // Usamos AES para encriptar
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    // Devuelve el texto cifrado en Base64
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        public static bool EsBase64(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            Span<byte> buffer = new Span<byte>(new byte[texto.Length]);
            return Convert.TryFromBase64String(texto, buffer, out _);
        }

        public static string DesencriptarSiEsBase64(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            return EsBase64(texto) ? Desencriptar(texto) : texto;
        }

        // Método para desencriptar un texto cifrado
        public static string Desencriptar(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText), "El texto cifrado no puede ser nulo o vacío.");

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(key);
                    aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al desencriptar: {ex.Message}");
            }
        }
    }
}

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Chubb_Entity.Utils
{
    public class EncryptHelper
    {
        private static RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();

        public static string HashClave(string clave)
        {
            int saltSize = 128 / 8;
            var salt = new byte[saltSize];
            _randomNumberGenerator.GetBytes(salt);

            var subkey = KeyDerivation.Pbkdf2(
                password: clave,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            );

            var outputBytes = new byte[salt.Length + subkey.Length];
            Buffer.BlockCopy(salt, 0, outputBytes, 0, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, salt.Length, subkey.Length);

            return Convert.ToBase64String(outputBytes);
        }

        public static bool VerificarClave(string clave, string hashAlmacenado)
        {
            var decoded = Convert.FromBase64String(hashAlmacenado);

            int saltSize = 128 / 8;
            var salt = new byte[saltSize];
            Buffer.BlockCopy(decoded, 0, salt, 0, salt.Length);

            var storedSubkey = new byte[decoded.Length - salt.Length];
            Buffer.BlockCopy(decoded, salt.Length, storedSubkey, 0, storedSubkey.Length);

            var subkeyToCheck = KeyDerivation.Pbkdf2(
                password: clave,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            );

            return CryptographicOperations.FixedTimeEquals(storedSubkey, subkeyToCheck);
        }
    }
}

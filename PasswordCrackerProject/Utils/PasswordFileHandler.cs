using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    namespace Password_Cracker.Utils
    {
        class PasswordFileHandler
        {
            private static readonly Converter<char, byte> Converter = CharToByte;

            /// <summary>
            /// With this method you can make you own password file
            /// </summary>
            /// <param name="filename">Name of password file</param>
            /// <param name="usernames">List of usernames</param>
            /// <param name="passwords">List of passwords in clear text</param>
            /// <exception cref="ArgumentException">if usernames and passwords have different lengths</exception>
            public static void WritePasswordFile(String filename, String[] usernames, String[] passwords)
            {
                HashAlgorithm messageDigest = new SHA1CryptoServiceProvider();
                if (usernames.Length != passwords.Length)
                {
                    throw new ArgumentException("usernames and passwords must be same lengths");
                }

                using (FileStream fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    for (int i = 0; i < usernames.Length; i++)
                    {
                        byte[] passwordAsBytes = Array.ConvertAll(passwords[i].ToCharArray(), GetConverter());
                        byte[] encryptedPassword = messageDigest.ComputeHash(passwordAsBytes);
                        String line = usernames[i] + ":" + Convert.ToBase64String(encryptedPassword) + "\n";
                        sw.WriteLine(line);
                    }
                }
            }

            public static Converter<char, byte> GetConverter()
            {
                return Converter;
            }

            /// <summary>
            /// Converting a char to a byte can be done in many ways.
            /// This is one way ...
            /// </summary>
            /// <param name="ch"></param>
            /// <returns></returns>
            private static byte CharToByte(char ch)
            {
                return Convert.ToByte(ch);
            }
        }
    }
}
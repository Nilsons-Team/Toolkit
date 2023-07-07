using System.Text;
using System.Security.Cryptography;

namespace Toolkit_Client.Modules
{
    public class Encryption
    {
        public static string GetHashSHA256(string input)
        {
            string result = null;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytesInput = Encoding.UTF8.GetBytes(input);
                byte[] bytesOutput = new byte[32];
            
                int bytesWritten;
                bool computed = sha256.TryComputeHash(bytesInput, bytesOutput, out bytesWritten);
                if (computed && bytesWritten == 32)
                {
                    StringBuilder hashBuilder = new StringBuilder();

                    for (int i = 0; i < bytesWritten; i++)
                        hashBuilder.Append(bytesOutput[i].ToString("x2"));

                    result = hashBuilder.ToString();
                }
            }

            return result;
        }
    }
}

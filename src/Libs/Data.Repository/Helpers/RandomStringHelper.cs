using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Helpers
{
    public static class RandomStringHelper
    {
        public static string GetRandomAlphanumericString(int length)
        {
            const string alphanumericCharacters =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "0123456789";
            return GetRandomString(length, alphanumericCharacters);
        }

        public static string GetRandomNumericalString(int length)
        {
            const string numbericalCharacters = "0123456789";
            return GetRandomString(length, numbericalCharacters);
        }

        public static string GetRandomString(int length, IEnumerable<char> characterSet)
        {
            if (length < 0)
                throw new ArgumentException("length must not be negative", "length");
            if (length > int.MaxValue / 8) // 250 million chars ought to be enough for anybody
                throw new ArgumentException("length is too big", "length");
            if (characterSet == null)
                throw new ArgumentNullException("characterSet");
            var characterArray = characterSet.Distinct().ToArray();
            if (characterArray.Length == 0)
                throw new ArgumentException("characterSet must not be empty", "characterSet");

            var bytes = new byte[length * 8];
            new RNGCryptoServiceProvider().GetBytes(bytes);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                ulong value = BitConverter.ToUInt64(bytes, i * 8);
                result[i] = characterArray[value % (uint)characterArray.Length];
            }
            return new string(result);
        }

        public static string GetRandomIntString(int length)
        {
            if (length < 0)
                throw new ArgumentException("length must not be negative", "length");
            if (length > 18)
                throw new ArgumentException("length is too big", "length");

            Random rand = new Random();
            StringBuilder sb = new StringBuilder();
            bool useful = true, isFirstDigit = true;

            do
            {
                sb.Clear();
                long num = rand.NextInt64(0, long.MaxValue); Console.Write(num);
                for (int i = length; i > 0; i--)
                {
                    string oneDigit = Convert.ToString((int)(num % (int)Math.Pow(10, i) / Math.Pow(10, i - 1)));
                    useful &= !isFirstDigit || oneDigit[0] != '0';
                    if (useful)
                        sb.Append(oneDigit);
                    else
                        break;
                    isFirstDigit = false;
                }
            } while (!useful);

            return sb.ToString();
        }
    }
}

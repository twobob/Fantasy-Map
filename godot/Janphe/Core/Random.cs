using System;
using System.Security.Cryptography;
using System.Text;

namespace Janphe
{
    public partial class Random
    {
        private static Rander random { get; set; } = new Rander(0);

        private static int GetRandomSeed()
        {
            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);
            return BitConverter.ToInt32(bytes);
        }

        public static void Seed(int seed)
        {
            random = new Rander(seed);
        }

        public static void Seed(string seed)
        {
            if (int.TryParse(seed, out var ret))
            {
                Seed(ret);
                return;
            }

            var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(seed));
            Seed(BitConverter.ToInt32(bytes, 0));
        }

        public static int Next()
        {
            return random.Next();
        }
        public static int Next(int maxValue)
        {
            return random.Next(maxValue);
        }
        public static int Next(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }
        public static double NextDouble()
        {
            return random.NextDouble();
        }
    }
}

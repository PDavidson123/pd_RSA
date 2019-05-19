using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace RSA
{
    class Program
    {
        private static bool modszer;

        private static Random rn = new Random();
        private static bool exit = true;

        static void Main(string[] args)
        {

            while(exit)
            {
                //Console.Clear();
                Console.WriteLine("Petrik Dávid RSA");
                Console.WriteLine();

                /*Console.WriteLine("RSA kódolás/visszafejtés? (kod/vissza)");
                string temp = Console.ReadLine();
                if (temp == "kod")
                    modszer = true;
                else if (temp == "vissza")
                    modszer = false;*/

                //Console.WriteLine(SearchForPrime(0, 156412312344541));

                Console.WriteLine("Hány bites legyen a szám? 2^(beírandó szám)");
                int bitS = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Mi legyen a titkosítandó üzenet?");
                int mes = Convert.ToInt32(Console.ReadLine());
                RSA(bitS, mes);

                Console.WriteLine("Újra? (y)");
                string str = Console.ReadLine();
                if (str != "y")
                    exit = false;

            }
            

        }

        private static void RSA(int keySizeIn, BigInteger mes)
        {
            int keySize = keySizeIn;
            BigInteger m = mes;
            
            (var n, var e, var d, var p, var q) = GenerateRSAKeys(keySize);
            BigInteger c = Gyorshatvanyoz(m, e, n);
            Console.WriteLine(c + " a titkosított üzenet.");
            //BigInteger mfejt = Gyorshatvanyoz(c, d, n);

            BigInteger[] cs = new[] { Gyorshatvanyoz(c, d % (p - 1), p), Gyorshatvanyoz(c, d % (q - 1), q) };
            BigInteger[] ms = new[] { p, q };

            BigInteger mfejt = KinaiMaradekTetel(ms, cs);

            Console.WriteLine(mfejt + " a visszafejtett kulcs.");
        }

        private static (BigInteger n, BigInteger e, BigInteger d, BigInteger p, BigInteger q) GenerateRSAKeys(int bitSize)
        {
            BigInteger min = BigInteger.Pow(2, bitSize - 1);
            BigInteger max = BigInteger.Pow(2, bitSize) - 1;

            BigInteger p = SearchForPrime(min, max);
            BigInteger q;

            do
            {
                q = SearchForPrime(min, max);
            }
            while (q.Equals(p)); //Ne egyezzen meg a 2 prím
            
            
            BigInteger n = p * q;
            //BigInteger fin = ((p - 1) / EuklidesziAlgoritmus(p - 1, q - 1)) * (q - 1);
            BigInteger fin = (p - 1) * (q - 1);

            Console.WriteLine("A 2 prímszám: p = {0}, q={1} és n = {2}, lnko: {3}", p, q,n, EuklidesziAlgoritmus(p, q));
            
            BigInteger e;
            do
            {
                e = BigRandom(2, fin);
            }
            while (EuklidesziAlgoritmus(fin, e) != 1 || e == p || e == q);

            (var lnko, var x, var y) = KiterjesztettEuklidesziAlgoritmus(fin, e);
            BigInteger d = y;
            if (d < 0)
                d += fin;

            return (n, e, d, p, q);
        }

        private static BigInteger SearchForPrime(BigInteger min, BigInteger max)
        {
            while (true)
            {
                var n = BigRandom(min, max);
                if (MillerRabinTeszt(n) && FermatTeszt(n))
                {
                    return n;
                }
            }
        }

        private static bool MillerRabinTeszt(BigInteger number)
        {
            BigInteger d = number - 1;
            BigInteger s = 0;
            bool lehetPrim = true;

            while (d % 2 == 0)
            {
                ++s;
                d /= 2;
            }


            for (int i = 0; i < 8; ++i)
            {
                BigInteger baseNum = BigRandom(d);
                if (Gyorshatvanyoz(baseNum, d, number) != 1)
                {
                    bool talalat = false;

                    for (int r = 0; r < s && !talalat; ++r)
                    {
                        talalat |= Gyorshatvanyoz(baseNum, BigInteger.Pow(2, r) * d, number) == number - 1;
                    }

                    lehetPrim &= talalat;
                }
            }

            return lehetPrim;
        }

        private static BigInteger Gyorshatvanyoz(BigInteger alap, BigInteger kitevo, BigInteger modulus)
        {
            BigInteger congr = alap;
            int max = (int)Math.Ceiling(BigInteger.Log(kitevo, 2)); //az adott számot hány biten lehet eltárolni
            BigInteger solution = 1;
            for (int j = 0; j < max; ++j)
            {
                if (kitevo % 2 == 1)
                {
                    solution *= congr;
                    solution %= modulus;
                }
                kitevo /= 2;

                BigInteger.DivRem(BigInteger.Pow(congr, 2), modulus, out congr); //Ez visszatér az első és a második paraméter egész osztásával és a congr változóba belerakja a maradékot
            }

            return solution;
        }
        
        private static BigInteger BigRandomByBits(int bitSize)
        {
            BigInteger result = 0;
            while (bitSize > 0)
            {
                result += rn.Next(256);
                result <<= 8;
                bitSize -= 8;
            }
            return result;
        }

        private static BigInteger BigRandom(BigInteger max)
        {
            return BigRandom(0, max);
        }

        private static BigInteger BigRandom(BigInteger min, BigInteger max)
        {
            if (min == max)
                return min;
            
            if (min > max)
                throw new ArgumentException();
            
            BigInteger extraMax = max - min;
            BigInteger result = 0;
            while (result < extraMax)
            {
                result += rn.Next(256);
                result <<= 8;
            }
            while (result >= extraMax)
            {
                result >>= 1;
            }

            return min + result;
        }

        private static (BigInteger lnko, BigInteger X, BigInteger Y) KiterjesztettEuklidesziAlgoritmus(BigInteger a, BigInteger b)
        {
            BigInteger q = 0, count = 1, x = 0, y = 1, xprev = 1, yprev = 0;

            if (b > a)
            {
                var tmp = a;
                a = b;
                b = tmp;
            }

            while (b != 0)
            {
                var tmp = b;
                q = BigInteger.DivRem(a, b, out b);
                a = tmp;
                if (b != 0)
                {
                    var xprevtmp = x;
                    var yprevtmp = y;
                    x = q * x + xprev;
                    y = q * y + yprev;
                    xprev = xprevtmp;
                    yprev = yprevtmp;
                }
                ++count;
            }

            return (a, (count % 2 == 0 ? -1 : 1) * x, (count % 2 != 0 ? -1 : 1) * y);
        }

        private static bool FermatTeszt(BigInteger number)
        {
            Random rn = new Random();
            bool canBePrime = true;

            for (int i = 0; i < 8 && canBePrime; ++i)
            {
                BigInteger baseNum = BigRandom(2, number);
                BigInteger lnko = EuklidesziAlgoritmus(baseNum, number);
                BigInteger maradek = Gyorshatvanyoz(baseNum, number - 1, number);
                canBePrime &= lnko == 1 && maradek == 1;
            }

            return canBePrime;
        }

        private static BigInteger EuklidesziAlgoritmus(BigInteger a, BigInteger b)
        {
            if (b > a) //ha nagyobb megcseréli, hogy biztos a nagyobb szám legyen elől.
            {
                var tmp = a;
                a = b;
                b = tmp;
            }

            while (b != 0)
            {
                var tmp = b;
                b = a % b;
                a = tmp;
            }

            return a;
        }

        private static BigInteger KinaiMaradekTetel(BigInteger[] m, BigInteger[] c)
        {
            BigInteger M = m.Aggregate(1, (BigInteger acc, BigInteger e) => acc *= e);
            BigInteger[] Ms = m.Select(e => M / e).ToArray();
            BigInteger X = 0;

            for (int i = 0; i < m.Length; ++i)
            {
                (var n, var x, var y) = KiterjesztettEuklidesziAlgoritmus(Ms[i], m[i]);
                while (x < 0)
                {
                    x += m[i];
                }
                while (y < 0)
                {
                    y += m[i];
                }
                if ((x * Ms[i]) % m[i] == 1)
                {
                    X += x * Ms[i] * c[i];
                }
                else
                {
                    X += y * Ms[i] * c[i];
                }
            }

            return X % M;
        }

    }
}

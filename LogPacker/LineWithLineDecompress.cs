using System;
using System.Text;

namespace Kontur.LogPacker
{
    class LineWithLineDecompress
    {
        public static string DecompressWithPreviousLine(string pattern, string decompressed)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));
            if (pattern == string.Empty || decompressed == string.Empty)
                return decompressed;

            var outStr = new StringBuilder(); // выходная строка
            int di = 0; // место положения текущего индекса расжимаемой строки
            int pi = 0; // место положения текущего индекса строки шаблона

            //ищем первый непустой символ в строках
            if (pattern[0] == ' ')
                pi = FindNextToken(pattern, pi);
            if (decompressed[0] == ' ')
            {
                di = FindNextToken(decompressed, di);
                outStr.Append(decompressed.Substring(0, di));
            }

            while (di < decompressed.Length)
            {
                // считывание из строки шаблона к символов
                int k = Convert.ToInt16(decompressed[di]);
                if (k == 0)
                {
                    k = Convert.ToInt16(decompressed[di + 1]) - 20;
                    outStr.Append(pattern.Substring(pi, k));
                    pi += k;
                    di += 2;
                }

                int nextc = FindNextToken(decompressed, di);
                outStr.Append(decompressed.Substring(di, nextc - di));
                pi = FindNextToken(pattern, pi);
                di = nextc;
            }

            return outStr.ToString();
        }

        private static int FindNextToken(string str, int i)
        {
            if (i == str.Length)
                return str.Length;
            i = str.IndexOf(' ', i);
            if (i == -1)
                return str.Length;
            while (i < str.Length && str[i] == ' ')
                i++;
            return i;
        }
    }
}

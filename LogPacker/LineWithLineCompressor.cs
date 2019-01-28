using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kontur.LogPacker
{
    class CompressLinebyLine
    {
        public static string CompressWithPreviousLine(string pattern, string compressed)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));
            if (pattern == string.Empty || compressed == string.Empty)
                return compressed;

            var outStr = new StringBuilder(); // выходная строка
            int ci = 0; // место положения текущего индекса сжимаемой строки
            int pi = 0; // место положения текущего индекса строки шаблона

            //ищем первый непустой символ в строках
            if (pattern[0] == ' ')
                pi = FindNextToken(pattern, pi);
            if (compressed[0] == ' ')
            {
                ci = FindNextToken(compressed, ci);
                outStr.Append(compressed.Substring(0, ci));
            }

            while (ci < compressed.Length)
            {
                //определение соответствия текущих символов строк
                int k = 0, limit = Math.Min(pattern.Length - pi, compressed.Length - ci);
                while (k < limit && pattern[pi + k] == compressed[ci + k])
                    k++;

                // сжатие к символов. Первые 20 символов пропускаются из-за символа Конец строки
                if (k > 2)
                    outStr.Append(Convert.ToChar(0) + "" + Convert.ToChar(k + 20));
                else
                    outStr.Append(compressed.Substring(ci, k));
                pi += k;
                ci += k;

                //запись остатка токена
                int nextc = FindNextToken(compressed, ci);
                outStr.Append(compressed.Substring(ci, nextc - ci));
                pi = FindNextToken(pattern, pi);
                ci = nextc;
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

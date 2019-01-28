using System;
using System.Collections.Generic;
using System.Text;

namespace Kontur.LogPacker
{
    enum EndChars // символы конца строки
    {
        cr, nf, crnf
    }

    class EndChar
    {
        public static int detectEndLineChars(char[] chars)
        {
            var cr = Array.IndexOf(chars, '\r');
            var nf = Array.IndexOf(chars, '\n');
            return (cr != -1) ? (nf != -1) ? (int)EndChars.crnf : (int)EndChars.cr
                                : (nf != -1) ? (int)EndChars.nf : -1;
        }

        public static string GetEndChars(int endChar)
        {
            return (endChar == (int)EndChars.cr) ? "\r" :
                        (endChar == (int)EndChars.nf) ? "\n" :
                            (endChar == (int)EndChars.crnf) ? "\r\n" :
                                throw new ArgumentOutOfRangeException("Тип конца строки отсутсвует");
        }
    }
}

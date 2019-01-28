using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kontur.LogPacker
{
    class BinaryChecker
    {
        public static byte[] ReadBytes(string inputFile, int size = 1024)
        {
            var bytes = new byte[size];
            var fi = File.OpenRead(inputFile);
            fi.Read(bytes, 0, size);
            fi.Close();
            return bytes;
        }

        public static bool canGetUTF8Chars(byte[] bytes, out char[] chars)
        {
            chars = null;
            UTF8Encoding UTF8Test = new UTF8Encoding(false, true);
            try
            {
                chars = UTF8Test.GetChars(bytes);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

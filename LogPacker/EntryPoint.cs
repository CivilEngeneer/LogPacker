using System;
using System.IO;

namespace Kontur.LogPacker
{
    internal static class EntryPoint
    {
        public const int OffsetZero = 20;
        public const int headerLength = 3;

        public static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                var (inputFile, outputFile) = (args[0], args[1]);

                if (File.Exists(inputFile))
                {
                    Compress(inputFile, outputFile);
                    return;
                }
            }

            if (args.Length == 3 && args[0] == "-d")
            {
                var (inputFile, outputFile) = (args[1], args[2]);

                if (File.Exists(inputFile))
                {
                    Decompress(inputFile, outputFile);
                    return;
                }
            }
            ShowUsage();
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"{AppDomain.CurrentDomain.FriendlyName} [-d] <inputFile> <outputFile>");
            Console.WriteLine("\t-d flag turns on the decompression mode");
            Console.WriteLine();
        }

        #region Compressor
        private static void Compress(string inputFile, string outputFile)
        {
            if (!File.Exists(inputFile))
                throw new FileNotFoundException("File not found", inputFile);

            string tempFile = "tempFile.txt";
            CheckBinaryOrStartCompress(inputFile, tempFile);

            using (var inputStream = File.OpenRead(tempFile))
            using (var outputStream = File.OpenWrite(outputFile))
                new GZipCompress().Compress(inputStream, outputStream);

            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }

        private static void CheckBinaryOrStartCompress(string inputFile, string outputFile)
        {
            // проверка на бинарный файл и определение символа конца строки
            int cNum;
            if (!BinaryChecker.canGetUTF8Chars(BinaryChecker.ReadBytes(inputFile), out char[] chars) ||
                    (cNum = EndChar.detectEndLineChars(chars)) == -1)
            {
                File.Copy(inputFile, outputFile, true);
                return;
            }

            StartCompressLines(inputFile, outputFile, cNum);
        }

        private static void StartCompressLines(string inputFile, string outputFile, int cNum)
        {
            using (var sr = new StreamReader(inputFile))
            {
                using (var sw = new StreamWriter(outputFile))
                {
                    // заголовок сжатого файла. Последнее значение - тип символа конца строки
                    sw.Write(string.Concat(Convert.ToChar(2), Convert.ToChar(2), Convert.ToChar(cNum)));

                    var queue = new WindowWorker();
                    string line, endChars = EndChar.GetEndChars(cNum);
                    while ((line = sr.ReadLine()) != null)
                    {
                        sw.Write(queue.Handle(line, endChars));
                    }
                }
            }
        }

        #endregion

        #region DeCompressor
        private static void Decompress(string inputFile, string outputFile)
        {
            string tempFile = "tempFile.txt";

            using (var inputStream = File.OpenRead(inputFile))
            using (var outputStream = File.OpenWrite(tempFile))
                new GZipCompress().Decompress(inputStream, outputStream);

            CheckBinaryOrStartDecompressLines(tempFile, outputFile);
            
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
        
        static void CheckBinaryOrStartDecompressLines(string inputFile, string outputFile)
        {
            int headerLength = 3;
            string endChars;
            // проверка на бинарный файл
            using (TextReader reader = File.OpenText(inputFile))
            {
                char[] block = new char[headerLength];
                reader.ReadBlock(block, 0, headerLength);
                if (Convert.ToInt32(block[0]) != 2 && Convert.ToInt32(block[1]) != 2)
                {
                    File.Copy(inputFile, outputFile, true);
                    return;
                }
                endChars = EndChar.GetEndChars(Convert.ToInt32(block[2]));
            }
            StartDecompressLines(inputFile, outputFile, endChars);
        }


        static void StartDecompressLines(string inputFile, string outputFile, string endChars)
        {
            using (var sr = new StreamReader(inputFile))
            {
                for (int i = 0; i < headerLength; i++) // пропускаем заголовок
                    sr.Read(); 
                using (var sw = new StreamWriter(outputFile))
                {
                    char zero = Convert.ToChar(0);
                    string line;
                    var window = new WQ<string>();

                    while ((line = sr.ReadLine()) != null)
                    {
                        //линия не сжата и не в очереди
                        if (string.IsNullOrEmpty(line) || line.Length < 2 || line[0] != zero)
                        {
                            sw.Write(line + endChars);
                            continue;
                        }

                        char sc;
                        string dl;
                        //не сжата, в очереди
                        if ((sc = line[1]) == zero)
                            dl = line.Substring(2);
                        //сжата и в очереди
                        else 
                        {
                            string pl = window.GetElement(Convert.ToInt32(sc) - OffsetZero);
                            dl = LineWithLineDecompress.DecompressWithPreviousLine (pl, line.Substring(2));
                        }
                        sw.Write(dl + endChars);
                        window.Enqueue(dl);
                    }
                }
            }
        }
        #endregion        
    }
}
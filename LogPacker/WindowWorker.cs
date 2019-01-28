using System;

namespace Kontur.LogPacker
{
    class WindowWorker
    {
        public const int DefaultQueueSize = 10;
        private const int DefaultMinWordCount = 4;
        private const bool DefaultCompareFirstLetterFlag = true;
        private const bool DefaultCompareAllLinesFlag = false;

        readonly int sizeQueue;
        readonly int skipWordCount; //строка пропускается, если слов меньше
        readonly bool shouldCompareFirstLetter;
        readonly bool shouldCompareAllLines; // если false проверяются только линии с одинаковым количеством слов
        WQ<LogMessage> window;

        public WindowWorker() : this(DefaultQueueSize, DefaultMinWordCount, DefaultCompareFirstLetterFlag, DefaultCompareAllLinesFlag)
        {
        }

        public WindowWorker(int sizeQueue, int skipWordCount, bool shouldCompareFirstLetter, bool shouldCompareAllLines)
        {
            this.sizeQueue = sizeQueue;
            this.skipWordCount = skipWordCount;
            this.shouldCompareFirstLetter = shouldCompareFirstLetter;
            this.shouldCompareAllLines = shouldCompareAllLines;

            window = new WQ<LogMessage>();
        }

        public struct LogMessage
        {
            public string Message { get; private set; }
            public string[] Words { get; private set; }

            public LogMessage(string s)
            {
                Message = s;
                Words = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public string Handle(string input, string endLine = "\r\n")
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (string.IsNullOrEmpty(input) ||
                !HasEnoughWords(input, out LogMessage message))
                return (input + endLine);

            string res;
            char zero = Convert.ToChar(0);
            // 0 - строка добавлена в window
            // i - номер в очереди(0 - не имеет совпадений)
            if (TryFindSimilar(message, out string similar, out int similarIndex))
                res = (zero + "" +
                    Convert.ToChar(similarIndex + EntryPoint.OffsetZero) + "" +
                         CompressLinebyLine.CompressWithPreviousLine(similar, input) + endLine);
            else
                res = (zero + "" + zero + "" + input + endLine);

            window.Enqueue(message);
            return res;
        }

        //строки с кол. слов меньше skipWordCount не учитываются
        private bool HasEnoughWords(string input, out LogMessage message)
        {
            message = new LogMessage(input);
            if (message.Words.Length < skipWordCount)
                return false;

            return true;
        }

        //выдает строку наиболее схожую с введенной
        public bool TryFindSimilar(LogMessage message, out string similar, out int similarIndex)
        {
            similar = string.Empty;
            similarIndex = -1;
            int maxSimilar = 0;

            for (int i = 0; i < window.QueueSize; i++)
            {
                LogMessage listMessage = window.GetElement(i);
                if (listMessage.Message == null ||
                        (shouldCompareFirstLetter && listMessage.Message[0] != message.Message[0]))
                    continue;

                int similarWordCount =
                    (shouldCompareAllLines || listMessage.Words.Length == message.Words.Length) ?
                        FindSimilarWordCount(listMessage.Words, message.Words) : 0;

                if (similarWordCount >= maxSimilar)
                {
                    similar = listMessage.Message;
                    similarIndex = i;
                    maxSimilar = similarWordCount;
                }
            }
            return !string.IsNullOrEmpty(similar);
        }

        private int FindSimilarWordCount(string[] splitPattern, string[] splitInput)
        {
            int similarWordCount = 0;
            for (int i = 0; i < Math.Min(splitPattern.Length, splitInput.Length); i++)
                similarWordCount = splitPattern[i] == splitInput[i] ? ++similarWordCount : similarWordCount;

            return similarWordCount;
        }
    }
}

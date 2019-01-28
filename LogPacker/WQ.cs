using System;
using System.Collections;
using System.Collections.Generic;

namespace Kontur.LogPacker
{
    //Динамическая очередь(окно)
    public class WQ<T> : IEnumerable<T>
    {
        public const int DefaultQueueSize = 10;

        public WQ() : this(DefaultQueueSize)
        {
        }

        public WQ(int queueSize)
        {
            QueueSize = queueSize;
            Window = new T[queueSize];
            LastIndex = queueSize - 1;
        }

        public T[] Window { get; private set; }
        public int QueueSize { get; private set; }
        public int LastIndex { get; private set; }

        //возвращает замененный элемент
        public T Enqueue(T item)
        {
            int zero = DefineArrayIndex(0);
            T res = Window[zero];
            Window[zero] = item;
            LastIndex = zero;
            return res;
        }

        public T GetElement(int i)
        {
            if (i >= Window.Length)
                throw new IndexOutOfRangeException();
            return Window[DefineArrayIndex(i)];
        }

        private int DefineArrayIndex(int i)
        {
            return (LastIndex + (i + 1)) % Window.Length;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Window).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)Window).GetEnumerator();
        }
    }
}

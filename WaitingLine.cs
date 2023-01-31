using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProducerNConsumer
{

    public interface IWaitingLine
    {
        public int Capacity { get; set; }
        public int Count { get;}
        public bool Add(string name);
        public bool Take(out string name);
    }

    public class WaitingLineBc : IWaitingLine
    {
        public int Capacity {
            get => capacity;
            set {
                capacity = value;
                line = new BlockingCollection<string>(capacity);
            }
        }
        public int Count => line.Count;
        public bool Add(string name)
        {
            // add p to Queue
            try
            {
                line.Add(name);
            }
            catch
            {
                Debug.WriteLine($"Interrupted.{Thread.CurrentThread.ManagedThreadId}");
                return false;
            }

            Debug.WriteLine($"\t\t >>> {name}");
            return true;
        }

        public bool Take(out string name)
        {
            name = null;
            try
            {
                name = line.Take();
            }
            catch
            {
                Debug.WriteLine($"Interrupted.{Thread.CurrentThread.ManagedThreadId}");
                return false;
            }

            Debug.WriteLine($"\t\t <<< {name}");
            return true;
        }

        private BlockingCollection<string> line;
        private int capacity;
    }

    public class WaitingLineCv : IWaitingLine
    {
        public int Capacity
        {
            get => capacity;
            set
            {
                capacity = value;
                line = new ConcurrentQueue<string>();
            }
        }
        public int Count => line.Count;
        public bool Add(string name)
        {
            //// if queue is full, wait for the queue not full
            lock (line)
            {
                if (Count == capacity)
                    notFull.WaitOne();
            }

            // add p to Queue
            try
            {
                line.Enqueue(name);
            }
            catch
            {
                Debug.WriteLine("Interrupted.");
                return false;
            }

            // if its not empty
            if (Count == 1)
                WaitHandle.SignalAndWait(notEmpty, clear2);

            clear1.Set();

            Debug.WriteLine($"\t\t >>> {name}");
            return true;
        }

        public bool Take(out string name)
        {
            name = null;
            // if queue is empty, wait for the queue not empty
            if (Count == 0)
                notEmpty.WaitOne();

            // Remove
            try
            {
                if (line.TryDequeue(out string buf))
                    name = buf;
            }
            catch
            {
                Debug.WriteLine("Interrupted.");
                return false;
            }

            if (Count == capacity - 1)
                WaitHandle.SignalAndWait(notFull, clear1);
            clear2.Set();

            Debug.WriteLine($"\t\t <<< {name}");
            return true;
        }

        private ConcurrentQueue<string> line;
        private int capacity;

        private readonly EventWaitHandle notFull = new(false, EventResetMode.AutoReset);
        private readonly EventWaitHandle clear1 = new(false, EventResetMode.ManualReset);
        private readonly EventWaitHandle notEmpty = new(false, EventResetMode.AutoReset);
        private readonly EventWaitHandle clear2 = new(false, EventResetMode.ManualReset);
    }
}
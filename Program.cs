using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerNConsumer
{
    class Program
    {

        static void Main(string[] args)
        {
            Bank bank = new Bank();

            Thread produce = new Thread(new ThreadStart(bank.serversWorking));
            Thread consume = new Thread(new ThreadStart(bank.clientsComing));

            produce.Start();
            consume.Start();

            Console.WriteLine("Opened bank");

            produce.Join();
            consume.Join();

            Console.WriteLine("Closed the bank");
        }



        class Bank
        {
            const int lineCap = 5;

            //int servers = 3;
            //int clients = 0;

            Queue<int> line = new Queue<int>(lineCap);
            private static EventWaitHandle notFull = new EventWaitHandle(false, EventResetMode.AutoReset);
            private static EventWaitHandle clear1 = new EventWaitHandle(false, EventResetMode.AutoReset);

            private static EventWaitHandle notEmpty = new EventWaitHandle(false, EventResetMode.AutoReset);
            private static EventWaitHandle clear2 = new EventWaitHandle(false, EventResetMode.AutoReset);

            void addToQ(int id)
            {
                // if queue is full, wait for the queue not full
                if (line.Count == lineCap)
                    notFull.WaitOne();

                // add p to Queue
                line.Enqueue(id);

                Console.WriteLine($"A Person #{id} entered");
                Console.WriteLine($"{line.Count} people is waiting in total\n");

                //Interlocked.Increment(ref clients);

                if (line.Count == 1)
                    WaitHandle.SignalAndWait(notEmpty, clear2);

                clear1.Set();
            }

            void removeFromQ()
            {
                // if queue is empty, wait for the queue not empty
                if (line.Count == 0)
                    notEmpty.WaitOne();

                // Remove
                int id = line.Dequeue();
                //Interlocked.Decrement(ref clients);

                Console.WriteLine($"A Person #{id} has been processed");
                Console.WriteLine($"{line.Count} people is waiting in total\n");

                if (line.Count == lineCap - 1)
                    WaitHandle.SignalAndWait(notFull, clear1);

                clear2.Set();
            }

            public void clientsComing()
            {
                Random rand = new Random();
                int p = 0; 
                int id = 1000;
                // Assume the several clients today
                while (p < 10)
                {
                    id = rand.Next(1000, 9999);
                    Console.WriteLine($"A person #{id} wants to enter, let us see the status\n");
                    addToQ(id);
                    p++;

                    // entering in random timing
                    int sleep = rand.Next(1, 5) * 1000;
                    Task.Delay(sleep).Wait();
                }
            }

            public void serversWorking()
            {
                Random rand = new Random();

                while (true)
                {
                    removeFromQ();
                    int sleep = rand.Next(1, 10) * 1000;
                    Task.Delay(sleep).Wait();
                }
            }
        }
    }
}

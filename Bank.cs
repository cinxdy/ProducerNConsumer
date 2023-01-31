using System;
using System.Collections.Generic;
using System.Threading;

namespace ProducerNConsumer
{
    public class Bank<Line> where Line : IWaitingLine , new()
    {
        // 
        // NOTICE: 
        // 
        public Bank(int clientCount, int employeeCount, int lineCapacity)
        {
            this.clientCount = clientCount;
            this.employeeCount = employeeCount;
            // BC : Blocking Conditional Variable
            // CV : Condition Variable -> Concurrent Queue
            this.line = new Line
            {
                Capacity = lineCapacity
            };
            this.threads = new List<Thread>();
        }

        public void Open()
        {
            for (int i = 0; i < clientCount; i++)
            {
                var c = new Client<Line>(line, i + 1);
                threads.Add(new Thread(c.Come));
            }

            for (int i = 0; i < employeeCount; i++)
            {
                var e = new Employee<Line>(line, i + 1);
                threads.Add(new Thread(e.Work));
            }

            foreach (Thread t in threads)
                t.Start();

            Console.WriteLine("Opened bank");
        }

        public void Close()
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(MyHandler);

            threads.ForEach(thread => thread.Join());

            Console.WriteLine("Closed the bank");
            //Console.WriteLine($"Today's summary:{line.GetSum() }");
        }

        private void MyHandler(object sender, ConsoleCancelEventArgs args)
        {
            threads.ForEach(thread => thread.Interrupt());
            args.Cancel = true;
        }

        private readonly int clientCount;
        private readonly int employeeCount;

        private readonly Line line;
        private readonly List<Thread> threads;
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerNConsumer
{
    public class Employee<Line> where Line : IWaitingLine
    {
        public Employee(Line line, int id)
        {
            this.line = line;
            this.id = id;
        }

        public void Work()
        {
            while (true)
            {
                try
                {
                    var rand = new Random();

                    if (!line.Take(out string name)) 
                        throw new ThreadInterruptedException();
                    Console.WriteLine($"\t << {name} : {line.Count}\n");

                    int sleep = rand.Next(1, 10) * 1000;
                    Task.Delay(sleep).Wait();
                }
                catch (ThreadInterruptedException)
                {
                    Console.WriteLine($"Interrupted.{Thread.CurrentThread.ManagedThreadId}");
                    Console.WriteLine($"{this} Closed");
                    break;
                }
            }
        }

        public override string ToString() => $"Employee ID:{id}";
        private readonly int id;
        private readonly Line line;
    }
}
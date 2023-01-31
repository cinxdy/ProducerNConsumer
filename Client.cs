using System;
using System.Threading;
using System.Threading.Tasks;

using RandomNameGen;

namespace ProducerNConsumer
{
    public class Client<Line> where Line : IWaitingLine
    {
        public Client(Line line, int id)
        {
            this.line = line;
            this.id = id;
        }

        public void Come()
        {
            var rand = new Random();
            var nameGen = new RandomName(rand);

            // Assume the several clients today
            while (true)
            {
                try
                {
                    var name = nameGen.Generate();
                    if (!line.Add(name)) 
                        throw new ThreadInterruptedException();

                    Console.WriteLine($"\t >> {name} : {line.Count}\n");

                    // entering in random timing
                    int sleep = rand.Next(1, 5) * 1000;
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

        public override string ToString() => $"Door ID:{id}";
        private readonly int id;
        private readonly Line line;
    }
}
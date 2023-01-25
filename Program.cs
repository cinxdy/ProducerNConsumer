using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ProducerNConsumer
{
    class Program
    {

        static void Main(string[] args)
        {
            Bank bank = new Bank(3,3,5); // 1:1, queuecapa=5
            bank.open();
            bank.close();
        }

        class Bank
        {
            private int n_client;
            private int n_employee;
            private int cap_line;

            private waitingLine line;

            //private List<Client> cList;
            //private List<Employee> eList;
            private List<Thread> tList;
             
            public Bank(int n_client, int n_employee, int cap_line)
            {
                this.n_client = n_client;
                this.n_employee = n_employee;
                this.cap_line = cap_line;

                this.line = new waitingLine(cap_line);
                //this.cList = new List<Client>(n_client);
                //this.eList = new List<Employee>(n_employee);
                this.tList = new List<Thread>();
            }

            public void open()
            {
                for (int i = 0; i < n_employee; i++)
                {
                    Employee e = new Employee(line, i + 1);
                    //eList.Add(e);
                    tList.Add(new Thread(e.work));
                }

                for (int i = 0; i < n_client; i++)
                {
                    Client c = new Client(line, i+1);
                    //cList.Add(c);
                    tList.Add(new Thread(c.come));
                }

                foreach (Thread t in tList)
                    t.Start();

                Console.WriteLine("Opened bank");
            }

            public void close()
            { 
                Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

                foreach (Thread t in tList)
                    t.Join();

                Console.WriteLine("Closed the bank");
                Console.WriteLine($"Today's summary:{line.getSum() }");
            }

            protected void myHandler(object sender, ConsoleCancelEventArgs args)
            {
                foreach (Thread t in tList)
                    t.Interrupt();

                args.Cancel = true;
            }
        }

        class waitingLine
        {
            public waitingLine(int cap){
                line = new BlockingCollection<int>(cap);
                capacity = cap;
                cnt = 0;
                sum = 0;
            }

            public bool add(int id)
            {
                //// if queue is full, wait for the queue not full
                //if (line.Count == capacity)
                //    notFull.WaitOne();

                // add p to Queue
                try
                {
                    line.Add(id);
                    Interlocked.Increment(ref cnt);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Interrupted. The bank will be closed soon.");
                    return false;
                }

                Console.WriteLine($"A Person #{id} entered");
                Console.WriteLine($"Total: {cnt} people are waiting\n");

                return true;

                //if (line.Count == 1)
                //    WaitHandle.SignalAndWait(notEmpty, clear2);

                //clear1.Set();
            }

            public bool remove(out int id)
            {
                // if queue is empty, wait for the queue not empty
                //if (line.Count == 0)
                //    notEmpty.WaitOne();

                // Remove
                try
                {
                    id = line.Take();
                    Interlocked.Decrement(ref cnt);   
                }
                catch (Exception e)
                {
                    Console.WriteLine("Interrupted. The bank will be closed soon.");
                    id = 0;
                    return false;
                }
                Console.WriteLine($"A Person #{id} has been processed");
                Console.WriteLine($"Total: {cnt} people are waiting\n");
                return true;

                //if (line.Count == capacity - 1)
                //    WaitHandle.SignalAndWait(notFull, clear1);

                //clear2.Set();

                //return id;
            }

            public int getSum()
            {
                return sum++;
            }

            private int sum;
            private BlockingCollection<int> line;
            //private readonly object cntLock = new object();
            private int capacity;
            private int cnt;

            //private EventWaitHandle notFull;
            //private EventWaitHandle clear1;
            //private EventWaitHandle notEmpty;
            //private EventWaitHandle clear2;

        }

        class Client
        {
            public Client(waitingLine line, int id)
            {
                this.line = line;
                this.id = id;
            }

            public void come()
            {
                Random rand = new Random();
                int userid = 1000;
                // Assume the several clients today
                while (true)
                {
                    try
                    {
                        userid = rand.Next(1000, 9999);
                        Console.WriteLine($"A person #{userid} wants to enter, let us see the status\n");
                        if (!line.add(userid)) break;

                        // entering in random timing
                        int sleep = rand.Next(1, 5) * 1000;
                        Task.Delay(sleep).Wait();
                    }
                    catch (ThreadInterruptedException)
                    {
                        Console.WriteLine($"{this.toString()} Closed");
                        break;
                    }
                }
            }
            public string toString() => $"Door ID:{id}";
            private int id;
            private waitingLine line;
        }

        class Employee
        {
            public Employee(waitingLine line, int id)
            {
                this.id = id;
                this.line = line;
            }

            public void work()
            {
                Random rand = new Random();
                int userid = default;
                while (true)
                {
                    try
                    {
                        if (!line.remove(out userid)) break;
                        Console.WriteLine($"A person #{userid} wants to leave, Bye Bye\n");
                        line.getSum();

                        int sleep = rand.Next(1, 10) * 1000;
                        Task.Delay(sleep).Wait();
                    }
                    catch (ThreadInterruptedException)
                    {
                        Console.WriteLine($"{this.toString()} Closed");
                        break;
                    }
                }
            }
            public string toString() => $"Employee ID:{id}";
            private int id;
            private waitingLine line;
        }
    }
}

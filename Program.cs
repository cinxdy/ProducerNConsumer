using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using RandomNameGen;

namespace ProducerNConsumer
{
    class Program
    {

        static void Main(string[] args)
        {
            var bank = new Bank(3,1,5); // 1:1, queuecapa=5
            bank.open();
            bank.close();
        }

        class Bank
        {            
            public Bank(int n_client, int n_employee, int cap_line)
            {
                this.n_client = n_client;
                this.n_employee = n_employee;
                this.cap_line = cap_line;
                this.line = new waitingLineBC(cap_line);

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
                    Client c = new Client(line, i + 1);
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

            private int n_client;
            private int n_employee;
            private int cap_line;

            private waitingLineBC line;

            //private List<Client> cList;
            //private List<Employee> eList;
            private List<Thread> tList;
        }

        //interface waitingLine
        //{
        //    //public waitingLine(int cap);
        //    public bool add(int id);
        //    public bool take(out int id);
        //    public int getSum();
        //}

        class waitingLineBC
        {
            public waitingLineBC(int cap){
                line = new BlockingCollection<string>(cap);
                capacity = cap;
                cnt = 0;
                sum = 0;
            }

            public bool add(string name)
            {
                // add p to Queue
                try
                {
                    line.Add(name);
                    Interlocked.Increment(ref cnt);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Interrupted. The bank will be closed soon.");
                    return false;
                }

                Console.WriteLine($"\t\t{name} entered");
                Console.WriteLine($"Total: {cnt} people are waiting\n");

                return true;
            }

            public bool take(out string name)
            {
                // Remove
                try
                {
                    name = line.Take();
                    Interlocked.Decrement(ref cnt);   
                }
                catch (Exception e)
                {
                    Console.WriteLine("Interrupted. The bank will be closed soon.");
                    name = "unknown";
                    return false;
                }
                Console.WriteLine($"\t\t{name} has been processed");
                Console.WriteLine($"Total: {cnt} people are waiting\n");
                return true;
            }

            public int getSum() => sum++;
            private int sum;
            private BlockingCollection<string> line;
            private int capacity;
            private int cnt;
        }

        class waitingLineCV
        {
            public waitingLineCV(int cap)
            {
                line = new ConcurrentQueue<string>();
                capacity = cap;
                cnt = 0;
                sum = 0;
            }

            public bool add(string name)
            {
                //// if queue is full, wait for the queue not full
                if (Interlocked.Read(ref cnt) == capacity)
                    notFull.WaitOne();

                // add p to Queue
                try
                {   
                    line.Enqueue(name);
                    Interlocked.Increment(ref cnt);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Interrupted. The bank will be closed soon.");
                    return false;
                }

                Console.WriteLine($"{name} entered");
                Console.WriteLine($"Total: {cnt} people are waiting\n");

                // if its not empty
                if (Interlocked.Read(ref cnt) == 1)
                    WaitHandle.SignalAndWait(notEmpty, clear2);

                clear1.Set();

                return true;
            }

            public bool take(out string name)
            {
                // if queue is empty, wait for the queue not empty
                if (Interlocked.Read(ref cnt) == 0)
                    notEmpty.WaitOne();

                // Remove
                try
                {
                    if (line.TryDequeue(out name))
                        Interlocked.Decrement(ref cnt);
                    else name = "unknown";
                }
                catch (Exception e)
                {
                    Console.WriteLine("Interrupted. The bank will be closed soon.");
                    name = "unknown";
                    return false;
                }
                Console.WriteLine($"{name} has been processed");
                Console.WriteLine($"Total: {cnt} people are waiting\n");
                
                if (Interlocked.Read(ref cnt) == capacity - 1)
                    WaitHandle.SignalAndWait(notFull, clear1);

                clear2.Set();

                return true;
            }

            public int getSum() => sum++;

            private int sum;
            private ConcurrentQueue<string> line;
            private int capacity;
            private long cnt;

            private EventWaitHandle notFull = new EventWaitHandle(false, EventResetMode.AutoReset);
            private EventWaitHandle clear1 = new EventWaitHandle(false, EventResetMode.ManualReset);
            private EventWaitHandle notEmpty = new EventWaitHandle(false, EventResetMode.AutoReset);
            private EventWaitHandle clear2 = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        class Client
        {
            public Client(waitingLineBC line, int id)
            {
                this.line = line;
                this.id = id;
            }

            public void come()
            {
                Random rand = new Random();
                RandomName nameGen = new RandomName(rand);

                string name = default;
                // Assume the several clients today
                while (true)
                {
                    try
                    {
                        name = nameGen.Generate();
                        Console.WriteLine($"\t{name} wants to enter, let us see the status\n");
                        if (!line.add(name)) break;

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
            private waitingLineBC line;
        }

        class Employee
        {
            public Employee(waitingLineBC line, int id)
            {
                this.id = id;
                this.line = line;
            }

            public void work()
            {
                Random rand = new Random();
                string username = default;
                while (true)
                {
                    try
                    {
                        if (!line.take(out username)) break;
                        Console.WriteLine($"\t{username} wants to leave, Bye Bye\n");
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
            private waitingLineBC line;
        }
    }
}

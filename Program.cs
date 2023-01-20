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
            Bank bank = new Bank(3,1,5); // 1:1, queuecapa=5
            bank.open();
            bank.close();
        }


        class Bank
        {
            private int n_client;
            private int n_employee;
            private int n_line;

            private waitingLine line;

            private List<Client> cList;
            private List<Employee> eList;
            private List<Thread> tList;
             
            public Bank(int n_client, int n_employee, int n_line)
            {
                this.n_client = n_client;
                this.n_employee = n_employee;
                this.n_line = n_line;

                this.line = new waitingLine(n_line);
                this.cList = new List<Client>();
                this.eList = new List<Employee>();
                this.tList = new List<Thread>();
            }

            public void open()
            {
                for (int i = 0; i < n_employee; i++)
                {
                    Employee e = new Employee(line, i + 1);
                    eList.Add(e);
                    tList.Add(new Thread(e.employeeWorking));
                }

                for (int i = 0; i < n_client; i++)
                {
                    Client c = new Client(line, i+1);
                    cList.Add(c);
                    tList.Add(new Thread(c.clientComing));
                }

                foreach (Thread t in tList)
                    t.Start();

                Console.WriteLine("Opened bank");
            }

            public void close()
            {
                foreach (Thread t in tList)
                    t.Join();

                Console.WriteLine("Closed the bank");
            }

        }

        class waitingLine
        {
            public waitingLine(int cap){
                line = new Queue<int>(cap);
                capacity = cap;

                notFull = new EventWaitHandle(false, EventResetMode.AutoReset);
                notEmpty = new EventWaitHandle(false, EventResetMode.AutoReset);
                clear1 = new EventWaitHandle(false, EventResetMode.AutoReset);
                clear2 = new EventWaitHandle(false, EventResetMode.AutoReset);

            }

            public void addToQ(int id)
            {
                // if queue is full, wait for the queue not full
                if (line.Count == capacity)
                    notFull.WaitOne();

                // add p to Queue
                line.Enqueue(id);

                Console.WriteLine($"A Person #{id} entered");
                Console.WriteLine($"Total: {line.Count} people are waiting\n");

                //Interlocked.Increment(ref clients);

                if (line.Count == 1)
                    WaitHandle.SignalAndWait(notEmpty, clear2);

                clear1.Set();
            }

            public int removeFromQ()
            {
                // if queue is empty, wait for the queue not empty
                if (line.Count == 0)
                    notEmpty.WaitOne();

                // Remove
                int id = line.Dequeue();
                //Interlocked.Decrement(ref clients);

                Console.WriteLine($"A Person #{id} has been processed");
                Console.WriteLine($"Total: {line.Count} people are waiting\n");

                if (line.Count == capacity - 1)
                    WaitHandle.SignalAndWait(notFull, clear1);

                clear2.Set();

                return id;
            }

            private Queue<int> line;
            private int capacity;

            private EventWaitHandle notFull;
            private EventWaitHandle clear1;
            private EventWaitHandle notEmpty;
            private EventWaitHandle clear2;
        }

        class Client
        {
            public Client(waitingLine line, int id)
            {
                this.line = line;
                this.id = id;
            }

            public void clientComing()
            {
                Random rand = new Random();
                int userid = 1000;
                // Assume the several clients today
                while (true)
                {
                    userid = rand.Next(1000, 9999);
                    Console.WriteLine($"A person #{userid} wants to enter, let us see the status\n");
                    line.addToQ(userid);

                    // entering in random timing
                    int sleep = rand.Next(1, 5) * 1000;
                    Task.Delay(sleep).Wait();
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

            public void employeeWorking()
            {
                Random rand = new Random();
                int userid;
                while (true)
                {
                    userid = line.removeFromQ();
                    Console.WriteLine($"A person #{userid} wants to leave, Bye Bye\n");

                    int sleep = rand.Next(1, 10) * 1000;
                    Task.Delay(sleep).Wait();
                }
            }
            public string toString() => $"Employee ID:{id}";
            private int id;
            private waitingLine line;
        }
    }
}

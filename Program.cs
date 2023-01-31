using System;

namespace ProducerNConsumer
{
    class Program
    {
        private static object e;

        static void Main(string[] args)
        {
            var bank = new Bank<WaitingLineBc>(3,3,5); // 1:1, queuecapa=5
            bank.Open();
            bank.Close();
        }
    }
}

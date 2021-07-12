using System;

namespace FlowConsoleTests
{
    class Program
    {
        static void Main()
        {
            (var branch, var summ) = new FlowExample().GetResult();
            Console.WriteLine("{0} selected with result {1}.", branch, summ);
        }
    }
}

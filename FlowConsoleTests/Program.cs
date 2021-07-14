using System;

namespace FlowConsoleTests
{
    class Program
    {
        static void Main()
        {
            using (var flow = new FlowExample())
            {
                (var branch, var summ) = flow.GetResult();
                Console.WriteLine("{0} selected with result {1}.", branch, summ);
            }
        }
    }
}

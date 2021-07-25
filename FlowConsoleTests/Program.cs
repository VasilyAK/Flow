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

            try
            {
                new FlowPerformanceTesting().DoPerfomanceTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }

            Console.WriteLine("Press \"Esc\" to close application.");
            while (Console.ReadKey().Key != ConsoleKey.Escape);
        }
    }
}

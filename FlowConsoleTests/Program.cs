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

            var perfomanceTest = new FlowPerformanceTesting().DoPerfomanceTest();
            Console.WriteLine(FlowPerformanceTesting.CreateTableHeader());
            foreach (var test in perfomanceTest)
                Console.WriteLine(FlowPerformanceTesting.CreateTableRow(test));
        }
    }
}

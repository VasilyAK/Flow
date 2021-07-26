using Flow;
using Flow.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FlowConsoleTests
{
    public class FlowPerformanceTesting
    {
        private static int tableRowLeftPadMin = 15;
        private static int tableRowLeftPadMax = 20;
        private static int testsRepeatCount = 5;
        
        public static string CreateTableHeader() =>
            "FlowCount |".PadLeft(tableRowLeftPadMin, ' ')
            + "StepsCount |".PadLeft(tableRowLeftPadMin, ' ')
            + "BranchCount |".PadLeft(tableRowLeftPadMin, ' ')
            + "IsParallelRuning |".PadLeft(tableRowLeftPadMax, ' ')
            + "IsThrowErrorOnInit |".PadLeft(tableRowLeftPadMax, ' ')
            + "IsCacheUsed |".PadLeft(tableRowLeftPadMin, ' ')
            + "TestResult |".PadLeft(tableRowLeftPadMax, ' ');

        public static string CreateTableRow(FlowPerfomanceTest test) =>
            $"{test.FlowCount} |".ToString().PadLeft(tableRowLeftPadMin, ' ')
            + $"{test.StepsCount} |".ToString().PadLeft(tableRowLeftPadMin, ' ')
            + $"{test.BranchCount} |".ToString().PadLeft(tableRowLeftPadMin, ' ')
            + $"{test.IsParallelRuning} |".ToString().PadLeft(tableRowLeftPadMax, ' ')
            + $"{test.ShouldCreateError} |".ToString().PadLeft(tableRowLeftPadMax, ' ')
            + $"{test.ShouldUseCache} |".ToString().PadLeft(tableRowLeftPadMin, ' ')
            + $"{test.TestResult} |".ToString().PadLeft(tableRowLeftPadMax, ' ');

        public FlowPerfomanceTest[] DoPerfomanceTest()
        {
            var perfomanceTestsWithoutCache = new FlowPerfomanceTest[]
            {
                new FlowPerfomanceTest(100, 10, 1, false, false, true),
                new FlowPerfomanceTest(100, 10, 1, true, false, true),
                new FlowPerfomanceTest(100, 10, 2, false, false, true),
                new FlowPerfomanceTest(100, 10, 2, true, false, true),
                new FlowPerfomanceTest(100, 10, 5, false, false, true),
                new FlowPerfomanceTest(100, 10, 5, true, false, true),
                new FlowPerfomanceTest(1000, 10, 1, false, false, true),
                new FlowPerfomanceTest(1000, 10, 1, true, false, true),
                new FlowPerfomanceTest(1000, 10, 2, false, false, true),
                new FlowPerfomanceTest(1000, 10, 2, true, false, true),
                new FlowPerfomanceTest(1000, 10, 5, false, false, true),
                new FlowPerfomanceTest(1000, 10, 5, true, false, true),
                new FlowPerfomanceTest(1000, 100, 1, false, false, true),
                new FlowPerfomanceTest(1000, 100, 1, true, false, true),
                new FlowPerfomanceTest(1000, 100, 2, false, false, true),
                new FlowPerfomanceTest(1000, 100, 2, true, false, true),
                new FlowPerfomanceTest(1000, 100, 5, false, false, true),
                new FlowPerfomanceTest(1000, 100, 5, true, false, true),
                new FlowPerfomanceTest(10000, 100, 5, false, false, true),
                new FlowPerfomanceTest(10000, 100, 5, true, false, true),
            };

            var cacheTestsWithoutError = new FlowPerfomanceTest[]
            {
                new FlowPerfomanceTest(1000, 100, 1, false, false, true),
                new FlowPerfomanceTest(1000, 100, 1, false, false, false),
                new FlowPerfomanceTest(1000, 100, 1, true, false, true),
                new FlowPerfomanceTest(1000, 100, 1, true, false, false),
                new FlowPerfomanceTest(1000, 100, 2, false, false, true),
                new FlowPerfomanceTest(1000, 100, 2, false, false, false),
                new FlowPerfomanceTest(1000, 100, 2, true, false, true),
                new FlowPerfomanceTest(1000, 100, 2, true, false, false),
                new FlowPerfomanceTest(1000, 100, 5, false, false, true),
                new FlowPerfomanceTest(1000, 100, 5, false, false, false),
                new FlowPerfomanceTest(1000, 100, 5, true, false, true),
                new FlowPerfomanceTest(1000, 100, 5, true, false, false),
                new FlowPerfomanceTest(10000, 100, 5, false, false, true),
                new FlowPerfomanceTest(10000, 100, 5, false, false, false),
                new FlowPerfomanceTest(10000, 100, 5, true, false, true),
                new FlowPerfomanceTest(10000, 100, 5, true, false, false),
            };

            var cacheTestsWithError = new FlowPerfomanceTest[]
            {
                new FlowPerfomanceTest(1000, 100, 1, false, true, true),
                new FlowPerfomanceTest(1000, 100, 1, false, true, false),
                new FlowPerfomanceTest(1000, 100, 1, true, true, true),
                new FlowPerfomanceTest(1000, 100, 1, true, true, false),
                new FlowPerfomanceTest(1000, 100, 2, false, true, true),
                new FlowPerfomanceTest(1000, 100, 2, false, true, false),
                new FlowPerfomanceTest(1000, 100, 2, true, true, true),
                new FlowPerfomanceTest(1000, 100, 2, true, true, false),
                new FlowPerfomanceTest(1000, 100, 5, false, true, true),
                new FlowPerfomanceTest(1000, 100, 5, false, true, false),
                new FlowPerfomanceTest(1000, 100, 5, true, true, true),
                new FlowPerfomanceTest(1000, 100, 5, true, true, false),
                new FlowPerfomanceTest(10000, 100, 5, false, true, true),
                new FlowPerfomanceTest(10000, 100, 5, false, true, false),
                new FlowPerfomanceTest(10000, 100, 5, true, true, true),
                new FlowPerfomanceTest(10000, 100, 5, true, true, false),
            };

            var allTestsTime = new TimeSpan();

            Console.WriteLine("Run perfomance tests");
            allTestsTime += RunTests(perfomanceTestsWithoutCache);
            Console.WriteLine();
            Console.WriteLine("Run cache perfomance tests without error");
            allTestsTime += RunTests(cacheTestsWithoutError);
            Console.WriteLine();
            Console.WriteLine("Run cache perfomance tests with error");
            allTestsTime += RunTests(cacheTestsWithError);
            Console.WriteLine();
            Console.WriteLine("Total test time: {0}", allTestsTime);
            
            return perfomanceTestsWithoutCache
                .Concat(cacheTestsWithoutError)
                .Concat(cacheTestsWithError)
                .ToArray();
        }

        private TimeSpan RunTests(FlowPerfomanceTest[] tests)
        {
            var allTestsTime = new TimeSpan();
            Console.WriteLine(CreateTableHeader());
            foreach (var test in tests)
            {
                var testTime = new TimeSpan();
                for (var index = 0; index < testsRepeatCount; index++)
                {
                    var sw = Stopwatch.StartNew();
                    var flows = GenerateFlows(test.FlowCount, test.StepsCount, test.BranchCount, test.ShouldCreateError, test.ShouldUseCache);
                    RunFlows(flows, test.IsParallelRuning);
                    sw.Stop();
                    testTime += sw.Elapsed;
                }
                test.TestResult = testTime / testsRepeatCount;
                Console.WriteLine(CreateTableRow(test));

                allTestsTime += testTime;
            }
            return allTestsTime;
        }

        private void RunFlows(List<FlowPerfomanceInctance> flows, bool isParallel)
        {
            if (isParallel)
                Parallel.ForEach(flows, flow => flow?.RunFlow());
            else 
                flows.ForEach(flow => flow?.RunFlow());
        }

        private List<FlowPerfomanceInctance> GenerateFlows(
            int flowCount,
            int stepsCount,
            int branchCount,
            bool shouldCreateError,
            bool shouldUseCache)
        {
            var flows = new List<FlowPerfomanceInctance>();
            FlowPerfomanceInctance.FlowCache<FlowPerfomanceInctance>().Flush();
            for (var index = 0; index < flowCount; index++)
            {
                FlowPerfomanceInctance.BranchCount = branchCount < 1 ? 1 : branchCount;
                FlowPerfomanceInctance.ShouldCreateError = shouldCreateError;
                FlowPerfomanceInctance.StepsCount = stepsCount < 1 ? 1 : stepsCount;
                FlowPerfomanceInctance flow = null;
                try
                {
                    flow = new FlowPerfomanceInctance(shouldUseCache);
                    flows.Add(flow);
                }
                catch (Exception)
                {
                    flows.Add(flow);
                }
            }
            return flows;
        }
    }

    class FlowPerfomanceContext : FlowContext { }

    class FlowPerfomanceInctance : Flow<FlowPerfomanceContext>
    {
        public static bool IsTree => BranchCount > 1;
        public static int BranchCount { get; set; } = 1;
        public static bool ShouldCreateError { get; set; } = false;
        public static int StepsCount { get; set; } = 1;

        public static void Reset()
        {
            BranchCount = 1;
            StepsCount = 1;
        }

        public FlowPerfomanceInctance(bool shouldUseCache) : base(shouldUseCache ? FlowCache<FlowPerfomanceInctance>() : null) { }

        protected void GenerateFlowMap(IFlowMap<FlowPerfomanceContext> flowMap, int stepsCount, bool shouldCreateError)
        {
            Action<FlowPerfomanceContext> flowAction = ctx => { };
            flowMap.AddRoot("0", flowAction);
            if (stepsCount < 2)
                return;

            for (var index = 1; index < stepsCount; index++)
                flowMap.GetNode((index - 1).ToString()).AddNext(index.ToString(), flowAction);

            if (shouldCreateError)
                flowMap.GetNode("0").AddNext("1");
        }

        protected void GenerateTreeFlowMap(IFlowMap<FlowPerfomanceContext> flowMap, int stepsCount, int branchCount, bool shouldCreateError)
        {
            Action<FlowPerfomanceContext> flowRootAction = ctx => ctx.SetNext("1");
            flowMap.AddRoot("0", flowRootAction);

            if (stepsCount < 2 || branchCount < 1)
                return;

            for (var index = 1; index < stepsCount; index++)
            {
                var parentIndex = index % branchCount == 0 ? index / branchCount - 1 : index / branchCount;
                var nextStep = index * branchCount + 1;
                Action<FlowPerfomanceContext> flowAction = ctx =>
                {
                    if (nextStep < stepsCount)
                        ctx.SetNext(nextStep.ToString());
                };
                flowMap.GetNode(parentIndex.ToString()).AddNext(index.ToString(), flowAction);
            }

            if (shouldCreateError)
                flowMap.GetNode("0").AddNext("1");
        }

        protected override void BuildFlowMap()
        {
            if (IsTree)
                GenerateTreeFlowMap(flowMap, StepsCount, BranchCount, ShouldCreateError);
            else
                GenerateFlowMap(flowMap, StepsCount, ShouldCreateError);
            Reset();
        }
    }

    public class FlowPerfomanceTest
    {
        public int BranchCount { get; set; }
        public int FlowCount { get; set; }
        public int StepsCount { get; set; }
        public bool ShouldCreateError { get; set; }
        public bool ShouldUseCache { get; set; }
        public bool IsParallelRuning{ get; set; }
        public TimeSpan TestResult { get; set; }

        public FlowPerfomanceTest(
            int flowCount,
            int stepsCount,
            int branchCount,
            bool isParallelRuning,
            bool shouldCreateError,
            bool shouldUseCache)
        {
            BranchCount = branchCount;
            FlowCount = flowCount;
            ShouldCreateError = shouldCreateError;
            ShouldUseCache = shouldUseCache;
            StepsCount = stepsCount;
            IsParallelRuning = isParallelRuning;
        }
    }
}

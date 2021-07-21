using Flow;
using Flow.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            + "TestResult |".PadLeft(tableRowLeftPadMax, ' ');

        public static string CreateTableRow(FlowPerfomanceTest test) =>
            $"{test.FlowCount} |".ToString().PadLeft(tableRowLeftPadMin, ' ')
            + $"{test.StepsCount} |".ToString().PadLeft(tableRowLeftPadMin, ' ')
            + $"{test.BranchCount} |".ToString().PadLeft(tableRowLeftPadMin, ' ')
            + $"{test.IsParallelRuning} |".ToString().PadLeft(tableRowLeftPadMax, ' ')
            + $"{test.TestResult} |".ToString().PadLeft(tableRowLeftPadMax, ' ');

        public FlowPerfomanceTest[] DoPerfomanceTest()
        {
            var tests = new FlowPerfomanceTest[]
            {
                new FlowPerfomanceTest(100, 10, 1, false),
                new FlowPerfomanceTest(100, 10, 1, true),
                new FlowPerfomanceTest(100, 10, 2, false),
                new FlowPerfomanceTest(100, 10, 2, true),
                new FlowPerfomanceTest(100, 10, 5, false),
                new FlowPerfomanceTest(100, 10, 5, true),
                new FlowPerfomanceTest(1000, 10, 1, false),
                new FlowPerfomanceTest(1000, 10, 1, true),
                new FlowPerfomanceTest(1000, 10, 2, false),
                new FlowPerfomanceTest(1000, 10, 2, true),
                new FlowPerfomanceTest(1000, 10, 5, false),
                new FlowPerfomanceTest(1000, 10, 5, true),
                new FlowPerfomanceTest(1000, 100, 1, false),
                new FlowPerfomanceTest(1000, 100, 1, true),
                new FlowPerfomanceTest(1000, 100, 2, false),
                new FlowPerfomanceTest(1000, 100, 2, true),
                new FlowPerfomanceTest(1000, 100, 5, false),
                new FlowPerfomanceTest(1000, 100, 5, true),
            };

            foreach (var test in tests)
            {
                var testTime = new TimeSpan();
                for (var index = 0; index < testsRepeatCount; index++)
                {
                    var sw = Stopwatch.StartNew();
                    var flows = GenerateFlows(test.FlowCount, test.StepsCount, test.BranchCount);
                    RunFlows(flows, test.IsParallelRuning);
                    sw.Stop();
                    testTime += sw.Elapsed;
                }
                test.TestResult = testTime / testsRepeatCount;
            }
            return tests;
        }

        private void RunFlows(List<FlowPerfomanceInctance> flows, bool isParallel)
        {
            if (isParallel)
                Parallel.ForEach(flows, flow => flow.RunFlow());
            else 
                flows.ForEach(flow => flow.RunFlow());
        }

        private List<FlowPerfomanceInctance> GenerateFlows(int flowCount, int stepsCount, int branchCount)
        {
            var flows = new List<FlowPerfomanceInctance>();
            for (var index = 0; index < flowCount; index++)
            {
                FlowPerfomanceInctance.BranchCount = branchCount < 1 ? 1 : branchCount;
                FlowPerfomanceInctance.StepsCount = stepsCount < 1 ? 1 : stepsCount;
                flows.Add(new FlowPerfomanceInctance());
            }
            return flows;
        }
    }

    class FlowPerfomanceContext : FlowContext { }

    class FlowPerfomanceInctance : Flow<FlowPerfomanceContext>
    {
        public static bool IsTree => BranchCount > 1;
        public static int BranchCount { get; set; } = 1;
        public static int StepsCount { get; set; } = 1;

        public static void Reset()
        {
            BranchCount = 1;
            StepsCount = 1;
        }

        protected void GenerateFlowMap(IFlowMap<FlowPerfomanceContext> flowMap, int stepsCount)
        {
            Action<FlowPerfomanceContext> flowAction = ctx => { };
            flowMap.AddRoot("0", flowAction);

            if (stepsCount < 2)
                return;

            for (var index = 1; index < stepsCount; index++)
                flowMap.GetNode((index - 1).ToString()).AddNext(index.ToString(), flowAction);
        }

        protected void GenerateTreeFlowMap(IFlowMap<FlowPerfomanceContext> flowMap, int stepsCount, int branchCount)
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
        }

        protected override void BuildFlowMap()
        {
            if (IsTree)
                GenerateTreeFlowMap(flowMap, StepsCount, BranchCount);
            else
                GenerateFlowMap(flowMap, StepsCount);
            Reset();
        }
    }

    public class FlowPerfomanceTest
    {
        public int BranchCount { get; set; }
        public int FlowCount { get; set; }
        public int StepsCount { get; set; }
        public bool IsParallelRuning{ get; set; }
        public TimeSpan TestResult { get; set; }

        public FlowPerfomanceTest(int flowCount, int stepsCount, int branchCount, bool isParallelRuning)
        {
            BranchCount = branchCount;
            FlowCount = flowCount;
            StepsCount = stepsCount;
            IsParallelRuning = isParallelRuning;
        }
    }
}

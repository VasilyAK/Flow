using Flow;
using System;
using System.Threading.Tasks;

namespace FlowConsoleTests
{
    public enum IndexExample
    {
        FirstStep,
        SecondStep,
        ThirdStep,
        FourthStep,
        FifthStep,
    }

    public class FlowContextExample : FlowContext
    {
        public string SelectedBranch { get; set; } = "No branch";
        public int FirstValue { get; set; }
        public int SecondValue { get; set; }
        public int ThirdValue { get; set; }
        public object NonControlResource { get; set; }

        public new void Dispose() => NonControlResource = null; //.Dispose()
    }

    public class FlowExample : Flow<FlowContextExample>
    {
        public FlowExample() : base(FlowCache<FlowExample>()) { } //micro optimization

        public (string branch, int summ) GetResult() => ProcessContext(RunFlow());

        public async Task<(string branch, int summ)> GetResultAsync() => ProcessContext(await RunFlowAsync());

        protected override void BuildFlowMap()
        {
            flowMap
                .AddRoot(IndexExample.FirstStep, FirstStepAction)
                .AddNext(IndexExample.SecondStep, SecondStepAction)
                .AddNext(IndexExample.ThirdStep, ThirdStepAction);

            flowMap
                .GetNode(IndexExample.FirstStep)
                .AddNext(IndexExample.FourthStep, FourthStepAction)
                .AddNext(IndexExample.FifthStep, FirthStepAction);
        }

        private static void FirstStepAction(FlowContextExample ctx)
        {
            ctx.FirstValue = new Random().Next(10);
            if (ctx.FirstValue < 5)
            {
                ctx.SelectedBranch = "Left branch";
                ctx.SetNext(IndexExample.SecondStep);
            }
            else
            {
                ctx.SelectedBranch = "Right branch";
                ctx.SetNext(IndexExample.FourthStep);
            }
        }

        private static void SecondStepAction(FlowContextExample ctx)
        {
            ctx.SecondValue = 10;
        }

        private static Task ThirdStepAction(FlowContextExample ctx)
        {
            ctx.ThirdValue = 20;
            return Task.CompletedTask;
        }

        private static Task FourthStepAction(FlowContextExample ctx)
        {
            ctx.SecondValue = 100;
            return Task.CompletedTask;
        }

        private static void FirthStepAction(FlowContextExample ctx)
        {
            ctx.ThirdValue = 200;
        }

        private (string branch, int summ) ProcessContext(FlowContextExample context)
        {
            var summ = context.FirstValue + context.SecondValue + context.ThirdValue;
            return (context.SelectedBranch, summ);
        }
    }
}

using Flow;
using Flow.Interfaces;
using FluentAssertions;
using System.Linq;
using System.Threading;

namespace FlowTests.Fakes
{
    public class FakeFlow : Flow<FakeFlowContext>
    {
        public FakeFlow(FakeFlowContext context) : base(context) { }

        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1.ToString(), FlowNodeAction1);
        }

        private void FlowNodeAction1(FakeFlowContext ctx) { }
    }

    public class FakeFlow1 : Flow<FakeFlowContext>
    {
        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1.ToString(), FlowNodeAction1)
                .AddNext(FakeNodeIndex.Index2.ToString(), FlowNodeAction2);
        }

        private void FlowNodeAction1(FakeFlowContext ctx) => ctx.SetNext(FakeNodeIndex.Index2.ToString());
        private void FlowNodeAction2(FakeFlowContext ctx) { }
    }

    public class FakeFlow2 : Flow<FakeFlowContext1>
    {
        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1.ToString(), FlowNodeAction1);
        }

        private void FlowNodeAction1(FakeFlowContext1 ctx) => ctx.ThreadIdInProcess = Thread.CurrentThread.ManagedThreadId;
    }

    public class FakeFlow3 : Flow<FakeFlowContext>
    {
        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1.ToString(), FlowNodeAction1)
                .AddNext(FakeNodeIndex.Index2.ToString())
                .AddNext(FakeNodeIndex.Index3.ToString());

            flowMap.GetNode(FakeNodeIndex.Index1.ToString())
                .AddNext(FakeNodeIndex.Index4.ToString())
                .AddNext(FakeNodeIndex.Index3.ToString());

            flowMap.GetNode(FakeNodeIndex.Index1.ToString())
                .AddNext(FakeNodeIndex.Index5.ToString(), FlowNodeAction5);
        }

        private void FlowNodeAction1(FakeFlowContext ctx) => ctx.SetNext(FakeNodeIndex.Index5.ToString());
        private void FlowNodeAction5(FakeFlowContext ctx) => ctx.SetNext(FakeNodeIndex.Index3.ToString());
    }

    public class FakeFlow4 : Flow<FakeFlowContext>
    {
        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1.ToString(), FlowNodeAction1)
                .AddNext(FakeNodeIndex.Index2.ToString(), FlowNodeAction2);
        }

        private void FlowNodeAction1(FakeFlowContext ctx)
        {
            // Assert
            ctx.CompletedNodes.Should().BeEmpty();
            ctx.CurrentFlowNode.Index.Should().Be(FakeNodeIndex.Index1.ToString());
            ctx.NextFlowNode.Should().BeNull();
            ctx.PreviousFlowNode.Should().BeNull();
            ctx.SetNext(FakeNodeIndex.Index2.ToString());
            ctx.NextFlowNode.Index.Should().Be(FakeNodeIndex.Index2.ToString());
        }

        private void FlowNodeAction2(FakeFlowContext ctx)
        {
            // Assert
            ctx.CompletedNodes.Select(fn => fn.Index).Should().BeEquivalentTo(new string[] { FakeNodeIndex.Index1.ToString() });
            ctx.CurrentFlowNode.Index.Should().Be(FakeNodeIndex.Index2.ToString());
            ctx.NextFlowNode.Should().BeNull();
            ctx.PreviousFlowNode.Index.Should().Be(FakeNodeIndex.Index1.ToString());
        }
    }

    public class FakeFlow5 : Flow<FakeFlowContext>
    {
        protected override void BuildFlowMap() { }
    }

    public class FakeFlow6 : Flow<FakeFlowContext>
    {
        public IFlowMap<FakeFlowContext> FlowMap => flowMap;

        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1.ToString());
        }
    }

    public class FakeFlow7 : Flow<FakeFlowContext>
    {
        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1.ToString(), FlowNodeAction1);
        }

        private void FlowNodeAction1(FakeFlowContext ctx) => ctx.SetNext(FakeNodeIndex.Index2.ToString());
    }

    public class FakeFlow8 : Flow<FakeFlowContext>
    {
        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1, FlowNodeAction1)
                .AddNext(FakeNodeIndex.Index2, FlowNodeAction2);
        }

        private void FlowNodeAction1(FakeFlowContext ctx) { }
        private void FlowNodeAction2(FakeFlowContext ctx) { }
    }

    public class FakeFlow9 : Flow<FakeFlowContext>
    {
        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1.ToString(), FlowNodeAction1)
                .AddNext(FakeNodeIndex.Index2.ToString(), FlowNodeAction2);

            flowMap.GetNode(FakeNodeIndex.Index1.ToString())
                .AddNext(FakeNodeIndex.Index3.ToString(), FlowNodeAction3);
        }

        private void FlowNodeAction1(FakeFlowContext ctx) { }
        private void FlowNodeAction2(FakeFlowContext ctx) { }
        private void FlowNodeAction3(FakeFlowContext ctx) { }
    }

    public class FakeFlow10 : Flow<FakeFlowContext2>
    {
        public FakeFlow10(FakeFlowContext2 context2) : base(context2) { }
 
        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1, FlowNodeAction1)
                .AddNext(FakeNodeIndex.Index2, FlowNodeAction2);
        }

        private void FlowNodeAction1(FakeFlowContext2 ctx) { }
        private void FlowNodeAction2(FakeFlowContext2 ctx) { }
    }

    public class FakeFlow11 : Flow<FakeFlowContext>
    {
        public FakeFlow11() : base(FlowCache<FakeFlow11>()) { }

        protected override void BuildFlowMap() { }
    }

    public class FakeFlow12 : Flow<FakeFlowContext>
    {
        public FakeFlow12() : base(FlowCache<FakeFlow12>()) { }

        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1.ToString(), FlowNodeAction1);
        }

        private void FlowNodeAction1(FakeFlowContext ctx) => ctx.SetNext(FakeNodeIndex.Index2.ToString());
    }

    public class FakeFlow13 : Flow<FakeFlowContext>
    {
        public FakeFlow13() : base(FlowCache<FakeFlow13>()) { }

        public FakeFlow13(FakeFlowContext context) : base(context) { }

        protected override void BuildFlowMap()
        {
            flowMap.AddRoot(FakeNodeIndex.Index1.ToString(), FlowNodeAction1);
        }

        private void FlowNodeAction1(FakeFlowContext ctx) { }
    }
}

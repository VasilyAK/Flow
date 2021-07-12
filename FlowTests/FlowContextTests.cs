using Flow;
using Flow.Exceptions;
using Flow.GraphNodes;
using FlowTests.Fakes;
using FluentAssertions;
using System;
using Xunit;

namespace FlowTests
{
    public class FlowContextTests
    {
        [Fact]
        public void CreatedFlowContext_ShouldHaveDefautProperties()
        {
            // Act
            var context = new FakeFlowContext();

            // Assert
            context.CompletedNodes.Should().BeEmpty();
            context.CurrentFlowNode.Should().BeNull();
            context.NextFlowNode.Should().BeNull();
            context.PreviousFlowNode.Should().BeNull();
        }

        #region SetNext
        [Fact]
        public void SetNext_ShouldSetNexFlowNodeToExecut()
        {
            // Arrange
            var context = new FakeFlowContext();
            Func<string, IReadOnlyFlowNode> eventHandler = (string flowNodeIndex) => new FlowNode<FlowContext>(FakeNodeIndex.Index2.ToString());
            context.SubscribeSetNextEvent(eventHandler);

            // Act
            context.SetNext(FakeNodeIndex.Index2.ToString());

            // Assert
            context.NextFlowNode.Index.Should().Be(FakeNodeIndex.Index2.ToString());
        }

        [Fact]
        public void SetNext_ShouldThrow_IfFlowNodeNotExist()
        {
            // Arrange
            var flow = new FakeFlow7();

            // Act
            Action act = () => flow.RunFlow();

            // Assert
            var error = act.Should().Throw<FlowExecutionException>();
            error.WithMessage("Flow execution error. Flow does not contain flow node.\r\n    - FlowNodeIndex: Index2.");
        }
        #endregion
    }
}

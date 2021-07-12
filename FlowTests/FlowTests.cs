using Flow.Exceptions;
using FlowTests.Fakes;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FlowTests
{
    public class FlowTests
    {
        [Fact]
        public void CreatedFlow_ShouldThrow_IfFlowMapIsInvalid()
        {
            // Act
            Action act = () => { var flow = new FakeFlow5(); };

            // Assert
            var error = act.Should().Throw<FlowException>();
        }

        [Fact]
        public void CreatedFlow_ShouldBanToMutateFlowMapOutsideOfConstructor()
        {
            // Arrange
            var flow = new FakeFlow6();

            // Act
            Action act = () => flow.FlowMap.GetRoot().AddNext(FakeNodeIndex.Index2.ToString());

            // Assert
            act.Should().Throw<NullReferenceException>();
        }

        #region RunFlow
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RunFlow_ShoulExecuteAllAction(bool isAsync)
        {
            // Arrange
            var flow = new FakeFlow1();

            // Act
            var result = isAsync ? await flow.RunFlowAsync() : flow.RunFlow();

            // Assert
            result.CompletedNodes
                .Select(fn => fn.Index)
                .Should().BeEquivalentTo(new string[]
                {
                    FakeNodeIndex.Index1.ToString(),
                    FakeNodeIndex.Index2.ToString()
                });
        }

        [Fact]
        public void RunFlow_ShoulExecuteAllActionInSameThred()
        {
            // Arrange
            var flow = new FakeFlow2();
            var threadIdBeforeProcess = Thread.CurrentThread.ManagedThreadId;

            // Act
            var result = flow.RunFlow();

            // Assert
            var threadIdAfterProcess = Thread.CurrentThread.ManagedThreadId;
            threadIdBeforeProcess.Should().Be(result.ThreadIdInProcess);
            result.ThreadIdInProcess.Should().Be(threadIdAfterProcess);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RunFlow_ShouldThrow_IfNodeNotAllowedToExecute(bool isAsync)
        {
            // Arrange
            var flow = new FakeFlow3();

            // Act
            Func<Task> act;
            if (isAsync)
                act = async () => await flow.RunFlowAsync();
            else
                act = () =>
                {
                    flow.RunFlow();
                    return Task.CompletedTask;
                };

            // Assert
            var expected = "Flow execution error. No flow route found to execute the flow action after \"Index5\"."
                + "\r\n    - FlowNodeIndex: Index3."
                + "\r\n    - Found 2 possible flow routes:"
                + "\r\n        Index3 -> Index2 -> Index1"
                + "\r\n        Index3 -> Index4 -> Index1";
            var error = act.Should().Throw<FlowExecutionException>();
            error.WithMessage(expected);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RunFlow_ShouldExecuteActionWithCurrentActionProperties(bool isAsync)
        {
            // Arrange
            var flow = new FakeFlow4();

            // Act
            var result = isAsync ? await flow.RunFlowAsync() : flow.RunFlow();

            // Assert
            result.CompletedNodes.Select(fn => fn.Index).Should().BeEquivalentTo(new string[] { FakeNodeIndex.Index1.ToString(), FakeNodeIndex.Index2.ToString() });
            result.CurrentFlowNode.Should().BeNull();
            result.NextFlowNode.Should().BeNull();
            result.PreviousFlowNode.Index.Should().Be(FakeNodeIndex.Index2.ToString());
        }
        #endregion
    }
}

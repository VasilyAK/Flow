using Flow;
using Flow.Contracts;
using Flow.Exceptions;
using Flow.Extensions;
using FlowTests.Fakes;
using FluentAssertions;
using Moq;
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

        [Fact]
        public void CreateFlow_ShouldThrowErrorFromFlowCache()
        {
            // Arrange
            var errorMessageFromCache = "Error from cache.";
            FakeFlow11.FlowCache<FakeFlow11>().FlowMapCreatingError = new FlowMapValidationError
            {
                Message = errorMessageFromCache
            };

            // Act
            Action act = () => { var flow = new FakeFlow11(); };

            // Assert
            var error = act.Should().Throw<FlowException>();
            error.WithMessage(errorMessageFromCache);
            Flow<FakeFlowContext>.FlowCache<Flow<FakeFlowContext>>().Should().BeEquivalentTo(new FlowCache());
            FakeFlow11.FlowCache<FakeFlow11>().Flush();
        }

        [Fact]
        public void CreatedFlow_ShouldSetFlowCache()
        {
            // Act
            Action act = () => { var flow = new FakeFlow11(); };

            // Assert
            var error = act.Should().Throw<FlowException>();
            FakeFlow11.FlowCache<FakeFlow11>().Should().BeEquivalentTo(new FlowCache
            {
                FlowMapCreatingError = new FlowMapValidationError
                {
                    Message = error.And.Message,
                },
            });
            FakeFlow11.FlowCache<FakeFlow11>().Flush();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreatedFlow_ShouldUseExternalContext(bool isAsync)
        {
            // Arrange
            var flowContext = new FakeFlowContext2();
            var flow = new FakeFlow10(flowContext);

            // Act
            var result = isAsync ? await flow.RunFlowAsync() : flow.RunFlow();

            result.TestData.Should().Be(flowContext.TestData);
        }

        #region Dispose
        [Fact]
        public void Dispose_ShouldDisposeContext()
        {
            // Arrange
            var context = new Mock<FakeFlowContext>();
            var flow = new FakeFlow(context.Object);

            // Act
            flow.Dispose();

            // Assert
            context.Verify(x => x.Dispose(), Times.Once);
        }
        #endregion

        #region RunFlow
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RunFlow_ShouldExecuteAllAction(bool isAsync)
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
        public void RunFlow_ShouldExecuteAllActionInSameThred()
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RunFlow_ShouldAutoSetNextStep(bool isAsync)
        {
            // Arrange
            var flow = new FakeFlow8();

            // Act
            var result = isAsync ? await flow.RunFlowAsync() : flow.RunFlow();

            // Assert
            result.CompletedNodes
                .Select(fn => fn.Index)
                .Should().BeEquivalentTo(new string[]
                {
                    FakeNodeIndex.Index1.FullName(),
                    FakeNodeIndex.Index2.FullName()
                });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RunFlow_ShouldThrowIfSomePossibleStepsAndUseAutoNextStep(bool isAsync)
        {
            // Arrange
            var flow = new FakeFlow9();

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
            var expected = "Flow execution error. Flow has detected possible next steps, but none of them is selected."
                + "\r\n    - Possible steps:"
                + "\r\n        Index2"
                + "\r\n        Index3"
                + "\r\n    - FlowNodeIndex: Index1.";
            var error = act.Should().Throw<FlowExecutionException>();
            error.WithMessage(expected);
        }
        #endregion
    }
}

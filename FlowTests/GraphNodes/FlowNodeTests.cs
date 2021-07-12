using FluentAssertions;
using Flow.Contracts;
using Flow.Contracts.Enums;
using Flow.Exceptions;
using Flow.GraphNodes;
using FlowTests.Fakes;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Flow.GraphMaps;
using System.Collections.Generic;

namespace FlowTests.GraphNodes
{
    public class FlowNodeTests
    {
        [Fact]
        public void CreatingFlowNode_ShoulHaveDefaultProperties()
        {
            // Act
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());

            // Assert
            flowNode.DirectedGraphMap.Should().BeNull();
            flowNode.GraphMap.Should().BeNull();
            flowNode.HasAction.Should().BeFalse();
            flowNode.NextNodes.Should().BeEmpty();
            flowNode.Index.Should().Be(FakeNodeIndex.Index1.ToString());
            flowNode.IsValid.Should().BeTrue();
            flowNode.Type.Should().Be(FlowNodeType.Variable);
            flowNode.ValidationErrors.Should().BeEmpty();
        }

        #region AddAction
        [Fact]
        public void AddAction_ShouldSetFlowNodeAction()
        {
            // Arrange
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            Action<FakeFlowContext> flowNodeAction = ctx => { };

            // Act
            flowNode.AddAction(flowNodeAction);

            // Assert
            flowNode.HasAction.Should().BeTrue();
        }

        [Fact]
        public void AddAction_ShouldSetAsyncFlowNodeAction()
        {
            // Arrange
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            Action<FakeFlowContext> flowNodeAction = async ctx => { await Task.CompletedTask; };

            // Act
            flowNode.AddAction(flowNodeAction);

            // Assert
            flowNode.HasAction.Should().BeTrue();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void AddAction_ShouldThrow_IfActionAlreadyAssign(bool isAsyncExistedAction, bool isAsyncAction)
        {
            // Arrange
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            Action<FakeFlowContext> existedFlowNodeAction = ctx => { };
            Func<FakeFlowContext, Task> existedFlowNodeActionAsync = async ctx => { await Task.CompletedTask; };
            if (isAsyncExistedAction)
                flowNode.AddAction(existedFlowNodeAction);
            else
                flowNode.AddAction(existedFlowNodeActionAsync);

            Action<FakeFlowContext> flowNodeAction = ctx => { };
            Func<FakeFlowContext, Task> flowNodeActionAsync = async ctx => { await Task.CompletedTask; };

            // Act
            Action act;
            if (isAsyncAction)
                act = () => flowNode.AddAction(flowNodeActionAsync);
            else
                act = () => flowNode.AddAction(flowNodeAction);

            // Assert
            var error = act.Should().Throw<CreatingFlowMapException>();
            error.WithMessage("Error creating flow map. An action has already been assigned to the flow node. Reassigning an action in runtime is prohibited.\r\n    - FlowNodeIndex: Index1.");
        }

        [Fact]
        public void AddAction_ShouldReturnThis()
        {
            // Arrange
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            Action<FakeFlowContext> flowNodeAction = ctx => { };

            // Act
            var result = flowNode.AddAction(flowNodeAction);

            // Assert
            ReferenceEquals(result, flowNode).Should().BeTrue();
        }
        #endregion

        #region AddNext
        [Fact]
        public void AddNext_ShouldReturnFlowNodeWithSameIndex()
        {
            // Arrange
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());

            // Act
            var result = flowNode.AddNext(FakeNodeIndex.Index2.ToString());

            // Assert
            result.Index.Should().Be(FakeNodeIndex.Index2.ToString());
        }

        [Fact]
        public void AddNext_ShouldAddExistedFlowNode()
        {
            // Arrange
            var existedFlowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());

            // Act
            var result = existedFlowNode.AddNext(FakeNodeIndex.Index1.ToString());

            // Assert
            result.Equals(existedFlowNode).Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddNext_ShouldAddExistedFlowNodeWithNewAction(bool isAsyncAction)
        {
            // Arrange
            var existedFlowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            Action<FakeFlowContext> flowNodeAction = ctx => { };
            Func<FakeFlowContext, Task> flowNodeActionAsync = async ctx => { await Task.CompletedTask; };

            // Act
            var result = isAsyncAction
                ? existedFlowNode.AddNext(FakeNodeIndex.Index1.ToString(), flowNodeActionAsync)
                : existedFlowNode.AddNext(FakeNodeIndex.Index1.ToString(), flowNodeAction);

            // Assert
            result.Equals(existedFlowNode).Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddNext_ShouldReturnFlowNodeWithSameIndexAndAction(bool isAsyncAction)
        {
            // Arrange
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            Action<FakeFlowContext> flowNodeAction = ctx => { };
            Func<FakeFlowContext, Task> flowNodeActionAsync = async ctx => { await Task.CompletedTask; };

            // Act
            var result = isAsyncAction
                ? flowNode.AddNext(FakeNodeIndex.Index2.ToString(), flowNodeActionAsync)
                : flowNode.AddNext(FakeNodeIndex.Index2.ToString(), flowNodeAction);

            // Assert
            result.Index.Should().Be(FakeNodeIndex.Index2.ToString());
            result.HasAction.Should().BeTrue();
        }

        [Fact]
        public void AddNext_ShouldReturnAddedFlowNode()
        {
            // Arrange
            var flowNode1 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index2.ToString());

            // Act
            var result = flowNode1.AddNext(flowNode2);

            // Assert
            result.Should().BeEquivalentTo(flowNode2);
        }

        [Fact]
        public void AddNext_ShouldDetectedReAddingFlowNodes()
        {
            // Arrange
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());

            // Act
            flowNode.AddNext(FakeNodeIndex.Index2.ToString());
            flowNode.AddNext(FakeNodeIndex.Index2.ToString());

            // Assert
            flowNode.IsValid.Should().BeFalse();
            flowNode.ValidationErrors.Should().HaveCount(1);
            flowNode.ValidationErrors.First().Should().BeEquivalentTo(new FlowNodeValidationError
            {
                FlowRoute = new string[] { FakeNodeIndex.Index2.ToString(), FakeNodeIndex.Index1.ToString() },
                Message = "Re-adding flow node detected.",
                Type = FlowNodeErrorType.ReAddingNextFlowNode,
            });
        }
        #endregion

        #region CloneWithoutDirections
        [Fact]
        public void CloneWithoutDirections_ShouldCloneBaseProperties()
        {
            // Arange
            var flowNode1 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index3.ToString());

            flowNode2.AddNext(flowNode1);
            flowNode1.AddNext(flowNode3);
            //flowNode1.GraphMapContainer = null;

            // Act
            var clone = flowNode1.CloneWithoutDirections();

            // assert
            clone.Should().BeEquivalentTo(
                flowNode1,
                options =>
                {
                    options.ComparingByMembers<FlowNode<FakeFlowContext>>();
                    options.Using<FlowMap<FakeFlowContext>>(x => x.Subject.Should().BeNull()).When(x => x.SelectedMemberPath.EndsWith("FlowMap"));
                    options.Using<object>(x => x.Subject.Should().BeNull()).When(x => x.SelectedMemberPath.EndsWith("GraphMapContainer"));
                    options.Using<Dictionary<string, DirectedGraphNode>>(x => x.Subject.Should().BeEmpty()).When(x => x.SelectedMemberPath.EndsWith("NextNodes"));
                    options.Using<Dictionary<string, DirectedGraphNode>>(x => x.Subject.Should().BeEmpty()).When(x => x.SelectedMemberPath.EndsWith("PreviousNodes"));
                    return options;
                });
            clone.Equals(flowNode1).Should().BeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CloneWithoutDirections_ShouldCloneWithFlowNodeAction(bool isAsyncAction)
        {
            // Arange
            FlowNode<FakeFlowContext> flowNode;
            if (isAsyncAction)
            {
                Func<FakeFlowContext, Task> flowNodeAction = async ctx => await Task.CompletedTask;
                flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString(), flowNodeAction);
                
            }
            else
            {
                Action<FakeFlowContext> flowNodeAction = ctx => { };
                flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString(), flowNodeAction);
            }

            // Act
            var clone = flowNode.CloneWithoutDirections();

            // assert
            clone.Should().BeEquivalentTo(
                flowNode,
                options =>
                {
                    options.ComparingByMembers<FlowNode<FakeFlowContext>>();
                    options.Using<FlowMap<FakeFlowContext>>(x => x.Subject.Should().BeNull()).When(x => x.SelectedMemberPath.EndsWith("FlowMap"));
                    options.Using<object>(x => x.Subject.Should().BeNull()).When(x => x.SelectedMemberPath.EndsWith("GraphMapContainer"));
                    return options;
                });
            clone.Equals(flowNode).Should().BeFalse();
        }

        [Fact]
        public void CloneWithoutDirections_ShouldNotCloneDirections()
        {
            // Arange
            var flowNode1 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index3.ToString());
            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode3);

            // Act
            var clone = flowNode2.CloneWithoutDirections();

            // assert
            clone.PreviousNodes.Should().BeEmpty();
            clone.NextNodes.Should().BeEmpty();
        }
        #endregion

        #region Run
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Run_ShouldExecuteFlowNodeAction(bool isAsync)
        {
            // Arrange
            var isActionExecuted = false;
            Action<FakeFlowContext> flowNodeAction = ctx => { isActionExecuted = true; };
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString(), flowNodeAction);
            var flowContext = new FakeFlowContext() { };

            // Act
            if (isAsync)
                await flowNode.RunAsync(flowContext);
            else
                flowNode.Run(flowContext);

            // Assert
            isActionExecuted.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Run_ShouldExecuteAsyncFlowNodeAction(bool isAsync)
        {
            // Arrange
            var isActionExecuted = false;
            Func<FakeFlowContext, Task> flowNodeAction = async ctx => { isActionExecuted = true; await Task.CompletedTask; };
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString(), flowNodeAction);
            var flowContext = new FakeFlowContext() { };

            // Act
            if (isAsync)
                await flowNode.RunAsync(flowContext);
            else
                flowNode.Run(flowContext);

            // Assert
            isActionExecuted.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Run_ShouldThrow_IfFlowNodeHasNoAction(bool isAsync)
        {
            // Arrange
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            var flowContext = new FakeFlowContext() { };

            // Act
            Func<Task> act;
            if (isAsync)
                act = async () => await flowNode.RunAsync(flowContext);
            else
                act = () =>
                {
                    flowNode.Run(flowContext);
                    return Task.CompletedTask;
                };

            // Assert
            act.Should().Throw<FlowExecutionException>().WithMessage("Flow execution error. Unable to assign action to flow node.\r\n    - FlowNodeIndex: Index1.");
        }
        #endregion

        #region ValidationErrors
        [Fact]
        public void ValidationErrors_ShouldContainsCyclicDependencyError()
        {
            // Arrange
            var flowNode1 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index2.ToString());

            // Act
            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode1);

            // Assert
            flowNode1.IsValid.Should().BeFalse();
            flowNode2.IsValid.Should().BeFalse();
            flowNode1.ValidationErrors.Should().HaveCount(1);
            flowNode1.ValidationErrors.First().Should().BeEquivalentTo(new FlowNodeValidationError
            {
                FlowRoute = new string[] { FakeNodeIndex.Index1.ToString(), FakeNodeIndex.Index2.ToString(), FakeNodeIndex.Index1.ToString() },
                Message = "Cyclic execution detected.",
                Type = FlowNodeErrorType.CyclicDependency,
            });
            flowNode2.ValidationErrors.Should().HaveCount(1);
            flowNode2.ValidationErrors.First().Should().BeEquivalentTo(new FlowNodeValidationError
            {
                FlowRoute = new string[] { FakeNodeIndex.Index2.ToString(), FakeNodeIndex.Index1.ToString(), FakeNodeIndex.Index2.ToString() },
                Message = "Cyclic execution detected.",
                Type = FlowNodeErrorType.CyclicDependency,
            });
        }

        [Fact]
        public void ValidationErrors_ShouldContainsInvalidRootError()
        {
            // Arrange
            var flowNode1 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index2.ToString(), null, FlowNodeType.Root);

            // Act
            flowNode1.AddNext(flowNode2);

            // Assert
            flowNode1.IsValid.Should().BeTrue();
            flowNode2.IsValid.Should().BeFalse();
            flowNode2.ValidationErrors.Should().HaveCount(1);
            flowNode2.ValidationErrors.First().Should().BeEquivalentTo(new FlowNodeValidationError
            {
                Message = "Root flow node has parent nodes.",
                Type = FlowNodeErrorType.InvalidRootFlowNode,
            });
        }

        [Fact]
        public void ValidationErrors_ShouldContansMixOfErrors()
        {
            // Arrange
            var flowNode1 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index2.ToString());
            var rootFlowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index3.ToString(), null, FlowNodeType.Root);


            // Act
            flowNode1.AddNext(rootFlowNode);
            flowNode1.AddNext(rootFlowNode);
            flowNode1.AddNext(flowNode1);
            rootFlowNode.AddNext(flowNode2);

            // Assert
            flowNode1.IsValid.Should().BeFalse();
            rootFlowNode.IsValid.Should().BeFalse();
            flowNode2.IsValid.Should().BeTrue();

            flowNode1.ValidationErrors.Should().HaveCount(2);
            flowNode1.ValidationErrors.Should().Contain(ve => ve.Type.Equals(FlowNodeErrorType.ReAddingNextFlowNode));
            flowNode1.ValidationErrors.Should().Contain(ve => ve.Type.Equals(FlowNodeErrorType.CyclicDependency));

            rootFlowNode.ValidationErrors.Should().HaveCount(1);
            rootFlowNode.ValidationErrors.Should().Contain(ve => ve.Type.Equals(FlowNodeErrorType.InvalidRootFlowNode));
        }

        [Fact]
        public void ValidationErrors_ShouldAlwaysReturnTheSameErrors()
        {
            // Arrange
            var rootFlowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString(), null, FlowNodeType.Root);

            // Act
            rootFlowNode.AddNext(rootFlowNode);

            // Assert
            var validationErrors1 = rootFlowNode.ValidationErrors;
            var validationErrors2 = rootFlowNode.ValidationErrors;
            validationErrors1.Should().BeEquivalentTo(validationErrors2);
            validationErrors2.Should().OnlyHaveUniqueItems();
        }
        #endregion
    }
}

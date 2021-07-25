using Flow.Contracts;
using Flow.Contracts.Enums;
using Flow.Exceptions;
using Flow.Extensions;
using Flow.GraphMaps;
using Flow.GraphNodes;
using FlowTests.Fakes;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FlowTests.GraphMaps
{
    public class FlowMapTests
    {
        #region AddRoot
        [Fact]
        public void AddRoot_ShouldAddRootFlowNodeWithTypeRootIntoFlowMap()
        {
            // Arrange
            var flowMap = new FlowMap<FakeFlowContext>();

            // Act
            flowMap.AddRoot(FakeNodeIndex.Index1);

            // Assert
            flowMap.IsValid.Should().BeTrue();
            flowMap.GetRoot().Index.Should().Be(FakeNodeIndex.Index1.FullName());
        }

        [Fact]
        public void AddRoot_ShouldAddRootFlowNodeWithActionIntoFlowMap()
        {
            // Arrange
            Action<FakeFlowContext> flowRootNodeAction = ctx => { };
            var flowMap = new FlowMap<FakeFlowContext>();

            // Act
            flowMap.AddRoot(FakeNodeIndex.Index1, flowRootNodeAction);

            // Assert
            flowMap.IsValid.Should().BeTrue();
            flowMap.GetRoot().HasAction.Should().BeTrue();
        }

        [Fact]
        public void AddRoot_ShouldAddRootFlowNodeWithAsyncActionIntoFlowMap()
        {
            // Arrange
            Func<FakeFlowContext, Task> flowRootNodeAction = async ctx => await Task.CompletedTask;
            var flowMap = new FlowMap<FakeFlowContext>();

            // Act
            flowMap.AddRoot(FakeNodeIndex.Index1, flowRootNodeAction);

            // Assert
            flowMap.IsValid.Should().BeTrue();
            flowMap.GetRoot().HasAction.Should().BeTrue();
        }
        #endregion

        #region Clone
        [Fact]
        public void Clone_ShouldCloneFlowNodeWithoutDirection()
        {
            // Arrange
            var flowNodeMock = new Mock<FlowNode<FakeFlowContext>>(FakeNodeIndex.Index1.ToString(), FlowNodeType.Variable, true, false);
            flowNodeMock.Setup(x => x.CloneWithoutDirections()).Returns(new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString()));

            var flowMap = new FlowMap<FakeFlowContext>();
            flowMap.AddNode(flowNodeMock.Object);

            // Act
            var clone = flowMap.Clone();

            // Assert
            clone.GetAllNodes().Should().HaveCount(1);
            var cloneFlowNode = (FlowNode<FakeFlowContext>)clone.GetNode(FakeNodeIndex.Index1.ToString());
            cloneFlowNode.Equals(flowNodeMock.Object).Should().BeFalse();
            cloneFlowNode.Should().BeEquivalentTo(flowNodeMock.Object, options => options.ComparingByMembers<object>());
            flowNodeMock.Verify(x => x.CloneWithoutDirections(), Times.Once);
            clone.Equals(flowMap).Should().BeFalse();
        }

        [Fact]
        public void Clone_ShouldRestoreFlowNodeDirections()
        {
            // Arrange
            var flowNode1 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index2.ToString());
            flowNode1.AddNext(flowNode2);

            var flowMap = new FlowMap<FakeFlowContext>();
            flowMap.AddNode(flowNode1);

            // Act
            var clone = flowMap.Clone();

            // Assert
            clone.GetAllNodes().Should().HaveCount(2);
            var cloneFlowNode1 = (FlowNode<FakeFlowContext>)clone.GetNode(FakeNodeIndex.Index1.ToString());
            var cloneFlowNode2 = (FlowNode<FakeFlowContext>)clone.GetNode(FakeNodeIndex.Index2.ToString());
            cloneFlowNode1.Equals(flowNode1).Should().BeFalse();
            cloneFlowNode2.Equals(flowNode2).Should().BeFalse();
            cloneFlowNode1.NextNodes.Should().HaveCount(1);
            cloneFlowNode1.NextNodes.First().Value.Equals(cloneFlowNode2).Should().BeTrue();
            cloneFlowNode2.PreviousNodes.Should().HaveCount(1);
            cloneFlowNode2.PreviousNodes.First().Value.Equals(cloneFlowNode1).Should().BeTrue();
        }
        #endregion

        #region Create
        [Fact]
        public void Create_ShouldReturnNewFlowMap()
        {
            // Act
            var flowMap = FlowMap<FakeFlowContext>.Create();

            // Assert
            (flowMap is FlowMap<FakeFlowContext>).Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldReturnNewFlowMapWithFlowNode()
        {
            // Arrange
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());

            // Act
            var flowMap = FlowMap<FakeFlowContext>.Create(flowNode);

            // Assert
            flowMap.FindNode(FakeNodeIndex.Index1.ToString()).Equals(flowNode).Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldLinkNewFlowMapWithFlowNode()
        {
            // Arrange
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString());

            // Act
            var flowMap = FlowMap<FakeFlowContext>.Create(flowNode);

            // Assert
            ReferenceEquals(flowNode.FlowMap, flowMap).Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldReturnNewFlowMapWithoutFlowNode()
        {
            // Act
            var flowMap = FlowMap<FakeFlowContext>.Create();

            // Assert
            flowMap.GetAllNodes().Should().BeEmpty();
        }

        [Fact]
        public void Create_ShouldReturnNewFlowMapWithoutNullFlowNode()
        {
            // Act
            var flowMap = FlowMap<FakeFlowContext>.Create(null);

            // Assert
            flowMap.GetAllNodes().Should().BeEmpty();
        }
        #endregion

        #region GetNode
        [Fact]
        public void GetNode_ShouldReturnFlowNode()
        {
            // Arrange
            var rootFlowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.FullName(), null, FlowNodeType.Root);
            var flowMap = new FlowMap<FakeFlowContext>();
            flowMap.AddNode(rootFlowNode);

            // Act
            var result = flowMap.GetNode(FakeNodeIndex.Index1);

            // Assert
            result.Should().Be(rootFlowNode);
        }

        [Fact]
        public void GetNode_ShouldThrow_IfFlowNodeNotExists()
        {
            // Arrange
            var rootFlowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.FullName(), null, FlowNodeType.Root);
            var flowMap = new FlowMap<FakeFlowContext>();
            flowMap.AddNode(rootFlowNode);

            // Act
            Action act = () => flowMap.GetNode(FakeNodeIndex.Index2);

            // Assert
            var error = act.Should().Throw<CreatingFlowMapException>();
            error.WithMessage("Error creating flow map. Flow does not contain flow node.\r\n    - FlowNodeIndex: FlowTests.Fakes.FakeNodeIndex.Index2.");
        }
        #endregion

        #region GetRoot
        [Fact]
        public void GetRoot_ShouldReturnFirstRootFlowNode()
        {
            // Arrange
            var rootFlowNode1 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString(), null, FlowNodeType.Root);
            var rootFlowNode2 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index2.ToString(), null, FlowNodeType.Root);
            var flowMap = new FlowMap<FakeFlowContext>();
            flowMap.AddNode(rootFlowNode1);
            flowMap.AddNode(rootFlowNode2);

            // Act
            var result = flowMap.GetRoot();

            // Assert
            result.Should().Be(rootFlowNode1);
        }

        [Fact]
        public void GetRoot_ShouldThrow_IfRootFlowNodeNotExists()
        {
            // Arrange
            var flowMap = new FlowMap<FakeFlowContext>();

            // Act
            Action act = () => flowMap.GetRoot();

            // Assert
            var error = act.Should().Throw<CreatingFlowMapException>();
            error.WithMessage($"Error creating flow map. Flow does not contain node with type \"{FlowNodeType.Root}\".");
        }
        #endregion

        #region ValidationErrors
        [Fact]
        public void ValidationErrors_ShouldContansRootFlowNodeNotExistError()
        {
            // Act
            var flowMap = new FlowMap<FakeFlowContext>();

            // Assert
            flowMap.IsValid.Should().BeFalse();
            flowMap.ValidationErrors.Should().HaveCount(1);
            flowMap.ValidationErrors.First().Should().BeEquivalentTo(new FlowMapValidationError
            {
                Message = $"Flow does not contain root flow node.",
                Type = FlowMapErrorType.RootFlowNodeNotExist,
            });
        }

        [Fact]
        public void ValidationErrors_ShouldContainsInvalidFlowNodeError()
        {
            // Arrange
            var rootFlowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString(), null, FlowNodeType.Root);
            var flowMap = new FlowMap<FakeFlowContext>();

            // Act
            flowMap
                .AddNode(rootFlowNode)
                .AddNext(rootFlowNode);

            // Assert
            flowMap.IsValid.Should().BeFalse();
            flowMap.GetNode(FakeNodeIndex.Index1.ToString()).IsValid.Should().BeFalse();
            flowMap.ValidationErrors.Should().HaveCount(1);
            flowMap.ValidationErrors.First().Type.Should().Be(FlowMapErrorType.InvalidFlowNode);
            flowMap.ValidationErrors.First().FlowNodeIndex.Should().Be(FakeNodeIndex.Index1.ToString());
        }

        [Fact]
        public void ValidationErrors_ShouldContansMixOfErrors()
        {
            // Arrange
            var rootFlowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString(), null, FlowNodeType.Root);
            var flowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index2.ToString());
            var rootFlowNode2 = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index3.ToString(), null, FlowNodeType.Root);
            var flowMap = new FlowMap<FakeFlowContext>();

            // Act
            rootFlowNode2.AddNext(flowNode);
            flowMap
                .AddNode(rootFlowNode)
                .AddNext(flowNode)
                .AddNext(rootFlowNode);

            // Assert
            flowMap.IsValid.Should().BeFalse();
            flowMap.GetNode(FakeNodeIndex.Index1.ToString()).IsValid.Should().BeFalse();
            flowMap.GetNode(FakeNodeIndex.Index2.ToString()).IsValid.Should().BeFalse();
            flowMap.GetNode(FakeNodeIndex.Index3.ToString()).IsValid.Should().BeTrue();
            flowMap.ValidationErrors.Should().HaveCount(4);
            flowMap.ValidationErrors.Should().Contain(ve => ve.Type.Equals(FlowMapErrorType.InvalidFlowNode) && string.Equals(ve.FlowNodeIndex, FakeNodeIndex.Index1.ToString()));
            flowMap.ValidationErrors.Should().Contain(ve => ve.Type.Equals(FlowMapErrorType.InvalidFlowNode) && string.Equals(ve.FlowNodeIndex, FakeNodeIndex.Index2.ToString()));
            flowMap.ValidationErrors.Should().Contain(ve => ve.Type.Equals(FlowMapErrorType.SomeRootFlowNodesDetected) && string.Equals(ve.FlowNodeIndex, FakeNodeIndex.Index1.ToString()));
            flowMap.ValidationErrors.Should().Contain(ve => ve.Type.Equals(FlowMapErrorType.SomeRootFlowNodesDetected) && string.Equals(ve.FlowNodeIndex, FakeNodeIndex.Index3.ToString()));
        }

        [Fact]
        public void ValidationErrors_ShouldAlwaysReturnTheSameErrors()
        {
            // Arrange
            var rootFlowNode = new FlowNode<FakeFlowContext>(FakeNodeIndex.Index1.ToString(), null, FlowNodeType.Root);
            var flowMap = new FlowMap<FakeFlowContext>();

            // Act
            flowMap
                .AddNode(rootFlowNode)
                .AddNext(rootFlowNode);

            // Assert
            flowMap.ValidationErrors.Should().OnlyHaveUniqueItems();
            flowMap.ValidationErrors.First().FlowNodeErrors.Should().OnlyHaveUniqueItems();
        }
        #endregion
    }
}
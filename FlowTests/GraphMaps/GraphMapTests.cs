using Flow.Exceptions;
using Flow.GraphMaps;
using Flow.GraphNodes;
using FlowTests.Fakes;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace FlowTests.GraphMaps
{
    public class GraphMapTests
    {
        #region Create
        [Fact]
        public void Create_ShouldReturnNewGraphMap()
        {
            // Act
            var graphMap = GraphMap<GraphNode>.Create();

            // Assert
            (graphMap is GraphMap<GraphNode>).Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldReturnNewGraphMapWithGraphNode()
        {
            // Arrange
            var graphNode = new GraphNode(FakeNodeIndex.Index1.ToString());

            // Act
            var graphMap = GraphMap<GraphNode>.Create(graphNode);

            // Assert
            graphMap.FindNode(FakeNodeIndex.Index1.ToString()).Equals(graphNode).Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldLinkNewGraphMapWithGraphNode_IfNodeWithoutMap()
        {
            // Arrange
            var graphNode = new GraphNode(FakeNodeIndex.Index1.ToString());
            graphNode.GraphMap = null;

            // Act
            var graphMap = GraphMap<GraphNode>.Create(graphNode);

            // Assert
            ReferenceEquals(graphNode.GraphMap, graphMap).Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldMergeNewGraphMapWithGraphNodeMap()
        {
            // Arrange
            var graphNode1 = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new GraphNode(FakeNodeIndex.Index2.ToString());
            (graphNode1.GraphMap).AddNode<GraphMap<GraphNode>>(graphNode2);

            // Act
            var graphMap = GraphMap<GraphNode>.Create(graphNode1);

            // Assert
            ReferenceEquals(graphNode1.GraphMap, graphMap).Should().BeTrue();
            ReferenceEquals(graphNode2.GraphMap, graphMap).Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldReturnNewGraphMapWithoutGraphNode()
        {
            // Act
            var graphMap = GraphMap<GraphNode>.Create();

            // Assert
            graphMap.GetAllNodes().Should().BeEmpty();
        }

        [Fact]
        public void Create_ShouldReturnNewGraphMapWithoutNullGraphNode()
        {
            // Act
            var graphMap = GraphMap<GraphNode>.Create(null);

            // Assert
            graphMap.GetAllNodes().Should().BeEmpty();
        }
        #endregion

        #region AddNode
        [Fact]
        public void AddNode_ShouldAddGraphNodeIntoGraphMap()
        {
            // Arrange
            var graphNode = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphMap = GraphMap<GraphNode>.Create();

            // Act
            graphMap.AddNode<GraphMap<GraphNode>>(graphNode);

            // Assert
            graphMap.FindNode(FakeNodeIndex.Index1.ToString()).Equals(graphNode).Should().BeTrue();
        }

        [Fact]
        public void AddNode_ShouldNotAddNullGraphNodeIntoGraphMap()
        {
            // Arrange
            var graphMap = GraphMap<GraphNode>.Create();

            // Act
            graphMap.AddNode<GraphMap<GraphNode>>(null);

            // Assert
            graphMap.GetAllNodes().Should().BeEmpty();
        }

        [Fact]
        public void AddNode_ShouldLinkNewGraphMapWithGraphNode_IfNodewithoutMap()
        {
            // Arrange
            var graphNode = new GraphNode(FakeNodeIndex.Index1.ToString());
            graphNode.GraphMap = null;
            var graphMap = GraphMap<GraphNode>.Create();

            // Act
            graphMap.AddNode<GraphMap<GraphNode>>(graphNode);

            // Assert
            ReferenceEquals(graphNode.GraphMap, graphMap).Should().BeTrue();
        }

        [Fact]
        public void AddNode_ShouldMergeNewGraphMapWithGraphNodeMap()
        {
            // Arrange
            var graphNode1 = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new GraphNode(FakeNodeIndex.Index2.ToString());
            (graphNode1.GraphMap).AddNode<GraphMap<GraphNode>>(graphNode2);
            var graphMap = GraphMap<GraphNode>.Create();

            // Act
            graphMap.AddNode<GraphMap<GraphNode>>(graphNode1);

            // Assert
            ReferenceEquals(graphNode1.GraphMap, graphMap).Should().BeTrue();
            ReferenceEquals(graphNode2.GraphMap, graphMap).Should().BeTrue();
        }
        #endregion

        #region Equals
        [Fact]
        public void Equals_ShouldBeEqualByReference()
        {
            // Arrange
            var graphMap = GraphMap<GraphNode>.Create();

            // Act
            var result = graphMap.Equals(graphMap);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldNotBeEqualByReference()
        {
            // Arrange
            var graphMap1 = GraphMap<GraphNode>.Create();
            var graphMap2 = GraphMap<GraphNode>.Create();

            // Act
            var result = graphMap1.Equals(graphMap2);

            // Assert
            result.Should().BeFalse();
        }
        #endregion

        #region FindNode
        [Fact]
        public void FindNode_ShouldReturnGraphNode()
        {
            // Arrange
            var graphNode = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphMap = GraphMap<GraphNode>.Create(graphNode);

            // Act
            var result = graphMap.FindNode(FakeNodeIndex.Index1.ToString());

            // Assert
            result.Equals(graphNode).Should().BeTrue();
        }

        [Fact]
        public void FindNode_ShouldReturnNull_IfGraphMapNotContansGraphNode()
        {
            // Arrange
            var graphMap = GraphMap<GraphNode>.Create();

            // Act
            var result = graphMap.FindNode(FakeNodeIndex.Index1.ToString());

            // Assert
            result.Should().BeNull();
        }
        #endregion

        #region GetAllNodes
        [Fact]
        public void GetAllNodes_ShouldReturnGraphNodes()
        {
            // Arrange
            var graphNode1 = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new GraphNode(FakeNodeIndex.Index2.ToString());
            var graphNode3 = new GraphNode(FakeNodeIndex.Index3.ToString());
            var graphMap = GraphMap<GraphNode>.Create(graphNode1);
            graphMap.AddNode<GraphMap<GraphNode>>(graphNode2);
            graphMap.AddNode<GraphMap<GraphNode>>(graphNode3);

            // Act
            var result = graphMap.GetAllNodes();

            // Assert
            result.Should().HaveCount(3);
            result.Any(gn => gn.Equals(graphNode1)).Should().BeTrue();
            result.Any(gn => gn.Equals(graphNode2)).Should().BeTrue();
            result.Any(gn => gn.Equals(graphNode3)).Should().BeTrue();
        }

        [Fact]
        public void GetAllNodes_ShouldReturnEmpty()
        {
            // Arrange
            var graphMap = GraphMap<GraphNode>.Create();

            // Act
            var result = graphMap.GetAllNodes();

            // Assert
            result.Should().BeEmpty();
        }
        #endregion

        #region Merge
        [Fact]
        public void AddNext_ShouldNotMerge_IfMapsAreEqual()
        {
            // Arrange
            var graphNode1 = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphMap1 = GraphMap<GraphNode>.Create(graphNode1);
            var graphMap2 = GraphMap<GraphNode>.Create(graphNode1);


            // Act
            graphMap1.Merge(graphMap2);

            // Assert
            graphMap1.FindNode(FakeNodeIndex.Index1.ToString()).Equals(graphNode1).Should().BeTrue();
        }

        [Fact]
        public void AddNext_ShouldMergeGraphNodes()
        {
            // Arrange
            var graphNode1 = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new GraphNode(FakeNodeIndex.Index2.ToString());
            var graphMap1 = GraphMap<GraphNode>.Create(graphNode1);
            var graphMap2 = GraphMap<GraphNode>.Create(graphNode2);

            // Act
            graphMap1.Merge(graphMap2);

            // Assert
            graphMap1.FindNode(FakeNodeIndex.Index1.ToString()).Equals(graphNode1).Should().BeTrue();
            graphMap1.FindNode(FakeNodeIndex.Index2.ToString()).Equals(graphNode2).Should().BeTrue();
        }

        [Fact]
        public void AddNext_ShouldThrow_IfMergeGraphNodesWithSameGraphNodeIndexButGraphNodesNotEqual()
        {
            // Arrange
            var graphNode1 = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphMap1 = GraphMap<GraphNode>.Create(graphNode1);
            var graphMap2 = GraphMap<GraphNode>.Create(graphNode2);

            // Act
            Action act = () => graphMap1.Merge(graphMap2);

            // Assert
            var exception = act.Should().Throw<MergeGrafhsException>();
            exception.And.GraphNodeIndexes.Should().Contain(FakeNodeIndex.Index1.ToString());
            exception.WithMessage("Grafhs cannot be combined. Some grafh nodes have the same index but are not equivalent.");
        }

        [Fact]
        public void AddNext_ShouldSetSameNewMapForEachGraphNodes()
        {
            // Arrange
            var graphNode1 = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new GraphNode(FakeNodeIndex.Index2.ToString());
            var graphMap1 = GraphMap<GraphNode>.Create(graphNode1);
            var graphMap2 = GraphMap<GraphNode>.Create(graphNode2);

            // Act
            graphMap1.Merge(graphMap2);

            // Assert
            ReferenceEquals(graphNode1.GraphMap, graphNode2.GraphMap).Should().BeTrue();
        }
        #endregion
    }
}

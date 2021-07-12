using Flow.GraphMaps;
using Flow.GraphNodes;
using FlowTests.Fakes;
using FluentAssertions;
using Xunit;

namespace FlowTests.GraphMaps
{
    public class DirectedGraphMapTests
    {
        #region Create
        [Fact]
        public void Create_ShouldReturnNewDirectedGraphMap()
        {
            // Act
            var directedGraphMap = DirectedGraphMap<DirectedGraphNode>.Create();

            // Assert
            (directedGraphMap is DirectedGraphMap<DirectedGraphNode>).Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldReturnNewDirectedGraphMapWithDirectedGraphNode()
        {
            // Arrange
            var directedGraphNode = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());

            // Act
            var directedGraphMap = DirectedGraphMap<DirectedGraphNode>.Create(directedGraphNode);

            // Assert
            directedGraphMap.FindNode(FakeNodeIndex.Index1.ToString()).Equals(directedGraphNode).Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldLinkNewGraphMapWithGraphNode()
        {
            // Arrange
            var directedGraphNode = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());

            // Act
            var directedGraphMap = DirectedGraphMap<DirectedGraphNode>.Create(directedGraphNode);

            // Assert
            ReferenceEquals(directedGraphNode.DirectedGraphMap, directedGraphMap).Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldReturnNewDirectedGraphMapWithoutDirectedGraphNode()
        {
            // Act
            var directedGraphMap = DirectedGraphMap<DirectedGraphNode>.Create();

            // Assert
            directedGraphMap.GetAllNodes().Should().BeEmpty();
        }

        [Fact]
        public void Create_ShouldReturnNewDirectedGraphMapWithoutNullDirectedGraphNode()
        {
            // Act
            var directedGraphMap = DirectedGraphMap<DirectedGraphNode>.Create(null);

            // Assert
            directedGraphMap.GetAllNodes().Should().BeEmpty();
        }
        #endregion
    }
}

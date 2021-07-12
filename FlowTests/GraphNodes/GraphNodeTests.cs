using Flow.GraphNodes;
using FlowTests.Fakes;
using FluentAssertions;
using Xunit;

namespace FlowTests.GraphNodes
{
    public class GraphNodeTests
    {
        #region Equals
        [Fact]
        public void Equals_ShouldBeEqualByReference()
        {
            // Arrange
            var graphNode = new GraphNode(FakeNodeIndex.Index1.ToString());

            // Act
            var result = graphNode.Equals(graphNode);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldNotBeEqualByReference()
        {
            // Arrange
            var graphNode1 = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new GraphNode(FakeNodeIndex.Index1.ToString());

            // Act
            var result = graphNode1.Equals(graphNode2);

            // Assert
            result.Should().BeFalse();
        }
        #endregion

        #region IsIdentical
        [Fact]
        public void IsIdentical_ShouldBeIsIdentical()
        {
            // Arrange
            var graphNode1 = new GraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new GraphNode(FakeNodeIndex.Index1.ToString());

            // Act
            var result = graphNode1.IsIdentical(graphNode2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsIdentical_ShouldNotBeIsIdentical()
        {
            // Arrange
            var graphNode1 = new GraphNode(FakeNodeIndex.Index1.ToString());

            // Act
            var result = graphNode1.IsIdentical(graphNode1);

            // Assert
            result.Should().BeFalse();
        }
        #endregion
    }
}

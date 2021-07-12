using Flow.Contracts;
using Flow.Contracts.Enums;
using Flow.Exceptions;
using Flow.GraphNodes;
using FlowTests.Fakes;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace FlowTests.GraphNodes
{
    public class DirectedGraphNodeTests
    {
        [Fact]
        public void CreatingDirectedGraphNode_ShoulHaveDefaultProperties()
        {
            // Act
            var directedGraphNode = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());

            // Assert
            directedGraphNode.GraphMap.Should().BeNull();
            directedGraphNode.NextNodes.Should().BeEmpty();
            directedGraphNode.Index.Should().Be(FakeNodeIndex.Index1.ToString());
        }

        #region AddNext
        [Fact]
        public void AddNext_ShouldAddThisGraphNodeAsParrentForAddedGraphNode()
        {
            // Arrange
            var graphNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());

            // Act
            graphNode1.AddNext(graphNode2);

            // Assert
            graphNode2.PreviousNodes[FakeNodeIndex.Index1.ToString()].Equals(graphNode1).Should().BeTrue();
        }

        [Fact]
        public void AddNext_ShouldAddAddedGraphNodeAsNextGraphNodeForThis()
        {
            // Arrange
            var graphNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());

            // Act
            graphNode1.AddNext(graphNode2);

            // Assert
            graphNode1.NextNodes[FakeNodeIndex.Index2.ToString()].Should().BeEquivalentTo(graphNode2);
        }

        [Fact]
        public void AddNext_ShouldSetHasCyclicRouteState()
        {
            // Arrange
            var graphNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var graphNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());

            // Act
            graphNode1.AddNext(graphNode2);
            graphNode2.AddNext(graphNode3);
            graphNode3.AddNext(graphNode2);

            // Assert
            graphNode1.HasCyclicRoute.Should().BeFalse();
            graphNode2.HasCyclicRoute.Should().BeTrue();
            graphNode3.HasCyclicRoute.Should().BeTrue();
        }

        [Fact]
        public void AddNext_ShouldMergeGraphNodes()
        {
            // Arrange
            var graphNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());

            // Act
            graphNode1.AddNext(graphNode2);

            // Assert
            ReferenceEquals(graphNode1.DirectedGraphMap, graphNode2.DirectedGraphMap).Should().BeTrue();
        }

        [Fact]
        public void AddNext_ShouldThrow_IfMergeGraphNodesWithSameGraphNodeIndexButGraphNodesNotEqual()
        {
            // Arrange
            var graphNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var graphNode2 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());

            // Act
            Action act = () => graphNode1.AddNext(graphNode2);

            // Assert
            var exception = act.Should().Throw<MergeGrafhsException>();
        }
        #endregion

        #region FindAnyDirectedRoute
        [Fact]
        public void FindAnyDirectedRoute_ShouldReturnRoute()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());
            var flowNode5 = new DirectedGraphNode(FakeNodeIndex.Index5.ToString());

            flowNode1.AddNext(flowNode3);
            flowNode2.AddNext(flowNode3);
            flowNode3.AddNext(flowNode4);
            flowNode3.AddNext(flowNode5);

            // Act
            var result = flowNode4.FindAnyDirectedRoute();

            // Assert
            var expectedRoute1 = new Route
            {
                NodesSequence = new DirectedGraphNode[] { flowNode4, flowNode3, flowNode1 },
                Type = RouteType.DirectedChain,
            };
            var expectedRoute2 = new Route
            {
                NodesSequence = new DirectedGraphNode[] { flowNode4, flowNode3, flowNode2 },
                Type = RouteType.DirectedChain,
            };
            var expectedRouteVariants = new Route[] { expectedRoute1, expectedRoute2 };
            expectedRouteVariants.Any(dr => dr.Equals(result)).Should().BeTrue();
        }

        [Fact]
        public void FindAnyDirectedRoute_ShouldReturnDirectedChainRouteInGraphWithCycles()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());
            var flowNode5 = new DirectedGraphNode(FakeNodeIndex.Index5.ToString());

            flowNode4.AddNext(flowNode2);
            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode3);
            flowNode3.AddNext(flowNode4);
            flowNode4.AddNext(flowNode5);

            // Act
            var result = flowNode5.FindAnyDirectedRoute();

            // Assert
            var expectedRoute = new Route
            {
                NodesSequence = new DirectedGraphNode[] { flowNode5, flowNode4, flowNode3, flowNode2, flowNode1 },
                Type = RouteType.DirectedChain,
            };
            result.Equals(expectedRoute).Should().BeTrue();
        }

        [Fact]
        public void FindAnyDirectedRoute_ShouldReturnCyclicRoute()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());
            var flowNode5 = new DirectedGraphNode(FakeNodeIndex.Index5.ToString());

            flowNode4.AddNext(flowNode2);
            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode3);
            flowNode3.AddNext(flowNode4);
            flowNode4.AddNext(flowNode5);

            // Act
            var result = flowNode4.FindAnyDirectedRoute(FakeNodeIndex.Index4.ToString());

            // Assert
            var expectedRoute = new Route
            {
                NodesSequence = new DirectedGraphNode[] { flowNode4, flowNode3, flowNode2, flowNode4 },
                Type = RouteType.DirectedCycle,
            };
            result.Equals(expectedRoute).Should().BeTrue();
        }

        [Fact]
        public void FindAnyDirectedRoute_ShouldReturnRouteToNode()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());
            var flowNode5 = new DirectedGraphNode(FakeNodeIndex.Index5.ToString());
            var flowNode6 = new DirectedGraphNode(FakeNodeIndex.Index6.ToString());

            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode3);
            flowNode3.AddNext(flowNode5);

            flowNode2.AddNext(flowNode4);
            flowNode4.AddNext(flowNode5);
            flowNode5.AddNext(flowNode6);

            // Act
            var result = flowNode6.FindAnyDirectedRoute(FakeNodeIndex.Index4.ToString());

            // Assert
            var expectedRoute = new Route
            {
                NodesSequence = new DirectedGraphNode[] { flowNode6, flowNode5, flowNode4 },
                Type = RouteType.DirectedChain,
            };
            result.Equals(expectedRoute).Should().BeTrue();
        }

        [Fact]
        public void FindAnyDirectedRoute_ShouldReturnNullToNode()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());
            var flowNode5 = new DirectedGraphNode(FakeNodeIndex.Index5.ToString());
            var flowNode6 = new DirectedGraphNode(FakeNodeIndex.Index6.ToString());

            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode3);
            flowNode3.AddNext(flowNode6);

            flowNode1.AddNext(flowNode4);
            flowNode4.AddNext(flowNode5);
            flowNode5.AddNext(flowNode6);

            // Act
            var result = flowNode3.FindAnyDirectedRoute(FakeNodeIndex.Index4.ToString());

            // Assert
            result.Should().BeNull();
        }
        #endregion

        #region FindDirectedRoutes
        [Fact]
        public void FindDirectedRoutes_ShouldReturnRoutes()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());
            var flowNode5 = new DirectedGraphNode(FakeNodeIndex.Index5.ToString());

            flowNode1.AddNext(flowNode3);
            flowNode2.AddNext(flowNode3);
            flowNode3.AddNext(flowNode4);
            flowNode3.AddNext(flowNode5);

            // Act
            var result = flowNode5.FindDirectedRoutes();

            // Assert
            result.Should().BeEquivalentTo(new Route[]
            {
                new Route
                {
                    NodesSequence = new DirectedGraphNode[] { flowNode5, flowNode3, flowNode1 },
                    Type = RouteType.DirectedChain,
                },
                new Route
                {
                    NodesSequence = new DirectedGraphNode[] { flowNode5, flowNode3, flowNode2 },
                    Type = RouteType.DirectedChain,
                },
            });
        }

        [Fact]
        public void FindDirectedRoutes_ShouldReturnRoutesToNode()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());
            var flowNode5 = new DirectedGraphNode(FakeNodeIndex.Index5.ToString());
            var flowNode6 = new DirectedGraphNode(FakeNodeIndex.Index6.ToString());

            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode3);
            flowNode3.AddNext(flowNode6);

            flowNode1.AddNext(flowNode4);
            flowNode4.AddNext(flowNode5);
            flowNode5.AddNext(flowNode6);

            // Act
            var result = flowNode6.FindDirectedRoutes(FakeNodeIndex.Index4.ToString());

            // Assert
            result.Should().BeEquivalentTo(new Route[]
            {
                new Route
                {
                    NodesSequence = new DirectedGraphNode[] { flowNode6, flowNode5, flowNode4 },
                    Type = RouteType.DirectedChain,
                },
            });
        }

        [Fact]
        public void FindDirectedRoutes_ShouldReturnCyclicRoutes()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());
            var flowNode5 = new DirectedGraphNode(FakeNodeIndex.Index5.ToString());

            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode3);
            flowNode3.AddNext(flowNode4);
            flowNode4.AddNext(flowNode2);

            flowNode3.AddNext(flowNode5);
            flowNode5.AddNext(flowNode2);

            // Act
            var result = flowNode2.FindDirectedRoutes(FakeNodeIndex.Index2.ToString());

            // Assert
            result.Should().BeEquivalentTo(new Route[]
            {
                new Route
                {
                    NodesSequence = new DirectedGraphNode[] { flowNode2, flowNode4, flowNode3, flowNode2, },
                    Type = RouteType.DirectedCycle,
                },
                new Route
                {
                    NodesSequence = new DirectedGraphNode[] { flowNode2, flowNode5, flowNode3, flowNode2, },
                    Type = RouteType.DirectedCycle,
                },
            });
        }

        [Fact]
        public void FindDirectedRoutes_ShouldReturnEmptyToNode()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());
            var flowNode5 = new DirectedGraphNode(FakeNodeIndex.Index5.ToString());
            var flowNode6 = new DirectedGraphNode(FakeNodeIndex.Index6.ToString());

            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode3);
            flowNode3.AddNext(flowNode6);

            flowNode1.AddNext(flowNode4);
            flowNode4.AddNext(flowNode5);
            flowNode5.AddNext(flowNode6);

            // Act
            var result = flowNode5.FindDirectedRoutes(FakeNodeIndex.Index2.ToString());

            // Assert
            result.Should().BeEmpty();
        }
        #endregion

        #region UpdateHasCyclicRouteState
        [Fact]
        public void UpdateHasCyclicRouteState_ShouldNotDetectedCyclicRoute()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());

            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode3);

            flowNode1.AddNext(flowNode4);
            flowNode4.AddNext(flowNode3);

            // Act
            flowNode3.UpdateHasCyclicRouteState();

            // Assert
            flowNode3.HasCyclicRoute.Should().BeFalse();
        }

        [Fact]
        public void UpdateHasCyclicRouteState_ShouldDetectedCyclicRoute_ForDirectedNodeWithoutNextNodes()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());

            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode3);
            flowNode3.AddNext(flowNode4);
            flowNode4.AddNext(flowNode2);

            // Act
            flowNode2.UpdateHasCyclicRouteState();

            // Assert
            flowNode2.HasCyclicRoute.Should().BeTrue();
        }

        [Fact]
        public void UpdateHasCyclicRouteState_ShouldDetectedCyclicRoute_ForDirectedNodeWithNextNodes()
        {
            // Arrange
            var flowNode1 = new DirectedGraphNode(FakeNodeIndex.Index1.ToString());
            var flowNode2 = new DirectedGraphNode(FakeNodeIndex.Index2.ToString());
            var flowNode3 = new DirectedGraphNode(FakeNodeIndex.Index3.ToString());
            var flowNode4 = new DirectedGraphNode(FakeNodeIndex.Index4.ToString());
            var flowNode5 = new DirectedGraphNode(FakeNodeIndex.Index5.ToString());

            flowNode1.AddNext(flowNode2);
            flowNode2.AddNext(flowNode3);
            flowNode3.AddNext(flowNode4);

            flowNode2.AddNext(flowNode5);
            flowNode5.AddNext(flowNode3);

            flowNode4.AddNext(flowNode2);

            // Act
            flowNode2.UpdateHasCyclicRouteState();

            // Assert
            flowNode1.HasCyclicRoute.Should().BeFalse();
            flowNode2.HasCyclicRoute.Should().BeTrue();
            flowNode3.HasCyclicRoute.Should().BeTrue();
            flowNode4.HasCyclicRoute.Should().BeTrue();
            flowNode5.HasCyclicRoute.Should().BeTrue();
        }
        #endregion
    }
}

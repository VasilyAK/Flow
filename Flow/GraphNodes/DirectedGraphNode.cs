using Flow.Contracts;
using Flow.Contracts.Enums;
using Flow.GraphMaps;
using System.Collections.Generic;
using System.Linq;

namespace Flow.GraphNodes
{
    public class DirectedGraphNode : GraphNode
    {
        public DirectedGraphMap<DirectedGraphNode> DirectedGraphMap
        {
            get => GraphMapContainer is DirectedGraphMap<DirectedGraphNode> ? (DirectedGraphMap<DirectedGraphNode>)GraphMapContainer : null;
            set => GraphMapContainer = value;
        }
        public bool HasCyclicRoute { get; set; }
        public readonly Dictionary<string, DirectedGraphNode> NextNodes;
        public readonly Dictionary<string, DirectedGraphNode> PreviousNodes;

        public DirectedGraphNode(string directedGraphNodeIndex, bool shouldCreateMap = true) : base(directedGraphNodeIndex, false)
        {
            HasCyclicRoute = false;
            NextNodes = new Dictionary<string, DirectedGraphNode>();
            PreviousNodes = new Dictionary<string, DirectedGraphNode>();

            if (shouldCreateMap)
                DirectedGraphMapInit();
        }

        public void AddNext(DirectedGraphNode directedGraphNode)
        {
            var shouldUpdateHasCyclicRouteState = DirectedGraphMap.FindNode(directedGraphNode.Index) != null;
            DirectedGraphMap.AddNode(directedGraphNode);
            CreateDirection(directedGraphNode);
            if (shouldUpdateHasCyclicRouteState)
                directedGraphNode.UpdateHasCyclicRouteState();
        }

        public void CreateDirection(DirectedGraphNode directedGraphNode)
        {
            directedGraphNode.PreviousNodes[Index] = this;
            NextNodes[directedGraphNode.Index] = directedGraphNode;
        }

        public Route FindAnyDirectedRoute(string directedGraphNodeIndex = null)
            => FindAnyDirectedRoute(new List<DirectedGraphNode>(), this, directedGraphNodeIndex);

        public Route[] FindDirectedRoutes(string directedGraphNodeIndex = null)
            => FindDirectedRoutes(this, directedGraphNodeIndex);

        public DirectedGraphNode UpdateHasCyclicRouteState()
        {
            HasCyclicRoute = FindAnyDirectedRoute(Index) != null;
            if (HasCyclicRoute)
            {
                foreach (var nextNode in NextNodes.Select(nn => nn.Value))
                    if (!nextNode.HasCyclicRoute)
                        nextNode.UpdateHasCyclicRouteState();
            }
            return this;
        }

        private void DirectedGraphMapInit()
        {
            GraphMapContainer = default(object);
            DirectedGraphMap = DirectedGraphMap<DirectedGraphNode>.Create(this);
        }

        private static Route FindAnyDirectedRoute(
            List<DirectedGraphNode> directedGraphNodesSequence,
            DirectedGraphNode directedGraphNode,
            string directedGraphNodeIndex)
        {
            directedGraphNodesSequence.Add(directedGraphNode);
            foreach (var previousNode in directedGraphNode.PreviousNodes.Select(pn => pn.Value))
            {
                var isCyclicDirectedRoute = directedGraphNodesSequence.Any(gn => string.Equals(previousNode.Index, gn.Index));
                var isRequiredNodeFound = string.Equals(previousNode.Index, directedGraphNodeIndex);
                if (isCyclicDirectedRoute && !isRequiredNodeFound)
                    continue;
                else if (isRequiredNodeFound)
                {
                    directedGraphNodesSequence.Add(previousNode);
                    return new Route
                    {
                        NodesSequence = directedGraphNodesSequence.ToArray(),
                        Type = isCyclicDirectedRoute ? RouteType.DirectedCycle : RouteType.DirectedChain,
                    };
                }
                else
                {
                    var anyDirectedRoute = FindAnyDirectedRoute(
                        new List<DirectedGraphNode>(directedGraphNodesSequence),
                        previousNode,
                        directedGraphNodeIndex);
                    if (anyDirectedRoute != null && anyDirectedRoute.NodesSequence.Any())
                        return anyDirectedRoute;
                }
            }
            if (directedGraphNodeIndex == null && !directedGraphNode.PreviousNodes.Any())
                return new Route
                {
                    NodesSequence = directedGraphNodesSequence.ToArray(),
                    Type = RouteType.DirectedChain,
                };
            return null;
        }

        private static Route[] FindDirectedRoutes(
            DirectedGraphNode directedGraphNode,
            string directedGraphNodeIndex = null)
        {
            var flowTraces = FindDirectedRoutes(
                new List<DirectedGraphNode>(),
                directedGraphNode,
                directedGraphNodeIndex);
            return flowTraces.ToArray();
        }

        private static List<Route> FindDirectedRoutes(
            List<DirectedGraphNode> directedGraphNodesSequence,
            DirectedGraphNode directedGraphNode,
            string directedGraphNodeIndex)
        {
            var directedRoutes = new List<Route>();
            directedGraphNodesSequence.Add(directedGraphNode);
            foreach (var previousNode in directedGraphNode.PreviousNodes.Select(pn => pn.Value))
            {
                var isCyclicDirectedRoute = directedGraphNodesSequence.Any(gn => string.Equals(previousNode.Index, gn.Index));
                var isRequiredNodeFound = string.Equals(previousNode.Index, directedGraphNodeIndex);
                if (isCyclicDirectedRoute && !isRequiredNodeFound)
                    continue;
                if (isRequiredNodeFound)
                {
                    directedGraphNodesSequence.Add(previousNode);
                    directedRoutes.Add(new Route
                    {
                        NodesSequence = directedGraphNodesSequence.ToArray(),
                        Type = isCyclicDirectedRoute ? RouteType.DirectedCycle : RouteType.DirectedChain,
                    });
                }
                else
                    directedRoutes.AddRange(FindDirectedRoutes(
                        new List<DirectedGraphNode>(directedGraphNodesSequence),
                        previousNode,
                        directedGraphNodeIndex));
            }

            if (directedGraphNodeIndex == null && !directedGraphNode.PreviousNodes.Any())
                directedRoutes.Add(new Route
                {
                    NodesSequence = directedGraphNodesSequence.ToArray(),
                    Type = RouteType.DirectedChain,
                });
            return directedRoutes;
        }
    }
}
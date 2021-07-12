using Flow.Exceptions;
using Flow.GraphNodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flow.GraphMaps
{
    public class GraphMap<TGraphNode> : IEquatable<GraphMap<TGraphNode>> where TGraphNode : GraphNode
    {
        protected readonly Dictionary<string, TGraphNode> graphNodes;

        public GraphMap()
        {
            graphNodes = new Dictionary<string, TGraphNode>();
        }

        public static GraphMap<TGraphNode> Create() => new GraphMap<TGraphNode>();

        public static GraphMap<TGraphNode> Create(TGraphNode graphNode)
        {
            var graphMap = new GraphMap<TGraphNode>();
            graphMap.AddNode<GraphMap<TGraphNode>>(graphNode);
            return graphMap;
        }

        public TGraphNode AddNode<TMapContainer>(TGraphNode graphNode) where TMapContainer : GraphMap<TGraphNode>
        {
            if (graphNode == null || graphNodes.Any(gn => gn.Value.Equals(graphNode)))
                return graphNode;

            if (graphNode.GraphMapContainer != default(object))
                Merge((TMapContainer)graphNode.GraphMapContainer);
            else
            {
                graphNodes[graphNode.Index] = graphNode;
                graphNode.GraphMapContainer = this;
            }
            return graphNode;
        }

        public bool Equals(GraphMap<TGraphNode> other) => ReferenceEquals(this, other);

        public TGraphNode FindNode(string graphNodeIndex)
        {
            graphNodes.TryGetValue(graphNodeIndex, out var graphNode);
            return graphNode;
        }

        public TGraphNode[] GetAllNodes() => graphNodes.Select(gn => gn.Value).ToArray();

        public void Merge(GraphMap<TGraphNode> graphMap)
        {
            if (Equals(graphMap))
                return;

            var identicalNodes = GetIdenticalNodes(graphMap);
            if (identicalNodes.Any())
                throw new MergeGrafhsException(
                    "Grafhs cannot be combined. Some grafh nodes have the same index but are not equivalent.",
                    identicalNodes.Select(gn => gn.Index).ToArray());

            foreach (var mergedGraphNode in graphMap.GetAllNodes())
            {
                graphNodes[mergedGraphNode.Index] = mergedGraphNode;
                mergedGraphNode.GraphMapContainer = this;
            }
        }

        private TGraphNode[] GetIdenticalNodes(GraphMap<TGraphNode> graphMap)
        {
            var processedGraphNodes = graphMap.GetAllNodes();
            return graphNodes.Select(gn => gn.Value)
                .Where(gn => processedGraphNodes.Any(pgn => gn.IsIdentical(pgn)))
                .ToArray();
        }
    }
}

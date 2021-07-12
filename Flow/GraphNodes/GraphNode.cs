using Flow.GraphMaps;
using System;

namespace Flow.GraphNodes
{
    public class GraphNode : IEquatable<GraphNode>
    {
        public object GraphMapContainer { get; set; }
        public string Index { get; }

        public GraphMap<GraphNode> GraphMap
        {
            get => GraphMapContainer is GraphMap<GraphNode> ? (GraphMap<GraphNode>)GraphMapContainer : null;
            set => GraphMapContainer = value;
        }

        public GraphNode(string graphNodeIndex, bool shouldCreateMap = true)
        {
            Index = graphNodeIndex;
            if (shouldCreateMap)
                GraphMap = GraphMap<GraphNode>.Create(this);
        }

        public bool Equals(GraphNode other) => ReferenceEquals(this, other);
        public bool IsIdentical(GraphNode other) => string.Equals(Index, other.Index) && !Equals(other);

        public override bool Equals(object obj) => Equals((GraphNode)obj);
        public override int GetHashCode() => base.GetHashCode();
    }
}

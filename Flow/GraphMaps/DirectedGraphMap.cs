using Flow.GraphNodes;

namespace Flow.GraphMaps
{
    public class DirectedGraphMap<TDirectedGraphNode> : GraphMap<TDirectedGraphNode>
        where TDirectedGraphNode : DirectedGraphNode
    {
        public static new DirectedGraphMap<TDirectedGraphNode> Create() => new DirectedGraphMap<TDirectedGraphNode>();

        public static new DirectedGraphMap<TDirectedGraphNode> Create(TDirectedGraphNode graphNode)
        {
            var graphMap = new DirectedGraphMap<TDirectedGraphNode>();
            graphMap.AddNode(graphNode);
            return graphMap;
        }

        public TDirectedGraphNode AddNode(TDirectedGraphNode directionGraphNode) => base.AddNode<DirectedGraphMap<TDirectedGraphNode>>(directionGraphNode);
    }
}

using Flow.Contracts.Enums;
using Flow.GraphNodes;
using System;
using System.Linq;

namespace Flow.Contracts
{
    public class Route : IEquatable<Route>
    {
        public GraphNode[] NodesSequence { get; set; }
        public RouteType Type { get; set; }

        public bool Equals(Route other) =>
            ReferenceEquals(this, other)
            || NodesSequence.All(ns => other.NodesSequence.Any(o => o.Equals(ns))) && Type.Equals(other.Type);
    }
}

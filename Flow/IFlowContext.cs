using System;

namespace Flow
{
    public interface IFlowContext : IDisposable
    {
        IReadOnlyFlowNode[] CompletedNodes { get ; }
        IReadOnlyFlowNode CurrentFlowNode { get ; }
        IReadOnlyFlowNode NextFlowNode { get ; }
        IReadOnlyFlowNode PreviousFlowNode { get ; }

        /// <summary>
        /// Assign the next executable node
        /// </summary>
        void SetNext(string flowNodeIndex);
        /// <summary>
        /// Assign the next executable node
        /// </summary>
        void SetNext<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
    }
}

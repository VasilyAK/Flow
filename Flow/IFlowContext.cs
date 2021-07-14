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
        /// Назначить следующую исполняемую ноду
        /// </summary>
        void SetNext(string flowNodeIndex);
        /// <summary>
        /// Назначить следующую исполняемую ноду
        /// </summary>
        void SetNext<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
    }
}

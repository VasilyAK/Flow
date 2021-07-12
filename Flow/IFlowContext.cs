namespace Flow
{
    public interface IFlowContext
    {
        IReadOnlyFlowNode[] CompletedNodes { get ; }
        IReadOnlyFlowNode CurrentFlowNode { get ; }
        IReadOnlyFlowNode NextFlowNode { get ; }
        IReadOnlyFlowNode PreviousFlowNode { get ; }

        void SetNext(string flowNodeIndex);
        void SetNext<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
    }
}

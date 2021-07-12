namespace Flow.Contracts
{
    public class FlowNodesExecutionSequence
    {
        public IReadOnlyFlowNode CurrentFlowNode { get; set; }
        public IReadOnlyFlowNode NextFlowNode { get; set; }
        public IReadOnlyFlowNode PreviousFlowNode { get; set; }
    }
}

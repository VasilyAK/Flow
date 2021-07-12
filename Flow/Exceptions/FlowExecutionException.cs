using Flow.Contracts.Enums;

namespace Flow.Exceptions
{
    public class FlowExecutionException : FlowException
    {
        public const string FollowUpMessage = "Flow execution error.";

        public FlowExecutionException(string flowNodeIndex, string message) : base(flowNodeIndex, AddFollowUpMessage(message, FollowUpMessage)) { }
        public FlowExecutionException(string flowNodeIndex, string message, string[][] flowtraces) : base(flowNodeIndex, AddFollowUpMessage(message, FollowUpMessage), flowtraces) { }
        public FlowExecutionException(string flowNodeIndex, FlowNodeType flowNodeType, string message) : base(flowNodeIndex, flowNodeType, message) { }
    }
}

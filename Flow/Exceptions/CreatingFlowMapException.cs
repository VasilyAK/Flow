namespace Flow.Exceptions
{
    public class CreatingFlowMapException : FlowException
    {
        public const string FollowUpMessage = "Error creating flow map.";
        public CreatingFlowMapException(string message) : base(AddFollowUpMessage(message, FollowUpMessage)) { }
        public CreatingFlowMapException(string flowNodeIndex, string message) : base(flowNodeIndex, AddFollowUpMessage(message, FollowUpMessage)) { }
    }
}

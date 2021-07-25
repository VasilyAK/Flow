namespace Flow.Contracts
{
    public class FlowCache
    {
        public FlowMapValidationError FlowMapCreatingError { get; set; }

        public void Flush() => FlowMapCreatingError = null;
    }
}

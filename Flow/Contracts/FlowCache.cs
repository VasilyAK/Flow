namespace Flow.Contracts
{
    public class FlowCache
    {
        public FlowMapValidationError FlowMapCreatingError { get; set; }
        public bool ShouldSkipValidations { get; set; }

        public void Flush()
        {
            FlowMapCreatingError = null;
            ShouldSkipValidations = false;
        }
    }
}

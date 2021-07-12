using Flow.Contracts.Enums;
using System;

namespace Flow.Contracts
{
    public class FlowMapValidationError : ValidationError
    {
        public FlowNodeValidationError[] FlowNodeErrors { get; set; } = Array.Empty<FlowNodeValidationError>();
        public string FlowNodeIndex { get; set; }
        public FlowMapErrorType Type { get; set; }
    }
}

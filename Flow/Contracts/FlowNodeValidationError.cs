using Flow.Contracts.Enums;
using System;

namespace Flow.Contracts
{
    public class FlowNodeValidationError : ValidationError
    {
        public string[] FlowRoute { get; set; } = Array.Empty<string>();
        public FlowNodeErrorType Type { get; set; }
    }
}

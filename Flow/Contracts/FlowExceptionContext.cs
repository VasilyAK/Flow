using Flow.Contracts.Enums;
using System;

namespace Flow.Contracts
{
    public class FlowExceptionContext
    {
        public string FlowNodeIndex { get; set; }
        public FlowNodeType? FlowNodeType { get; set; }
        public string[][] FlowRoutes { get; set; } = Array.Empty<string[]>();
        public string Message { get; set; } = string.Empty;
    }
}

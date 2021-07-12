using Flow.Contracts.Enums;

namespace Flow
{
    public interface IReadOnlyFlowNode
    {
        string Index { get; }
        bool HasAction { get; }
        bool IsValid { get; }
        FlowNodeType Type { get; }
    }
}

using Flow.Contracts.Enums;

namespace Flow.Exceptions
{
    public class FlowExceptionsHelper
    {
        public static FlowExecutionException FlowMapMergeException(string flowNodeIndex, FlowNodeType flowNodeType, string message)
            => new FlowExecutionException(flowNodeIndex, flowNodeType, message);

        public static CreatingFlowMapException FlowNodeActionAlreadyAssignException(string flowNodeIndex)
            => new CreatingFlowMapException(flowNodeIndex, "An action has already been assigned to the flow node. Reassigning an action in runtime is prohibited.");

        public static CreatingFlowMapException FlowNodeAlreadyExistsException(string flowNodeIndex)
            => new CreatingFlowMapException(flowNodeIndex, $"Flow node already exists.");

        public static FlowExecutionException FlowNodeNoActionAssignException(string flowNodeIndex)
            => new FlowExecutionException(flowNodeIndex, "Unable to assign action to flow node.");

        public static CreatingFlowMapException FlowNodeNotExistException(string flowNodeIndex)
            => new CreatingFlowMapException(flowNodeIndex, "Flow does not contain flow node.");

        public static FlowExecutionException FlowNodeNotExistExecutionException(string flowNodeIndex)
            => new FlowExecutionException(flowNodeIndex, "Flow does not contain flow node.");

        public static FlowExecutionException NoFlowRouteToExecuteException(string flowNodeIndex, string previousFlowNodeIndex, string[][] flowtraces)
            => new FlowExecutionException(flowNodeIndex, $"No flow route found to execute the flow action after \"{previousFlowNodeIndex}\".", flowtraces);

        public static CreatingFlowMapException RootFlowNodeNotExistsException()
            => new CreatingFlowMapException($"Flow does not contain node with type \"{FlowNodeType.Root}\".");
    }
}

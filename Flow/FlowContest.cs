using Flow.Contracts;
using Flow.Extensions;
using System;
using System.Collections.Generic;

namespace Flow
{
    //ToDo проверить работоспособность примеров в документации
    public abstract class FlowContext : IFlowContext
    {
        private delegate IReadOnlyFlowNode SetNextFlowNode(string flowNodeIndex);
        private event SetNextFlowNode setNextFlowNodeEvent;
        private readonly FlowNodesExecutionSequence flowNodesExecutionSequence;
        private readonly List<IReadOnlyFlowNode> completedNodes;

        public IReadOnlyFlowNode[] CompletedNodes { get => completedNodes.ToArray(); }
        public IReadOnlyFlowNode CurrentFlowNode { get => flowNodesExecutionSequence.CurrentFlowNode; }
        public IReadOnlyFlowNode NextFlowNode { get => flowNodesExecutionSequence.NextFlowNode; }
        public IReadOnlyFlowNode PreviousFlowNode { get => flowNodesExecutionSequence.PreviousFlowNode; }

        public FlowContext()
        {
            completedNodes = new List<IReadOnlyFlowNode>();
            flowNodesExecutionSequence = new FlowNodesExecutionSequence();
        }

        public void Dispose() { }

        public void RefreshFlowNodesExecutionSequence()
        {
            if (flowNodesExecutionSequence.CurrentFlowNode != null)
                completedNodes.Add(flowNodesExecutionSequence.CurrentFlowNode);
            flowNodesExecutionSequence.PreviousFlowNode = flowNodesExecutionSequence.CurrentFlowNode;
            flowNodesExecutionSequence.CurrentFlowNode = flowNodesExecutionSequence.NextFlowNode;
            flowNodesExecutionSequence.NextFlowNode = null;
        }

        /// <summary>
        /// Назначить следующую исполняемую ноду
        /// </summary>
        public void SetNext(string flowNodeIndex)
        {
            flowNodesExecutionSequence.NextFlowNode = setNextFlowNodeEvent?.Invoke(flowNodeIndex);
        }

        public void SetNext<TIndex>(TIndex flowNodeIndex) where TIndex : struct
            => SetNext(flowNodeIndex.FullName());

        public void SubscribeSetNextEvent(Func<string, IReadOnlyFlowNode> eventHandler)
        {
            setNextFlowNodeEvent -= eventHandler.Invoke;
            setNextFlowNodeEvent += eventHandler.Invoke;
        }
    }
}
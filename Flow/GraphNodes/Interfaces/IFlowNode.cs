using Flow.Contracts;
using System;
using System.Threading.Tasks;

namespace Flow
{
    public interface IFlowNode<TFlowContext> : IReadOnlyFlowNode where TFlowContext : IFlowContext
    {
        FlowNodeValidationError[] ValidationErrors { get; }

        /// <summary>
        /// Assign an executable action to the node
        /// </summary>
        IFlowNode<TFlowContext> AddAction(Action<TFlowContext> flowNodeAction);
        /// <summary>
        /// Assign an executable action to the node
        /// </summary>
        IFlowNode<TFlowContext> AddAction(Func<TFlowContext, Task> flowNodeAction);
        /// <summary>
        /// Add a link to the next executable node
        /// </summary>
        IFlowNode<TFlowContext> AddNext(string flowNodeIndex);
        /// <summary>
        /// Add a link to the next executable node
        /// </summary>
        IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
        /// <summary>
        /// Add a link to the next executable node
        /// </summary>
        IFlowNode<TFlowContext> AddNext(string flowNodeIndex, Action<TFlowContext> flowNodeAction);
        /// <summary>
        /// Add a link to the next executable node
        /// </summary>
        IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex, Action<TFlowContext> flowNodeAction) where TIndex : struct;
        /// <summary>
        /// Add a link to the next executable node
        /// </summary>
        IFlowNode<TFlowContext> AddNext(string flowNodeIndex, Func<TFlowContext, Task> flowNodeAction);
        /// <summary>
        /// Add a link to the next executable node
        /// </summary>
        IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex, Func<TFlowContext, Task> flowNodeAction) where TIndex : struct;
    }
}

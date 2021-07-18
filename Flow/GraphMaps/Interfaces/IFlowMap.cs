using Flow.Contracts;
using System;
using System.Threading.Tasks;

namespace Flow.Interfaces
{
    public interface IFlowMap<TFlowContext> where TFlowContext : IFlowContext
    {
        bool IsValid { get; }
        FlowMapValidationError[] ValidationErrors { get; }

        /// <summary>
        /// Add a root node to the map
        /// </summary>
        IFlowNode<TFlowContext> AddRoot(string flowNodeIndex);
        /// <summary>
        /// Add a root node to the map
        /// </summary>
        IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
        /// <summary>
        /// Add a root node to the map
        /// </summary>
        IFlowNode<TFlowContext> AddRoot(string flowNodeIndex, Action<TFlowContext> flowNodeAction);
        /// <summary>
        /// Add a root node to the map
        /// </summary>
        IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex, Action<TFlowContext> flowNodeAction) where TIndex : struct;
        /// <summary>
        /// Add a root node to the map
        /// </summary>
        IFlowNode<TFlowContext> AddRoot(string flowNodeIndex, Func<TFlowContext, Task> flowNodeAction);
        /// <summary>
        /// Add a root node to the map
        /// </summary>
        IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex, Func<TFlowContext, Task> flowNodeAction) where TIndex : struct;
        /// <summary>
        /// Get a node by its index
        /// </summary>
        IFlowNode<TFlowContext> GetNode(string flowNodeIndex);
        /// <summary>
        /// Get a node by its index
        /// </summary>
        IFlowNode<TFlowContext> GetNode<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
        /// <summary>
        /// Get root node
        /// </summary>
        IFlowNode<TFlowContext> GetRoot();
    }
}

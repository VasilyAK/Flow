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
        /// Добавить в карту корневую ноду
        /// </summary>
        IFlowNode<TFlowContext> AddRoot(string flowNodeIndex);
        IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
        /// <summary>
        /// Добавить в карту корневую ноду
        /// </summary>
        IFlowNode<TFlowContext> AddRoot(string flowNodeIndex, Action<TFlowContext> flowNodeAction);
        IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex, Action<TFlowContext> flowNodeAction) where TIndex : struct;
        /// <summary>
        /// Добавить в карту корневую ноду
        /// </summary>
        IFlowNode<TFlowContext> AddRoot(string flowNodeIndex, Func<TFlowContext, Task> flowNodeAction);
        IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex, Func<TFlowContext, Task> flowNodeAction) where TIndex : struct;
        /// <summary>
        /// Получить ноду по ее ключу
        /// </summary>
        IFlowNode<TFlowContext> GetNode(string flowNodeIndex);
        IFlowNode<TFlowContext> GetNode<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
        /// <summary>
        /// Получить корневую ноду
        /// </summary>
        IFlowNode<TFlowContext> GetRoot();
    }
}

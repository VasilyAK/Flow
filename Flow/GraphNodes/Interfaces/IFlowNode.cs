using Flow.Contracts;
using System;
using System.Threading.Tasks;

namespace Flow
{
    //ToDo добавть тесты на интерфейсы с TIndex
    public interface IFlowNode<TFlowContext> : IReadOnlyFlowNode where TFlowContext : IFlowContext
    {
        FlowNodeValidationError[] ValidationErrors { get; }

        /// <summary>
        /// Назначить ноде исполняемое действие
        /// </summary>
        IFlowNode<TFlowContext> AddAction(Action<TFlowContext> flowNodeAction);
        /// <summary>
        /// Назначить ноде исполняемое действие
        /// </summary>
        IFlowNode<TFlowContext> AddAction(Func<TFlowContext, Task> flowNodeAction);
        /// <summary>
        /// Добавить ссылку на следующую исполняемую ноду
        /// </summary>
        IFlowNode<TFlowContext> AddNext(string flowNodeIndex);
        IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
        /// <summary>
        /// Добавить ссылку на следующую исполняемую ноду
        /// </summary>
        IFlowNode<TFlowContext> AddNext(string flowNodeIndex, Action<TFlowContext> flowNodeAction);
        IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex, Action<TFlowContext> flowNodeAction) where TIndex : struct;
        /// <summary>
        /// Добавить ссылку на следующую исполняемую ноду
        /// </summary>
        IFlowNode<TFlowContext> AddNext(string flowNodeIndex, Func<TFlowContext, Task> flowNodeAction);
        IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex, Func<TFlowContext, Task> flowNodeAction) where TIndex : struct;
    }
}

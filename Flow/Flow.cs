using Flow.Contracts.Enums;
using Flow.Exceptions;
using Flow.Extensions;
using Flow.GraphMaps;
using Flow.GraphNodes;
using Flow.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Flow
{
    public abstract class Flow<TFlowContext> : IDisposable where TFlowContext : FlowContext, new()
    {
        private readonly TFlowContext flowContext;
        private IFlowMap<TFlowContext> flowMapContainer { get; set; }
        protected IFlowMap<TFlowContext> flowMap { get; private set; }

        public Flow()
        {
            flowContext = new TFlowContext();
            flowContext.SubscribeSetNextEvent(SetNext);
            InitializeFlowMap();
            InitializeFlowStart();
        }

        public Flow(TFlowContext flowContext)
        {
            this.flowContext = flowContext;
            flowContext.SubscribeSetNextEvent(SetNext);
            InitializeFlowMap();
            InitializeFlowStart();
        }

        //ToDo перевести все описания на английский
        /// <summary>
        /// Запустить поток на исполнение
        /// </summary>
        public TFlowContext RunFlow()
        {
            RunInternal(false);
            return flowContext;
        }

        /// <summary>
        /// Запустить поток на исполнение
        /// </summary>
        public async Task<TFlowContext> RunFlowAsync()
        {
            RunInternal(true);
            return await Task.FromResult(flowContext);
        }

        /// <summary>
        /// Построить карту потока
        /// </summary>
        protected virtual void BuildFlowMap() { }

        private void CloseAccessToFlowMap()
        {
            flowMapContainer = ((FlowMap<TFlowContext>)flowMap).Clone();
            flowMap = null;
        }

        public virtual void Dispose()
        {
            flowContext.Dispose();
        }

        private void ExecuteFlowNode(FlowNode<TFlowContext> flowNode, bool isAsync)
        {
            if (flowNode.Type.Equals(FlowNodeType.Root)
                || flowNode.FindAnyDirectedRoute(flowContext.PreviousFlowNode.Index) != null)
                RunFlowNode(flowNode, isAsync);
            else
            {
                var routesFlowNodeIndexSequence = flowNode.FindDirectedRoutes()
                    .Select(r => r.NodesSequence.Select(fn => fn.Index).ToArray())
                    .ToArray();
                throw FlowExceptionsHelper.NoFlowRouteToExecuteException(
                    flowNode.Index,
                    flowContext.PreviousFlowNode.Index,
                    routesFlowNodeIndexSequence);
            }

            if (flowContext.NextFlowNode == null)
                SetNextAuto(flowNode);

            flowContext.RefreshFlowNodesExecutionSequence();
        }

        //ToDo продумать кеширование, чтобы не запускать валидацию карты при каждом создании инстанса
        private void InitializeFlowMap()
        {
            flowMap = new FlowMap<TFlowContext>();
            BuildFlowMap();
            ValidateFlowMap();
            CloseAccessToFlowMap();
        }

        private void InitializeFlowStart()
        {
            var rootFlowNode = (FlowNode<TFlowContext>)flowMapContainer.GetRoot();
            flowContext.SetNext(rootFlowNode.Index);
            flowContext.RefreshFlowNodesExecutionSequence();
        }

        private void RunFlowNode(FlowNode<TFlowContext> flowNode, bool isAsync)
        {
            if (isAsync)
                flowNode.RunAsync(flowContext).ConfigureAwait(false).GetAwaiter().GetResult();
            else
                flowNode.Run(flowContext);
        }

        private void RunInternal(bool isAsync)
        {
            while (flowContext.CurrentFlowNode != null)
            {
                var flowNodeToExecute = (FlowNode<TFlowContext>)flowMapContainer.GetNode(flowContext.CurrentFlowNode.Index);
                ExecuteFlowNode(flowNodeToExecute, isAsync);
            }
        }

        private IReadOnlyFlowNode SetNext(string flowNodeIndex)
        {
            var nextFlowNode = ((FlowMap<TFlowContext>)flowMapContainer).FindNode(flowNodeIndex);
            if (nextFlowNode == null)
                throw FlowExceptionsHelper.FlowNodeNotExistExecutionException(flowNodeIndex);
            return nextFlowNode;
        }

        private void SetNextAuto(FlowNode<TFlowContext> currentNode)
        {
            if (currentNode.NextNodes.Keys.Count > 1)
                throw FlowExceptionsHelper.NextStepUndefinedException(currentNode.Index, currentNode.NextNodes.Select(x => x.Value.Index).ToArray());
            else if (currentNode.NextNodes.Keys.Count == 1)
                flowContext.SetNext(currentNode.NextNodes.Values.First().Index);
        }

        private void ValidateFlowMap()
        {
            if (flowMap.IsValid)
                return;

            var flowMapValidateError = flowMap.ValidationErrors.First();
            throw flowMapValidateError.ToFlowException();
        }
    }
}

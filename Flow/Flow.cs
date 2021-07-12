using Flow.Contracts.Enums;
using Flow.Exceptions;
using Flow.Extensions;
using Flow.GraphMaps;
using Flow.GraphNodes;
using Flow.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Flow
{
    //ToDo добавть тесты на интерфейсы с TIndex
    public abstract class Flow<TFlowContext> where TFlowContext : IFlowContext, new()
    {
        private readonly TFlowContext flowContext;
        private IFlowMap<TFlowContext> flowMapContainer { get; set; }
        protected IFlowMap<TFlowContext> flowMap { get; private set; }

        public Flow()
        {
            flowContext = new TFlowContext();
            (flowContext as FlowContext).SubscribeSetNextEvent(SetNext);
            InitializeFlowMap();
            InitializeFlowStart();
        }

        public Flow(TFlowContext flowContext)
        {
            this.flowContext = flowContext;
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
            (flowContext as FlowContext).RefreshFlowNodesExecutionSequence();
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
            (flowContext as FlowContext).SetNext(rootFlowNode.Index);
            (flowContext as FlowContext).RefreshFlowNodesExecutionSequence();
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

        private void ValidateFlowMap()
        {
            if (flowMap.IsValid)
                return;

            var flowMapValidateError = flowMap.ValidationErrors.First();
            throw flowMapValidateError.ToFlowException();
        }
    }
}

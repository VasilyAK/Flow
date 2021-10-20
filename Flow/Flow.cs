using Flow.Contracts;
using Flow.Contracts.Enums;
using Flow.Exceptions;
using Flow.Extensions;
using Flow.GraphMaps;
using Flow.GraphNodes;
using Flow.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flow
{
    public abstract class Flow<TFlowContext> : IDisposable where TFlowContext : FlowContext, new()
    {
        protected static Dictionary<string, FlowCache> flowCache = new Dictionary<string, FlowCache>();
        public static FlowCache FlowCache<TFlow>() where TFlow : Flow<TFlowContext> =>
            flowCache.TryGetValue(typeof(TFlow).FullName, out var cache) ? cache : flowCache[typeof(TFlow).FullName] = new FlowCache();

        private readonly TFlowContext flowContext;
        private IFlowMap<TFlowContext> flowMapContainer;
        protected IFlowMap<TFlowContext> flowMap { get; private set; } // todo to private

        public Flow(FlowCache childFlowCache = null)
        {
            flowContext = new TFlowContext();
            FlowInit(childFlowCache);
        }

        public Flow(TFlowContext flowContext, FlowCache childFlowCache = null)
        {
            this.flowContext = flowContext;
            FlowInit(childFlowCache);
        }

        /// <summary>
        /// Start the flow for execution
        /// </summary>
        public TFlowContext RunFlow()
        {
            RunInternal(false);
            return flowContext;
        }

        /// <summary>
        /// Start the flow for execution
        /// </summary>
        public async Task<TFlowContext> RunFlowAsync()
        {
            RunInternal(true);
            return await Task.FromResult(flowContext);
        }

        /// <summary>
        /// Execute after fetching each flow node
        /// </summary>
        protected virtual void AfterEach(TFlowContext flowContext) { }

        /// <summary>
        /// Execute before fetching each flow node
        /// </summary>
        protected virtual void BeforeEach(TFlowContext flowContext) { }

        /// <summary>
        /// Build a flow map
        /// </summary>
        protected abstract void BuildFlowMap();

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
            if (!flowNode.Type.Equals(FlowNodeType.Root)
                && flowNode.FindAnyDirectedRoute(flowContext.PreviousFlowNode.Index) == null)
            {
                var routesFlowNodeIndexSequence = flowNode.FindDirectedRoutes()
                    .Select(r => r.NodesSequence.Select(fn => fn.Index).ToArray())
                    .ToArray();
                throw FlowExceptionsHelper.NoFlowRouteToExecuteException(
                    flowNode.Index,
                    flowContext.PreviousFlowNode.Index,
                    routesFlowNodeIndexSequence);
            }

            BeforeEach(flowContext);
            RunFlowNode(flowNode, isAsync);

            if (flowContext.NextFlowNode == null)
                SetNextAuto(flowNode);

            AfterEach(flowContext);

            flowContext.RefreshFlowNodesExecutionSequence();
        }

        private void FlowInit(FlowCache childFlowCache)
        {
            flowContext.SubscribeSetNextEvent(SetNext);
            InitializeFlowMap(childFlowCache);
            InitializeFlowStart();
        }

        private void InitializeFlowMap(FlowCache childFlowCache)
        {
            if (childFlowCache?.FlowMapCreatingError != null)
                throw childFlowCache.FlowMapCreatingError.ToFlowException();

            var shouldSkipValidations = childFlowCache?.ShouldSkipValidations == true;
            flowMap = new FlowMap<TFlowContext>(shouldSkipValidations);
            BuildFlowMap();
            ValidateFlowMap(childFlowCache);
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

        private void ValidateFlowMap(FlowCache childFlowCache)
        {
            if (childFlowCache?.ShouldSkipValidations == true || flowMap.IsValid)
            {
                if (childFlowCache != null)
                    childFlowCache.ShouldSkipValidations = true;
                return;
            }

            var error = flowMap.ValidationErrors.First();
            if (childFlowCache != null)
                childFlowCache.FlowMapCreatingError = error;
             throw error.ToFlowException();
        }
    }
}

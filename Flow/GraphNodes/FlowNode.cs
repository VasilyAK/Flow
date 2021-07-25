using Flow.Contracts;
using Flow.Contracts.Enums;
using Flow.Exceptions;
using Flow.Extensions;
using Flow.GraphMaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flow.GraphNodes
{
    public class FlowNode<TFlowContext> : DirectedGraphNode, IFlowNode<TFlowContext>
        where TFlowContext : IFlowContext
    {
        private delegate void FlowNodeAction(TFlowContext ctx);
        private FlowNodeAction flowNodeAction { get; set; }
        private delegate Task FlowNodeActionAsync(TFlowContext ctx);
        private FlowNodeActionAsync flowNodeActionAsync { get; set; }

        private bool shouldSkipValidation { get; set; }
        private List<FlowNodeValidationError> validationErrors { get; set; }

        #region public to interface
        public bool HasAction => flowNodeAction != null || flowNodeActionAsync != null;
        public bool IsValid => !ValidationErrors.Any();
        public FlowNodeType Type { get; private set; }
        public FlowNodeValidationError[] ValidationErrors
        {
            get
            {
                ValidateCyclicExecution();
                ValidateRootFlowNode();
                return validationErrors.ToArray();
            }
        }
        #endregion

        public FlowMap<TFlowContext> FlowMap
        {
            get => GraphMapContainer is FlowMap<TFlowContext> ? (FlowMap<TFlowContext>)GraphMapContainer : null;
            set => GraphMapContainer = value;
        }

        public FlowNode(
            string flowNodeIndex,
            FlowNodeType flowNodeType = FlowNodeType.Variable,
            bool shouldCreateMap = true,
            bool shouldSkipValidation = false)
            : base(flowNodeIndex, false)
        {
            FlowNodeInit(flowNodeType, shouldCreateMap, shouldSkipValidation);
        }

        public FlowNode(
            string flowNodeIndex,
            Action<TFlowContext> flowNodeAction,
            FlowNodeType flowNodeType = FlowNodeType.Variable,
            bool shouldCreateMap = true,
            bool shouldSkipValidation = false)
            : base(flowNodeIndex, false)
        {
            FlowNodeInit(flowNodeType, shouldCreateMap, shouldSkipValidation);
            AddAction(flowNodeAction);
        }

        public FlowNode(
            string flowNodeIndex,
            Func<TFlowContext, Task> flowNodeAction,
            FlowNodeType flowNodeType = FlowNodeType.Variable,
            bool shouldCreateMap = true,
            bool shouldSkipValidation = false)
            : base(flowNodeIndex, false)
        {
            FlowNodeInit(flowNodeType, shouldCreateMap, shouldSkipValidation);
            AddAction(flowNodeAction);
        }

        #region public to interface
        public IFlowNode<TFlowContext> AddAction(Action<TFlowContext> flowNodeAction)
        {
            if (HasAction && !ReferenceEquals(this.flowNodeAction, flowNodeAction))
                throw FlowExceptionsHelper.FlowNodeActionAlreadyAssignException(Index);

            this.flowNodeAction = null;
            if (flowNodeAction != null)
                this.flowNodeAction = flowNodeAction.Invoke;
            return this;
        }

        public IFlowNode<TFlowContext> AddAction(Func<TFlowContext, Task> flowNodeAction)
        {
            if (HasAction && !ReferenceEquals(flowNodeActionAsync, flowNodeAction))
                throw FlowExceptionsHelper.FlowNodeActionAlreadyAssignException(Index);

            flowNodeActionAsync = null;
            if (flowNodeAction != null)
                flowNodeActionAsync = flowNodeAction.Invoke;
            return this;
        }

        public IFlowNode<TFlowContext> AddNext(string flowNodeIndex)
        {
            var existedFlowNode = FlowMap.FindNode(flowNodeIndex);
            var flowNodeToAdd = existedFlowNode ?? new FlowNode<TFlowContext>(flowNodeIndex, FlowNodeType.Variable, false, shouldSkipValidation);
            return AddNext(flowNodeToAdd);
        }

        public IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex) where TIndex : struct
            => AddNext(flowNodeIndex.FullName());

        public IFlowNode<TFlowContext> AddNext(string flowNodeIndex, Action<TFlowContext> flowNodeAction = null)
        {
            var flowNode = AddNext(flowNodeIndex);
            flowNode.AddAction(flowNodeAction);
            return flowNode;
        }

        public IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex, Action<TFlowContext> flowNodeAction = null)
            where TIndex : struct
            => AddNext(flowNodeIndex.FullName(), flowNodeAction);

        public IFlowNode<TFlowContext> AddNext(string flowNodeIndex, Func<TFlowContext, Task> flowNodeAction = null)
        {
            var flowNode = AddNext(flowNodeIndex);
            flowNode.AddAction(flowNodeAction);
            return flowNode;
        }

        public IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex, Func<TFlowContext, Task> flowNodeAction = null)
            where TIndex : struct
            => AddNext(flowNodeIndex.FullName(), flowNodeAction);
        #endregion

        public FlowNode<TFlowContext> AddNext(FlowNode<TFlowContext> flowNode)
        {
            try
            {
                if (!shouldSkipValidation)
                    ValidateReAddingNextFlowNode(flowNode.Index);

                var shouldUpdateHasCyclicRouteState = !shouldSkipValidation && FlowMap.FindNode(flowNode.Index) != null;
                FlowMap.AddNode(flowNode);
                CreateDirection(flowNode);

                if (shouldUpdateHasCyclicRouteState)
                    flowNode.UpdateHasCyclicRouteState();
                return flowNode;
            }
            catch (MergeGrafhsException ex)
            {
                var message = $"{ex.Message} {string.Join(", ", ex.GraphNodeIndexes)}";
                throw FlowExceptionsHelper.FlowMapMergeException(Index, Type, message);
            }
        }

        public virtual FlowNode<TFlowContext> CloneWithoutDirections()
        {
            var clone = new FlowNode<TFlowContext>(Index, null, Type, false, shouldSkipValidation);
            if (flowNodeAction != null)
                clone.AddAction(flowNodeAction.Invoke);
            if (flowNodeActionAsync != null)
                clone.AddAction(flowNodeActionAsync.Invoke);
            clone.HasCyclicRoute = HasCyclicRoute;
            return clone;
        }

        public void Run(TFlowContext flowContext) => RunInternal(flowContext).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task RunAsync(TFlowContext flowContext) => await RunInternal(flowContext);

        private void FlowNodeInit(FlowNodeType flowNodeType, bool shouldCreateMap, bool shouldSkipValidation)
        {
            Type = flowNodeType;
            this.shouldSkipValidation = shouldSkipValidation;
            validationErrors = new List<FlowNodeValidationError>();

            if (shouldCreateMap)
                FlowMapInit();
        }

        private void FlowMapInit()
        {
            GraphMapContainer = default(object);
            FlowMap = FlowMap<TFlowContext>.Create(this);
        }

        private async Task RunInternal(TFlowContext flowContext)
        {
            if (flowNodeAction != null)
                flowNodeAction(flowContext);
            else if (flowNodeActionAsync != null)
                await flowNodeActionAsync(flowContext);
            else
                throw FlowExceptionsHelper.FlowNodeNoActionAssignException(Index);
        }

        private void ValidateCyclicExecution()
        {
            if (!HasCyclicRoute)
            {
                validationErrors.Remove((sr) => sr.Type.Equals(FlowNodeErrorType.CyclicDependency));
                return;
            }
            var routes = FindDirectedRoutes(Index);
            var cyclicFlowTraces = FindDirectedRoutes(Index).Where(dr => dr.Type.Equals(RouteType.DirectedCycle)).Select(dr => dr.NodesSequence);
            foreach (var flowTrace in cyclicFlowTraces)
            {
                var flowIndexTrace = flowTrace.Select(vt => vt.Index).ToArray();
                bool searchRule(FlowNodeValidationError sr) =>
                    sr.Type.Equals(FlowNodeErrorType.CyclicDependency)
                    && sr.FlowRoute.SequenceEqual(flowIndexTrace);
                Action<FlowNodeValidationError> onAdd = (ve) =>
                {
                    ve.FlowRoute = flowIndexTrace;
                    ve.Message = "Cyclic execution detected.";
                    ve.Type = FlowNodeErrorType.CyclicDependency;
                };

                validationErrors.Actualize(searchRule, onAdd);
            }
        }

        private void ValidateRootFlowNode()
        {
            if (Type != FlowNodeType.Root || !PreviousNodes.Any())
                return;
            bool searchRule(FlowNodeValidationError sr) => sr.Type == FlowNodeErrorType.InvalidRootFlowNode;
            Action<FlowNodeValidationError> onAdd = (ve) =>
            {
                ve.Message = "Root flow node has parent nodes.";
                ve.Type = FlowNodeErrorType.InvalidRootFlowNode;
            };
            validationErrors.Actualize(searchRule, onAdd);
        }

        private void ValidateReAddingNextFlowNode(string flowNodeIndex)
        {
            if (!NextNodes.ContainsKey(flowNodeIndex))
                return;

            bool searchRule(FlowNodeValidationError sr) =>
                sr.Type == FlowNodeErrorType.ReAddingNextFlowNode
                && sr.FlowRoute[0].Equals(flowNodeIndex)
                && sr.FlowRoute[1].Equals(Index);
            Action<FlowNodeValidationError> onAdd = (ve) =>
            {
                ve.FlowRoute = new string[] { flowNodeIndex, Index };
                ve.Message = "Re-adding flow node detected.";
                ve.Type = FlowNodeErrorType.ReAddingNextFlowNode;
            };
            validationErrors.Actualize(searchRule, onAdd);
        }
    }
}

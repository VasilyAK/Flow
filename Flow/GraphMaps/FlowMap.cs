using Flow.Contracts;
using Flow.Contracts.Enums;
using Flow.Exceptions;
using Flow.Extensions;
using Flow.GraphNodes;
using Flow.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flow.GraphMaps
{
    public class FlowMap<TFlowContext> : DirectedGraphMap<FlowNode<TFlowContext>>, IFlowMap<TFlowContext>
        where TFlowContext : IFlowContext
    {
        protected readonly List<FlowMapValidationError> validationErrors;

        public bool IsValid => !ValidationErrors.Any();
        public FlowMapValidationError[] ValidationErrors
        {
            get
            {
                var flowNodes = GetAllNodes();
                foreach (var flowNode in flowNodes)
                {
                    ValidateFlowNode(flowNode);
                    if (flowNode.Type.Equals(FlowNodeType.Root))
                        ValidateRootFlowNode(flowNode);
                }
                if (!flowNodes.Any())
                    ValidateRootFlowNode(null);
                return validationErrors.ToArray();
            }
        }

        public FlowMap()
        {
            validationErrors = new List<FlowMapValidationError>();
        }

        public static new FlowMap<TFlowContext> Create() => new FlowMap<TFlowContext>();

        public static new FlowMap<TFlowContext> Create(FlowNode<TFlowContext> flowNode)
        {
            var flowMap = new FlowMap<TFlowContext>();
            flowMap.AddNode(flowNode);
            return flowMap;
        }

        public new FlowNode<TFlowContext> AddNode(FlowNode<TFlowContext> flowNode) => base.AddNode<FlowMap<TFlowContext>>(flowNode);

        public IFlowNode<TFlowContext> AddRoot(string flowNodeIndex)
        {
            var rootFlowNode = new FlowNode<TFlowContext>(flowNodeIndex.ToString(), FlowNodeType.Root, false);
            return AddNode(rootFlowNode);
        }

        public IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex) where TIndex : struct
            => AddRoot(flowNodeIndex.FullName());

        public IFlowNode<TFlowContext> AddRoot(string flowNodeIndex, Action<TFlowContext> flowNodeAction = null)
        {
            var rootFlowNode = new FlowNode<TFlowContext>(flowNodeIndex.ToString(), flowNodeAction, FlowNodeType.Root, false);
            return AddNode(rootFlowNode);
        }

        public IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex, Action<TFlowContext> flowNodeAction = null)
            where TIndex : struct
            => AddRoot(flowNodeIndex.FullName(), flowNodeAction);

        public IFlowNode<TFlowContext> AddRoot(string flowNodeIndex, Func<TFlowContext, Task> flowNodeAction = null)
        {
            var rootFlowNode = new FlowNode<TFlowContext>(flowNodeIndex.ToString(), flowNodeAction, FlowNodeType.Root, false);
            return AddNode(rootFlowNode);
        }

        public IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex, Func<TFlowContext, Task> flowNodeAction = null)
            where TIndex : struct
            => AddRoot(flowNodeIndex.FullName(), flowNodeAction);

        public FlowMap<TFlowContext> Clone()
        {
            var clone = new FlowMap<TFlowContext>();
            var flowNodes = graphNodes.Select(fn => fn.Value).ToArray();

            foreach (var flowNode in flowNodes)
                clone.AddNode(flowNode.CloneWithoutDirections());

            foreach (var flowNode in flowNodes)
            {
                var flowNodeClone = (FlowNode<TFlowContext>)clone.GetNode(flowNode.Index);
                foreach (var nextNode in flowNode.NextNodes.Select(fn => fn.Value).ToArray())
                    flowNodeClone.AddNext((FlowNode<TFlowContext>)clone.GetNode(nextNode.Index));
            }
            return clone;
        }

        public IFlowNode<TFlowContext> GetNode(string flowNodeIndex)
        {
            var flowNode = FindNode(flowNodeIndex);
            if (flowNode == null)
                throw FlowExceptionsHelper.FlowNodeNotExistException(flowNodeIndex);
            return flowNode;
        }

        public IFlowNode<TFlowContext> GetNode<TIndex>(TIndex flowNodeIndex) where TIndex : struct
            => GetNode(flowNodeIndex.FullName());

        public IFlowNode<TFlowContext> GetRoot()
        {
            var rootFlowNode = graphNodes.FirstOrDefault(node => node.Value.Type.Equals(FlowNodeType.Root)).Value;
            if (rootFlowNode == null)
                throw FlowExceptionsHelper.RootFlowNodeNotExistsException();
            return rootFlowNode;
        }

        private void ValidateFlowNode(FlowNode<TFlowContext> flowNode)
        {
            bool searchRule(FlowMapValidationError sr) =>
                string.Equals(sr.FlowNodeIndex, flowNode.Index)
                && sr.Type == FlowMapErrorType.InvalidFlowNode;
            Action<FlowMapValidationError> onAdd = (ve) =>
            {
                ve.FlowNodeErrors = flowNode.ValidationErrors;
                ve.FlowNodeIndex = flowNode.Index;
                ve.Message = "Flow node is invalid.";
                ve.Type = FlowMapErrorType.InvalidFlowNode;
            };
            Action<FlowMapValidationError> onUpdate = (ve) =>
            {
                ve.FlowNodeErrors = flowNode.ValidationErrors;
            };

            if (!flowNode.IsValid)
                validationErrors.Actualize(searchRule, onAdd, onUpdate);
            else
                validationErrors.Remove(searchRule);
        }

        private void ValidateRootFlowNode(FlowNode<TFlowContext> flowNode)
        {
            var rootFlowNodeIndexes = graphNodes
                .Where(fn => fn.Value.Type == FlowNodeType.Root)
                .Select(fn => fn.Value.Index)
                .ToArray();
            if (rootFlowNodeIndexes.Length > 1)
            {
                bool searchRule(FlowMapValidationError sr) =>
                    sr.Type == FlowMapErrorType.SomeRootFlowNodesDetected
                    && string.Equals(sr.FlowNodeIndex, flowNode.Index);
                Action<FlowMapValidationError> onAdd = (ve) =>
                {
                    ve.FlowNodeIndex = flowNode.Index;
                    ve.Message = "Flow contains some root flow nodes.";
                    ve.Type = FlowMapErrorType.SomeRootFlowNodesDetected;
                };
                validationErrors.Actualize(searchRule, onAdd);
            }
            else if (rootFlowNodeIndexes.Length == 1)
            {
                var rootNotExistValidateError = validationErrors.FirstOrDefault(ve => ve.Type == FlowMapErrorType.RootFlowNodeNotExist);
                if (rootNotExistValidateError != null)
                    validationErrors.Remove(rootNotExistValidateError);
            }
            else
            {
                bool searchRule(FlowMapValidationError sr) => sr.Type == FlowMapErrorType.RootFlowNodeNotExist;
                Action<FlowMapValidationError> onAdd = (ve) =>
                {
                    ve.Message = "Flow does not contain root flow node.";
                    ve.Type = FlowMapErrorType.RootFlowNodeNotExist;
                };
                validationErrors.Actualize(searchRule, onAdd);
            }
        }
    }
}

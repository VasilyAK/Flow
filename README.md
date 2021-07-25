# Flow <img src="https://github.com/VasilyAK/Flow/workflows/Flow-CI/badge.svg?branch=master"/>

## Introduction

Flow is a C # library whose internal logic is based on graph theory. The library provides a tool for programmers when it is necessary to have access to initial or intermediate results over several sequential processes.

## Goals

- Be able to quickly and conveniently embed new logic into existing code
- Control the sequence of code execution in a convenient form
- Writing tests simply and concisely

## Terms conventions

- Flow: sequence of operations performed, each of which is identified by a unique code.

- FlowNode: an atomic thread structure that is responsible for executing a designated operation.

- FlowMap: a set of instructions describing the flow of operations and dependencies between flow nodes.

- FlowContext: an object for transmitting commands to a flow, as well as for transmitting the results of operations between the nodes of the flow.

## User documentation

<details>
  <summary>Main example</summary>

### Operation flow chart

          -----| FirstStep | -----
          |                      |
    | SecondStep |         | FourthStep |
          |                      |
    | ThirdStep  |         |  FifthStep |

### Implementation example

```c#
    public enum IndexExample
    {
        FirstStep,
        SecondStep,
        ThirdStep,
        FourthStep,
        FifthStep,
    }

    public class FlowContextExample : FlowContext
    {
        public string SelectedBranch { get; set; } = "No branch";
        public int FirstValue { get; set; }
        public int SecondValue { get; set; }
        public int ThirdValue { get; set; }
        public object NonControlResource { get; set; }

        public new void Dispose() => NonControlResource = null; //.Dispose()
    }

    public class FlowExample : Flow<FlowContextExample>
    {
        public FlowExample() : base(FlowCache<FlowExample>()) { } //micro optimization

        public (string branch, int summ) GetResult() => ProcessContext(RunFlow());

        public async Task<(string branch, int summ)> GetResultAsync() => ProcessContext(await RunFlowAsync());

        protected override void BuildFlowMap()
        {
            flowMap
                .AddRoot(IndexExample.FirstStep, FirstStepAction)
                .AddNext(IndexExample.SecondStep, SecondStepAction)
                .AddNext(IndexExample.ThirdStep, ThirdStepAction);

            flowMap
                .GetNode(IndexExample.FirstStep)
                .AddNext(IndexExample.FourthStep, FourthStepAction)
                .AddNext(IndexExample.FifthStep, FirthStepAction);
        }

        private static void FirstStepAction(FlowContextExample ctx)
        {
            ctx.FirstValue = new Random().Next(10);
            if (ctx.FirstValue < 5)
            {
                ctx.SelectedBranch = "Left branch";
                ctx.SetNext(IndexExample.SecondStep);
            }
            else
            {
                ctx.SelectedBranch = "Right branch";
                ctx.SetNext(IndexExample.FourthStep);
            }
        }

        private static void SecondStepAction(FlowContextExample ctx)
        {
            ctx.SecondValue = 10;
        }

        private static Task ThirdStepAction(FlowContextExample ctx)
        {
            ctx.ThirdValue = 20;
            return Task.CompletedTask;
        }

        private static Task FourthStepAction(FlowContextExample ctx)
        {
            ctx.SecondValue = 100;
            return Task.CompletedTask;
        }

        private static void FirthStepAction(FlowContextExample ctx)
        {
            ctx.ThirdValue = 200;
        }

        private (string branch, int summ) ProcessContext(FlowContextExample context)
        {
            var summ = context.FirstValue + context.SecondValue + context.ThirdValue;
            return (context.SelectedBranch, summ);
        }
```
</details>

## Programming interfaces

<details>
  <summary>Flow</summary>

```c#
    public abstract class Flow
    {
        public virtual void Dispose();

        // Start the flow for execution
        public TFlowContext RunFlow();
        public async Task<TFlowContext> RunFlowAsync();

        // Build a flow map
        protected virtual void BuildFlowMap() { };
    }
```
</details>

<details>
  <summary>IFlowContext</summary>

```c#
    public interface IFlowContext
    {
        IReadOnlyFlowNode[] CompletedNodes { get ; }
        IReadOnlyFlowNode CurrentFlowNode { get ; }
        IReadOnlyFlowNode NextFlowNode { get ; }
        IReadOnlyFlowNode PreviousFlowNode { get ; }
        
        void Dispose();

        // Assign the next executable node
        void SetNext(string flowNodeIndex);
        void SetNext<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
    }
```
</details>

<details>
  <summary>IFlowMap</summary>

```c#
    public interface IFlowMap<TFlowContext> where TFlowContext : IFlowContext
    {
        bool IsValid { get; }
        FlowMapValidationError[] ValidationErrors { get; }

        // Add a root node to the map
        IFlowNode<TFlowContext> AddRoot(string flowNodeIndex);
        IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
        IFlowNode<TFlowContext> AddRoot(string flowNodeIndex, Action<TFlowContext> flowNodeAction);
        IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex, Action<TFlowContext> flowNodeAction) where TIndex : struct;
        IFlowNode<TFlowContext> AddRoot(string flowNodeIndex, Func<TFlowContext, Task> flowNodeAction);
        IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex, Func<TFlowContext, Task> flowNodeAction) where TIndex : struct;

        // Get a node by its index
        IFlowNode<TFlowContext> GetNode(string flowNodeIndex);
        IFlowNode<TFlowContext> GetNode<TIndex>(TIndex flowNodeIndex) where TIndex : struct;

        // Get root node
        IFlowNode<TFlowContext> GetRoot();
    }
```
</details>

<details>
  <summary>FlowNode</summary>

```c#
    public interface IReadOnlyFlowNode
    {
        string Index { get; }
        bool HasAction { get; }
        bool IsValid { get; }
        FlowNodeType Type { get; }
    }
```

```c#
    public interface IFlowNode<TFlowContext> : IReadOnlyFlowNode where TFlowContext : IFlowContext
    {
        FlowNodeValidationError[] ValidationErrors { get; }

        // Assign an executable action to the node
        IFlowNode<TFlowContext> AddAction(Action<TFlowContext> flowNodeAction);
        IFlowNode<TFlowContext> AddAction(Func<TFlowContext, Task> flowNodeAction);

        // Add a link to the next executable node
        IFlowNode<TFlowContext> AddNext(string flowNodeIndex);
        IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
        IFlowNode<TFlowContext> AddNext(string flowNodeIndex, Action<TFlowContext> flowNodeAction);
        IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex, Action<TFlowContext> flowNodeAction) where TIndex : struct;
        IFlowNode<TFlowContext> AddNext(string flowNodeIndex, Func<TFlowContext, Task> flowNodeAction);
        IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex, Func<TFlowContext, Task> flowNodeAction) where TIndex : struct;
    }
```
</details>

using Flow;
using System.Collections.Generic;

namespace FlowTests.Fakes
{
    public class FakeFlowContext : FlowContext
    {
    }

    public class FakeFlowContext1 : FlowContext
    {
        public int ThreadIdInProcess { get; set; }
    }

    public class FakeFlowContext2 : FlowContext
    {
        public string TestData { get; set; } = "TestData";
    }

    public class FakeFlowContext3 : FlowContext
    {
        public List<int> AfterEachSequence { get; } = new List<int>();
        public List<int> BeforeEachSequence { get; } = new List<int>();
        public List<int> FlowNode1ExecutionSequence { get; } = new List<int>();
        public List<int> FlowNode2ExecutionSequence { get; } = new List<int>();
    }
}

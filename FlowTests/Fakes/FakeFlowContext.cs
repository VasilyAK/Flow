using Flow;

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
}

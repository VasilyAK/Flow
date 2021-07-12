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
}

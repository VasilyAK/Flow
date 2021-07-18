using Flow.Contracts;
using Flow.Exceptions;
using System.Linq;

namespace Flow.Extensions
{
    public static class FlowMapValidationErrorExtensions
    {
        public static FlowException ToFlowException(this FlowMapValidationError flowMapValidationError)
        {
            var flowErrorContext = new FlowExceptionContext { FlowNodeIndex = flowMapValidationError.FlowNodeIndex };

            var flowNodeValidateError = flowMapValidationError.FlowNodeErrors.FirstOrDefault();
            if (flowNodeValidateError != null)
            {
                flowErrorContext.FlowRoutes = flowMapValidationError.FlowNodeErrors.Select(x => x.FlowRoute).ToArray();
                flowErrorContext.Message = $"{flowMapValidationError.Message}. {flowNodeValidateError.Message}.";
            }
            else
                flowErrorContext.Message = flowMapValidationError.Message;

            return new FlowException(flowErrorContext);
        }
    }
}

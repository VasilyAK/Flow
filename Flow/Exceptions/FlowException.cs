using Flow.Contracts;
using Flow.Contracts.Enums;
using System;
using System.Text;

namespace Flow.Exceptions
{
    public class FlowException : Exception
    {
        public FlowException(FlowExceptionContext context) : base(CreateMessage(context)) { }

        public FlowException(string message)
            : base(CreateMessage(new FlowExceptionContext { Message = message })) { }

        public FlowException(string flowNodeIndex, string message)
            : base(CreateMessage(new FlowExceptionContext { FlowNodeIndex = flowNodeIndex, Message = message })) { }
        public FlowException(string flowNodeIndex, string message, string[][] flowRoutes)
            : base(CreateMessage(new FlowExceptionContext { FlowNodeIndex = flowNodeIndex, Message = message, FlowRoutes = flowRoutes })) { }

        public FlowException(string flowNodeIndex, FlowNodeType flowNodeType, string message)
            : base(CreateMessage(new FlowExceptionContext { FlowNodeIndex = flowNodeIndex, FlowNodeType = flowNodeType, Message = message })) { }

        private static string CreateMessage(FlowExceptionContext context)
        {
            var prefix = "    -";
            var errorMessage = new StringBuilder();

            if (!string.IsNullOrEmpty(context.Message))
                errorMessage.Append(context.Message);

            if (!string.IsNullOrEmpty(context.FlowNodeIndex))
            {
                errorMessage.AppendLine(string.Empty);
                errorMessage.Append($"{prefix} FlowNodeIndex: {context.FlowNodeIndex}.");
            }

            if (context.FlowNodeType != null)
            {
                errorMessage.AppendLine(string.Empty);
                errorMessage.Append($"{prefix} FlowNodeType: {context.FlowNodeType}.");
            }

            if (context.FlowRoutes.Length > 0)
            {
                errorMessage.AppendLine(string.Empty);
                errorMessage.Append($"{prefix} Found {context.FlowRoutes.Length} possible flow routes:");

                var flowRoutePrefix = "        ";
                foreach (var flowRoute in context.FlowRoutes)
                {
                    errorMessage.AppendLine(string.Empty);
                    errorMessage.Append($"{flowRoutePrefix}{string.Join(" -> ", flowRoute)}");
                }
            }
            return errorMessage.ToString();
        }

        protected static string AddFollowUpMessage(string message, string followUpMessage) =>
            string.IsNullOrEmpty(message) ? followUpMessage : $"{followUpMessage} {message}";

        protected static FlowExceptionContext AddFollowUpMessage(FlowExceptionContext context, string followUpMessage)
        {
            context.Message = AddFollowUpMessage(context.Message, followUpMessage);
            return context;
        }
    }
}

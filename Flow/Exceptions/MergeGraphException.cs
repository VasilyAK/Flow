using System;

namespace Flow.Exceptions
{
    public class MergeGrafhsException : Exception
    {
        public string[] GraphNodeIndexes { get; set; }
        public MergeGrafhsException(string message, string[] graphNodes = null) : base(message)
        {
            GraphNodeIndexes = graphNodes ?? Array.Empty<string>();
        }
    }
}

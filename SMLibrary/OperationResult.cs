using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SMLibrary
{
    public enum OperationStatus
    {
        InProgress,
        Failed,
        Succeeded,
        TimedOut
    }


    public struct OperationResult
    {
        public String RequestID { get; set; } 

        // The status: InProgress, Failed, Succeeded, or TimedOut.
        public OperationStatus Status { get; set; }

        // The http status code of the requestId operation, if any.
        public HttpStatusCode StatusCode { get; set; }

        // The approximate running time for PollGetOperationStatus.
        public TimeSpan RunningTime { get; set; }

        // The error code for the failed operation.
        public String Code { get; set; }

        // The message for the failed operation.
        public String Message { get; set; }

        public override String ToString()
        {
            return String.Format("Requested ID: {0}\n Status: {0}\n Message: {1}\n Elapsed Seconds: {2}", RequestID, Status.ToString(), Message, RunningTime.Seconds);
        }
    }
}

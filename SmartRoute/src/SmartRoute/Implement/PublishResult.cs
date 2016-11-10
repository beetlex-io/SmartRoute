using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartRoute
{
    class PublishResult : IAsyncResult
    {
        private System.Threading.ManualResetEvent mResetEvent = new ManualResetEvent(false);

        private int mStatus = -1;

        private System.Threading.SpinWait mSpinWait = new SpinWait();

        public object AsyncState
        {
            get
            {
                return null;
            }
        }

        public void Reset()
        {
            mStatus = -1;
            Error = null;
            IsCompleted = false;
            Result = null;
            mSpinWait.Reset();
            mResetEvent.Reset();
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return mResetEvent;
            }
        }

        public Exception Error
        {
            get;
            internal set;
        }

        public object Result
        {
            get;
            internal set;
        }

        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        public bool Wait(int millisecondsTimeout)
        {
            while (Interlocked.CompareExchange(ref mStatus, 0, -1) != -1)
            {
                mSpinWait.SpinOnce();
                if (IsCompleted)
                    return true;
            }
            return mResetEvent.WaitOne(millisecondsTimeout);
        }

        public void Completed(object result, Exception error)
        {

            IsCompleted = true;
            Result = result;
            Error = error;
            if (Interlocked.Exchange(ref mStatus, 1) == 0)
            {
                mResetEvent.Set();
            }


        }

        public bool IsCompleted
        {
            get;
            private set;

        }
    }
}

using System;
using System.Threading;

namespace wscloudy.Threads
{
    public class ActionTimeout
    {
#region 调用范例
        private static void Example(string[] args)
        {
            CallWithTimeout(FiveSecondMethod, 6000);
            CallWithTimeout(FiveSecondMethod, 4000);
        }
        static void FiveSecondMethod()
        {
            Thread.Sleep(5000);
        }
#endregion
        public static void CallWithTimeout(Action action, int timeoutMilliseconds)
        {
            Thread threadToKill = null;
            Action wrappedAction = () =>
            {
                threadToKill = Thread.CurrentThread;
                action();
            };
            IAsyncResult result = wrappedAction.BeginInvoke(null, null);
            if (result.AsyncWaitHandle.WaitOne(timeoutMilliseconds))
            {
                wrappedAction.EndInvoke(result);
            }
            else
            {
                threadToKill.Abort();
                throw new TimeoutException();
            }
        }
    }
}

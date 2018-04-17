using System;
using System.Threading;

/// <summary>
/// 线程控制、超时判定等
/// </summary>
namespace wscloudy.Threads
{
#if !NET_20
    /// <summary>
    /// .net 4.0无参数函数执行超时限制
    /// </summary>
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
#endif

    /// <summary>
    /// 控制函数执行时间,超时返回null不继续执行
    /// 调用方法
    /// FuncTimeout.EventNeedRun action = delegate(object[] param)
    /// {
    ///     //调用自定义函数
    ///     return Test(param[0].ToString(), param[1].ToString(), (DateTime)param[2]);
    /// };
    /// FuncTimeout ft = new FuncTimeout(action, 2000);
    /// var result = ft.doAction("1", "2", DateTime.Now);
    /// if (result != null)………………
    /// </summary>
    public class FuncTimeout
    {
        /// <summary>
        /// 信号量
        /// </summary>
        public ManualResetEvent manu = new ManualResetEvent(false);
        /// <summary>
        /// 是否接受到信号
        /// </summary>
        public bool isGetSignal;
        /// <summary>
        /// 设置超时时间
        /// </summary>
        public int timeout;
        /// <summary>
        /// 定义一个委托 ，输入参数可选，输出object
        /// </summary>
        public delegate object EventNeedRun(params object[] param);
        /// <summary>  
        /// 要调用的方法的一个委托  
        /// </summary>  
        private EventNeedRun FunctionNeedRun;

        /// <summary>
        /// 构造函数，传入超时的时间以及运行的方法
        /// </summary>
        /// <param name="_action">运行的方法 </param>
        /// <param name="_timeout">超时的时间</param>
        public FuncTimeout(EventNeedRun _action, int _timeout)
        {
            FunctionNeedRun = _action;
            timeout = _timeout;
        }

        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="ar"></param>
        public void MyAsyncCallback(IAsyncResult ar)
        {
            //isGetSignal为false,表示异步方法其实已经超出设置的时间，此时不再需要执行回调方法。
            if (isGetSignal == false)
            {
                //放弃执行回调函数;
                Thread.CurrentThread.Abort();
            }
        }

        /// <summary>
        /// 调用函数
        /// </summary>
        /// <param name="input">可选个数的输入参数</param>
        /// <returns></returns>
        public object doAction(params object[] input)
        {
            EventNeedRun WhatTodo = CombineActionAndManuset;
            //通过BeginInvoke方法，在线程池上异步的执行方法。
            var r = WhatTodo.BeginInvoke(input, MyAsyncCallback, null);
            //设置阻塞,如果上述的BeginInvoke方法在timeout之前运行完毕，则manu会收到信号。此时isGetSignal为true。
            //如果timeout时间内，还未收到信号，即异步方法还未运行完毕，则isGetSignal为false。
            isGetSignal = manu.WaitOne(timeout);

            if (isGetSignal == true)
            {
                return WhatTodo.EndInvoke(r);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 把要传进来的方法，和 manu.Set()的方法合并到一个方法体。
        /// action方法运行完毕后，设置信号量，以取消阻塞。
        /// </summary>
        /// <param name="input">输入参数</param>
        /// <returns></returns>
        public object CombineActionAndManuset(params object[] input)
        {
            var output = FunctionNeedRun(input);
            manu.Set();
            return output;
        }
    }
}

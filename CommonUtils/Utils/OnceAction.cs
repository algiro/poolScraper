using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace CommonUtils.Utils
{
    public class OnceAction
    {
        private static ILog log = LogManager.GetLogger(typeof(OnceAction));

        private readonly OnceFlag _onceFlag = new OnceFlag();

        public OnceAction(Action action)
        {
            Action = action;
        }

        public void Execute()
        {
            log.Debug("<<Execute>> invoked");
            if (_onceFlag.CheckIfCalledAndSet)
            {
                log.Debug("<<Execute>> executing once");
                Action();
            }
        }

        private Action Action { get; }

        public override string ToString() => Action.Method.Name + " invoked: " + _onceFlag;
    }
}

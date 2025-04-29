using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace CommonUtils.Utils
{
    public class OnceFlag
    {
        private const int NOT_CALLED = 0;
        private const int CALLED = 1;
        private int _state = NOT_CALLED;

        public bool CheckIfCalledAndSet
        {
            get
            {
                var prev = Interlocked.Exchange(ref _state, CALLED);

                return prev == NOT_CALLED;
            }
        }

        public bool IsTrue => _state == CALLED;

        public void Reset()
        {
            Interlocked.Exchange(ref _state, NOT_CALLED);
        }

        public override string ToString() => _state.ToString();
    }
}

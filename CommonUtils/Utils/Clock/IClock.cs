using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonUtils.Utils.Clock
{
    public interface IClock
    {
        DateTime UTCNow { get; }
        long UtcTimeNow { get; }
        void Sleep(int ms);
        Task SleepAsync(int ms, CancellationToken cancellationToken);
    }
}

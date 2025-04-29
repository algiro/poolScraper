using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Utils.Clock
{
    public interface IStopWatch
    {
        TimeSpan Elapsed();
    }
}

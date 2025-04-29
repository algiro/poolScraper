using CommonUtils.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Inspect
{
    public static class ModelUtils
    {
        public static IEnumerable<string> GetNullPropertyNames<T>(this T obj) where T : class
        {
            return obj.GetType()
                      .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                      .Where(prop => prop.GetValue(obj) == null)
                      .SelectNotNull(prop => prop.Name);
        }
    }
}

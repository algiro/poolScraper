using CommonUtils.Utils;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MongoDB.Driver.Linq;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PoolScraper.Model.Consolidation
{
    public enum Granularity
    {
        Minutes = 10,
        Hours = 20,
        Days = 30,
        Weeks = 40,
        Months = 50,
        Custom = 60
    }
    public static class ConsolidationGranularityExtension
    {
        delegate string GetIdDelegate(DateTime dateTime);
        delegate DateTime GetDTDelegate(string id);
        private static readonly Dictionary<Granularity, (GetIdDelegate GetId, GetDTDelegate GetDT, int Weight)> _granularityToIdMap = new()
        {
            { Granularity.Minutes, ((d) => d.ToString("yyyyMMdd.HHmm"),(id) => GetDateTimeFromId(id,"yyyyMMdd.HHmm"),1) },
            { Granularity.Hours, ((d) => d.ToString("yyyyMMdd.HH"),(id) => GetDateTimeFromId(id,"yyyyMMdd.HH"),60) },
            { Granularity.Days, ((d) => d.ToString("yyyyMMdd"),(id) => GetDateTimeFromId(id,"yyyyMMdd"),1440) },
            { Granularity.Weeks, ((d) => d.ToString("yyyy") + "." + d.GetWeekOfYear(),(id) => GetDateTimeFromId(id,"yyyy.ww"),10080) },
            { Granularity.Months, ((d) => d.ToString("yyyy") + "." + d.Month,(id) => GetDateTimeFromId(id,"yyyy.MM"),43200) },
            { Granularity.Custom, ((d) => d.ToString("yyyyMMdd.HHmm"),(id) => GetDateTimeFromId(id,"yyyyMMdd.HHmm"),0) }

        };
        public static string GetId(this Granularity granularity, DateTime dateTime)
            => _granularityToIdMap[granularity].GetId(dateTime);
        public static string GetId(this Granularity granularity, IDateRange dateRange)
            => _granularityToIdMap[granularity].GetId(dateRange.MiddleDateTime());

        public static DateTime GetDateTime(this Granularity granularity, string id)
            => _granularityToIdMap[granularity].GetDT(id);
        public static int GetWeight(this Granularity granularity)
            => _granularityToIdMap[granularity].Weight;

        private static DateTime GetDateTimeFromId(string id,string format)
            =>  DateTime.ParseExact(id, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
        
    }
}

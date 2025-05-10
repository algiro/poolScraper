using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using TimeProvider = CommonUtils.Utils.Clock.TimeProvider;

namespace CommonUtils.Utils;

public static class DateUtils
{        
    private const string DATETIME_FORMAT =                      "yyyyMMdd";
    private const string DATE_TIME_FORMAT =                     "yyyy-MM-dd";
    private const string DATETIME_ONLY_MONTH_FORMAT =           "yyyyMM";
    private const string DATE_TIME_ONLY_MONTH_FORMAT =          "yyyy-MM";
    private static readonly string[] DATETIME_FORMATS = new string[] { DATE_TIME_FORMAT, DATETIME_FORMAT, DATETIME_ONLY_MONTH_FORMAT, DATE_TIME_ONLY_MONTH_FORMAT };

    private const string SEC_TIMESTAMP_FORMAT =                 "yyyyMMdd-HH:mm:ss";
    private const string MS_TIMESTAMP_FORMAT =                  "yyyyMMdd-HH:mm:ss.fff";
    private const string DEFAULT_TIMESTAMP_FORMAT =             "yyyyMMdd-HH:mm:ss.fffff";
    private const string MICROSEC_TIMESTAMP_FORMAT =            "yyyyMMdd-HH:mm:ss.ffffff";
    private const string ONE_HUNDRED_NANOSEC_TIMESTAMP_FORMAT = "yyyyMMdd-HH:mm:ss.fffffff";
    private const string TIMESTAMP_FORMAT_1 =                   "yyyy-MM-dd HH:mm:ss.f";
    private static readonly string[] TIMESTAMP_FORMATS = new string[] { DEFAULT_TIMESTAMP_FORMAT, MICROSEC_TIMESTAMP_FORMAT, ONE_HUNDRED_NANOSEC_TIMESTAMP_FORMAT, MS_TIMESTAMP_FORMAT, SEC_TIMESTAMP_FORMAT, TIMESTAMP_FORMAT_1 };

    /* "yyyyMMdd-HH:mm:ss.fffff"  => "20161013-16:41:53.96937" */
    public static DateTime ParseDefaultTimeStamp(this string timeStampString) => DateTime.ParseExact(timeStampString, TIMESTAMP_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None);

    public static DateTime? ParseNullableDefaultTimeStamp(this string? timeStampString) => timeStampString.IsEmpty() ? null : timeStampString.ParseDefaultTimeStamp();

    public static DateTime? ParseNullableDefaultDateTime(this string? dateTimeString) => dateTimeString.IsEmpty() ? null : dateTimeString.ParseDefaultDateTime();

    public static DateTime ParseDefaultDateTime(this string dateTimeString) => DateTime.ParseExact(dateTimeString, DATETIME_FORMATS, CultureInfo.InvariantCulture,DateTimeStyles.None);

    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime AsDateTimeSinceUnixEpoch(this ulong nanosecs) => UnixEpoch.AddTicks((long)(nanosecs / 100));
    
    public static DateTime AsDateTimeSinceUnixEpochMicroSec(this ulong microsecs) => UnixEpoch.AddTicks((long)(microsecs * 10));
    
    public static DateTime AsDateTimeSinceUnixEpochMilliSec(this ulong millisecs) => UnixEpoch.AddTicks((long)(millisecs * 10_000));

#if NET48
    public static ulong MicrosecondsSinceUnixEpoch(this DateTime dateTime) => (ulong)(dateTime - UnixEpoch).Ticks / 10;
    public static ulong NanosecondsSinceUnixEpoch(this DateTime dateTime) => (ulong)(dateTime - UnixEpoch).Ticks * 100;
#else
    public static ulong MicrosecondsSinceUnixEpoch(this DateTime dateTime) => (ulong)(dateTime - UnixEpoch).TotalMicroseconds;
    public static ulong NanosecondsSinceUnixEpoch(this DateTime dateTime) => (ulong)(dateTime - UnixEpoch).TotalNanoseconds;
#endif

    public static DateTime Last(this DateTime dt, DayOfWeek dayOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - dayOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }

    public static DateTime PreviousWorkingDate(this DateTime date)
    {
        DayOfWeek previousWorkingDayOfWeek = date switch 
        {
            { DayOfWeek: <= DayOfWeek.Monday } => DayOfWeek.Friday,
            _ => date.DayOfWeek - 1,
        };
        return date.Last(previousWorkingDayOfWeek);
    }

    public static DateTime Next(this DateTime dt, DayOfWeek dayOfWeek) => dt.Last(dayOfWeek).AddDays(7).Date;

    public static TimeSpan DaysTo(this DateTime futureDateTime) => new DateTime(futureDateTime.Year,futureDateTime.Month, futureDateTime.Day) - DateTime.Today;

    public static TimeSpan TimeSpentFrom(this DateTime start) => new DateTime() - start;

    public static string ToYYYYMMddString(this DateTime dateTime) => dateTime.ToString("yyyyMMdd");

    public static string FormatDefaultTimeStamp(this DateTime dateTime) => new(dateTime.Format10MicrosecTime(new char[23]));

    public static string FormatMsTimeStamp(this DateTime dateTime) => new(dateTime.FormatMillisecondsTime(new char[21]));

    public static string FormatMicroSecTimeStamp(this DateTime dateTime) => new(dateTime.FormatMicrosecTime(new char[24]));

    public static string Format100NanoSecTimeStamp(this DateTime dateTime) => new(dateTime.Format100NanosecTime(new char[25]));
    
    public static string FormatNanoSecTimeStamp(this DateTime dateTime) => new(dateTime.FormatNanosecTime(new char[27]));

    public static string FormatSecTimeStamp(this DateTime dateTime) => new(dateTime.FormatSecTime(new char[17]));
    
    public static double CalculateMillisecondsFromNow(this DateTime date) => CalculateDifferenceInMilliseconds(TimeProvider.Current.UtcNow, date);
    
    public static double CalculateDifferenceInMilliseconds(this DateTime newDate,DateTime oldDate) => (newDate - oldDate).TotalMilliseconds;

    public static string FormatTime_YYYYMMDD_HHmm(this DateTime dt)
    {
        char[] chars = new char[13];
        Write4Chars(chars, 0, dt.Year);
        Write2Chars(chars, 4, dt.Month);
        Write2Chars(chars, 6, dt.Day);
        chars[8] = ' ';
        Write2Chars(chars, 9, dt.Hour);
        Write2Chars(chars, 11, dt.Minute);
        return new string(chars);
    }
    public static string FormatTime_YYYYMMDD(this DateTime dt)
    {
        char[] chars = new char[8];
        Write4Chars(chars, 0, dt.Year);
        Write2Chars(chars, 4, dt.Month);
        Write2Chars(chars, 6, dt.Day);
        return new string(chars);
    }

    public static string FormatTime_HHmmssfff(this DateTime dt)
    {
        char[] chars = new char[9];
        Write2Chars(chars, 0, dt.Hour);
        Write2Chars(chars, 2, dt.Minute);
        Write2Chars(chars, 4, dt.Second);
        Write3Chars(chars, 6, dt.Millisecond);
        return new string(chars);
    }

    public static string FormatTime_SecTimestampFormat(this DateTime dt) => dt.ToString(SEC_TIMESTAMP_FORMAT);

    private static char[] FormatSecTime(this DateTime dt, char[] chars)
    {
        chars[0] = Digit(dt.Year / 1000);
        chars[1] = Digit(dt.Year % 1000 / 100);
        chars[2] = Digit(dt.Year % 100 / 10);
        chars[3] = Digit(dt.Year % 10);
        Write2Chars(chars, 4, dt.Month);
        Write2Chars(chars, 6, dt.Day);
        chars[8] = '-';
        Write2Chars(chars, 9, dt.Hour);
        chars[11] = ':';
        Write2Chars(chars, 12, dt.Minute);
        chars[14] = ':';
        Write2Chars(chars, 15, dt.Second);
        return chars;
    }

    private static char[] FormatMillisecondsTime(this DateTime dt, char[] chars)
    {
        dt.FormatSecTime(chars);
        chars[17] = '.';
        Write3Chars(chars, 18, dt.Millisecond);
        return chars;
    }

    private static int Get(this DateTime dt, TIME_FRACTION timeFraction) => dt.Ticks.Get(timeFraction);

    private static int Get(this long ticks, TIME_FRACTION timeFraction) => (int)(ticks % 10_000) / (int)timeFraction;

    private static char[] Format10MicrosecTime(this DateTime dt, char[] chars) => dt.FormatMillisecondsTime(chars).Write2Chars(21, dt.Get(TIME_FRACTION.MICROSEC_10));

    private static char[] FormatMicrosecTime(this DateTime dt, char[] chars) => dt.FormatMillisecondsTime(chars).Write3Chars(21, dt.Get(TIME_FRACTION.MICROSEC));

    private static char[] Format100NanosecTime(this DateTime dt, char[] chars) => dt.FormatMillisecondsTime(chars).Write4Chars(21, dt.Get(TIME_FRACTION.NANOSEC_100));
    
    private static char[] FormatNanosecTime(this DateTime dt, char[] chars) => dt.Format100NanosecTime(chars).Write2Chars(23, 0); // we just add two '0' digits at the end

    /// <summary>
    /// Write2Chars
    /// </summary>
    private static char[] Write2Chars(this char[] buffer, int offset, int value)
    {
        buffer[offset] = (value / 10).Digit();
        buffer[offset + 1] = (value % 10).Digit();
        return buffer;
    }

    /// <summary>
    /// Write3Chars
    /// </summary>
    private static char[] Write3Chars(this char[] buffer, int offset, int value)
    {
        buffer[offset++] = (value / 100).Digit();
        buffer[offset++] = ((value / 10) % 10).Digit();
        buffer[offset] = (value % 10).Digit();
        return buffer;
    }

    /// <summary>
    /// Write4Chars
    /// </summary>
    private static char[] Write4Chars(this char[] buffer, int offset, int value)
    {
        buffer[offset++] = (value / 1000).Digit();
        buffer[offset++] = ((value / 100) % 10).Digit();
        buffer[offset++] = ((value / 10) % 10).Digit();
        buffer[offset] = (value % 10).Digit();
        return buffer;
    }

    /// <summary>
    /// Digit
    /// </summary>
    private static char Digit(this int value) => (char)(value + '0');

    public static DateTime GetOldest(params DateTime[] dates) => dates.Sort().First();
    
    public static DateTime GetLatest(params DateTime[] dates) => dates.Sort().Last();

    private enum TIME_FRACTION
    {
        MICROSEC_10 = 100,
        MICROSEC = 10,
        NANOSEC_100 = 1,
    }
    public static DateTime AddWorkingDays(this DateTime sourceDay, int days)
    {
        DateTime currentDay = sourceDay;
        int workingDays = 0;
        int dayToAdd = days < 0 ? -1 : 1;
        while (workingDays < Math.Abs(days))
        {
            currentDay = currentDay.AddDays(dayToAdd);
            if (currentDay.DayOfWeek != DayOfWeek.Sunday && currentDay.DayOfWeek != DayOfWeek.Saturday) { workingDays++; }
        }
        return currentDay;
    }
    public static bool IsSameDate(this DateTime source, DateTime target) => source.Date == target.Date;
    
    public static int LastDayOfTheMonth(this DateTime  dt) => DateTime.DaysInMonth(dt.Year, dt.Month);
    public static DateTime FirstDateOfTheMonth(this DateTime dt) => new DateTime(dt.Year, dt.Month, 1);
    public static DateTime LastDateOfTheMonth(this DateTime dt) => new DateTime(dt.Year,dt.Month,dt.LastDayOfTheMonth());

    public static DateTime GetLastDayOfTheMonth(this DateTime dt) => new DateTime(dt.Year, dt.Month, dt.LastDayOfTheMonth());

#if NET48
#else
    private static readonly TimeOnly MIDNIGHT = new TimeOnly(0, 0);
    private static readonly TimeOnly LAST_MOMENT_OF_A_DAY = new TimeOnly(23, 59,59,999,999);

    public static DateOnly Today => DateTime.Today.ToDateOnly();
    
    public static DateOnly Tomorrow => Today.AddDays(1);
    public static int GetWeekOfYear(this DateTime date) => ISOWeek.GetWeekOfYear(date);

    public static TimeSpan GetTimeTillEndOf(this DateOnly date) => date >= Today ? date.AddDays(1).ToMidnightDateTime() - DateTime.Now : throw new ArgumentException($"Specified {nameof(date)}:{date} cannot be before today:{Today}");

    public static DateTime AddWorkingDays(this DateOnly sourceDay, int days) => sourceDay.ToMidnightDateTime().AddWorkingDays(days);

    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateOnly? ToDateOnly(this DateTime? dateTime) => dateTime == null ? null : DateOnly.FromDateTime(dateTime.Value);
    
    public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);

    public static DateTime At(this DateTime dateTime, TimeOnly time) => dateTime.ToDateOnly().ToDateTime(time);

    [return: NotNullIfNotNull(nameof(dates))]
    public static (DateTime From, DateTime? To)? ToDateTimes(this (DateOnly From, DateOnly? To)? dates) => dates?.ToDateTimes();

    public static (DateTime From, DateTime? To) ToDateTimes(this (DateOnly From, DateOnly? To) dates) => (dates.From.ToDateTime(), dates.To.ToDateTime());

    public static DateTime ToDateTime(this DateOnly date) => date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    [return: NotNullIfNotNull(nameof(date))]
    public static DateTime? ToDateTime(this DateOnly? date) => date?.ToDateTime();

    public static DateTime ToMidnightDateTime(this DateOnly dateOnly) => dateOnly.ToDateTime(MIDNIGHT, DateTimeKind.Utc);
    public static DateTime GetBeginOfDay(this DateOnly dateOnly) => dateOnly.ToMidnightDateTime();
    public static DateTime GetEndOfDay(this DateOnly dateOnly) => dateOnly.ToDateTime(LAST_MOMENT_OF_A_DAY,DateTimeKind.Utc);
    public static DateTime GetBeginOfDay(this DateTime dateTime) => DateOnly.FromDateTime(dateTime).ToMidnightDateTime();
    public static DateTime GetEndOfDay(this DateTime dateTime) => DateOnly.FromDateTime(dateTime).ToDateTime(LAST_MOMENT_OF_A_DAY, DateTimeKind.Utc);

    public static DateTime GetBeginOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }
    public static int DaysToNow(this DateTime date) => DaysToNow(date.ToDateOnly());
    public static int DaysToNow(this DateOnly date) => DaysDiff(date, DateOnly.FromDateTime(TimeProvider.Current.UtcNow));

    public static IEnumerable<DateOnly> AllDays(this (DateOnly From, DateOnly To) date)
    {
        if (date.To < date.From) throw new ArgumentException($"{date.From.FormatTime_YYYYMMDD()} must be greater or equal to {date.To.FormatTime_YYYYMMDD()}");
        while (date.To >= date.From)
        {
            yield return date.From;
            date.From = date.From.AddDays(1);
        }
    }

    public static IEnumerable<DateOnly> AllDays(this (DateOnly From, DateOnly? To) date) => (date.From, date.To ?? date.From).AllDays();
    
    public static IEnumerable<DateOnly> AllDaysSince(this DateOnly fromDate) => (fromDate, Today).AllDays();

    public static int DaysDiff(this DateOnly date1, DateOnly date2) => date1.DayNumber - date2.DayNumber;

    public static bool IsSameDate(this DateOnly date1, DateOnly date2) => date1.DayNumber == date2.DayNumber;

    public static bool TryParseDateOnly(string? value, string format, out DateOnly dateTime, DateOnly? defaultValue = null)
    {
        if (DateOnly.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            dateTime = result;
            return true;
        }
        else
        {
            dateTime = defaultValue ?? default;
            return false;
        }
    }

    public static string FormatTime_YYYYMMDD(this DateOnly dt)
    {
        char[] chars = new char[8];
        Write4Chars(chars, 0, dt.Year);
        Write2Chars(chars, 4, dt.Month);
        Write2Chars(chars, 6, dt.Day);
        return new string(chars);
    }

    public static DateOnly ParseDefaultDateOnly(this string dateTimeString) => dateTimeString.ParseDefaultDateTime().ToDateOnly();
#endif
}

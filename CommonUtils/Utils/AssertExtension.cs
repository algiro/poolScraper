using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;

namespace CommonUtils.Utils
{
    public static class AssertExtension
    {
        public delegate void ActionInCaseNotTrue();

        public delegate Exception CreateException(string msg);

        public delegate string GetErrorMessage();

        private static CreateException ArgumentException => msg => new ArgumentException(msg);

        private static CreateException ArgumentNullException => msg => new ArgumentNullException(msg);

        private static CreateException ArgumentOutOfRangeException => msg => new ArgumentOutOfRangeException(msg);

        public static T CheckTrue<T>(this T obj, bool value, string msg, CreateException? CreateException = null)
            => obj.CheckTrue(value, () => msg, CreateException);

        public static T CheckTrue<T>(this T obj, bool value, GetErrorMessage GetErrorMessage, CreateException? CreateException = null)
            => obj.CheckTrue(value, (ActionInCaseNotTrue)(() => throw (CreateException ?? ArgumentException)(GetErrorMessage())));

        public static T CheckTrue<T>(this T obj, bool value, ActionInCaseNotTrue ActionInCaseNotTrue)
        {
            if (!value)
            {
                ActionInCaseNotTrue();
            }
            return obj;
        }

        public static T CheckFalse<T>(this T obj, bool value, string msg) => obj.CheckTrue(!value, msg);

        public static T CheckNull<T>(this T obj, object? value, string msg) => obj.CheckTrue(value == null, msg);

        public static T CheckNotNull<T>([NotNull] this T? obj, string objValueName = "value") => (obj != null) ? obj : throw new ArgumentNullException($"{objValueName} cannot be null!");
        
        public static T CheckNotNull<T>([NotNull] this T? obj, string objValueName = "value") where T: struct => (obj != null) ? (T)obj : throw new ArgumentNullException($"{objValueName} cannot be null!");

        public static T CheckNotNull<T>(this T obj, [NotNull] object? value, string objValueName)
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
            => obj.CheckTrue(value != null, $"{objValueName} cannot be null!", ArgumentNullException);
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.

        public static T CheckNotEmpty<T>(this T obj, [NotNull] string? value, string objValueName)
        {
            CheckNotEmpty(value, objValueName);
            return obj;
        }

        public static string CheckNotEmpty([NotNull] this string? value, string objValueName = "string")
        {
            CheckNotEmpty((IEnumerable<char>?)value, objValueName);
            return value;
        }

        public static T CheckNotEmpty<T, V>(this T obj, [NotNull] IEnumerable<V>? value, string objValueName)
        {
            value.CheckNotEmpty(objValueName);
            return obj;
        }

        public static IEnumerable<V> CheckNotEmpty<V>([NotNull] this IEnumerable<V>? value, string valueName = "enumerable") =>
            value.CheckNotNull(valueName).CheckTrue(value.Count() > 0, $"{valueName} cannot be empty!");

        public static T CheckNotEmptyWhenNotNull<T>(this T obj, string? value, string objValueName) => value != null ? obj.CheckNotEmpty(value, objValueName) : obj;

        public static T CheckInRange<T>(this T obj, int value, int minValue, int maxValue, string objValueName) =>
            obj.CheckTrue(value >= minValue && value <= maxValue, $"{objValueName} must be in range [{minValue},{maxValue}] not ({value})", ArgumentOutOfRangeException);

        public static T CheckValidIpAddress<T>(this T obj, string? ipAddress, string objValueName) =>
            obj.CheckNotEmpty(ipAddress, objValueName)
               .CheckTrue(IPAddress.TryParse(ipAddress, out IPAddress? validIpAddress) && validIpAddress.ToString() == ipAddress, $"{objValueName} is not a valid ipAddress:{ipAddress}");

        public static T CheckValidPort<T>(this T obj, string? port, string objValueName) =>
            obj.CheckNotEmpty(port, objValueName)
               .CheckTrue(int.TryParse(port, out int validPort) && validPort.ToString() == port && validPort <= 65535 && validPort > 0, $"{objValueName} is not a valid port:{port}");

        public static T CheckValidIpAddressPort<T>(this T obj, string? ipAddressPort, string objValueName) =>
            obj.CheckValidIpAddress(ipAddressPort?.Substring(0, ipAddressPort.IndexOf(':')), objValueName)
               .CheckValidPort(ipAddressPort?.Substring(ipAddressPort.IndexOf(':') + 1), objValueName);

        public static T Check<T>(this T obj, Action Check)
        {
            Check();
            return obj;
        }

        public static T Check<T>(this T obj, Func<object> Check) => obj.Check(() => { Check(); });
        
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Utils
{
    public static class MathUtils
    {
        // Fast access for 10^n where n is 0-18        
        private static readonly long[] Powers10 = new long[] {
        1,
        10,
        100,
        1000,
        10000,
        100000,
        1000000,
        10000000,
        100000000,
        1000000000,
        10000000000,
        100000000000,
        1000000000000,
        10000000000000,
        100000000000000,
        1000000000000000,
        10000000000000000,
        100000000000000000,
        1000000000000000000,
    };

        public static long Pow10(int exponent) => exponent >= 0 && exponent < 19 ? Powers10[exponent] : throw new OverflowException($"Exponent too big or too small, it should be [0-18], not {exponent}");

        public static int LCM(int a, int b) => Math.Abs(a * b) / GCD(a, b);
        public static long LCM(long a, long b) => Math.Abs(a * b) / GCD(a, b);
        public static decimal LCM(decimal a, decimal b) => Math.Abs(a * b) / GCD((long)a, (long)b);

        public static int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);
        public static long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);

        public static ushort Difference(ushort maxValue, ushort minValue) => (ushort)(maxValue - minValue);

        public static (ushort Min, ushort Max) MinMax(ushort v1, ushort v2) => v1 < v2 ? (v1, v2) : (v2, v1);
        public static (short Min, short Max) MinMax(short v1, short v2) => v1 < v2 ? (v1, v2) : (v2, v1);
        public static (int Min, int Max) MinMax(int v1, int v2) => v1 < v2 ? (v1, v2) : (v2, v1);
        public static (long Min, long Max) MinMax(long v1, long v2) => v1 < v2 ? (v1, v2) : (v2, v1);

        public static ushort MaxBetweenNearValues(ushort v1, ushort v2) => v1 == v2 ? v2 : MinMax(v1, v2) is var value && Difference(value.Max, value.Min) > Difference(value.Min, value.Max) ? value.Min : value.Max;

        public static long MaxTruncate(double value)
        {
            int log10 = (int)Math.Log10(value);
            long pow10 = log10 >= 0 ? Pow10(log10) : (long)Math.Pow(10, log10);
            long base10 = Math.Max(pow10, 1);
            return ((long)value / base10) * base10;
        }

        public static double TruncateDecimal(double value, int precision)
        {
            long step = Pow10(precision);
            decimal tmp = Math.Truncate((decimal)(step * value));
            return (double)(tmp / step);
        }

        public static double CeilingDecimal(double value, int precision)
        {
            long step = Pow10(precision);
            decimal decimalValue = (decimal)(step * value);
            decimal tmp = Math.Truncate(decimalValue);
            tmp = tmp == decimalValue ? tmp : tmp + 1; // CeilingDecimal(1.1, precision: 1) = 1.1 not 1.2
            return (double)(tmp / step);
        }

        public delegate double Round(double value, int places);

        public static double RoundDown(double value, int places) => RoundDownStep(value, places, 1);

        public static double RoundUp(double value, int places) => RoundUpStep(value, places, 1);

        public static decimal RoundUp(decimal value, int places) => RoundUpStep(value, places, 1);

        public static double RoundUpStep(double value, int places, int step)
        {
            if (value < 0)
                return -RoundDownStep(-value, places, step);

            // Formula:
            // result = { ceil( value * 10^places) + [ (step - ceil( value * 10^places) % step) % step ] } / 10^places

            double pow_10_places = Pow10(places);
            double value_per_pow_10_places = value * pow_10_places;
            // Double is faster in other calculations
            // Only here the decimal type is really needed to avoid wrong decimal cut
            long integerPart = (long)Math.Ceiling((decimal)value_per_pow_10_places);
            long tickAdd = (step - (integerPart % step)) % step;
            long integerResult = integerPart + tickAdd;
            return integerResult / pow_10_places;
        }

        public static decimal RoundUpStep(decimal value, int places, int step)
        {
            if (value < 0)
                return -RoundDownStep(-value, places, step);

            // Formula:
            // result = { ceil( value * 10^places) + [ (step - ceil( value * 10^places) % step) % step ] } / 10^places

            decimal pow_10_places = Pow10(places);
            decimal value_per_pow_10_places = value * pow_10_places;
            // Double is faster in other calculations
            // Only here the decimal type is really needed to avoid wrong decimal cut
            long integerPart = (long)Math.Ceiling(value_per_pow_10_places);
            long tickAdd = (step - (integerPart % step)) % step;
            long integerResult = integerPart + tickAdd;
            return integerResult / pow_10_places;
        }

        public static double RoundDownStep(double value, int places, int step)
        {
            if (value < 0)
                return -RoundUpStep(-value, places, step);

            // Formula:
            // result = { floor( value * 10^places) - [ floor( value * 10^places) % step ] } / 10^places

            double pow_10_places = Pow10(places);
            double value_per_pow_10_places = value * pow_10_places;
            // Double is faster in other calculations
            // Only here the decimal type is really needed to avoid wrong decimal cut
            long integerPart = (long)Math.Floor((decimal)value_per_pow_10_places);
            long tickRemove = integerPart % step;
            long integerResult = integerPart - tickRemove;
            return integerResult / pow_10_places;
        }
        public static decimal RoundDownStep(decimal value, int places, int step)
        {
            if (value < 0)
                return -RoundUpStep(-value, places, step);

            // Formula:
            // result = { floor( value * 10^places) - [ floor( value * 10^places) % step ] } / 10^places

            decimal pow_10_places = Pow10(places);
            decimal value_per_pow_10_places = value * pow_10_places;
            // Double is faster in other calculations
            // Only here the decimal type is really needed to avoid wrong decimal cut
            long integerPart = (long)Math.Floor(value_per_pow_10_places);
            long tickRemove = integerPart % step;
            long integerResult = integerPart - tickRemove;
            return integerResult / pow_10_places;
        }

        public static double DecimalPartRoundUp(double value, int precision) => RoundUp(value, precision) - Math.Floor(value);

        public static decimal DecimalPartRoundUp(decimal value, int precision) => RoundUp(value, precision) - Math.Floor(value);

        public static double SumPrecision(double op1, double op2) => Convert.ToDouble(Convert.ToDecimal(op1) + Convert.ToDecimal(op2));

        public static int GetDecimalPlaces(decimal value) => BitConverter.GetBytes(decimal.GetBits(value)[3])[2];

        /// <summary>
        /// This method verifies if the specified values are equals or one is more accurate than the other. 
        /// It returns false if they are not compatible.
        /// One value is more accurate then the other if their difference is less then the order of magnitude of the one with less decimal digits.
        /// <example>1,0.1 -> true: 0.1</example>
        /// <example>0.9,0 -> true: 0.9</example>
        /// <example>-0.9,0 -> true: -0.9</example>
        /// <example>-0.9,-1 -> true: -0.9</example>
        /// <example>5.932,5.93234 -> true: 5.93234</example>
        /// <example>5.932,5.933 -> false</example>
        /// <example>1,0 -> false</example>
        /// </summary>
        /// <param name="value">the first value.</param>
        /// <param name="other">the second value.</param>
        /// <param name="moreAccurateValue">it will contain the mostAccurate value between the 2 specified or null if they are not compatible</param>
        /// <returns>true if the specified values are equals or one is more accurate than the other</returns>
        public static bool TryGetMoreAccurate(decimal value, decimal otherValue, [NotNullWhen(true)] out decimal? moreAccurateValue)
        {
            if (value == otherValue)
            {
                moreAccurateValue = value;
                return true;
            }
            else
            {
                int n = 10;
                decimal dvalue = value;
                decimal dotherValue = otherValue;
                decimal roundDValue = decimal.Round(dvalue);
                decimal roundDOtherValue = decimal.Round(dotherValue);
                while (dvalue != roundDValue && dotherValue != roundDOtherValue)
                {
                    dvalue *= n;
                    dotherValue *= n;
                    roundDValue = decimal.Round(dvalue);
                    roundDOtherValue = decimal.Round(dotherValue);
                }
                if (Math.Abs(dvalue - dotherValue) < 1 && Math.Abs(roundDValue - roundDOtherValue) <= 1)
                {
                    moreAccurateValue = dvalue != roundDValue || otherValue == 0 ? value : otherValue;
                    return true;
                }
                else
                {
                    moreAccurateValue = null;
                    return false;
                }
            }
        }

        public static bool TryGetMoreAccurate<T>(T obj, T otherObj, Func<T, decimal> GetValue, [NotNullWhen(true)] out T? moreAccurateObj) where T : class
        {
            decimal value = GetValue(obj);
            decimal otherValue = GetValue(otherObj);
            if (MathUtils.TryGetMoreAccurate(value, otherValue, out var moreAccurateValue))
            {
                moreAccurateObj = value == moreAccurateValue ? obj : otherObj;
                return true;
            }
            else
            {
                moreAccurateObj = null;
                return false;
            }
        }

        public static bool EqualsOrMoreAccurate(decimal v1, decimal v2) => TryGetMoreAccurate(v1, v2, out var _);

        public static bool EqualsApproximatively(decimal v1, decimal v2, decimal allowedErrorPercentage)
            => allowedErrorPercentage < 0 ? throw new ArgumentException($"{nameof(allowedErrorPercentage)} must be >= 0 not {allowedErrorPercentage}") :
                v1 == v2 || Math.Abs(v1 - v2) <= Math.Max(Math.Abs(v1), Math.Abs(v2)) * (allowedErrorPercentage / 100);
    }

}

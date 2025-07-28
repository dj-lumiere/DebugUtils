using System.ComponentModel;
using System.Numerics;

public static partial class DebugUtils
{
        private static string ReprDouble(this double t, FloatReprConfig floatReprConfig)
    {
        var bits = BitConverter.DoubleToInt64Bits(t);
        var sign = bits < 0;
        var rawExponent = (int)((bits >> 52) & 0x7FFL);
        var realExponent = rawExponent - 1023;
        var mantissa = bits & 0xFFFFFFFFFFFFFL;
        var significand = rawExponent == 0
            ? mantissa
            : (BigInteger.One << 52) + mantissa;
        var expBits = Convert.ToString(rawExponent, 2)
            .PadLeft(11);
        var mantissaBits = Convert.ToString(mantissa, 2)
            .PadLeft(52);
        if (double.IsPositiveInfinity(t))
        {
            return floatReprConfig.mode switch
            {
                FloatReprMode.RawBytesHex => $"Double(0x{bits:X16})",
                FloatReprMode.BitField => $"Double({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    "Double(Infinity)",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        if (double.IsNegativeInfinity(t))
        {
            return floatReprConfig.mode switch
            {
                FloatReprMode.RawBytesHex => $"Double(0x{bits:X16})",
                FloatReprMode.BitField => $"Double({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    "Double(-Infinity)",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        if (double.IsNaN(t))
        {
            var isQuiet = (bits & 0x8000000000000) != 0;
            var nanType = isQuiet
                ? "quiet"
                : "signaling";
            if (isQuiet)
            {
                return floatReprConfig.mode switch
                {
                    FloatReprMode.RawBytesHex => $"Double(0x{bits:X16})",
                    FloatReprMode.BitField => $"Double({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                    FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                        or FloatReprMode.General =>
                        $"Double({nanType})",
                    _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
                };
            }

            return floatReprConfig.mode switch
            {
                FloatReprMode.RawBytesHex => $"Double(0x{bits:X16})",
                FloatReprMode.BitField => $"Double({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    $"Double({nanType}, Payload: {mantissa:X13})",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        return floatReprConfig.mode switch
        {
            FloatReprMode.RawBytesHex => $"Double(0x{bits:X16})",
            FloatReprMode.BitField => $"Double({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
            FloatReprMode.Round => $"Double({Math.Round(t, floatReprConfig.precision)})",
            FloatReprMode.Scientific =>
                $"Double({t.ToString("E" + (floatReprConfig.precision > 0 ? floatReprConfig.precision - 1 : 0))})",
            // subtracting realExponent - 52 because the significand is form of 0.XXXX or 1.XXXX (depending on whether it is subnormal or not)
            FloatReprMode.Exact =>
                $"Double({PreciseScientificSimple(realExponent - 52, significand, sign)})",
            FloatReprMode.General => $"Double({t})",
            _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
        };
    }
    private static string ReprFloat(this float t, FloatReprConfig floatReprConfig)
    {
        var bits = BitConverter.SingleToInt32Bits(t);
        var sign = bits < 0;
        var rawExponent = (int)((bits >> 23) & 0xFF);
        var realExponent = rawExponent - 127;
        var mantissa = bits & 0x7FFFFF;
        var significand = rawExponent == 0
            ? mantissa
            : (BigInteger.One << 23) + mantissa;
        var expBits = Convert.ToString(rawExponent, 2)
            .PadLeft(8);
        var mantissaBits = Convert.ToString(mantissa, 2)
            .PadLeft(23);
        if (float.IsPositiveInfinity(t))
        {
            return floatReprConfig.mode switch
            {
                FloatReprMode.RawBytesHex => $"Float(0x{bits:X8})",
                FloatReprMode.BitField => $"Float({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    "Float(Infinity)",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        if (float.IsNegativeInfinity(t))
        {
            return floatReprConfig.mode switch
            {
                FloatReprMode.RawBytesHex => $"Float(0x{bits:X8})",
                FloatReprMode.BitField => $"Float({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    "Float(-Infinity)",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        if (float.IsNaN(t))
        {
            var isQuiet = (bits & 0x400000) != 0;
            var nanType = isQuiet
                ? "quiet"
                : "signaling";
            if (isQuiet)
            {
                return floatReprConfig.mode switch
                {
                    FloatReprMode.RawBytesHex => $"Float(0x{bits:X8})",
                    FloatReprMode.BitField => $"Float({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                    FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                        or FloatReprMode.General =>
                        $"Float({nanType})",
                    _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
                };
            }

            return floatReprConfig.mode switch
            {
                FloatReprMode.RawBytesHex => $"Float(0x{bits:X8})",
                FloatReprMode.BitField => $"Float({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    $"Float({nanType}, Payload: {mantissa:X6})",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        return floatReprConfig.mode switch
        {
            FloatReprMode.RawBytesHex => $"Float(0x{bits:X8})",
            FloatReprMode.BitField => $"Float({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
            FloatReprMode.Round => $"Float({Math.Round(t, floatReprConfig.precision)})",
            FloatReprMode.Scientific =>
                $"Float({t.ToString("E" + (floatReprConfig.precision > 0 ? floatReprConfig.precision - 1 : 0))})",
            // subtracting realExponent - 23 because the significand is form of 0.XXXX or 1.XXXX (depending on whether it is subnormal or not)
            FloatReprMode.Exact =>
                $"Float({PreciseScientificSimple(realExponent - 23, significand, sign)})",
            FloatReprMode.General => $"Float({t})",
            _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
        };
    }
    private static string ReprHalf(this Half t, FloatReprConfig floatReprConfig)
    {
        var bits = BitConverter.HalfToInt16Bits(t);
        var sign = bits < 0;
        var rawExponent = (int)((bits >> 10) & 0x1F);
        var mantissa = bits & 0x3FF;
        // Calculate significand and real exponent
        var realExponent = rawExponent - 15;
        var significand = rawExponent == 0
            ? mantissa
            : (BigInteger.One << 10) + mantissa;
        var expBits = Convert.ToString(rawExponent, 2)
            .PadLeft(5);
        var mantissaBits = Convert.ToString(mantissa, 2)
            .PadLeft(10);
        if (Half.IsPositiveInfinity(t))
        {
            return floatReprConfig.mode switch
            {
                FloatReprMode.RawBytesHex => $"Half(0x{bits:X4})",
                FloatReprMode.BitField => $"Half({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    "Half(Infinity)",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        if (Half.IsNegativeInfinity(t))
        {
            return floatReprConfig.mode switch
            {
                FloatReprMode.RawBytesHex => $"Half(0x{bits:X4})",
                FloatReprMode.BitField => $"Half({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    "Half(-Infinity)",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        if (Half.IsNaN(t))
        {
            var isQuiet = (bits & 0x200) != 0;
            var nanType = isQuiet
                ? "quiet"
                : "signaling";
            if (isQuiet)
            {
                return floatReprConfig.mode switch
                {
                    FloatReprMode.RawBytesHex => $"Half(0x{bits:X4})",
                    FloatReprMode.BitField => $"Half({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                    FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                        or FloatReprMode.General =>
                        $"Half({nanType})",
                    _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
                };
            }

            return floatReprConfig.mode switch
            {
                FloatReprMode.RawBytesHex => $"Half(0x{bits:X4})",
                FloatReprMode.BitField => $"Half({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    $"Half({nanType}, Payload: {mantissa:X3})",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        return floatReprConfig.mode switch
        {
            FloatReprMode.RawBytesHex => $"Half(0x{bits:X4})",
            FloatReprMode.BitField => $"Half({(sign ? 1 : 0)}|{expBits}|{mantissaBits})",
            FloatReprMode.Round =>
                $"Half({t.ToString("F" + (floatReprConfig.precision > 0 ? floatReprConfig.precision : 0))})",
            FloatReprMode.Scientific =>
                $"Half({t.ToString("E" + (floatReprConfig.precision > 0 ? floatReprConfig.precision - 1 : 0))})",
            // subtracting realExponent - 10 because the significand is form of 0.XXXX or 1.XXXX (depending on whether it is subnormal or not)
            FloatReprMode.Exact =>
                $"Half({PreciseScientificSimple(realExponent - 10, significand, sign)})",
            FloatReprMode.General => $"Half({t})",
            _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
        };
    }
    private static string ReprDecimal(this decimal t, FloatReprConfig floatReprConfig)
    {
        var exactStr = FormatDecimalExact(Math.Abs(t));

        // Get the internal bits
        var bits = decimal.GetBits(t);

        // Extract components
        var lo = (uint)bits[0]; // Low 32 bits of 96-bit integer
        var mid = (uint)bits[1]; // Middle 32 bits  
        var hi = (uint)bits[2]; // High 32 bits
        var flags = bits[3]; // Scale and sign
        var scale = (byte)(flags >> 16); // How many digits after decimal
        var isNegative = (flags & 0x80000000) != 0;
        var scaleBits = Convert.ToString(scale, 2)
            .PadLeft(8, '0');
        var hiBits = Convert.ToString(hi, 2)
            .PadLeft(32, '0');
        var midBits = Convert.ToString(mid, 2)
            .PadLeft(32, '0');
        var loBits = Convert.ToString(lo, 2)
            .PadLeft(32, '0');

        return floatReprConfig.mode switch
        {
            FloatReprMode.RawBytesHex =>
                $"Decimal(0x{flags:X8}{hi:X8}{mid:X8}{lo:X8})",
            FloatReprMode.BitField =>
                $"Decimal({(isNegative ? 1 : 0)}|{scaleBits}|{hiBits}{midBits}{loBits})",
            FloatReprMode.Round =>
                $"Decimal({t.ToString("F" + (floatReprConfig.precision > 0 ? floatReprConfig.precision : 0))})",
            FloatReprMode.Scientific =>
                $"Decimal({t.ToString("E" + (floatReprConfig.precision > 0 ? floatReprConfig.precision - 1 : 0))})",
            // subtracting realExponent - 10 because the significand is form of 0.XXXX or 1.XXXX (depending on whether it is subnormal or not)
            FloatReprMode.Exact => $"Decimal({FormatDecimalScientific(exactStr, t < 0)})",
            FloatReprMode.General => $"Decimal({t})",
            _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
        };
    }

}
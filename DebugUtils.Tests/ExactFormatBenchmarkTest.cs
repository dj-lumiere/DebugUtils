using System.Diagnostics;
using DebugUtils.Repr;
using Xunit.Abstractions;

namespace DebugUtils.Tests;

public class ExactFormatBenchmarkTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    public ExactFormatBenchmarkTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    private static readonly ReprConfig OldExactConfig = new(FloatMode: FloatReprMode.Exact_Old);
    private static readonly ReprConfig NewExactConfig = new(FloatMode: FloatReprMode.Exact);
    private const int BenchmarkIterations = 100000;

    [Fact]
    public void BenchmarkFloat_RandomValues_Performance()
    {
        // Generate 100,000 random float values
        var random = new Random(Seed: 42); // Fixed seed for reproducibility
        var randomFloats = new float[BenchmarkIterations];
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            // Generate random bits to cover full range of float values
            var bits = (uint)random.Next() << 1 | (uint)random.Next(minValue: 0, maxValue: 2);
            randomFloats[i] = BitConverter.UInt32BitsToSingle(value: bits);

            // Skip special values for pure performance testing
            if (Single.IsNaN(f: randomFloats[i]) || Single.IsInfinity(f: randomFloats[i]))
            {
                randomFloats[i] = random.NextSingle() * 1000.0f; // Use normal range instead
            }
        }

        for (var i = 0; i < randomFloats.Length; i++)
        {
            Assert.Equal(expected: randomFloats[i]
               .Repr(config: OldExactConfig), actual: randomFloats[i]
               .Repr(config: NewExactConfig));
        }

        // Warm up JIT
        for (var i = 0; i < 100; i++)
        {
            _ = randomFloats[0]
               .Repr(config: OldExactConfig);
            _ = randomFloats[0]
               .Repr(config: NewExactConfig);
        }

        // Benchmark Exact mode
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = randomFloats[i]
               .Repr(config: OldExactConfig);
        }

        sw.Stop();
        var exactTime = sw.ElapsedMilliseconds;

        // Benchmark ExactBeta mode
        sw.Restart();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = randomFloats[i]
               .Repr(config: NewExactConfig);
        }

        sw.Stop();
        var exactBetaTime = sw.ElapsedMilliseconds;

        // Output results for analysis
        var speedupRatio = (double)exactTime / exactBetaTime;

        // Use _testOutputHelper.WriteLine instead of test output for visibility
        _testOutputHelper.WriteLine(
            message:
            $"Float Random Values Performance Test ({BenchmarkIterations:N0} iterations):");
        _testOutputHelper.WriteLine(
            message:
            $"  Exact mode:     {exactTime:N0} ms ({exactTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  ExactBeta mode: {exactBetaTime:N0} ms ({exactBetaTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  Performance improvement: {speedupRatio:F2}x {(speedupRatio > 1 ? "faster" : "slower")}");

        // Assert that both modes complete successfully (basic correctness check)
        Assert.True(condition: exactTime > 0,
            userMessage: "Exact mode should take some measurable time");
        Assert.True(condition: exactBetaTime > 0,
            userMessage: "ExactBeta mode should take some measurable time");
    }

    [Fact]
    public void BenchmarkFloat_WorstCaseValues_Performance()
    {
        // Create array of worst-case float values that require maximum precision
        var worstCaseFloats = new float[BenchmarkIterations];
        var worstCaseBits = new uint[]
        {
            0x00FF_FFFF, // Large mantissa, small exponent (normal)
            0x007F_FFFF, // Large mantissa, small exponent (subnormal)
            0x7F7F_FFFF, // Max normal value with full mantissa
            0x0000_0001, // Smallest subnormal
            0x007F_FFFF, // Largest subnormal
            0x3F80_0001, // Just above 1.0
            0xBF80_0001, // Just below -1.0
            0x7F00_0000, // Large number
            0x0100_0000 // Small normal number
        };

        // var currentConfig = new ReprConfig(FloatMode: FloatReprMode.Exact,
        //     ContainerReprMode: ContainerReprMode.UseParentConfig);
        // var betaConfig = new ReprConfig(FloatMode: FloatReprMode.ExactBeta,
        //     ContainerReprMode: ContainerReprMode.UseParentConfig);
        // _testOutputHelper.WriteLine(message: worstCaseBits
        //                                     .Select(BitConverter.UInt32BitsToSingle)
        //                                     .ToList()
        //                                     .Repr(config: currentConfig));
        // _testOutputHelper.WriteLine(message: worstCaseBits
        //                                     .Select(BitConverter.UInt32BitsToSingle)
        //                                     .ToList()
        //                                     .Repr(config: betaConfig));

        // Fill array with cycling through worst-case patterns
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            worstCaseFloats[i] =
                BitConverter.UInt32BitsToSingle(value: worstCaseBits[i % worstCaseBits.Length]);
        }

        // Warm up JIT
        for (var i = 0; i < 100; i++)
        {
            _ = worstCaseFloats[0]
               .Repr(config: OldExactConfig);
            _ = worstCaseFloats[0]
               .Repr(config: NewExactConfig);
        }

        // Benchmark Exact mode
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = worstCaseFloats[i]
               .Repr(config: OldExactConfig);
        }

        sw.Stop();
        var exactTime = sw.ElapsedMilliseconds;

        // Benchmark ExactBeta mode
        sw.Restart();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = worstCaseFloats[i]
               .Repr(config: NewExactConfig);
        }

        sw.Stop();
        var exactBetaTime = sw.ElapsedMilliseconds;

        // Output results for analysis
        var speedupRatio = (double)exactTime / exactBetaTime;

        _testOutputHelper.WriteLine(
            message:
            $"Float Worst-Case Values Performance Test ({BenchmarkIterations:N0} iterations):");
        _testOutputHelper.WriteLine(
            message:
            $"  Exact mode:     {exactTime:N0} ms ({exactTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  ExactBeta mode: {exactBetaTime:N0} ms ({exactBetaTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  Performance improvement: {speedupRatio:F2}x {(speedupRatio > 1 ? "faster" : "slower")}");

        // Assert that both modes complete successfully
        Assert.True(condition: exactTime > 0,
            userMessage: "Exact mode should take some measurable time");
        Assert.True(condition: exactBetaTime > 0,
            userMessage: "ExactBeta mode should take some measurable time");
    }

    [Fact]
    public void BenchmarkDouble_RandomValues_Performance()
    {
        // Generate 100,000 random double values
        var random = new Random(Seed: 42); // Fixed seed for reproducibility
        var randomDoubles = new double[BenchmarkIterations];
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            // Generate random bits to cover full range of double values
            var bits = (ulong)random.Next() << 32 | (uint)random.Next();
            randomDoubles[i] = BitConverter.UInt64BitsToDouble(value: bits);

            // Skip special values for pure performance testing
            if (Double.IsNaN(d: randomDoubles[i]) || Double.IsInfinity(d: randomDoubles[i]))
            {
                randomDoubles[i] = random.NextDouble() * 1000.0; // Use normal range instead
            }
        }

        for (var i = 0; i < randomDoubles.Length; i++)
        {
            Assert.Equal(expected: randomDoubles[i]
               .Repr(config: OldExactConfig), actual: randomDoubles[i]
               .Repr(config: NewExactConfig));
        }

        // Warm up JIT
        for (var i = 0; i < 100; i++)
        {
            _ = randomDoubles[0]
               .Repr(config: OldExactConfig);
            _ = randomDoubles[0]
               .Repr(config: NewExactConfig);
        }

        // Benchmark Exact mode
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = randomDoubles[i]
               .Repr(config: OldExactConfig);
        }

        sw.Stop();
        var exactTime = sw.ElapsedMilliseconds;

        // Benchmark ExactBeta mode
        sw.Restart();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = randomDoubles[i]
               .Repr(config: NewExactConfig);
        }

        sw.Stop();
        var exactBetaTime = sw.ElapsedMilliseconds;

        // Output results for analysis
        var speedupRatio = (double)exactTime / exactBetaTime;

        _testOutputHelper.WriteLine(
            message:
            $"Double Random Values Performance Test ({BenchmarkIterations:N0} iterations):");
        _testOutputHelper.WriteLine(
            message:
            $"  Exact mode:     {exactTime:N0} ms ({exactTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  ExactBeta mode: {exactBetaTime:N0} ms ({exactBetaTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  Performance improvement: {speedupRatio:F2}x {(speedupRatio > 1 ? "faster" : "slower")}");

        // Assert that both modes complete successfully
        Assert.True(condition: exactTime > 0,
            userMessage: "Exact mode should take some measurable time");
        Assert.True(condition: exactBetaTime > 0,
            userMessage: "ExactBeta mode should take some measurable time");
    }

    [Fact]
    public void BenchmarkDouble_WorstCaseValues_Performance()
    {
        // Create array of worst-case double values
        var worstCaseDoubles = new double[BenchmarkIterations];
        var worstCaseBits = new ulong[]
        {
            0x001F_FFFF_FFFF_FFFF, // Large mantissa, small exponent (normal)
            0x000F_FFFF_FFFF_FFFF, // Large mantissa, small exponent (subnormal)
            0x3FF7_FFFF_FFFF_FFFF, // Max normal value with full mantissa
            0x0000_0000_0000_0001, // Smallest subnormal
            0x000F_FFFF_FFFF_FFFF, // Largest subnormal
            0x3FF0_0000_0000_0001, // Just above 1.0
            0xBFF0_0000_0000_0001, // Just below -1.0
            0x7FE0_0000_0000_0000, // Large number
            0x0010_0000_0000_0000 // Small normal number
        };

        // var currentConfig = new ReprConfig(FloatMode: FloatReprMode.Exact,
        //     ContainerReprMode: ContainerReprMode.UseParentConfig);
        // var betaConfig = new ReprConfig(FloatMode: FloatReprMode.ExactBeta,
        //     ContainerReprMode: ContainerReprMode.UseParentConfig);
        // _testOutputHelper.WriteLine(message: worstCaseBits
        //                                     .Select(BitConverter.UInt64BitsToDouble)
        //                                     .ToList()
        //                                     .Repr(config: currentConfig));
        // _testOutputHelper.WriteLine(message: worstCaseBits
        //                                     .Select(BitConverter.UInt64BitsToDouble)
        //                                     .ToList()
        //                                     .Repr(config: betaConfig));

        // Fill array with cycling through worst-case patterns
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            worstCaseDoubles[i] =
                BitConverter.UInt64BitsToDouble(value: worstCaseBits[i % worstCaseBits.Length]);
        }

        // Warm up JIT
        for (var i = 0; i < 100; i++)
        {
            _ = worstCaseDoubles[0]
               .Repr(config: OldExactConfig);
            _ = worstCaseDoubles[0]
               .Repr(config: NewExactConfig);
        }

        // Benchmark Exact mode
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = worstCaseDoubles[i]
               .Repr(config: OldExactConfig);
        }

        sw.Stop();
        var exactTime = sw.ElapsedMilliseconds;

        // Benchmark ExactBeta mode
        sw.Restart();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = worstCaseDoubles[i]
               .Repr(config: NewExactConfig);
        }

        sw.Stop();
        var exactBetaTime = sw.ElapsedMilliseconds;

        // Output results for analysis
        var speedupRatio = (double)exactTime / exactBetaTime;

        _testOutputHelper.WriteLine(
            message:
            $"Double Worst-Case Values Performance Test ({BenchmarkIterations:N0} iterations):");
        _testOutputHelper.WriteLine(
            message:
            $"  Exact mode:     {exactTime:N0} ms ({exactTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  ExactBeta mode: {exactBetaTime:N0} ms ({exactBetaTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  Performance improvement: {speedupRatio:F2}x {(speedupRatio > 1 ? "faster" : "slower")}");

        // Assert that both modes complete successfully
        Assert.True(condition: exactTime > 0,
            userMessage: "Exact mode should take some measurable time");
        Assert.True(condition: exactBetaTime > 0,
            userMessage: "ExactBeta mode should take some measurable time");
    }

    [Fact]
    public void BenchmarkDecimal_RandomValues_Performance()
    {
        // Generate 100,000 random decimal values
        var random = new Random(Seed: 42); // Fixed seed for reproducibility
        var randomDecimals = new decimal[BenchmarkIterations];
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            // Create random decimal values with varying scales and magnitudes
            var lo = random.Next();
            var mid = random.Next();
            var hi = random.Next();
            var isNegative = random.Next(maxValue: 2) == 1;
            var scale = random.Next(minValue: 0, maxValue: 29); // Decimal scale range is 0-28

            randomDecimals[i] = new decimal(lo: lo, mid: mid, hi: hi, isNegative: isNegative,
                scale: (byte)scale);
        }

        for (var i = 0; i < randomDecimals.Length; i++)
        {
            Assert.Equal(expected: randomDecimals[i]
               .Repr(config: OldExactConfig), actual: randomDecimals[i]
               .Repr(config: NewExactConfig));
        }

        // Warm up JIT
        for (var i = 0; i < 100; i++)
        {
            _ = randomDecimals[0]
               .Repr(config: OldExactConfig);
            _ = randomDecimals[0]
               .Repr(config: NewExactConfig);
        }

        // Benchmark Exact mode
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = randomDecimals[i]
               .Repr(config: OldExactConfig);
        }

        sw.Stop();
        var exactTime = sw.ElapsedMilliseconds;

        // Benchmark ExactBeta mode
        sw.Restart();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = randomDecimals[i]
               .Repr(config: NewExactConfig);
        }

        sw.Stop();
        var exactBetaTime = sw.ElapsedMilliseconds;

        // Output results for analysis
        var speedupRatio = (double)exactTime / exactBetaTime;

        _testOutputHelper.WriteLine(
            message:
            $"Decimal Random Values Performance Test ({BenchmarkIterations:N0} iterations):");
        _testOutputHelper.WriteLine(
            message:
            $"  Exact mode:     {exactTime:N0} ms ({exactTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  ExactBeta mode: {exactBetaTime:N0} ms ({exactBetaTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  Performance improvement: {speedupRatio:F2}x {(speedupRatio > 1 ? "faster" : "slower")}");

        // Assert that both modes complete successfully
        Assert.True(condition: exactTime > 0,
            userMessage: "Exact mode should take some measurable time");
        Assert.True(condition: exactBetaTime > 0,
            userMessage: "ExactBeta mode should take some measurable time");
    }

    [Fact]
    public void BenchmarkHalf_RandomValues_Performance()
    {
        // Generate 100,000 random Half values
        var random = new Random(Seed: 42); // Fixed seed for reproducibility
        var randomHalves = new Half[BenchmarkIterations];
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            // Generate random bits to cover full range of Half values
            var bits = (ushort)random.Next(minValue: 0, maxValue: 65536);
            randomHalves[i] = BitConverter.UInt16BitsToHalf(value: bits);

            // Skip special values for pure performance testing
            if (Half.IsNaN(value: randomHalves[i]) || Half.IsInfinity(value: randomHalves[i]))
            {
                randomHalves[i] =
                    (Half)(random.NextSingle() * 1000.0f); // Use normal range instead
            }
        }

        for (var i = 0; i < randomHalves.Length; i++)
        {
            Assert.Equal(expected: randomHalves[i]
               .Repr(config: OldExactConfig), actual: randomHalves[i]
               .Repr(config: NewExactConfig));
        }

        // Warm up JIT
        for (var i = 0; i < 100; i++)
        {
            _ = randomHalves[0]
               .Repr(config: OldExactConfig);
            _ = randomHalves[0]
               .Repr(config: NewExactConfig);
        }

        // Benchmark Exact mode
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = randomHalves[i]
               .Repr(config: OldExactConfig);
        }

        sw.Stop();
        var exactTime = sw.ElapsedMilliseconds;

        // Benchmark ExactBeta mode
        sw.Restart();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = randomHalves[i]
               .Repr(config: NewExactConfig);
        }

        sw.Stop();
        var exactBetaTime = sw.ElapsedMilliseconds;

        // Output results for analysis
        var speedupRatio = (double)exactTime / exactBetaTime;

        _testOutputHelper.WriteLine(
            message:
            $"Half Random Values Performance Test ({BenchmarkIterations:N0} iterations):");
        _testOutputHelper.WriteLine(
            message:
            $"  Exact mode:     {exactTime:N0} ms ({exactTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  ExactBeta mode: {exactBetaTime:N0} ms ({exactBetaTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  Performance improvement: {speedupRatio:F2}x {(speedupRatio > 1 ? "faster" : "slower")}");

        // Assert that both modes complete successfully
        Assert.True(condition: exactTime > 0,
            userMessage: "Exact mode should take some measurable time");
        Assert.True(condition: exactBetaTime > 0,
            userMessage: "ExactBeta mode should take some measurable time");
    }

    [Fact]
    public void BenchmarkHalf_WorstCaseValues_Performance()
    {
        // Create array of worst-case Half values that require maximum precision
        var worstCaseHalves = new Half[BenchmarkIterations];
        var worstCaseBits = new ushort[]
        {
            0x07FF, // Large mantissa, small exponent (normal)
            0x03FF, // Large mantissa, small exponent (subnormal)
            0x7BFF, // Max normal value with full mantissa
            0x0001, // Smallest subnormal
            0x03FF, // Largest subnormal
            0x3C01, // Just above 1.0
            0xBC01, // Just below -1.0
            0x7800, // Large number
            0x0400 // Small normal number
        };

        // var currentConfig = new ReprConfig(FloatMode: FloatReprMode.Exact,
        //     ContainerReprMode: ContainerReprMode.UseParentConfig);
        // var betaConfig = new ReprConfig(FloatMode: FloatReprMode.ExactBeta,
        //     ContainerReprMode: ContainerReprMode.UseParentConfig);
        // _testOutputHelper.WriteLine(message: worstCaseBits
        //                                     .Select(BitConverter.UInt16BitsToHalf)
        //                                     .ToList()
        //                                     .Repr(config: currentConfig));
        // _testOutputHelper.WriteLine(message: worstCaseBits
        //                                     .Select(BitConverter.UInt16BitsToHalf)
        //                                     .ToList()
        //                                     .Repr(config: betaConfig));

        // Fill array with cycling through worst-case patterns
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            worstCaseHalves[i] =
                BitConverter.UInt16BitsToHalf(value: worstCaseBits[i % worstCaseBits.Length]);
        }

        // Warm up JIT
        for (var i = 0; i < 100; i++)
        {
            _ = worstCaseHalves[0]
               .Repr(config: OldExactConfig);
            _ = worstCaseHalves[0]
               .Repr(config: NewExactConfig);
        }

        // Benchmark Exact mode
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = worstCaseHalves[i]
               .Repr(config: OldExactConfig);
        }

        sw.Stop();
        var exactTime = sw.ElapsedMilliseconds;

        // Benchmark ExactBeta mode
        sw.Restart();
        for (var i = 0; i < BenchmarkIterations; i++)
        {
            _ = worstCaseHalves[i]
               .Repr(config: NewExactConfig);
        }

        sw.Stop();
        var exactBetaTime = sw.ElapsedMilliseconds;

        // Output results for analysis
        var speedupRatio = (double)exactTime / exactBetaTime;

        _testOutputHelper.WriteLine(
            message:
            $"Half Worst-Case Values Performance Test ({BenchmarkIterations:N0} iterations):");
        _testOutputHelper.WriteLine(
            message:
            $"  Exact mode:     {exactTime:N0} ms ({exactTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  ExactBeta mode: {exactBetaTime:N0} ms ({exactBetaTime * 1000.0 / BenchmarkIterations:F2} μs per operation)");
        _testOutputHelper.WriteLine(
            message:
            $"  Performance improvement: {speedupRatio:F2}x {(speedupRatio > 1 ? "faster" : "slower")}");

        // Assert that both modes complete successfully
        Assert.True(condition: exactTime > 0,
            userMessage: "Exact mode should take some measurable time");
        Assert.True(condition: exactBetaTime > 0,
            userMessage: "ExactBeta mode should take some measurable time");
    }
}
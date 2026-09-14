// -----------------------------------------------------------------------------
// Cosmos.Kernel.Tests.Math — validates System.Math and System.MathF behave
// identically to standard .NET. Every expected value here matches what
// desktop .NET 10 produces; any deviation is a Cosmos bug.
// -----------------------------------------------------------------------------

using Cosmos.Kernel.System.Diagnostics;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;
using SysMath = global::System.Math;
using SysMathF = global::System.MathF;

namespace Cosmos.Kernel.Tests.Math;

public class Kernel : Sys.Kernel
{
    // Standard IEEE 754 double precision gives ~15-16 significant digits.
    // We allow 1e-9 for double which is still well within spec.
    private const double Epsilon = 1e-9;
    private const float EpsilonF = 1e-5f;

    private static bool ApproxEqual(double actual, double expected, double eps = Epsilon)
    {
        if (double.IsNaN(expected))
        {
            return double.IsNaN(actual);
        }

        if (double.IsPositiveInfinity(expected))
        {
            return double.IsPositiveInfinity(actual);
        }

        if (double.IsNegativeInfinity(expected))
        {
            return double.IsNegativeInfinity(actual);
        }

        return SysMath.Abs(actual - expected) < eps;
    }

    private static bool ApproxEqualF(float actual, float expected, float eps = EpsilonF)
    {
        if (float.IsNaN(expected))
        {
            return float.IsNaN(actual);
        }

        if (float.IsPositiveInfinity(expected))
        {
            return float.IsPositiveInfinity(actual);
        }

        if (float.IsNegativeInfinity(expected))
        {
            return float.IsNegativeInfinity(actual);
        }

        return SysMathF.Abs(actual - expected) < eps;
    }

    protected override void BeforeRun()
    {
        Log.WriteString("[Math] BeforeRun() reached!\n");
        Log.WriteString("[Math] Starting System.Math tests...\n");

        TR.Start("Math Tests", expectedTests: 30);

        // Abs
        TR.Run("Math_Abs_Double", Test_Abs_Double);
        TR.Run("Math_Abs_Int", Test_Abs_Int);

        // Min / Max
        TR.Run("Math_MinMax_Double", Test_MinMax_Double);
        TR.Run("Math_MinMax_Int", Test_MinMax_Int);

        // Ceiling / Floor / Truncate / Round
        TR.Run("Math_Ceiling", Test_Ceiling);
        TR.Run("Math_Floor", Test_Floor);
        TR.Run("Math_Truncate", Test_Truncate);
        TR.Run("Math_Round", Test_Round);

        // Sqrt
        TR.Run("Math_Sqrt", Test_Sqrt);

        // Trig
        TR.Run("Math_Sin", Test_Sin);
        TR.Run("Math_Cos", Test_Cos);
        TR.Run("Math_Tan", Test_Tan);

        // Inverse trig
        TR.Run("Math_Asin", Test_Asin);
        TR.Run("Math_Acos", Test_Acos);
        TR.Run("Math_Atan", Test_Atan);
        TR.Run("Math_Atan2", Test_Atan2);

        // Pow / Exp
        TR.Run("Math_Pow", Test_Pow);
        TR.Run("Math_Exp", Test_Exp);

        // Log
        TR.Run("Math_Log", Test_Log);
        TR.Run("Math_Log10", Test_Log10);
        TR.Run("Math_Log2", Test_Log2);
        TR.Run("Math_Log_Base", Test_Log_Base);

        // MathF (float variants)
        TR.Run("MathF_Sin_Cos_Tan", Test_MathF_Sin_Cos_Tan);
        TR.Run("MathF_Sqrt", Test_MathF_Sqrt);
        TR.Run("MathF_Exp_Log", Test_MathF_Exp_Log);
        TR.Run("MathF_Pow", Test_MathF_Pow);

        // Misc
        TR.Run("Math_Sign", Test_Sign);
        TR.Run("Math_Clamp", Test_Clamp);
        TR.Run("Math_FusedMultiplyAdd", Test_FusedMultiplyAdd);
        TR.Run("Math_CopySign", Test_CopySign);

        TR.Finish();

        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run()
    {
        Stop();
    }

    protected override void AfterRun()
    {
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // =========================================================================
    // Abs
    // =========================================================================

    private static void Test_Abs_Double()
    {
        Assert.True(SysMath.Abs(-5.5) == 5.5, "Abs(-5.5) == 5.5");
        Assert.True(SysMath.Abs(3.0) == 3.0, "Abs(3.0) == 3.0");
        Assert.True(SysMath.Abs(0.0) == 0.0, "Abs(0.0) == 0.0");
        Assert.True(double.IsNaN(SysMath.Abs(double.NaN)), "Abs(NaN) == NaN");
        Assert.True(double.IsPositiveInfinity(SysMath.Abs(double.NegativeInfinity)), "Abs(-Inf) == +Inf");
    }

    private static void Test_Abs_Int()
    {
        Assert.Equal(5, SysMath.Abs(-5), "Abs(-5) == 5");
        Assert.Equal(0, SysMath.Abs(0), "Abs(0) == 0");
        Assert.Equal(42, SysMath.Abs(42), "Abs(42) == 42");
    }

    // =========================================================================
    // Min / Max
    // =========================================================================

    private static void Test_MinMax_Double()
    {
        Assert.True(SysMath.Min(1.0, 2.0) == 1.0, "Min(1,2) == 1");
        Assert.True(SysMath.Min(-1.0, 0.0) == -1.0, "Min(-1,0) == -1");
        Assert.True(SysMath.Max(1.0, 2.0) == 2.0, "Max(1,2) == 2");
        Assert.True(SysMath.Max(-1.0, 0.0) == 0.0, "Max(-1,0) == 0");
        Assert.True(double.IsNaN(SysMath.Min(double.NaN, 1.0)), "Min(NaN,1) == NaN");
        Assert.True(double.IsNaN(SysMath.Max(double.NaN, 1.0)), "Max(NaN,1) == NaN");
    }

    private static void Test_MinMax_Int()
    {
        Assert.Equal(1, SysMath.Min(1, 2), "Min(1,2) == 1");
        Assert.Equal(2, SysMath.Max(1, 2), "Max(1,2) == 2");
        Assert.Equal(-5, SysMath.Min(-5, 5), "Min(-5,5) == -5");
        Assert.Equal(5, SysMath.Max(-5, 5), "Max(-5,5) == 5");
    }

    // =========================================================================
    // Ceiling / Floor / Truncate / Round
    // =========================================================================

    private static void Test_Ceiling()
    {
        Assert.True(SysMath.Ceiling(1.1) == 2.0, "Ceiling(1.1) == 2");
        Assert.True(SysMath.Ceiling(-1.1) == -1.0, "Ceiling(-1.1) == -1");
        Assert.True(SysMath.Ceiling(3.0) == 3.0, "Ceiling(3.0) == 3");
        Assert.True(SysMath.Ceiling(0.0) == 0.0, "Ceiling(0.0) == 0");
        Assert.True(double.IsPositiveInfinity(SysMath.Ceiling(double.PositiveInfinity)), "Ceiling(+Inf) == +Inf");
        Assert.True(double.IsNaN(SysMath.Ceiling(double.NaN)), "Ceiling(NaN) == NaN");
    }

    private static void Test_Floor()
    {
        Assert.True(SysMath.Floor(1.9) == 1.0, "Floor(1.9) == 1");
        Assert.True(SysMath.Floor(-1.1) == -2.0, "Floor(-1.1) == -2");
        Assert.True(SysMath.Floor(3.0) == 3.0, "Floor(3.0) == 3");
        Assert.True(SysMath.Floor(0.0) == 0.0, "Floor(0.0) == 0");
        Assert.True(double.IsNegativeInfinity(SysMath.Floor(double.NegativeInfinity)), "Floor(-Inf) == -Inf");
        Assert.True(double.IsNaN(SysMath.Floor(double.NaN)), "Floor(NaN) == NaN");
    }

    private static void Test_Truncate()
    {
        Assert.True(SysMath.Truncate(1.9) == 1.0, "Truncate(1.9) == 1");
        Assert.True(SysMath.Truncate(-1.9) == -1.0, "Truncate(-1.9) == -1");
        Assert.True(SysMath.Truncate(3.0) == 3.0, "Truncate(3.0) == 3");
        Assert.True(double.IsNaN(SysMath.Truncate(double.NaN)), "Truncate(NaN) == NaN");
    }

    private static void Test_Round()
    {
        // .NET default: MidpointRounding.ToEven (banker's rounding)
        Assert.True(SysMath.Round(1.5) == 2.0, "Round(1.5) == 2");
        Assert.True(SysMath.Round(2.5) == 2.0, "Round(2.5) == 2 (banker's)");
        Assert.True(SysMath.Round(3.5) == 4.0, "Round(3.5) == 4 (banker's)");
        Assert.True(SysMath.Round(1.4) == 1.0, "Round(1.4) == 1");
        Assert.True(SysMath.Round(-1.5) == -2.0, "Round(-1.5) == -2");
        Assert.True(SysMath.Round(-2.5) == -2.0, "Round(-2.5) == -2 (banker's)");
    }

    // =========================================================================
    // Sqrt
    // =========================================================================

    private static void Test_Sqrt()
    {
        Assert.True(ApproxEqual(SysMath.Sqrt(4.0), 2.0), "Sqrt(4) == 2");
        Assert.True(ApproxEqual(SysMath.Sqrt(9.0), 3.0), "Sqrt(9) == 3");
        Assert.True(ApproxEqual(SysMath.Sqrt(2.0), 1.4142135623730951), "Sqrt(2)");
        Assert.True(SysMath.Sqrt(0.0) == 0.0, "Sqrt(0) == 0");
        Assert.True(double.IsNaN(SysMath.Sqrt(-1.0)), "Sqrt(-1) == NaN");
        Assert.True(double.IsPositiveInfinity(SysMath.Sqrt(double.PositiveInfinity)), "Sqrt(+Inf) == +Inf");
    }

    // =========================================================================
    // Sin / Cos / Tan
    // =========================================================================

    private static void Test_Sin()
    {
        Assert.True(SysMath.Sin(0.0) == 0.0, "Sin(0) == 0 (exact)");
        Assert.True(ApproxEqual(SysMath.Sin(SysMath.PI / 6), 0.5), "Sin(PI/6) == 0.5");
        Assert.True(ApproxEqual(SysMath.Sin(SysMath.PI / 2), 1.0), "Sin(PI/2) == 1");
        Assert.True(ApproxEqual(SysMath.Sin(SysMath.PI), 0.0), "Sin(PI) == 0");
        Assert.True(ApproxEqual(SysMath.Sin(3 * SysMath.PI / 2), -1.0), "Sin(3PI/2) == -1");
        Assert.True(ApproxEqual(SysMath.Sin(-SysMath.PI / 2), -1.0), "Sin(-PI/2) == -1");
        Assert.True(double.IsNaN(SysMath.Sin(double.NaN)), "Sin(NaN) == NaN");
        Assert.True(double.IsNaN(SysMath.Sin(double.PositiveInfinity)), "Sin(+Inf) == NaN");
    }

    private static void Test_Cos()
    {
        Assert.True(SysMath.Cos(0.0) == 1.0, "Cos(0) == 1 (exact)");
        Assert.True(ApproxEqual(SysMath.Cos(SysMath.PI / 3), 0.5), "Cos(PI/3) == 0.5");
        Assert.True(ApproxEqual(SysMath.Cos(SysMath.PI / 2), 0.0), "Cos(PI/2) == 0");
        Assert.True(ApproxEqual(SysMath.Cos(SysMath.PI), -1.0), "Cos(PI) == -1");
        Assert.True(ApproxEqual(SysMath.Cos(2 * SysMath.PI), 1.0), "Cos(2PI) == 1");
        Assert.True(double.IsNaN(SysMath.Cos(double.NaN)), "Cos(NaN) == NaN");
        Assert.True(double.IsNaN(SysMath.Cos(double.PositiveInfinity)), "Cos(+Inf) == NaN");
    }

    private static void Test_Tan()
    {
        Assert.True(SysMath.Tan(0.0) == 0.0, "Tan(0) == 0 (exact)");
        Assert.True(ApproxEqual(SysMath.Tan(SysMath.PI / 4), 1.0), "Tan(PI/4) == 1");
        Assert.True(ApproxEqual(SysMath.Tan(-SysMath.PI / 4), -1.0), "Tan(-PI/4) == -1");
        Assert.True(double.IsNaN(SysMath.Tan(double.NaN)), "Tan(NaN) == NaN");
        Assert.True(double.IsNaN(SysMath.Tan(double.PositiveInfinity)), "Tan(+Inf) == NaN");
    }

    // =========================================================================
    // Asin / Acos / Atan / Atan2
    // =========================================================================

    private static void Test_Asin()
    {
        Assert.True(SysMath.Asin(0.0) == 0.0, "Asin(0) == 0 (exact)");
        Assert.True(ApproxEqual(SysMath.Asin(0.5), SysMath.PI / 6), "Asin(0.5) == PI/6");
        Assert.True(ApproxEqual(SysMath.Asin(1.0), SysMath.PI / 2), "Asin(1) == PI/2");
        Assert.True(ApproxEqual(SysMath.Asin(-1.0), -SysMath.PI / 2), "Asin(-1) == -PI/2");
        Assert.True(double.IsNaN(SysMath.Asin(1.1)), "Asin(1.1) == NaN");
        Assert.True(double.IsNaN(SysMath.Asin(-1.1)), "Asin(-1.1) == NaN");
    }

    private static void Test_Acos()
    {
        Assert.True(ApproxEqual(SysMath.Acos(1.0), 0.0), "Acos(1) == 0");
        Assert.True(ApproxEqual(SysMath.Acos(0.5), SysMath.PI / 3), "Acos(0.5) == PI/3");
        Assert.True(ApproxEqual(SysMath.Acos(0.0), SysMath.PI / 2), "Acos(0) == PI/2");
        Assert.True(ApproxEqual(SysMath.Acos(-1.0), SysMath.PI), "Acos(-1) == PI");
        Assert.True(double.IsNaN(SysMath.Acos(1.1)), "Acos(1.1) == NaN");
        Assert.True(double.IsNaN(SysMath.Acos(-1.1)), "Acos(-1.1) == NaN");
    }

    private static void Test_Atan()
    {
        Assert.True(SysMath.Atan(0.0) == 0.0, "Atan(0) == 0 (exact)");
        Assert.True(ApproxEqual(SysMath.Atan(1.0), SysMath.PI / 4), "Atan(1) == PI/4");
        Assert.True(ApproxEqual(SysMath.Atan(-1.0), -SysMath.PI / 4), "Atan(-1) == -PI/4");
        Assert.True(ApproxEqual(SysMath.Atan(double.PositiveInfinity), SysMath.PI / 2), "Atan(+Inf) == PI/2");
        Assert.True(ApproxEqual(SysMath.Atan(double.NegativeInfinity), -SysMath.PI / 2), "Atan(-Inf) == -PI/2");
        Assert.True(double.IsNaN(SysMath.Atan(double.NaN)), "Atan(NaN) == NaN");
    }

    private static void Test_Atan2()
    {
        Assert.True(SysMath.Atan2(0.0, 1.0) == 0.0, "Atan2(0,1) == 0 (exact)");
        Assert.True(ApproxEqual(SysMath.Atan2(1.0, 0.0), SysMath.PI / 2), "Atan2(1,0) == PI/2");
        Assert.True(ApproxEqual(SysMath.Atan2(1.0, 1.0), SysMath.PI / 4), "Atan2(1,1) == PI/4");
        Assert.True(ApproxEqual(SysMath.Atan2(-1.0, 0.0), -SysMath.PI / 2), "Atan2(-1,0) == -PI/2");
        Assert.True(ApproxEqual(SysMath.Atan2(0.0, -1.0), SysMath.PI), "Atan2(0,-1) == PI");
    }

    // =========================================================================
    // Pow / Exp
    // =========================================================================

    private static void Test_Pow()
    {
        Assert.True(SysMath.Pow(2.0, 3.0) == 8.0, "Pow(2,3) == 8");
        Assert.True(SysMath.Pow(2.0, 0.0) == 1.0, "Pow(x,0) == 1");
        Assert.True(SysMath.Pow(2.0, -1.0) == 0.5, "Pow(2,-1) == 0.5");
        Assert.True(ApproxEqual(SysMath.Pow(4.0, 0.5), 2.0), "Pow(4,0.5) == 2");
        Assert.True(ApproxEqual(SysMath.Pow(27.0, 1.0 / 3.0), 3.0), "Pow(27,1/3) == 3");
        Assert.True(SysMath.Pow(0.0, 0.0) == 1.0, "Pow(0,0) == 1");
        Assert.True(double.IsNaN(SysMath.Pow(-1.0, 0.5)), "Pow(-1,0.5) == NaN");
        Assert.True(double.IsPositiveInfinity(SysMath.Pow(double.PositiveInfinity, 1.0)), "Pow(+Inf,1) == +Inf");
    }

    private static void Test_Exp()
    {
        Assert.True(SysMath.Exp(0.0) == 1.0, "Exp(0) == 1 (exact)");
        Assert.True(ApproxEqual(SysMath.Exp(1.0), SysMath.E), "Exp(1) == E");
        Assert.True(ApproxEqual(SysMath.Exp(-1.0), 1.0 / SysMath.E), "Exp(-1) == 1/E");
        Assert.True(ApproxEqual(SysMath.Exp(2.0), SysMath.E * SysMath.E), "Exp(2) == E^2");
        Assert.True(double.IsPositiveInfinity(SysMath.Exp(double.PositiveInfinity)), "Exp(+Inf) == +Inf");
        Assert.True(SysMath.Exp(double.NegativeInfinity) == 0.0, "Exp(-Inf) == 0");
    }

    // =========================================================================
    // Log / Log10 / Log2 / Log(x, base)
    // =========================================================================

    private static void Test_Log()
    {
        Assert.True(SysMath.Log(1.0) == 0.0, "Log(1) == 0 (exact)");
        Assert.True(ApproxEqual(SysMath.Log(SysMath.E), 1.0), "Log(E) == 1");
        Assert.True(ApproxEqual(SysMath.Log(SysMath.E * SysMath.E), 2.0), "Log(E^2) == 2");
        Assert.True(double.IsNegativeInfinity(SysMath.Log(0.0)), "Log(0) == -Inf");
        Assert.True(double.IsNaN(SysMath.Log(-1.0)), "Log(-1) == NaN");
        Assert.True(double.IsPositiveInfinity(SysMath.Log(double.PositiveInfinity)), "Log(+Inf) == +Inf");
    }

    private static void Test_Log10()
    {
        Assert.True(SysMath.Log10(1.0) == 0.0, "Log10(1) == 0 (exact)");
        Assert.True(ApproxEqual(SysMath.Log10(10.0), 1.0), "Log10(10) == 1");
        Assert.True(ApproxEqual(SysMath.Log10(100.0), 2.0), "Log10(100) == 2");
        Assert.True(ApproxEqual(SysMath.Log10(0.001), -3.0), "Log10(0.001) == -3");
        Assert.True(double.IsNaN(SysMath.Log10(-1.0)), "Log10(-1) == NaN");
    }

    private static void Test_Log2()
    {
        Assert.True(SysMath.Log2(1.0) == 0.0, "Log2(1) == 0 (exact)");
        Assert.True(ApproxEqual(SysMath.Log2(2.0), 1.0), "Log2(2) == 1");
        Assert.True(ApproxEqual(SysMath.Log2(8.0), 3.0), "Log2(8) == 3");
        Assert.True(ApproxEqual(SysMath.Log2(0.5), -1.0), "Log2(0.5) == -1");
        Assert.True(double.IsNaN(SysMath.Log2(-1.0)), "Log2(-1) == NaN");
    }

    private static void Test_Log_Base()
    {
        Assert.True(ApproxEqual(SysMath.Log(8.0, 2.0), 3.0), "Log(8,2) == 3");
        Assert.True(ApproxEqual(SysMath.Log(81.0, 3.0), 4.0), "Log(81,3) == 4");
        Assert.True(ApproxEqual(SysMath.Log(1000.0, 10.0), 3.0), "Log(1000,10) == 3");
    }

    // =========================================================================
    // MathF (float variants)
    // =========================================================================

    private static void Test_MathF_Sin_Cos_Tan()
    {
        Assert.True(SysMathF.Sin(0.0f) == 0.0f, "SinF(0) == 0 (exact)");
        Assert.True(ApproxEqualF(SysMathF.Sin(SysMathF.PI / 2), 1.0f), "SinF(PI/2) == 1");
        Assert.True(ApproxEqualF(SysMathF.Sin(SysMathF.PI / 6), 0.5f), "SinF(PI/6) == 0.5");
        Assert.True(SysMathF.Cos(0.0f) == 1.0f, "CosF(0) == 1 (exact)");
        Assert.True(ApproxEqualF(SysMathF.Cos(SysMathF.PI), -1.0f), "CosF(PI) == -1");
        Assert.True(ApproxEqualF(SysMathF.Tan(SysMathF.PI / 4), 1.0f), "TanF(PI/4) == 1");
        Assert.True(float.IsNaN(SysMathF.Sin(float.NaN)), "SinF(NaN) == NaN");
    }

    private static void Test_MathF_Sqrt()
    {
        Assert.True(ApproxEqualF(SysMathF.Sqrt(4.0f), 2.0f), "SqrtF(4) == 2");
        Assert.True(ApproxEqualF(SysMathF.Sqrt(9.0f), 3.0f), "SqrtF(9) == 3");
        Assert.True(SysMathF.Sqrt(0.0f) == 0.0f, "SqrtF(0) == 0");
        Assert.True(float.IsNaN(SysMathF.Sqrt(-1.0f)), "SqrtF(-1) == NaN");
    }

    private static void Test_MathF_Exp_Log()
    {
        Assert.True(SysMathF.Exp(0.0f) == 1.0f, "ExpF(0) == 1 (exact)");
        Assert.True(ApproxEqualF(SysMathF.Exp(1.0f), SysMathF.E), "ExpF(1) == E");
        Assert.True(SysMathF.Log(1.0f) == 0.0f, "LogF(1) == 0 (exact)");
        Assert.True(ApproxEqualF(SysMathF.Log(SysMathF.E), 1.0f), "LogF(E) == 1");
        Assert.True(ApproxEqualF(SysMathF.Log10(10.0f), 1.0f), "Log10F(10) == 1");
        Assert.True(ApproxEqualF(SysMathF.Log2(8.0f), 3.0f), "Log2F(8) == 3");
    }

    private static void Test_MathF_Pow()
    {
        Assert.True(SysMathF.Pow(2.0f, 3.0f) == 8.0f, "PowF(2,3) == 8");
        Assert.True(SysMathF.Pow(2.0f, 0.0f) == 1.0f, "PowF(x,0) == 1");
        Assert.True(ApproxEqualF(SysMathF.Pow(4.0f, 0.5f), 2.0f), "PowF(4,0.5) == 2");
        Assert.True(float.IsNaN(SysMathF.Pow(-1.0f, 0.5f)), "PowF(-1,0.5) == NaN");
    }

    // =========================================================================
    // Sign / Clamp / FusedMultiplyAdd / CopySign
    // =========================================================================

    private static void Test_Sign()
    {
        Assert.Equal(1, SysMath.Sign(5.0), "Sign(5) == 1");
        Assert.Equal(-1, SysMath.Sign(-5.0), "Sign(-5) == -1");
        Assert.Equal(0, SysMath.Sign(0.0), "Sign(0) == 0");
        Assert.Equal(1, SysMath.Sign(0.1), "Sign(0.1) == 1");
        Assert.Equal(-1, SysMath.Sign(-0.1), "Sign(-0.1) == -1");
    }

    private static void Test_Clamp()
    {
        Assert.True(SysMath.Clamp(5.0, 0.0, 10.0) == 5.0, "Clamp(5,0,10) == 5");
        Assert.True(SysMath.Clamp(-1.0, 0.0, 10.0) == 0.0, "Clamp(-1,0,10) == 0");
        Assert.True(SysMath.Clamp(15.0, 0.0, 10.0) == 10.0, "Clamp(15,0,10) == 10");
        Assert.Equal(5, SysMath.Clamp(5, 0, 10), "Clamp(5,0,10) == 5 (int)");
        Assert.Equal(0, SysMath.Clamp(-1, 0, 10), "Clamp(-1,0,10) == 0 (int)");
    }

    private static void Test_FusedMultiplyAdd()
    {
        Assert.True(ApproxEqual(SysMath.FusedMultiplyAdd(2.0, 3.0, 4.0), 10.0), "FMA(2,3,4) == 10");
        Assert.True(ApproxEqual(SysMath.FusedMultiplyAdd(-1.0, 5.0, 3.0), -2.0), "FMA(-1,5,3) == -2");
        Assert.True(SysMath.FusedMultiplyAdd(0.0, 100.0, 7.0) == 7.0, "FMA(0,100,7) == 7");
    }

    private static void Test_CopySign()
    {
        Assert.True(SysMath.CopySign(5.0, -1.0) == -5.0, "CopySign(5,-1) == -5");
        Assert.True(SysMath.CopySign(-5.0, 1.0) == 5.0, "CopySign(-5,1) == 5");
        Assert.True(SysMath.CopySign(5.0, 1.0) == 5.0, "CopySign(5,1) == 5");
        Assert.True(SysMath.CopySign(-5.0, -1.0) == -5.0, "CopySign(-5,-1) == -5");
    }
}

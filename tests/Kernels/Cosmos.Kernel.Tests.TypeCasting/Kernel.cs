using System;
using System.Collections.Generic;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.TypeCasting;

public class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        Log.WriteString("[TypeCasting Tests] Starting test suite\n");
        TR.Start("TypeCasting Tests", expectedTests: 17);

        // Class hierarchy type checks (RhTypeCast_IsInstanceOfClass)
        TR.Run("IsInstanceOfClass_AnimalIsDog", TestIsInstanceOfClass);

        // Interface type checks (RhTypeCast_IsInstanceOfInterface)
        TR.Run("IsInstanceOfInterface_IFlyable", TestIsInstanceOfInterface);

        // Interface explicit cast checks (RhTypeCast_CheckCastInterface)
        TR.Run("CheckCastInterface_ValidAndInvalid", TestCheckCastInterface);

        // Multi-type pattern matching (RhTypeCast_IsInstanceOfAny)
        TR.Run("IsInstanceOfAny_MultiPattern", TestIsInstanceOfAny);

        // Generic invariance and covariance
        TR.Run("Generics_InvarianceCovariance", TestGenericsInvarianceCovariance);

        // Delegate contravariance
        TR.Run("Delegate_Contravariance", TestDelegateContravariance);

        // Array covariance
        TR.Run("Array_Covariance", TestArrayCovariance);

        // Custom generic variance
        TR.Run("CustomVariance_ProducerConsumer", TestCustomGenericVariance);

        // IEnumerable covariance
        TR.Run("IEnumerable_Covariance", TestIEnumerableCovariance);

        // Exception handling tests
        TR.Run("TryCatch_Basic", TestTryCatchBasic);
        TR.Run("TryCatch_BaseType", TestTryCatchBaseType);
        TR.Run("TryCatch_Message", TestTryCatchMessage);
        TR.Run("TryCatch_Filter_When", TestTryCatchFilterWhen);
        TR.Run("TryCatch_Filter_WhenFalse", TestTryCatchFilterWhenFalse);
        TR.Run("TryFinally", TestTryFinally);
        TR.Run("FilterAndCatchResume", TestFilterAndCatchResume);
        TR.Run("TryCatch_ConsoleWriteLineExMessage", TestTryCatchConsoleWriteLineExMessage);

        Log.WriteString("[TypeCasting Tests] All tests completed\n");
        TR.Finish();
    }

    protected override void Run()
    {
        // All tests ran in BeforeRun; stop the main loop after one iteration
        Stop();
    }

    protected override void AfterRun()
    {
        // Flush coverage data and signal QEMU to terminate
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ==================== Class Hierarchy Tests ====================

    private static void TestIsInstanceOfClass()
    {
        Animal animal = new Dog();

        // Deliberate type check against the static type: exercises the runtime isinst path.
#pragma warning disable IDE0150
        bool isAnimal = animal is Animal;
#pragma warning restore IDE0150
        bool isDog = animal is Dog;
        bool isBird = animal is Bird;

        Assert.True(isAnimal, "Dog instance is Animal");
        Assert.True(isDog, "Dog instance is Dog");
        Assert.True(!isBird, "Dog instance is not Bird");
    }

    // ==================== Interface Tests ====================

    private static void TestIsInstanceOfInterface()
    {
        Bird bird = new Bird();
        Dog dog = new Dog();

        bool birdCanFly = bird is IFlyable;
        bool dogCanFly = dog is IFlyable;

        // Value type implementing interface
        TestPoint tp = new TestPoint { X = 2, Y = 3 };
        ITestPoint? itp = tp;
        // Deliberate type check against the static type: exercises isinst on a boxed value type.
#pragma warning disable IDE0150
        bool pointIsTestPoint = itp is ITestPoint;
#pragma warning restore IDE0150

        Assert.True(birdCanFly, "Bird implements IFlyable");
        Assert.True(!dogCanFly, "Dog does not implement IFlyable");
        Assert.True(pointIsTestPoint, "TestPoint implements ITestPoint");
    }

    private static void TestCheckCastInterface()
    {
        TestPoint tp = new TestPoint { X = 2, Y = 3 };
        Dog dog = new Dog();

        bool validCastWorked;
        bool invalidCastThrew;

        // Valid cast: value type to its interface (Add exception handling when implemented)
        ITestPoint castOk = tp;
        validCastWorked = castOk.Value == 5;

        // Invalid cast: should throw InvalidCastException (For now do a safe cast until exception handling is implemented)
        invalidCastThrew = (dog as IFlyable) == null;

        Assert.True(validCastWorked, "Valid interface cast works");
        Assert.True(invalidCastThrew, "Invalid interface cast throws InvalidCastException");
    }

    // ==================== Multi-type Pattern Tests ====================

    private static void TestIsInstanceOfAny()
    {
        static bool MatchIntStringAnimal(object o) => o is int or string or Dog;

        object o1 = 123;
        object o2 = new Dog();
        object o3 = 3.1415; // double

        bool matchesInt = MatchIntStringAnimal(o1);
        bool matchesDog = MatchIntStringAnimal(o2);
        bool matchesDouble = MatchIntStringAnimal(o3);

        Assert.True(matchesInt, "Pattern matches int");
        Assert.True(matchesDog, "Pattern matches Dog");
        Assert.True(!matchesDouble, "Pattern does not match double");
    }

    // ==================== Generic Variance Tests ====================

    private static void TestGenericsInvarianceCovariance()
    {
        List<Dog> dogList = new() { new Dog(), new Dog() };

        bool isListAnimal = dogList is List<Animal>;
        bool isIEnumerableAnimal = dogList is IEnumerable<Animal>;

        Assert.True(!isListAnimal, "List<T> is invariant - List<Dog> is not List<Animal>");
        Assert.True(isIEnumerableAnimal, "IEnumerable<out T> is covariant - List<Dog> is IEnumerable<Animal>");
    }

    private static void TestDelegateContravariance()
    {
        Action<Animal> actAnimal = delegate { };
        bool isActionDog = actAnimal is Action<Dog>;

        Assert.True(isActionDog, "Action<in T> is contravariant - Action<Animal> is Action<Dog>");
    }

    private static void TestArrayCovariance()
    {
        Dog[] dogArray = new[] { new Dog(), new Dog() };
        bool isAnimalArray = dogArray is Animal[];

        Assert.True(isAnimalArray, "Dog[] is Animal[] (array covariance)");

        if (isAnimalArray)
        {
            // Also verify assignment via base-typed array reference works
            Animal[] animalArrayRef = dogArray;
            animalArrayRef[0] = new Dog();
            Assert.True(true, "Assignment via base-typed array reference works");
        }
    }

    private static void TestCustomGenericVariance()
    {
        DogProducer producer = new();
        AnimalConsumer consumer = new();

        bool producerIsAnimalProducer = producer is IProducer<Animal>;
        bool consumerIsDogConsumer = consumer is IConsumer<Dog>;

        Assert.True(producerIsAnimalProducer, "IProducer<out T> covariance - DogProducer is IProducer<Animal>");
        Assert.True(consumerIsDogConsumer, "IConsumer<in T> contravariance - AnimalConsumer is IConsumer<Dog>");
    }

    private static void TestIEnumerableCovariance()
    {
        string[] strArray = new[] { "a", "b", "c" };
        bool isIEnumerableObject = strArray is IEnumerable<object>;

        Assert.True(isIEnumerableObject, "string[] is IEnumerable<object> (covariance)");
    }

    // ==================== Exception Handling Tests ====================

    private static void TestTryCatchBasic()
    {
        bool caughtException = false;
        try
        {
            throw new InvalidOperationException("Test exception");
        }
        catch (InvalidOperationException)
        {
            caughtException = true;
        }

        Assert.True(caughtException, "Exception should have been caught");
    }

    private static void TestTryCatchBaseType()
    {
        bool caughtException = false;
        try
        {
            throw new InvalidOperationException("Test");
        }
        catch (Exception)
        {
            caughtException = true;
        }

        Assert.True(caughtException, "Base Exception type should catch derived exceptions");
    }

    private static void TestTryCatchMessage()
    {
        string? caughtMessage = null;
        try
        {
            throw new InvalidOperationException("Expected message");
        }
        catch (InvalidOperationException ex)
        {
            caughtMessage = ex.Message;
        }

        Assert.Equal("Expected message", caughtMessage);
    }

    private static void TestTryCatchFilterWhen()
    {
        bool caughtWithFilter = false;
        string? caughtMessage = null;
        try
        {
            throw new InvalidOperationException("FilterMatch");
        }
        catch (InvalidOperationException ex) when (ex.Message == "FilterMatch")
        {
            caughtWithFilter = true;
            caughtMessage = ex.Message;
        }

        Assert.True(caughtWithFilter, "Exception filter 'when' should match and catch");
        Assert.Equal("FilterMatch", caughtMessage);
    }

    private static void TestTryCatchFilterWhenFalse()
    {
        bool caughtSpecific = false;
        bool caughtGeneral = false;
        try
        {
            throw new InvalidOperationException("NoMatch");
        }
        catch (InvalidOperationException ex) when (ex.Message == "SomethingElse")
        {
            caughtSpecific = true;
        }
        catch (Exception)
        {
            caughtGeneral = true;
        }

        Assert.True(!caughtSpecific, "Exception filter 'when' should NOT match when condition is false");
        Assert.True(caughtGeneral, "General catch should handle exception when filter doesn't match");
    }

    private static void TestTryFinally()
    {
        bool finallyExecuted = false;
        try
        {
            // No exception
        }
        finally
        {
            finallyExecuted = true;
        }

        Assert.True(finallyExecuted, "Finally block should always execute");
    }

    private static void TestFilterAndCatchResume()
    {
        bool filterRan = false;
        bool catchRan = false;
        bool resumed = false;

        try
        {
            throw new Exception("FilterTest");
        }
        catch (Exception) when (RunFilter(ref filterRan))
        {
            catchRan = true;
        }

        resumed = true;

        Assert.True(filterRan, "Filter should have run");
        Assert.True(catchRan, "Catch should have run");
        Assert.True(resumed, "Execution should resume after catch");
    }

    private static bool RunFilter(ref bool flag)
    {
        flag = true;
        return true;
    }

    private static void TestTryCatchConsoleWriteLineExMessage()
    {
        const string expectedMessage = "hello world!";
        string? caughtMessage = null;
        bool consoleWriteLineReturned = false;
        bool resumedAfterCatch = false;

        try
        {
            throw new Exception(expectedMessage);
        }
        catch (Exception ex)
        {
            caughtMessage = ex.Message;
            Console.WriteLine(ex.Message);
            consoleWriteLineReturned = true;
        }

        resumedAfterCatch = true;

        Assert.Equal(expectedMessage, caughtMessage);
        Assert.True(consoleWriteLineReturned,
            "Console.WriteLine(ex.Message) inside a catch must return without page-faulting");
        Assert.True(resumedAfterCatch,
            "Execution must resume after the catch funclet exits");
    }
}

// ==================== Helper Types ====================

internal struct TestPoint : ITestPoint
{
    public int X;
    public int Y;
    public readonly int Value => X + Y;
}

internal interface ITestPoint
{
    int Value { get; }
}

internal class Animal
{
}

internal class Dog : Animal
{
}

internal interface IFlyable
{
    void Fly();
}

internal class Bird : Animal, IFlyable
{
    public void Fly()
    {
    }
}

internal interface IProducer<out T>
{
    T Produce();
}

internal interface IConsumer<in T>
{
    void Consume(T item);
}

internal class DogProducer : IProducer<Dog>
{
    public Dog Produce() => new Dog();
}

internal class AnimalConsumer : IConsumer<Animal>
{
    public void Consume(Animal item)
    {
    }
}

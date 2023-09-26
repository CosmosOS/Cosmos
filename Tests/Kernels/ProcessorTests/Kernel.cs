using System;
using Sys = Cosmos.System;
using Cosmos.TestRunner;
using Cosmos.System.Graphics;
using System.Text;
using Cosmos.System.ExtendedASCII;
using Cosmos.System.ScanMaps;
using Cosmos.Core.Multiboot;
using Cosmos.Core;
using System.Runtime.InteropServices;
using Cosmos.HAL;

namespace ProcessorTests
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting Tests");
        }

        protected override void Run()
        {
            try
            {
                TestMultibootMemoryMap();
                TestGetRam();
                TestVendorNameIsNotBlank();
                TestBrandStringBlank();
                TestCycleCount();
                TestCycleRateIsNotZero();
                TestMultiboot();

                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);
                TestController.Failed();
            }
        }

        public void TestGetRam()
        {
            Assert.IsTrue(CPU.GetAmountOfRAM() > 0, "CPU.GetAmountOfRAM() returns a positive value: " + CPU.GetAmountOfRAM());
        }

        public void TestMultibootMemoryMap()
        {
            var memoryMap = CPU.GetMemoryMap();
            for (int i = 0; i < memoryMap.Length; i++)
            {
                mDebugger.Send($"Memory Map: {memoryMap[i].Address} " +
                    $"Length: {memoryMap[i].Length} Type: {memoryMap[i].Type}");
            }
            Assert.IsTrue(memoryMap.Length != 0, "Memory Map is not empty! Length " + memoryMap.Length);
        }

        public void TestMultiboot()
        {
            Assert.IsTrue(Multiboot2.GetMBIAddress() != 0, $"Multiboot.GetMBIAddress works {Multiboot2.GetMBIAddress()}");
        }
         
        public void TestBrandStringBlank()
        {
            string brandString = CPU.GetCPUBrandString();
            mDebugger.Send("Brand String: " + brandString);
            bool isBrandStringBlank = string.IsNullOrWhiteSpace(brandString);
            Assert.IsFalse(isBrandStringBlank, "Processor brand string is blank.");
        }

        public void TestVendorNameIsNotBlank()
        {
            string vendorName = CPU.GetCPUVendorName();
            mDebugger.Send("Vendor name: " + vendorName);
            bool isVendorNameBlank = string.IsNullOrWhiteSpace(vendorName);
            mDebugger.Send("Vendor name: ");
            mDebugger.Send(vendorName);
            Assert.IsFalse(isVendorNameBlank, "Processor vendor name is blank.");
        }

        public void TestCycleCount()
        {
            ulong cycleCount = CPU.GetCPUUptime();
            mDebugger.Send($"CycleCount: {cycleCount}");
            bool isCycleCountZero = cycleCount == 0;
            Assert.IsFalse(isCycleCountZero, "Processor cycle count is not zero.");
            ulong secondCount = CPU.GetCPUUptime();
            Assert.IsTrue(secondCount > cycleCount, "Processor cycle count is increasing");
        }

        public void TestCycleRateIsNotZero()
        {
            long cycleRate = CPU.GetCPUCycleSpeed();
            mDebugger.Send($"CycleRate: {cycleRate}");
            bool isCycleRateZero = cycleRate == 0;
            Assert.IsFalse(isCycleRateZero, "Processor cycle rate is not zero.");
            Assert.IsTrue(CPU.GetCPUCycleSpeed() == cycleRate, "Processor cycle speed is not constant");
        }
    }
}

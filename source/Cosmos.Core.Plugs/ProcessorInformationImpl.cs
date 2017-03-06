using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Xml.Schema;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Core.PCInformation;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;

using XSharp.Compiler;

namespace Cosmos.Core.Plugs
{
    [Plug(Target = typeof(ProcessorInformation))]
    public unsafe class ProcessorInformationImpl
    {

        /// <summary>
        /// Use the rdtsc instruction to read the current time stamp counter
        /// In edx will be stored the highest part of the rtdsc
        /// In eax the lowest part of the rtdsc
        /// </summary>
        /// <param name="edx">Lowest part of rtdsc</param>
        /// <param name="eax">Highest part of rtdsc</param>
        [Inline]
        public static void GetCurrentTimeStampCounter(uint* edx, uint* eax)
        {
            XS.Rdtsc();
            //Get the edx pointer
            XS.Set(XSRegisters.EBX, XSRegisters.EBP, sourceDisplacement: 8);
            //Store the value on the variable edx
            XS.Set(XSRegisters.EBX, XSRegisters.EDX, destinationIsIndirect: true);
            XS.Set(XSRegisters.EBX, XSRegisters.EBP, sourceDisplacement: 12);
            XS.Set(XSRegisters.EBX, XSRegisters.EAX, destinationIsIndirect: true);
        }

        /// <summary>
        /// This function queries cpuid to get the registers involved.
        /// If a value is not used it will contain garbage.
        /// Requires that none of the arguments are null. THIS IS PROGRAMMER RESPONSABILITY
        /// </summary>
        /// call example <c>CPUID(0, &eax, &ebx, &ecx, &edx);</c> where eax, ebx, and edx are UINT
        /// <param name="eaxOperation">Number of the operation that cpuid will do.</param>
        /// <param name="eax">returned eax register (not null)</param>
        /// <param name="ebx">returned ebx register (not null)</param>
        /// <param name="ecx">returned ecx register (not null)</param>
        /// <param name="edx">returned edx register (not null)</param>
        [Inline]
        public static void CPUID(uint eaxOperation, uint* eax, uint* ebx, uint* ecx, uint* edx)
        {
            //Note that the arguments are pushed left to right. Thus, eaxoperation will be on the bottom of the stack.
            //Since the stack grows to 0, we need to put 24 to get the first arg.
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 24);
            //Call cpuid to get the information
            XS.Cpuid();

            //Now comes a trick to only use 4 general purpose registers (eax, ebx, ecx, edx)

            //Save the possible value ebx contains
            XS.Push(XSRegisters.EBX);
            //Set in ebx a pointer to the data (in this case, a pointer to "uint* eax", i.e, the second argument)
            XS.Set(XSRegisters.EBX, XSRegisters.EBP, sourceDisplacement: (20));
            //Exchange the eax and ebx registers.
            //Now eax has the pointer and ebx has the value returned by cpuid in eax
            XS.Exchange(XSRegisters.EAX, XSRegisters.EBX); 
            //Store the cpuid eax value in uint *eax, i.e, store the cpuid eax in the second argument
            XS.Set(XSRegisters.EAX, XSRegisters.EBX, destinationIsIndirect: true);
            //Set ebx as it were
            XS.Pop(XSRegisters.EBX);

            //Do the same strategy for all the rest
            XS.Push(XSRegisters.EAX);
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: (16));
            XS.Exchange(XSRegisters.EBX, XSRegisters.EAX); 
            XS.Set(XSRegisters.EBX, XSRegisters.EAX, destinationIsIndirect: true);
            XS.Pop(XSRegisters.EAX);

            XS.Push(XSRegisters.EAX);
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 12);
            XS.Exchange(XSRegisters.ECX, XSRegisters.EAX);
            XS.Set(XSRegisters.ECX, XSRegisters.EAX, destinationIsIndirect: true);
            XS.Pop(XSRegisters.EAX);

            XS.Push(XSRegisters.EAX);
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
            XS.Exchange(XSRegisters.EDX, XSRegisters.EAX);
            XS.Set(XSRegisters.EDX, XSRegisters.EAX, destinationIsIndirect: true);
            XS.Pop(XSRegisters.EAX);
    }

        [Inline]
        public static int CanReadCPUID()
        {
            /*
             * pushfd
             * pushfd
             * xor dword [esp], 00200000h
             * popfd
             * pushfd
             * pop eax
             * xor eax, [esp]
             * and eax, 00200000h
             * ret
             */
            XS.Pushfd();
            XS.Pushfd();
            XS.Xor(XSRegisters.ESP, 0x00200000, destinationIsIndirect: true);
            XS.Popfd();
            XS.Pushfd();
            XS.Pop(XSRegisters.EAX);
            XS.Xor(XSRegisters.EAX, XSRegisters.ESP, destinationIsIndirect: true);
            XS.Popfd();
            XS.And(XSRegisters.EAX, 0x00200000);
            XS.Set(XSRegisters.EAX, 1);
            XS.Push(XSRegisters.EAX);

            return 0;
        }
    }
} 
// Disable some warning, because although they should work, they might not.
#pragma warning disable 1718 
using System;
using Cosmos.Hardware;

namespace TestRunner
{
    public class TestRunnerKernel : Cosmos.System.Kernel
    {
        public Logger log = new Logger(2);

        public TestRunnerKernel()
        {
            base.ClearScreen = true;
        }
        protected override void BeforeRun() { }


        protected override void Run()
        {
            Console.WriteLine("Starting Tests. " + CurrentTime());
            log.WriteString("Starting Tests. " + CurrentTime() + "\r\n\r\n");

            #region Test the Logger
            Console.WriteLine("Starting Tests of Logger. " + CurrentTime());
            log.WriteString("Starting Tests of Logger. " + CurrentTime() + "\r\n");
            log.WriteString(GTN() + "255 as Byte:");
            log.WriteData((byte)0xFF);
            log.WriteString("\r\n");
            log.WriteString(GTN() + "255 as SByte:");
            log.WriteData(unchecked((sbyte)0xFF));
            log.WriteString("\r\n");
            log.WriteString(GTN() + "255 as UShort:");
            log.WriteData((ushort)0xFF);
            log.WriteString("\r\n");
            log.WriteString(GTN() + "255 as Short:");
            log.WriteData((short)0xFF);
            log.WriteString("\r\n");
            log.WriteString(GTN() + "255 as UInt:");
            log.WriteData((uint)0xFF);
            log.WriteString("\r\n");
            log.WriteString(GTN() + "255 as Int:");
            log.WriteData((int)0xFF);
            log.WriteString("\r\n");
            log.WriteString(GTN() + "255 as ULong:");
            log.WriteData((ulong)0xFF);
            log.WriteString("\r\n");
            log.WriteString(GTN() + "255 as Long:");
            log.WriteData((long)0xFF);
            log.WriteString("\r\n");
            log.WriteString(GTN() + "255 as Float:");
            log.WriteData((float)0xFF);
            log.WriteString("\r\n");
            log.WriteString(GTN() + "255 as Double:");
            log.WriteData((double)0xFF);
            log.WriteString("\r\n");
            log.WriteString("Finished Testing Logger. " + CurrentTime() + "\r\n\r\n");
            Console.WriteLine("Finished Testing Logger. " + CurrentTime());
            #endregion

            #region Test the Operators

            #region Byte
            {
                Console.WriteLine("Testing Byte Comparisons. " + CurrentTime());
                log.WriteString("Starting Byte Comparison Tests. " + CurrentTime() + "\r\n");
                byte zero = 0;
                byte one = 1;

                #region Variable to Variable
                log.WriteString("Starting Byte Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                {
                    if (zero != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Byte-Var-Var)\r\n");

                    if (one != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Byte-Var-Var)\r\n");
                }
                {
                    if (zero == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Byte-Var-Var)\r\n");

                    if (one == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Byte-Var-Var)\r\n");
                }
                {
                    if (zero > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Byte-Var-Var)\r\n");

                    if (one > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Byte-Var-Var)\r\n");
                }
                {
                    if (zero >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Byte-Var-Var)\r\n");

                    if (one >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Byte-Var-Var)\r\n");
                }
                {
                    if (zero < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Byte-Var-Var)\r\n");

                    if (one < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Byte-Var-Var)\r\n");
                }
                {
                    if (zero <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Byte-Var-Var)\r\n");

                    if (one <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Byte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Byte-Var-Var)\r\n");
                }
                log.WriteString("Finished Byte Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                #endregion

                #region Variable to Constant
                log.WriteString("Starting Byte Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                {
                    if (zero != 0)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Byte-Var-Const)\r\n");

                    if (one != 0)
                        log.WriteString(GTN() + "Pass: one != zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Byte-Var-Const)\r\n");
                }
                {
                    if (zero == 0)
                        log.WriteString(GTN() + "Pass: zero == zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Byte-Var-Const)\r\n");

                    if (one == 0)
                        log.WriteString(GTN() + "FAILURE: one == zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Byte-Var-Const)\r\n");
                }
                {
                    if (zero > 0)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Byte-Var-Const)\r\n");

                    if (one > 0)
                        log.WriteString(GTN() + "Pass: one > zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Byte-Var-Const)\r\n");
                }
                {
                    if (zero >= 0)
                        log.WriteString(GTN() + "Pass: zero >= zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Byte-Var-Const)\r\n");

                    if (one >= 0)
                        log.WriteString(GTN() + "Pass: one >= zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Byte-Var-Const)\r\n");
                }
                {
                    if (zero < 0)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Byte-Var-Const)\r\n");

                    if (one < 0)
                        log.WriteString(GTN() + "FAILURE: one < zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Byte-Var-Const)\r\n");
                }
                {
                    if (zero <= 0)
                        log.WriteString(GTN() + "Pass: zero <= zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Byte-Var-Const)\r\n");

                    if (one <= 0)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Byte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Byte-Var-Const)\r\n");
                }
                log.WriteString("Finished Byte Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                #endregion

                #region Constant to Variable
                log.WriteString("Starting Byte Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                {
                    if (0 != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Byte-Const-Var)\r\n");

                    if (1 != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Byte-Const-Var)\r\n");
                }
                {
                    if (0 == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Byte-Const-Var)\r\n");

                    if (1 == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Byte-Const-Var)\r\n");
                }
                {
                    if (0 > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Byte-Const-Var)\r\n");

                    if (1 > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Byte-Const-Var)\r\n");
                }
                {
                    if (0 >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Byte-Const-Var)\r\n");

                    if (1 >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Byte-Const-Var)\r\n");
                }
                {
                    if (0 < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Byte-Const-Var)\r\n");

                    if (1 < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Byte-Const-Var)\r\n");
                }
                {
                    if (0 <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Byte-Const-Var)\r\n");

                    if (1 <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Byte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Byte-Const-Var)\r\n");
                }
                log.WriteString("Finished Byte Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                #endregion

                Console.WriteLine("Finished Testing Byte Comparisons. " + CurrentTime());
                log.WriteString("Finished Testing Byte Comparisons. " + CurrentTime() + "\r\n\r\n");
            }
            #endregion

            #region SByte
            {
                Console.WriteLine("Testing SByte Comparisons. " + CurrentTime());
                log.WriteString("Starting SByte Comparison Tests. " + CurrentTime() + "\r\n");
                sbyte zero = 0;
                sbyte one = 1;

                #region Variable to Variable
                log.WriteString("Starting SByte Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                {
                    if (zero != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (SByte-Var-Var)\r\n");

                    if (one != zero)
                        log.WriteString(GTN() + "Pass: one != zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (SByte-Var-Var)\r\n");
                }
                {
                    if (zero == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (SByte-Var-Var)\r\n");

                    if (one == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (SByte-Var-Var)\r\n");
                }
                {
                    if (zero > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (SByte-Var-Var)\r\n");

                    if (one > zero)
                        log.WriteString(GTN() + "Pass: one > zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (SByte-Var-Var)\r\n");
                }
                {
                    if (zero >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (SByte-Var-Var)\r\n");

                    if (one >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (SByte-Var-Var)\r\n");
                }
                {
                    if (zero < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (SByte-Var-Var)\r\n");

                    if (one < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (SByte-Var-Var)\r\n");
                }
                {
                    if (zero <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (SByte-Var-Var)\r\n");

                    if (one <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (SByte-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (SByte-Var-Var)\r\n");
                }
                log.WriteString("Finished SByte Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                #endregion

                #region Variable to Constant
                log.WriteString("Starting SByte Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                {
                    if (zero != 0)
                        log.WriteString(GTN() + "FAILURE: zero != zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (SByte-Var-Const)\r\n");

                    if (one != 0)
                        log.WriteString(GTN() + "Pass: one != zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (SByte-Var-Const)\r\n");
                }
                {
                    if (zero == 0)
                        log.WriteString(GTN() + "Pass: zero == zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (SByte-Var-Const)\r\n");

                    if (one == 0)
                        log.WriteString(GTN() + "FAILURE: one == zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (SByte-Var-Const)\r\n");
                }
                {
                    if (zero > 0)
                        log.WriteString(GTN() + "FAILURE: zero > zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (SByte-Var-Const)\r\n");

                    if (one > 0)
                        log.WriteString(GTN() + "Pass: one > zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (SByte-Var-Const)\r\n");
                }
                {
                    if (zero >= 0)
                        log.WriteString(GTN() + "Pass: zero >= zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (SByte-Var-Const)\r\n");

                    if (one >= 0)
                        log.WriteString(GTN() + "Pass: one >= zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (SByte-Var-Const)\r\n");
                }
                {
                    if (zero < 0)
                        log.WriteString(GTN() + "FAILURE: zero < zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (SByte-Var-Const)\r\n");

                    if (one < 0)
                        log.WriteString(GTN() + "FAILURE: one < zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (SByte-Var-Const)\r\n");
                }
                {
                    if (zero <= 0)
                        log.WriteString(GTN() + "Pass: zero <= zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (SByte-Var-Const)\r\n");

                    if (one <= 0)
                        log.WriteString(GTN() + "FAILURE: one <= zero (SByte-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (SByte-Var-Const)\r\n");
                }
                log.WriteString("Finished SByte Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                #endregion

                #region Constant to Variable
                log.WriteString("Starting SByte Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                {
                    if (0 != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (SByte-Const-Var)\r\n");

                    if (1 != zero)
                        log.WriteString(GTN() + "Pass: one != zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (SByte-Const-Var)\r\n");
                }
                {
                    if (0 == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (SByte-Const-Var)\r\n");

                    if (1 == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (SByte-Const-Var)\r\n");
                }
                {
                    if (0 > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (SByte-Const-Var)\r\n");

                    if (1 > zero)
                        log.WriteString(GTN() + "Pass: one > zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (SByte-Const-Var)\r\n");
                }
                {
                    if (0 >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (SByte-Const-Var)\r\n");

                    if (1 >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (SByte-Const-Var)\r\n");
                }
                {
                    if (0 < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (SByte-Const-Var)\r\n");

                    if (1 < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (SByte-Const-Var)\r\n");
                }
                {
                    if (0 <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (SByte-Const-Var)\r\n");

                    if (1 <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (SByte-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (SByte-Const-Var)\r\n");
                }
                log.WriteString("Finished SByte Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                #endregion

                Console.WriteLine("Finished Testing SByte Comparisons. " + CurrentTime());
                log.WriteString("Finished Testing SByte Comparisons. " + CurrentTime() + "\r\n\r\n");
            }
            #endregion

            #region UShort
            {
                Console.WriteLine("Testing UShort Comparisons. " + CurrentTime());
                log.WriteString("Starting UShort Comparison Tests. " + CurrentTime() + "\r\n");
                ushort zero = 0;
                ushort one = 1;

                #region Variable to Variable
                log.WriteString("Starting UShort Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                {
                    if (zero != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (UShort-Var-Var)\r\n");

                    if (one != zero)
                        log.WriteString(GTN() + "Pass: one != zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (UShort-Var-Var)\r\n");
                }
                {
                    if (zero == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (UShort-Var-Var)\r\n");

                    if (one == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (UShort-Var-Var)\r\n");
                }
                {
                    if (zero > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (UShort-Var-Var)\r\n");

                    if (one > zero)
                        log.WriteString(GTN() + "Pass: one > zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (UShort-Var-Var)\r\n");
                }
                {
                    if (zero >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (UShort-Var-Var)\r\n");

                    if (one >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (UShort-Var-Var)\r\n");
                }
                {
                    if (zero < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (UShort-Var-Var)\r\n");

                    if (one < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (UShort-Var-Var)\r\n");
                }
                {
                    if (zero <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (UShort-Var-Var)\r\n");

                    if (one <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (UShort-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (UShort-Var-Var)\r\n");
                }
                log.WriteString("Finished UShort Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                #endregion

                #region Variable to Constant
                log.WriteString("Starting UShort Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                {
                    if (zero != 0)
                        log.WriteString(GTN() + "FAILURE: zero != zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (UShort-Var-Const)\r\n");

                    if (one != 0)
                        log.WriteString(GTN() + "Pass: one != zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (UShort-Var-Const)\r\n");
                }
                {
                    if (zero == 0)
                        log.WriteString(GTN() + "Pass: zero == zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (UShort-Var-Const)\r\n");

                    if (one == 0)
                        log.WriteString(GTN() + "FAILURE: one == zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (UShort-Var-Const)\r\n");
                }
                {
                    if (zero > 0)
                        log.WriteString(GTN() + "FAILURE: zero > zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (UShort-Var-Const)\r\n");

                    if (one > 0)
                        log.WriteString(GTN() + "Pass: one > zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (UShort-Var-Const)\r\n");
                }
                {
                    if (zero >= 0)
                        log.WriteString(GTN() + "Pass: zero >= zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (UShort-Var-Const)\r\n");

                    if (one >= 0)
                        log.WriteString(GTN() + "Pass: one >= zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (UShort-Var-Const)\r\n");
                }
                {
                    if (zero < 0)
                        log.WriteString(GTN() + "FAILURE: zero < zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (UShort-Var-Const)\r\n");

                    if (one < 0)
                        log.WriteString(GTN() + "FAILURE: one < zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (UShort-Var-Const)\r\n");
                }
                {
                    if (zero <= 0)
                        log.WriteString(GTN() + "Pass: zero <= zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (UShort-Var-Const)\r\n");

                    if (one <= 0)
                        log.WriteString(GTN() + "FAILURE: one <= zero (UShort-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (UShort-Var-Const)\r\n");
                }
                log.WriteString("Finished UShort Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                #endregion

                #region Constant to Variable
                log.WriteString("Starting UShort Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                {
                    if (0 != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (UShort-Const-Var)\r\n");

                    if (1 != zero)
                        log.WriteString(GTN() + "Pass: one != zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (UShort-Const-Var)\r\n");
                }
                {
                    if (0 == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (UShort-Const-Var)\r\n");

                    if (1 == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (UShort-Const-Var)\r\n");
                }
                {
                    if (0 > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (UShort-Const-Var)\r\n");

                    if (1 > zero)
                        log.WriteString(GTN() + "Pass: one > zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (UShort-Const-Var)\r\n");
                }
                {
                    if (0 >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (UShort-Const-Var)\r\n");

                    if (1 >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (UShort-Const-Var)\r\n");
                }
                {
                    if (0 < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (UShort-Const-Var)\r\n");

                    if (1 < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (UShort-Const-Var)\r\n");
                }
                {
                    if (0 <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (UShort-Const-Var)\r\n");

                    if (1 <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (UShort-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (UShort-Const-Var)\r\n");
                }
                log.WriteString("Finished UShort Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                #endregion

                Console.WriteLine("Finished Testing UShort Comparisons. " + CurrentTime());
                log.WriteString("Finished Testing UShort Comparisons. " + CurrentTime() + "\r\n\r\n");
            }
            #endregion

            #region Short
            {
                Console.WriteLine("Testing Short Comparisons. " + CurrentTime());
                log.WriteString("Starting Short Comparison Tests. " + CurrentTime() + "\r\n");
                short zero = 0;
                short one = 1;

                #region Variable to Variable
                log.WriteString("Starting Short Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                {
                    if (zero != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Short-Var-Var)\r\n");

                    if (one != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Short-Var-Var)\r\n");
                }
                {
                    if (zero == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Short-Var-Var)\r\n");

                    if (one == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Short-Var-Var)\r\n");
                }
                {
                    if (zero > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Short-Var-Var)\r\n");

                    if (one > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Short-Var-Var)\r\n");
                }
                {
                    if (zero >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Short-Var-Var)\r\n");

                    if (one >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Short-Var-Var)\r\n");
                }
                {
                    if (zero < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Short-Var-Var)\r\n");

                    if (one < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Short-Var-Var)\r\n");
                }
                {
                    if (zero <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Short-Var-Var)\r\n");

                    if (one <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Short-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Short-Var-Var)\r\n");
                }
                log.WriteString("Finished Short Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                #endregion

                #region Variable to Constant
                log.WriteString("Starting Short Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                {
                    if (zero != 0)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Short-Var-Const)\r\n");

                    if (one != 0)
                        log.WriteString(GTN() + "Pass: one != zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Short-Var-Const)\r\n");
                }
                {
                    if (zero == 0)
                        log.WriteString(GTN() + "Pass: zero == zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Short-Var-Const)\r\n");

                    if (one == 0)
                        log.WriteString(GTN() + "FAILURE: one == zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Short-Var-Const)\r\n");
                }
                {
                    if (zero > 0)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Short-Var-Const)\r\n");

                    if (one > 0)
                        log.WriteString(GTN() + "Pass: one > zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Short-Var-Const)\r\n");
                }
                {
                    if (zero >= 0)
                        log.WriteString(GTN() + "Pass: zero >= zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Short-Var-Const)\r\n");

                    if (one >= 0)
                        log.WriteString(GTN() + "Pass: one >= zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Short-Var-Const)\r\n");
                }
                {
                    if (zero < 0)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Short-Var-Const)\r\n");

                    if (one < 0)
                        log.WriteString(GTN() + "FAILURE: one < zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Short-Var-Const)\r\n");
                }
                {
                    if (zero <= 0)
                        log.WriteString(GTN() + "Pass: zero <= zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Short-Var-Const)\r\n");

                    if (one <= 0)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Short-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Short-Var-Const)\r\n");
                }
                log.WriteString("Finished Short Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                #endregion

                #region Constant to Variable
                log.WriteString("Starting Short Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                {
                    if (0 != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Short-Const-Var)\r\n");

                    if (1 != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Short-Const-Var)\r\n");
                }
                {
                    if (0 == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Short-Const-Var)\r\n");

                    if (1 == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Short-Const-Var)\r\n");
                }
                {
                    if (0 > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Short-Const-Var)\r\n");

                    if (1 > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Short-Const-Var)\r\n");
                }
                {
                    if (0 >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Short-Const-Var)\r\n");

                    if (1 >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Short-Const-Var)\r\n");
                }
                {
                    if (0 < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Short-Const-Var)\r\n");

                    if (1 < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Short-Const-Var)\r\n");
                }
                {
                    if (0 <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Short-Const-Var)\r\n");

                    if (1 <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Short-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Short-Const-Var)\r\n");
                }
                log.WriteString("Finished Short Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                #endregion

                Console.WriteLine("Finished Testing Short Comparisons. " + CurrentTime());
                log.WriteString("Finished Testing Short Comparisons. " + CurrentTime() + "\r\n\r\n");
            }
            #endregion

            #region UInt
            {
                Console.WriteLine("Testing UInt Comparisons. " + CurrentTime());
                log.WriteString("Starting UInt Comparison Tests. " + CurrentTime() + "\r\n");
                uint zero = 0;
                uint one = 1;

                #region Variable to Variable
                log.WriteString("Starting UInt Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                {
                    if (zero != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (UInt-Var-Var)\r\n");

                    if (one != zero)
                        log.WriteString(GTN() + "Pass: one != zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (UInt-Var-Var)\r\n");
                }
                {
                    if (zero == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (UInt-Var-Var)\r\n");

                    if (one == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (UInt-Var-Var)\r\n");
                }
                {
                    if (zero > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (UInt-Var-Var)\r\n");

                    if (one > zero)
                        log.WriteString(GTN() + "Pass: one > zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (UInt-Var-Var)\r\n");
                }
                {
                    if (zero >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (UInt-Var-Var)\r\n");

                    if (one >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (UInt-Var-Var)\r\n");
                }
                {
                    if (zero < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (UInt-Var-Var)\r\n");

                    if (one < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (UInt-Var-Var)\r\n");
                }
                {
                    if (zero <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (UInt-Var-Var)\r\n");

                    if (one <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (UInt-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (UInt-Var-Var)\r\n");
                }
                log.WriteString("Finished UInt Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                #endregion

                #region Variable to Constant
                log.WriteString("Starting UInt Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                {
                    if (zero != 0)
                        log.WriteString(GTN() + "FAILURE: zero != zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (UInt-Var-Const)\r\n");

                    if (one != 0)
                        log.WriteString(GTN() + "Pass: one != zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (UInt-Var-Const)\r\n");
                }
                {
                    if (zero == 0)
                        log.WriteString(GTN() + "Pass: zero == zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (UInt-Var-Const)\r\n");

                    if (one == 0)
                        log.WriteString(GTN() + "FAILURE: one == zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (UInt-Var-Const)\r\n");
                }
                {
                    if (zero > 0)
                        log.WriteString(GTN() + "FAILURE: zero > zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (UInt-Var-Const)\r\n");

                    if (one > 0)
                        log.WriteString(GTN() + "Pass: one > zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (UInt-Var-Const)\r\n");
                }
                {
                    if (zero >= 0)
                        log.WriteString(GTN() + "Pass: zero >= zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (UInt-Var-Const)\r\n");

                    if (one >= 0)
                        log.WriteString(GTN() + "Pass: one >= zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (UInt-Var-Const)\r\n");
                }
                {
                    if (zero < 0)
                        log.WriteString(GTN() + "FAILURE: zero < zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (UInt-Var-Const)\r\n");

                    if (one < 0)
                        log.WriteString(GTN() + "FAILURE: one < zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (UInt-Var-Const)\r\n");
                }
                {
                    if (zero <= 0)
                        log.WriteString(GTN() + "Pass: zero <= zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (UInt-Var-Const)\r\n");

                    if (one <= 0)
                        log.WriteString(GTN() + "FAILURE: one <= zero (UInt-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (UInt-Var-Const)\r\n");
                }
                log.WriteString("Finished UInt Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                #endregion

                #region Constant to Variable
                log.WriteString("Starting UInt Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                {
                    if (0 != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (UInt-Const-Var)\r\n");

                    if (1 != zero)
                        log.WriteString(GTN() + "Pass: one != zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (UInt-Const-Var)\r\n");
                }
                {
                    if (0 == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (UInt-Const-Var)\r\n");

                    if (1 == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (UInt-Const-Var)\r\n");
                }
                {
                    if (0 > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (UInt-Const-Var)\r\n");

                    if (1 > zero)
                        log.WriteString(GTN() + "Pass: one > zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (UInt-Const-Var)\r\n");
                }
                {
                    if (0 >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (UInt-Const-Var)\r\n");

                    if (1 >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (UInt-Const-Var)\r\n");
                }
                {
                    if (0 < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (UInt-Const-Var)\r\n");

                    if (1 < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (UInt-Const-Var)\r\n");
                }
                {
                    if (0 <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (UInt-Const-Var)\r\n");

                    if (1 <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (UInt-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (UInt-Const-Var)\r\n");
                }
                log.WriteString("Finished UInt Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                #endregion

                Console.WriteLine("Finished Testing UInt Comparisons. " + CurrentTime());
                log.WriteString("Finished Testing UInt Comparisons. " + CurrentTime() + "\r\n\r\n");
            }
            #endregion

            #region Int
            {
                Console.WriteLine("Testing Int Comparisons. " + CurrentTime());
                log.WriteString("Starting Int Comparison Tests. " + CurrentTime() + "\r\n");
                int zero = 0;
                int one = 1;

                #region Variable to Variable
                log.WriteString("Starting Int Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                {
                    if (zero != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Int-Var-Var)\r\n");

                    if (one != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Int-Var-Var)\r\n");
                }
                {
                    if (zero == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Int-Var-Var)\r\n");

                    if (one == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Int-Var-Var)\r\n");
                }
                {
                    if (zero > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Int-Var-Var)\r\n");

                    if (one > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Int-Var-Var)\r\n");
                }
                {
                    if (zero >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Int-Var-Var)\r\n");

                    if (one >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Int-Var-Var)\r\n");
                }
                {
                    if (zero < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Int-Var-Var)\r\n");

                    if (one < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Int-Var-Var)\r\n");
                }
                {
                    if (zero <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Int-Var-Var)\r\n");

                    if (one <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Int-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Int-Var-Var)\r\n");
                }
                log.WriteString("Finished Int Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                #endregion

                #region Variable to Constant
                log.WriteString("Starting Int Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                {
                    if (zero != 0)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Int-Var-Const)\r\n");

                    if (one != 0)
                        log.WriteString(GTN() + "Pass: one != zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Int-Var-Const)\r\n");
                }
                {
                    if (zero == 0)
                        log.WriteString(GTN() + "Pass: zero == zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Int-Var-Const)\r\n");

                    if (one == 0)
                        log.WriteString(GTN() + "FAILURE: one == zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Int-Var-Const)\r\n");
                }
                {
                    if (zero > 0)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Int-Var-Const)\r\n");

                    if (one > 0)
                        log.WriteString(GTN() + "Pass: one > zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Int-Var-Const)\r\n");
                }
                {
                    if (zero >= 0)
                        log.WriteString(GTN() + "Pass: zero >= zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Int-Var-Const)\r\n");

                    if (one >= 0)
                        log.WriteString(GTN() + "Pass: one >= zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Int-Var-Const)\r\n");
                }
                {
                    if (zero < 0)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Int-Var-Const)\r\n");

                    if (one < 0)
                        log.WriteString(GTN() + "FAILURE: one < zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Int-Var-Const)\r\n");
                }
                {
                    if (zero <= 0)
                        log.WriteString(GTN() + "Pass: zero <= zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Int-Var-Const)\r\n");

                    if (one <= 0)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Int-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Int-Var-Const)\r\n");
                }
                log.WriteString("Finished Int Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                #endregion

                #region Constant to Variable
                log.WriteString("Starting Int Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                {
                    if (0 != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Int-Const-Var)\r\n");

                    if (1 != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Int-Const-Var)\r\n");
                }
                {
                    if (0 == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Int-Const-Var)\r\n");

                    if (1 == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Int-Const-Var)\r\n");
                }
                {
                    if (0 > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Int-Const-Var)\r\n");

                    if (1 > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Int-Const-Var)\r\n");
                }
                {
                    if (0 >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Int-Const-Var)\r\n");

                    if (1 >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Int-Const-Var)\r\n");
                }
                {
                    if (0 < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Int-Const-Var)\r\n");

                    if (1 < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Int-Const-Var)\r\n");
                }
                {
                    if (0 <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Int-Const-Var)\r\n");

                    if (1 <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Int-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Int-Const-Var)\r\n");
                }
                log.WriteString("Finished Int Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                #endregion

                Console.WriteLine("Finished Testing Int Comparisons. " + CurrentTime());
                log.WriteString("Finished Testing Int Comparisons. " + CurrentTime() + "\r\n\r\n");
            }
            #endregion

            #region ULong
            {
                Console.WriteLine("Testing ULong Comparisons. " + CurrentTime());
                log.WriteString("Starting ULong Comparison Tests. " + CurrentTime() + "\r\n");
                ulong zero = 0;
                ulong one = 1;

                #region Variable to Variable
                log.WriteString("Starting ULong Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                {
                    if (zero != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (ULong-Var-Var)\r\n");

                    if (one != zero)
                        log.WriteString(GTN() + "Pass: one != zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (ULong-Var-Var)\r\n");
                }
                {
                    if (zero == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (ULong-Var-Var)\r\n");

                    if (one == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (ULong-Var-Var)\r\n");
                }
                {
                    if (zero > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (ULong-Var-Var)\r\n");

                    if (one > zero)
                        log.WriteString(GTN() + "Pass: one > zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (ULong-Var-Var)\r\n");
                }
                {
                    if (zero >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (ULong-Var-Var)\r\n");

                    if (one >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (ULong-Var-Var)\r\n");
                }
                {
                    if (zero < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (ULong-Var-Var)\r\n");

                    if (one < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (ULong-Var-Var)\r\n");
                }
                {
                    if (zero <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (ULong-Var-Var)\r\n");

                    if (one <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (ULong-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (ULong-Var-Var)\r\n");
                }
                log.WriteString("Finished ULong Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                #endregion

                #region Variable to Constant
                log.WriteString("Starting ULong Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                {
                    if (zero != 0)
                        log.WriteString(GTN() + "FAILURE: zero != zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (ULong-Var-Const)\r\n");

                    if (one != 0)
                        log.WriteString(GTN() + "Pass: one != zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (ULong-Var-Const)\r\n");
                }
                {
                    if (zero == 0)
                        log.WriteString(GTN() + "Pass: zero == zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (ULong-Var-Const)\r\n");

                    if (one == 0)
                        log.WriteString(GTN() + "FAILURE: one == zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (ULong-Var-Const)\r\n");
                }
                {
                    if (zero > 0)
                        log.WriteString(GTN() + "FAILURE: zero > zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (ULong-Var-Const)\r\n");

                    if (one > 0)
                        log.WriteString(GTN() + "Pass: one > zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (ULong-Var-Const)\r\n");
                }
                {
                    if (zero >= 0)
                        log.WriteString(GTN() + "Pass: zero >= zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (ULong-Var-Const)\r\n");

                    if (one >= 0)
                        log.WriteString(GTN() + "Pass: one >= zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (ULong-Var-Const)\r\n");
                }
                {
                    if (zero < 0)
                        log.WriteString(GTN() + "FAILURE: zero < zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (ULong-Var-Const)\r\n");

                    if (one < 0)
                        log.WriteString(GTN() + "FAILURE: one < zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (ULong-Var-Const)\r\n");
                }
                {
                    if (zero <= 0)
                        log.WriteString(GTN() + "Pass: zero <= zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (ULong-Var-Const)\r\n");

                    if (one <= 0)
                        log.WriteString(GTN() + "FAILURE: one <= zero (ULong-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (ULong-Var-Const)\r\n");
                }
                log.WriteString("Finished ULong Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                #endregion

                #region Constant to Variable
                log.WriteString("Starting ULong Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                {
                    if (0 != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (ULong-Const-Var)\r\n");

                    if (1 != zero)
                        log.WriteString(GTN() + "Pass: one != zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (ULong-Const-Var)\r\n");
                }
                {
                    if (0 == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (ULong-Const-Var)\r\n");

                    if (1 == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (ULong-Const-Var)\r\n");
                }
                {
                    if (0 > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (ULong-Const-Var)\r\n");

                    if (1 > zero)
                        log.WriteString(GTN() + "Pass: one > zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (ULong-Const-Var)\r\n");
                }
                {
                    if (0 >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (ULong-Const-Var)\r\n");

                    if (1 >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (ULong-Const-Var)\r\n");
                }
                {
                    if (0 < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (ULong-Const-Var)\r\n");

                    if (1 < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (ULong-Const-Var)\r\n");
                }
                {
                    if (0 <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (ULong-Const-Var)\r\n");

                    if (1 <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (ULong-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (ULong-Const-Var)\r\n");
                }
                log.WriteString("Finished ULong Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                #endregion

                Console.WriteLine("Finished Testing ULong Comparisons. " + CurrentTime());
                log.WriteString("Finished Testing ULong Comparisons. " + CurrentTime() + "\r\n\r\n");
            }
            #endregion

            #region Long
            {
                Console.WriteLine("Testing Long Comparisons. " + CurrentTime());
                log.WriteString("Starting Long Comparison Tests. " + CurrentTime() + "\r\n");
                long zero = 0;
                long one = 1;

                #region Variable to Variable
                log.WriteString("Starting Long Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                {
                    if (zero != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Long-Var-Var)\r\n");

                    if (one != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Long-Var-Var)\r\n");
                }
                {
                    if (zero == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Long-Var-Var)\r\n");

                    if (one == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Long-Var-Var)\r\n");
                }
                {
                    if (zero > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Long-Var-Var)\r\n");

                    if (one > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Long-Var-Var)\r\n");
                }
                {
                    if (zero >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Long-Var-Var)\r\n");

                    if (one >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Long-Var-Var)\r\n");
                }
                {
                    if (zero < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Long-Var-Var)\r\n");

                    if (one < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Long-Var-Var)\r\n");
                }
                {
                    if (zero <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Long-Var-Var)\r\n");

                    if (one <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Long-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Long-Var-Var)\r\n");
                }
                log.WriteString("Finished Long Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                #endregion

                #region Variable to Constant
                log.WriteString("Starting Long Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                {
                    if (zero != 0)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Long-Var-Const)\r\n");

                    if (one != 0)
                        log.WriteString(GTN() + "Pass: one != zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Long-Var-Const)\r\n");
                }
                {
                    if (zero == 0)
                        log.WriteString(GTN() + "Pass: zero == zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Long-Var-Const)\r\n");

                    if (one == 0)
                        log.WriteString(GTN() + "FAILURE: one == zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Long-Var-Const)\r\n");
                }
                {
                    if (zero > 0)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Long-Var-Const)\r\n");

                    if (one > 0)
                        log.WriteString(GTN() + "Pass: one > zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Long-Var-Const)\r\n");
                }
                {
                    if (zero >= 0)
                        log.WriteString(GTN() + "Pass: zero >= zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Long-Var-Const)\r\n");

                    if (one >= 0)
                        log.WriteString(GTN() + "Pass: one >= zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Long-Var-Const)\r\n");
                }
                {
                    if (zero < 0)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Long-Var-Const)\r\n");

                    if (one < 0)
                        log.WriteString(GTN() + "FAILURE: one < zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Long-Var-Const)\r\n");
                }
                {
                    if (zero <= 0)
                        log.WriteString(GTN() + "Pass: zero <= zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Long-Var-Const)\r\n");

                    if (one <= 0)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Long-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Long-Var-Const)\r\n");
                }
                log.WriteString("Finished Long Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                #endregion

                #region Constant to Variable
                log.WriteString("Starting Long Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                {
                    if (0 != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Long-Const-Var)\r\n");

                    if (1 != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Long-Const-Var)\r\n");
                }
                {
                    if (0 == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Long-Const-Var)\r\n");

                    if (1 == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Long-Const-Var)\r\n");
                }
                {
                    if (0 > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Long-Const-Var)\r\n");

                    if (1 > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Long-Const-Var)\r\n");
                }
                {
                    if (0 >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Long-Const-Var)\r\n");

                    if (1 >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Long-Const-Var)\r\n");
                }
                {
                    if (0 < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Long-Const-Var)\r\n");

                    if (1 < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Long-Const-Var)\r\n");
                }
                {
                    if (0 <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Long-Const-Var)\r\n");

                    if (1 <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Long-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Long-Const-Var)\r\n");
                }
                log.WriteString("Finished Long Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                #endregion

                Console.WriteLine("Finished Testing Long Comparisons. " + CurrentTime());
                log.WriteString("Finished Testing Long Comparisons. " + CurrentTime() + "\r\n\r\n");
            }
            #endregion

            #region Float
            {
                Console.WriteLine("Testing Float Comparisons. " + CurrentTime());
                log.WriteString("Starting Float Comparison Tests. " + CurrentTime() + "\r\n");
                float zero = 0;
                float one = 1;

                #region Variable to Variable
                log.WriteString("Starting Float Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                {
                    if (zero != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Float-Var-Var)\r\n");

                    if (one != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Float-Var-Var)\r\n");
                }
                {
                    if (zero == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Float-Var-Var)\r\n");

                    if (one == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Float-Var-Var)\r\n");
                }
                {
                    if (zero > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Float-Var-Var)\r\n");

                    if (one > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Float-Var-Var)\r\n");
                }
                {
                    if (zero >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Float-Var-Var)\r\n");

                    if (one >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Float-Var-Var)\r\n");
                }
                {
                    if (zero < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Float-Var-Var)\r\n");

                    if (one < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Float-Var-Var)\r\n");
                }
                {
                    if (zero <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Float-Var-Var)\r\n");

                    if (one <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Float-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Float-Var-Var)\r\n");
                }
                log.WriteString("Finished Float Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                #endregion

                #region Variable to Constant
                log.WriteString("Starting Float Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                {
                    if (zero != 0)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Float-Var-Const)\r\n");

                    if (one != 0)
                        log.WriteString(GTN() + "Pass: one != zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Float-Var-Const)\r\n");
                }
                {
                    if (zero == 0)
                        log.WriteString(GTN() + "Pass: zero == zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Float-Var-Const)\r\n");

                    if (one == 0)
                        log.WriteString(GTN() + "FAILURE: one == zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Float-Var-Const)\r\n");
                }
                {
                    if (zero > 0)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Float-Var-Const)\r\n");

                    if (one > 0)
                        log.WriteString(GTN() + "Pass: one > zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Float-Var-Const)\r\n");
                }
                {
                    if (zero >= 0)
                        log.WriteString(GTN() + "Pass: zero >= zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Float-Var-Const)\r\n");

                    if (one >= 0)
                        log.WriteString(GTN() + "Pass: one >= zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Float-Var-Const)\r\n");
                }
                {
                    if (zero < 0)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Float-Var-Const)\r\n");

                    if (one < 0)
                        log.WriteString(GTN() + "FAILURE: one < zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Float-Var-Const)\r\n");
                }
                {
                    if (zero <= 0)
                        log.WriteString(GTN() + "Pass: zero <= zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Float-Var-Const)\r\n");

                    if (one <= 0)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Float-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Float-Var-Const)\r\n");
                }
                log.WriteString("Finished Float Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                #endregion

                #region Constant to Variable
                log.WriteString("Starting Float Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                {
                    if (0 != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Float-Const-Var)\r\n");

                    if (1 != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Float-Const-Var)\r\n");
                }
                {
                    if (0 == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Float-Const-Var)\r\n");

                    if (1 == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Float-Const-Var)\r\n");
                }
                {
                    if (0 > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Float-Const-Var)\r\n");

                    if (1 > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Float-Const-Var)\r\n");
                }
                {
                    if (0 >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Float-Const-Var)\r\n");

                    if (1 >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Float-Const-Var)\r\n");
                }
                {
                    if (0 < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Float-Const-Var)\r\n");

                    if (1 < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Float-Const-Var)\r\n");
                }
                {
                    if (0 <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Float-Const-Var)\r\n");

                    if (1 <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Float-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Float-Const-Var)\r\n");
                }
                log.WriteString("Finished Float Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                #endregion

                Console.WriteLine("Finished Testing Float Comparisons. " + CurrentTime());
                log.WriteString("Finished Testing Float Comparisons. " + CurrentTime() + "\r\n\r\n");
            }
            #endregion

            #region Double
            {
                Console.WriteLine("Testing Double Comparisons. " + CurrentTime());
                log.WriteString("Starting Double Comparison Tests. " + CurrentTime() + "\r\n");
                double zero = 0;
                double one = 1;

                #region Variable to Variable
                log.WriteString("Starting Double Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                {
                    if (zero != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Double-Var-Var)\r\n");

                    if (one != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Double-Var-Var)\r\n");
                }
                {
                    if (zero == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Double-Var-Var)\r\n");

                    if (one == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Double-Var-Var)\r\n");
                }
                {
                    if (zero > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Double-Var-Var)\r\n");

                    if (one > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Double-Var-Var)\r\n");
                }
                {
                    if (zero >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Double-Var-Var)\r\n");

                    if (one >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Double-Var-Var)\r\n");
                }
                {
                    if (zero < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Double-Var-Var)\r\n");

                    if (one < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Double-Var-Var)\r\n");
                }
                {
                    if (zero <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Double-Var-Var)\r\n");

                    if (one <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Double-Var-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Double-Var-Var)\r\n");
                }
                log.WriteString("Finished Double Comparison Tests (Var to Var). " + CurrentTime() + "\r\n");
                #endregion

                #region Variable to Constant
                log.WriteString("Starting Double Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                {
                    if (zero != 0)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Double-Var-Const)\r\n");

                    if (one != 0)
                        log.WriteString(GTN() + "Pass: one != zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Double-Var-Const)\r\n");
                }
                {
                    if (zero == 0)
                        log.WriteString(GTN() + "Pass: zero == zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Double-Var-Const)\r\n");

                    if (one == 0)
                        log.WriteString(GTN() + "FAILURE: one == zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Double-Var-Const)\r\n");
                }
                {
                    if (zero > 0)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Double-Var-Const)\r\n");

                    if (one > 0)
                        log.WriteString(GTN() + "Pass: one > zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Double-Var-Const)\r\n");
                }
                {
                    if (zero >= 0)
                        log.WriteString(GTN() + "Pass: zero >= zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Double-Var-Const)\r\n");

                    if (one >= 0)
                        log.WriteString(GTN() + "Pass: one >= zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Double-Var-Const)\r\n");
                }
                {
                    if (zero < 0)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Double-Var-Const)\r\n");

                    if (one < 0)
                        log.WriteString(GTN() + "FAILURE: one < zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Double-Var-Const)\r\n");
                }
                {
                    if (zero <= 0)
                        log.WriteString(GTN() + "Pass: zero <= zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Double-Var-Const)\r\n");

                    if (one <= 0)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Double-Var-Const)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Double-Var-Const)\r\n");
                }
                log.WriteString("Finished Double Comparison Tests (Var to Const). " + CurrentTime() + "\r\n");
                #endregion

                #region Constant to Variable
                log.WriteString("Starting Double Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                {
                    if (0 != zero)
                        log.WriteString(GTN() + "FAILURE: zero != zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero != zero (Double-Const-Var)\r\n");

                    if (1 != zero)
                        log.WriteString(GTN() + "Pass: one != zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one != zero (Double-Const-Var)\r\n");
                }
                {
                    if (0 == zero)
                        log.WriteString(GTN() + "Pass: zero == zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero == zero (Double-Const-Var)\r\n");

                    if (1 == zero)
                        log.WriteString(GTN() + "FAILURE: one == zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one == zero (Double-Const-Var)\r\n");
                }
                {
                    if (0 > zero)
                        log.WriteString(GTN() + "FAILURE: zero > zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero > zero (Double-Const-Var)\r\n");

                    if (1 > zero)
                        log.WriteString(GTN() + "Pass: one > zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one > zero (Double-Const-Var)\r\n");
                }
                {
                    if (0 >= zero)
                        log.WriteString(GTN() + "Pass: zero >= zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero >= zero (Double-Const-Var)\r\n");

                    if (1 >= zero)
                        log.WriteString(GTN() + "Pass: one >= zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: one >= zero (Double-Const-Var)\r\n");
                }
                {
                    if (0 < zero)
                        log.WriteString(GTN() + "FAILURE: zero < zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: zero < zero (Double-Const-Var)\r\n");

                    if (1 < zero)
                        log.WriteString(GTN() + "FAILURE: one < zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one < zero (Double-Const-Var)\r\n");
                }
                {
                    if (0 <= zero)
                        log.WriteString(GTN() + "Pass: zero <= zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "FAILURE: zero <= zero (Double-Const-Var)\r\n");

                    if (1 <= zero)
                        log.WriteString(GTN() + "FAILURE: one <= zero (Double-Const-Var)\r\n");
                    else
                        log.WriteString(GTN() + "Pass: one <= zero (Double-Const-Var)\r\n");
                }
                log.WriteString("Finished Double Comparison Tests (Const to Var). " + CurrentTime() + "\r\n");
                #endregion

                Console.WriteLine("Finished Testing Double Comparisons. " + CurrentTime());
                log.WriteString("Finished Testing Double Comparisons. " + CurrentTime() + "\r\n\r\n");
            }
            #endregion

            #endregion

            #region Test the System.Math Plug
            {
                // We have to compare via strings, because of the nature of doubles.
                Console.WriteLine("Starting Tests for System.Math. " + CurrentTime());
                log.WriteLine("Starting Tests for System.Math " + CurrentTime());
                double d = 40.23;
                double d2 = 93.210;
                double d3 = -412.569;
                double d4 = 0.45912;

                if (Math.Abs(d3) == 412.569)
                    log.WriteLine(GTN() + "Pass: System.Math.Abs(double)");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Abs(double) Got: '" + Math.Abs(d3).ToString() + "' expected '412.569'");

                if (Math.Abs(-419.102f) == 419.102f)
                    log.WriteLine(GTN() + "Pass: System.Math.Abs(float)");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Abs(float) Got: '" + Math.Abs(-419.102f).ToString() + "' expected '419.102'");

                if (Math.Acos(d4).ToString() == "1.09379195562398")
                    log.WriteLine(GTN() + "Pass: System.Math.Acos");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Acos Got: '" + Math.Acos(d4).ToString() + "' expected '1.09379195562398'");

                if (Math.Asin(d4).ToString() == "0.477004371170913")
                    log.WriteLine(GTN() + "Pass: System.Math.Asin");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Asin Got: '" + Math.Asin(d4).ToString() + "' expected '0.477004371170913'");

                if (Math.Atan(d4).ToString() == "0.430412185787624")
                    log.WriteLine(GTN() + "Pass: System.Math.Atan");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Atan Got: '" + Math.Atan(d4).ToString() + "' expected '0.430412185787624'");

                if (Math.Atan2(d, d2).ToString() == "0.40745269951331")
                    log.WriteLine(GTN() + "Pass: System.Math.Atan2");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Atan2 Got: '" + Math.Atan2(d, d2).ToString() + "' expected '0.40745269951331'");

                if (Math.Ceiling(d) == 41)
                    log.WriteLine(GTN() + "Pass: System.Math.Ceiling");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Ceiling Got: '" + Math.Ceiling(d).ToString() + "' expected '41'");

                if (Math.Cos(d).ToString() == "-0.819244231260389")
                    log.WriteLine(GTN() + "Pass: System.Math.Cos");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Cos Got: '" + Math.Cos(d).ToString() + "' expected '-0.819244231260389'");

                if (Math.Cosh(d).ToString() == "1.48127949589163E+17")
                    log.WriteLine(GTN() + "Pass: System.Math.Cosh");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Cosh Got: '" + Math.Cosh(d).ToString() + "' expected '1.48127949589163E+17'");

                if (Math.Exp(d).ToString() == "2.96255899178325E+17")
                    log.WriteLine(GTN() + "Pass: System.Math.Exp");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Exp Got: '" + Math.Exp(d).ToString() + "' expected '2.96255899178325E+17'");

                if (Math.Floor(d) == 40)
                    log.WriteLine(GTN() + "Pass: System.Math.Floor");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Floor Got: '" + Math.Floor(d).ToString() + "' expected '40'");

                if (Math.Log(d).ToString() == "3.6946129859617")
                    log.WriteLine(GTN() + "Pass: System.Math.Log(double)");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Log(double) Got: '" + Math.Log(d).ToString() + "' expected '3.6946129859617'");

                if (Math.Log(d, d2).ToString() == "0.814714687928411")
                    log.WriteLine(GTN() + "Pass: System.Math.Log(double, double)");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Log(double, double) Got: '" + Math.Log(d, d2).ToString() + "' expected '0.814714687928411'");

                if (Math.Log10(d).ToString() == "1.60455003257126")
                    log.WriteLine(GTN() + "Pass: System.Math.Log10");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Log10 Got: '" + Math.Log10(d).ToString() + "' expected '1.60455003257126'");

                if (Math.Pow(d, d2).ToString() == "3.63168804144286E+149")
                    log.WriteLine(GTN() + "Pass: System.Math.Pow");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Pow Got: '" + Math.Pow(d, d2).ToString() + "' expected '3.63168804144286E+149'");

                if (Math.Round(d) == 40)
                    log.WriteLine(GTN() + "Pass: System.Math.Round");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Round Got: '" + Math.Round(d).ToString() + "' expected '40'");

                if (Math.Sinh(d).ToString() == "1.48127949589163E+17")
                    log.WriteLine(GTN() + "Pass: System.Math.Sinh");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Sinh Got: '" + Math.Sinh(d).ToString() + "' expected '1.48127949589163E+17'");

                if (Math.Sqrt(d).ToString() == "6.34271235355979")
                    log.WriteLine(GTN() + "Pass: System.Math.Sqrt");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Sqrt Got: '" + Math.Sqrt(d).ToString() + "' expected '6.34271235355979'");

                if (Math.Tan(d).ToString() == "-0.699968013575042")
                    log.WriteLine(GTN() + "Pass: System.Math.Tan");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Tan Got: '" + Math.Tan(d).ToString() + "' expected '-0.699968013575042'");

                if (Math.Tanh(d) == 1)
                    log.WriteLine(GTN() + "Pass: System.Math.Tanh");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Tanh Got: '" + Math.Tanh(d).ToString() + "' expected '1'");

                if (Math.Truncate(d) == 40)
                    log.WriteLine(GTN() + "Pass: System.Math.Truncate");
                else
                    log.WriteLine(GTN() + "FAILURE: System.Math.Truncate Got: '" + Math.Truncate(d).ToString() + "' expected '40'");

                Console.WriteLine("Finished Testing System.Math. " + CurrentTime());
                log.WriteLine("Finished Testing System.Math " + CurrentTime() + "\r\n");
            }
            #endregion

            log.WriteString("Testing Finished. " + CurrentTime());
            Console.WriteLine("Tests Finished. " + CurrentTime());
            while (true)
            {

            }
        }

        private static uint LastTestNum = 0;

        /// <summary>
        /// Gets the test number, including '.' and ' '.
        /// </summary>
        /// <returns></returns>
        private static string GTN()
        {
            return (++LastTestNum).ToString() + ". ";
        }

        private static string CurrentTime()
        {
            string sf = "(";
            switch (RTC.DayOfTheWeek)
            {
                case 1:
                    sf += "Sunday ";
                    break;
                case 2:
                    sf += "Monday ";
                    break;
                case 3:
                    sf += "Tuesday ";
                    break;
                case 4:
                    sf += "Wednesday ";
                    break;
                case 5:
                    sf += "Thursday ";
                    break;
                case 6:
                    sf += "Friday ";
                    break;
                case 7:
                    sf += "Saturday ";
                    break;
            }
            switch (RTC.Month)
            {
                case 1:
                    sf += "January ";
                    break;
                case 2:
                    sf += "February ";
                    break;
                case 3:
                    sf += "March ";
                    break;
                case 4:
                    sf += "April ";
                    break;
                case 5:
                    sf += "May ";
                    break;
                case 6:
                    sf += "June ";
                    break;
                case 7:
                    sf += "July ";
                    break;
                case 8:
                    sf += "August ";
                    break;
                case 9:
                    sf += "September ";
                    break;
                case 10:
                    sf += "October ";
                    break;
                case 11:
                    sf += "November ";
                    break;
                case 12:
                    sf += "December ";
                    break;
            }
            sf += RTC.DayOfTheMonth.ToString() + ", ";
            sf += RTC.Century.ToString() + RTC.Year.ToString() + " ";
            sf += RTC.Hour.ToString() + ":";
            sf += RTC.Minute.ToString() + ":";
            sf += RTC.Second.ToString() + ")";
            return sf;
        }
    }
}

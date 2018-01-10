#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.TestRunner;
using Cosmos.Compiler.Tests.Bcl.Helper;
using Cosmos.Debug.Kernel;
using Cosmos.System.ExtendedASCII;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class EncodingTest
    {
        static Debugger mDebugger = new Debugger("System", "Enconding Test");

        static byte[] UTF8EnglishText   = new byte[] { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0x69, 0x73, 0x20,
                                                       0x77, 0x6F, 0x6E, 0x64, 0x65, 0x72, 0x66, 0x75, 0x6C, 0x21 };
        static byte[] UTF8ItalianText   = new byte[] { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0xC3, 0xA8, 0x20,
                                                       0x66, 0x61, 0x6E, 0x74, 0x61, 0x73, 0x74, 0x69, 0x63, 0x6F,
                                                       0x21 };
        static byte[] UTF8SpanishText   = new byte[] { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0x65, 0x73, 0x20,
                                                       0x67, 0x65, 0x6E, 0x69, 0x61, 0x6C, 0x21 };
        static byte[] UTF8GermanicText  = new byte[] { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0x69, 0x73, 0x74,
                                                       0x20, 0x67, 0x72, 0x6F, 0xC3, 0x9F, 0x61, 0x72, 0x74, 0x69,
                                                       0x67, 0x21 };
        static byte[] UTF8GreekText     = new byte[] { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0xCE, 0xB5, 0xCE,
                                                       0xAF, 0xCE, 0xBD, 0xCE, 0xB1, 0xCE, 0xB9, 0x20, 0xCF, 0x85,
                                                       0xCF, 0x80, 0xCE, 0xAD, 0xCF, 0x81, 0xCE, 0xBF, 0xCF, 0x87,
                                                       0xCE, 0xBF, 0xCF, 0x82, 0x21 };
        static byte[] UTF8JapanaseText  = new byte[] { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0xE7, 0xB4, 0xA0,
                                                       0xE6, 0x99, 0xB4, 0xE3, 0x82, 0x89, 0xE3, 0x81, 0x97, 0xE3,
                                                       0x81, 0x84, 0xE3, 0x81, 0xA7, 0xE3, 0x81, 0x99, 0x21 };
        static byte[] UTF8GothicText    = new byte[] { 0xF0, 0x90, 0x8D, 0x88 };
        static byte[] CP437EnglishText = new byte[] { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0x69, 0x73, 0x20,
                                                      0x77, 0x6F, 0x6E, 0x64, 0x65, 0x72, 0x66, 0x75, 0x6C, 0x21 };
        static byte[] CP437ItalianText = new byte[] { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0x8A, 0x20, 0x66,
                                                      0x61, 0x6E, 0x74, 0x61, 0x73, 0x74, 0x69, 0x63, 0x6F, 0x21 };

        static byte[] CP437SpanishText = new byte[] { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0x65, 0x73, 0x20,
                                                      0x67, 0x65, 0x6E, 0x69, 0x61, 0x6C, 0x21 };

        static byte[] CP437GermanicText = new byte[] { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0x69, 0x73, 0x74, 0x20,
                                                       0x67, 0x72, 0x6F, 0xE1, 0x61, 0x72, 0x74, 0x69, 0x67, 0x21 };
        static byte[] CP437GreekText = new byte[]    { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0xEE, 0x3F, 0x3F, 0xE0,
                                                       0x3F, 0x20, 0x3F, 0xE3, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x21 };
        static byte[] CP437JapanaseText = new byte[] { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0x3F, 0x3F, 0x3F, 0x3F,
                                                       0x3F, 0x3F, 0x3F, 0x21 };
        static byte[] CP437GothicText = new byte[]   { 0x3F, 0x3F };
        static byte[] CP858EnglishText = CP437EnglishText;
        static byte[] CP858ItalianText = CP437ItalianText;
        static byte[] CP858SpanishText = CP437SpanishText;
        static byte[] CP858GermanicText = CP437GermanicText;
        /* CP858 has no Greek characters they are all replaced by '?' (0x3F) */
        static byte[] CP858GreekText = new byte[]   { 0x43, 0x6F, 0x73, 0x6D, 0x6F, 0x73, 0x20, 0x3F, 0x3F, 0x3F, 0x3F,
                                                      0x3F, 0x20, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x21 };
        static byte[] CP858JapanaseText = CP437JapanaseText;
        static byte[] CP858GothicText = CP437GothicText;

        static void TestGetBytes(Encoding xEncoding, string text, byte[] expectedResult, string desc)
        {
            byte[] result;

            result = xEncoding.GetBytes(text);
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), $"{xEncoding.BodyName} Encoding of {desc} text failed byte arrays different");
        }


        static void TestGetString(Encoding xEncoding, byte[] bytes, string expectedText, string desc)
        {
            string text;

            text = xEncoding.GetString(bytes);
            Assert.IsTrue((text == expectedText), $"{xEncoding.BodyName} Decoding of {desc} text failed strings different");
        }

        static void TestUTF8()
        {
            //Encoding xEncoding = new UTF8Encoding();
            Encoding xEncoding = Encoding.UTF8;
            mDebugger.SendInternal($"Starting Test {xEncoding.BodyName} Encoding / Decoding");

            Assert.IsTrue(xEncoding.BodyName == "UTF-8", "UTF8 BodyName failed not 'UTF-8");
            Assert.IsTrue(xEncoding.IsSingleByte == false, "UTF8.IsSingleByte failed: it returns true");

            TestGetBytes(xEncoding, "Cosmos is wonderful!", UTF8EnglishText, "English");
            TestGetBytes(xEncoding, "Cosmos è fantastico!", UTF8ItalianText, "Italian");
            TestGetBytes(xEncoding, "Cosmos es genial!", UTF8SpanishText, "Spanish");
            TestGetBytes(xEncoding, "Cosmos ist großartig!", UTF8GermanicText, "Germanic");
            TestGetBytes(xEncoding, "Cosmos είναι υπέροχος!", UTF8GreekText, "Greek");
            TestGetBytes(xEncoding, "Cosmos 素晴らしいです!", UTF8JapanaseText, "Japanese");
            TestGetBytes(xEncoding, "𐍈", UTF8GothicText, "Gothic");

            TestGetString(xEncoding, UTF8EnglishText, "Cosmos is wonderful!", "English");
            TestGetString(xEncoding, UTF8ItalianText, "Cosmos è fantastico!", "Italian");
            TestGetString(xEncoding, UTF8SpanishText, "Cosmos es genial!", "Spanish");
            TestGetString(xEncoding, UTF8GermanicText, "Cosmos ist großartig!", "Germanic");
            /* CP437 replaces not representable characters with '?' */
            TestGetString(xEncoding, UTF8GreekText, "Cosmos είναι υπέροχος!", "Greek");
            TestGetString(xEncoding, UTF8JapanaseText, "Cosmos 素晴らしいです!", "Japanese");
            TestGetString(xEncoding, UTF8GothicText, "𐍈", "Gothic");

            mDebugger.SendInternal($"Finished Test {xEncoding.BodyName} Encoding / Decoding");
        }

        static void TestCP437()
        {
            Encoding xEncoding = Encoding.GetEncoding(437);

            mDebugger.SendInternal($"Starting Test {xEncoding.BodyName} Encoding / Decoding");

            Assert.IsTrue(xEncoding.BodyName == "IBM437", "437 BodyName failed not 'IBM437");
            Assert.IsTrue(xEncoding.IsSingleByte == true, "437.IsSingleByte failed: it returns false");

            TestGetBytes(xEncoding, "Cosmos is wonderful!", CP437EnglishText, "English");
            TestGetBytes(xEncoding, "Cosmos è fantastico!", CP437ItalianText, "Italian");
            TestGetBytes(xEncoding, "Cosmos es genial!", CP437SpanishText, "Spanish");
            TestGetBytes(xEncoding, "Cosmos ist großartig!", CP437GermanicText, "Germanic");
            /* 
             * From this point on a lot of characters will be replaced by 0x3F ('?') because
             * cannot really represented on CP437
             */
            TestGetBytes(xEncoding, "Cosmos είναι υπέροχος!", CP437GreekText, "Greek");
            TestGetBytes(xEncoding, "Cosmos 素晴らしいです!", CP437JapanaseText, "Japanese");
            TestGetBytes(xEncoding, "𐍈", CP437GothicText, "Gothic");

            TestGetString(xEncoding, CP437EnglishText, "Cosmos is wonderful!", "English");
            TestGetString(xEncoding, CP437ItalianText, "Cosmos è fantastico!", "Italian");
            TestGetString(xEncoding, CP437SpanishText, "Cosmos es genial!", "Spanish");
            TestGetString(xEncoding, CP437GermanicText, "Cosmos ist großartig!", "Germanic");
            /* CP437 replaces not representable characters with '?' */
            TestGetString(xEncoding, CP437GreekText, "Cosmos ε??α? ?π??????!", "Greek");
            TestGetString(xEncoding, CP437JapanaseText, "Cosmos ???????!", "Japanese");
            TestGetString(xEncoding, CP437GothicText, "??", "Gothic");

            mDebugger.SendInternal("Finished Test {xEncoding.BodyName} Encoding / Decoding");
        }

        static void TestCP858()
        {
            Encoding xEncoding = Encoding.GetEncoding(858);

            mDebugger.SendInternal($"Starting Test {xEncoding.BodyName} Encoding / Decoding");

            Assert.IsTrue(xEncoding.BodyName == "IBM00858", "858 BodyName failed not 'IBM00858");
            Assert.IsTrue(xEncoding.IsSingleByte == true, "858.IsSingleByte failed: it returns false");

            TestGetBytes(xEncoding, "Cosmos is wonderful!", CP858EnglishText, "English");
            TestGetBytes(xEncoding, "Cosmos è fantastico!", CP858ItalianText, "Italian");
            TestGetBytes(xEncoding, "Cosmos es genial!", CP858SpanishText, "Spanish");
            TestGetBytes(xEncoding, "Cosmos ist großartig!", CP858GermanicText, "Germanic");
            /* 
             * From this point on a lot of characters will be replaced by 0x3F ('?') because
             * cannot really represented on CP858
             */
            TestGetBytes(xEncoding, "Cosmos είναι υπέροχος!", CP858GreekText, "Greek");
            TestGetBytes(xEncoding, "Cosmos 素晴らしいです!", CP858JapanaseText, "Japanese");
            TestGetBytes(xEncoding, "𐍈", CP858GothicText, "Gothic");

            TestGetString(xEncoding, CP858EnglishText, "Cosmos is wonderful!", "English");
            TestGetString(xEncoding, CP858ItalianText, "Cosmos è fantastico!", "Italian");
            TestGetString(xEncoding, CP858SpanishText, "Cosmos es genial!", "Spanish");
            TestGetString(xEncoding, CP858GermanicText, "Cosmos ist großartig!", "Germanic");
            /* CP858 replaces not representable characters with '?' */
            TestGetString(xEncoding, CP858GreekText, "Cosmos ????? ????????!", "Greek");
            TestGetString(xEncoding, CP858JapanaseText, "Cosmos ???????!", "Japanese");
            TestGetString(xEncoding, CP858GothicText, "??", "Gothic");

            mDebugger.SendInternal("Finished Test CP858 Encoding / Decoding");
        }

#if true
        public static void Execute()
        {
            /*
             * Net Core has removed all the legacy codepages from Encoding, only Unicode and ASCII are supported
             * the correct way to add them is to create an Encoding Provider.
             * Microsoft has created a CodePageEncodingProvider for this but it is too much complex to use it in
             * Cosmos now, but we should use surely this in future.
             * As a replacement for it I have created CosmosEncodingProvider that is more simple (but less efficient).
             */
            Encoding.RegisterProvider(CosmosEncodingProvider.Instance);

            TestUTF8();
            // TestAscii();

            TestCP437();
            TestCP858();
        }
#endif

#if false
        public static void Execute()
        {
            /*
             * Net Core has removed all the legacy codepages from Encoding, only Unicode and ASCII are supported
             * the correct way to add them is to create an Encoding Provider.
             * Microsoft has created a CodePageEncodingProvider for this but it is too much complex to use it in
             * Cosmos now, but we should use surely this in future.
             * As a replacement for it I have created CosmosEncodingProvider that is more simple (but less efficient).
             */
            Encoding.RegisterProvider(CosmosEncodingProvider.Instance);

            Encoding xEncoding = new UTF8Encoding();
            string text;
            byte[] result;
            byte[] expectedResult;

            Assert.IsTrue(!xEncoding.IsSingleByte, "IsSingleByte failed return true for UTF8");

#if true
            mDebugger.SendInternal($"Starting Test {xEncoding.BodyName} Encoding / Decoding");

            text = "Cosmos is wonderful!";
            result = xEncoding.GetBytes(text);
            expectedResult = UTF8EnglishText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of English text failed byte arrays different");

            text = "Cosmos è fantastico!";
            result = xEncoding.GetBytes(text);
            expectedResult = UTF8ItalianText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Italian text failed byte arrays different");

            text = "Cosmos es genial!";
            result = xEncoding.GetBytes(text);
            expectedResult = UTF8SpanishText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Spanish text failed byte arrays different");

            text = "Cosmos ist großartig!";
            result = xEncoding.GetBytes(text);
            expectedResult = UTF8GermanicText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Germanic text failed byte arrays different");

            text = "Cosmos είναι υπέροχος!";
            result = xEncoding.GetBytes(text);
            expectedResult = UTF8GreekText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Greek text failed byte arrays different");

            text = "Cosmos 素晴らしいです!";
            result = xEncoding.GetBytes(text);
            expectedResult = UTF8JapanaseText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Japanese text failed byte arrays different");

            /* This the only case on which UFT-16 must use a surrugate pairs... it is a Gothic letter go figure! */
            text = "𐍈";
            result = xEncoding.GetBytes(text);
            expectedResult = UTF8GothicText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Gothic text failed byte arrays different");

            /* Now we do the other way: we have a UFT8 byte array and try to convert it in a UFT16 String */
            string expectedText;

            text = xEncoding.GetString(UTF8EnglishText);
            expectedText = "Cosmos is wonderful!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of English text failed strings different");

            text = xEncoding.GetString(UTF8ItalianText);
            expectedText = "Cosmos è fantastico!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Italian text failed strings different");

            text = xEncoding.GetString(UTF8SpanishText);
            expectedText = "Cosmos es genial!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Spanish text failed strings different");

            text = xEncoding.GetString(UTF8GermanicText);
            expectedText = "Cosmos ist großartig!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Germanic text failed strings different");

            text = xEncoding.GetString(UTF8GreekText);
            expectedText = "Cosmos είναι υπέροχος!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Greek text failed strings different");

            text = xEncoding.GetString(UTF8JapanaseText);
            expectedText = "Cosmos 素晴らしいです!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Japanese text failed strings different");

            text = xEncoding.GetString(UTF8GothicText);
            expectedText = "𐍈";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Gothic text failed strings different");

            xEncoding = Encoding.ASCII;

            Assert.IsTrue(xEncoding.IsSingleByte, "IsSingleByte failed return false for ASCII");

            text = "Cosmos is wonderful!";
            result = xEncoding.GetBytes(text);
            expectedResult = UTF8EnglishText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "Ascii Encoding of English text failed byte arrays different");
#endif

            xEncoding = Encoding.GetEncoding(437);
            var yEncoding = Encoding.GetEncoding("IBM437");

            Assert.IsTrue(xEncoding.CodePage == yEncoding.CodePage, "437 and 'IBM437' not the same Encoding");

            Assert.IsTrue(xEncoding.IsSingleByte, "IsSingleByte failed return false for CP437");

            text = "Cosmos is wonderful!";
            result = xEncoding.GetBytes(text);
            expectedResult = CP437EnglishText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "CP437 Encoding of English text failed byte arrays different");

            text = "Cosmos è fantastico!";
            result = xEncoding.GetBytes(text);
            expectedResult = CP437ItalianText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "CP437 Encoding of Italian text failed byte arrays different");

            text = "Cosmos es genial!";
            result = xEncoding.GetBytes(text);
            expectedResult = CP437SpanishText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "CP437 Encoding of Spanish text failed byte arrays different");

            text = "Cosmos ist großartig!";
            result = xEncoding.GetBytes(text);
            expectedResult = CP437GermanicText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "CP437 Encoding of Germanic text failed byte arrays different");

            /* 
             * From this point on a lot of characters will be replaced by 0x3F ('?') because
             * cannot be really represented on CP437
             */
            text = "Cosmos είναι υπέροχος!";
            result = xEncoding.GetBytes(text);
            expectedResult = CP437GreekText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "CP437 Encoding of Greek text failed byte arrays different");

            text = "Cosmos 素晴らしいです!";
            result = xEncoding.GetBytes(text);
            expectedResult = CP437JapanaseText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "CP437 Encoding of Japanese text failed byte arrays different");

            text = "𐍈";
            result = xEncoding.GetBytes(text);
            expectedResult = CP437GothicText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "CP437 Encoding of Gothic text failed byte arrays different");

            //string expectedText;
            /* Now we do the other way: we have a CP437 byte array and try to convert it in a UFT16 String */
            text = xEncoding.GetString(CP437EnglishText);
            expectedText = "Cosmos is wonderful!";
            Assert.IsTrue((text == expectedText), "CP437 Decoding of English text failed strings different");

            text = xEncoding.GetString(CP437ItalianText);
            expectedText = "Cosmos è fantastico!";
            Assert.IsTrue((text == expectedText), "CP437 Decoding of Italian text failed strings different");

            text = xEncoding.GetString(CP437SpanishText);
            expectedText = "Cosmos es genial!";
            Assert.IsTrue((text == expectedText), "CP437 Decoding of Spanish text failed strings different");

            text = xEncoding.GetString(CP437GermanicText);
            expectedText = "Cosmos ist großartig!";
            Assert.IsTrue((text == expectedText), "CP437 Decoding of Germanic text failed strings different");

            /* CP437 replaces not representable characters with '?' */
            text = xEncoding.GetString(CP437GreekText);
            expectedText = "Cosmos ε??α? ?π??????!";
            Assert.IsTrue((text == expectedText), "CP437 Decoding of Greek text failed strings different");

            text = xEncoding.GetString(CP437JapanaseText);
            expectedText = "Cosmos ???????!";
            Assert.IsTrue((text == expectedText), "CP437 Decoding of Japanese text failed strings different");

            text = xEncoding.GetString(CP437GothicText);
            expectedText = "??";
            Assert.IsTrue((text == expectedText), "CP437 Decoding of Gothic text failed strings different");
        }
#endif
    }
}

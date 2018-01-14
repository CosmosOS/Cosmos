using System.Text;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System.Text
{
    class EncodingTest
    {
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

        public static void Execute()
        {
            //CosmosUTF8Encoding Encoder = new CosmosUTF8Encoding();
            //Encoder Encoder = new UTF8Encoding().GetEncoder();
            Encoding Encoder = new UTF8Encoding();
            string text;
            byte[] result;
            byte[] expectedResult;

            text = "Cosmos is wonderful!";
            result = Encoder.GetBytes(text);
            expectedResult = UTF8EnglishText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of English text failed byte arrays different");

            text = "Cosmos è fantastico!";
            result = Encoder.GetBytes(text);
            expectedResult = UTF8ItalianText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Italian text failed byte arrays different");

            text = "Cosmos es genial!";
            result = Encoder.GetBytes(text);
            expectedResult = UTF8SpanishText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Spanish text failed byte arrays different");

            text = "Cosmos ist großartig!";
            result = Encoder.GetBytes(text);
            expectedResult = UTF8GermanicText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Germanic text failed byte arrays different");

            text = "Cosmos είναι υπέροχος!";
            result = Encoder.GetBytes(text);
            expectedResult = UTF8GreekText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Greek text failed byte arrays different");

            text = "Cosmos 素晴らしいです!";
            result = Encoder.GetBytes(text);
            expectedResult = UTF8JapanaseText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Japanese text failed byte arrays different");

            /* This the only case on which UFT-16 must use a surrugate pairs... it is a Gothic letter go figure! */
            text = "𐍈";
            result = Encoder.GetBytes(text);
            expectedResult = UTF8GothicText;
            Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "UTF8 Encoding of Gothic text failed byte arrays different");

            /* Now we do the other way: we have a UFT8 byte array and try to convert it in a UFT16 String */
            string expectedText;

            text = Encoder.GetString(UTF8EnglishText);
            expectedText = "Cosmos is wonderful!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of English text failed strings different");

            text = Encoder.GetString(UTF8ItalianText);
            expectedText = "Cosmos è fantastico!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Italian text failed strings different");

            text = Encoder.GetString(UTF8SpanishText);
            expectedText = "Cosmos es genial!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Spanish text failed strings different");

            text = Encoder.GetString(UTF8GermanicText);
            expectedText = "Cosmos ist großartig!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Germanic text failed strings different");

            text = Encoder.GetString(UTF8GreekText);
            expectedText = "Cosmos είναι υπέροχος!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Greek text failed strings different");

            text = Encoder.GetString(UTF8JapanaseText);
            expectedText = "Cosmos 素晴らしいです!";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Japanese text failed strings different");

            text = Encoder.GetString(UTF8GothicText);
            expectedText = "𐍈";
            Assert.IsTrue((text == expectedText), "UTF8 Decoding of Gothic text failed strings different");

            /* But this not work is searching '437' in some native Windows tables, we need plugs for this sadly! */
            //Encoder = Encoding.GetEncoding(437);
            //text = "àèìòù";
            //result = Encoder.GetBytes(text);
            //expectedResult = new byte[] { 0x85, 0x8A, 0x8D, 0x95, 0x97 };
            //Assert.IsTrue(EqualityHelper.ByteArrayAreEquals(result, expectedResult), "CP437 Encoding of accents text failed byte arrays different");

        }
    }
}

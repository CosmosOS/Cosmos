using System;
using CosmosHttp.Client;
using Cosmos.TestRunner.Core;
using Cosmos.TestRunner.Full;

namespace CosmosHttp.Tests
{
    public class HttpFunctionalTests
    {
        private const string TestDomain = "mojox.org"; 
        private const string TestIp = "185.159.153.85";

        public static void RunTests()
        {
            Console.WriteLine("Running CosmosHttp Functional Tests...");
            TestGetRequest();
            TestPutRequest();
            Console.WriteLine("Tests Completed.");
        }

        private static void TestGetRequest()
        {
            try
            {
                using (var request = new HttpRequest())
                {
                    request.Domain = TestDomain;
                    request.IP = TestIp;
                    request.Path = "/COSMOS/Test/Http/get?test=123";
                    request.Send();

                    var response = request.Response;
                    if (response != null && response.Content.Contains("test=123"))
                    {
                        Console.WriteLine("GET Request: Work");
                    }
                    else
                    {
                        Console.WriteLine("GET Request: No Work");
                        if (response != null) Console.WriteLine($"Response: {response.Content}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GET Request: No Work - Exception: {ex.Message}");
            }
        }

        private static void TestPutRequest()
        {
            try
            {
                using (var request = new HttpRequest())
                {
                    request.Method = "PUT";
                    request.Domain = TestDomain;
                    request.IP = TestIp;
                    request.Path = "/COSMOS/Test/Http/put";
                    string testData = "test=putdata";
                    request.Send(testData);

                    var response = request.Response;
                    if (response != null && response.Content.Contains("test=putdata"))
                    {
                        Console.WriteLine("PUT Request: Work");
                    }
                    else
                    {
                        Console.WriteLine("PUT Request: No Work");
                        if (response != null) Console.WriteLine($"Response: {response.Content}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PUT Request: No Work - Exception: {ex.Message}");
            }
        }

        public static void Main(string[] args)
        {
            RunTests();
        }
    }
}

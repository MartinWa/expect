using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;

namespace expect
{
    internal static class Program
    {
        private const string Test = @"http://api.comaround.com:10401/legacy";
        private const string Live = @"http://api.comaround.com/legacy";

        private static void Main(string[] args)
        {
            var url = Test;
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "test":
                        url = Test;
                        break;
                    case "live":
                        url = Live;
                        break;
                    default:
                        url = args[0];
                        break;
                }
            }
            Console.WriteLine("Calling search on URL {0}", url);
            var request = CreateWebRequest(url);
            var soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
                <Envelope xmlns=""http://www.w3.org/2003/05/soap-envelope"">
                    <Body>
                        <Search xmlns=""http://zero.comaround.com"">
                            <loginName>maria</loginName>
                            <passWord>ap3lsin</passWord>
                            <groupID>0</groupID>
                            <searchString>word</searchString>
                        </Search>
                    </Body>
                </Envelope >");
            using (var stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            var sw = Stopwatch.StartNew();
            try
            {
                using (var response = request.GetResponse())
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        Console.WriteLine("Error");
                        return;
                    }
                    using (var rd = new StreamReader(responseStream))
                    {
                        var soapResult = rd.ReadToEnd();
                        Console.WriteLine(soapResult);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            sw.Stop();
            Console.WriteLine("Request took {0} milliseconds", sw.ElapsedMilliseconds);
        }

        private static HttpWebRequest CreateWebRequest(string url)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

    }
}

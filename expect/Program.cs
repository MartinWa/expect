using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;

namespace expect
{
    internal static class Program
    {
        private const string Test = @"http://api.comaround.com:10400/legacy";
        private const string Live = @"http://api.comaround.com/legacy";
        const string Xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                                <Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                                    <Body>
                                        <Search xmlns=""http://zero.comaround.com"">
                                            <loginName>maria</loginName>
                                            <passWord></passWord>
                                            <groupID>-1</groupID>
                                            <searchString>word</searchString>
                                        </Search>
                                    </Body>
                                </Envelope>";

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
            Console.WriteLine("Calling search on URL \"{0}\"{1}", url, args.Length > 1 ? " without expect header" : " with expect header");
            for (var i = 0; i < 10; i++)
            {
                var request = CreateWebRequest(url);
                if (args.Length > 1)
                    request.ServicePoint.Expect100Continue = false;
                var soapEnvelopeXml = new XmlDocument();
                soapEnvelopeXml.LoadXml(Xml);
                using (var stream = request.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                var sw = Stopwatch.StartNew();

                var size = 0;


                var status = 0;
                try
                {
                    using (var response = request.GetResponse())
                    {
                        status = (int)((HttpWebResponse) response).StatusCode;
                        var responseStream = response.GetResponseStream();
                        if (responseStream == null)
                        {
                            Console.WriteLine("Error");
                            return;
                        }
                        using (var rd = new StreamReader(responseStream))
                        {
                            var soapResult = rd.ReadToEnd();
                            size = soapResult.Length;
                        }
                    }
                }

                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
                sw.Stop();
                Console.WriteLine("Request {0}: {1} took {2} milliseconds, size: {3}", i+1, status, sw.ElapsedMilliseconds, size);
            }
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

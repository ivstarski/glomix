namespace glomix.console
{
    using System;
    using System.IO;
    using System.Net;

    public class Downloader
    {
        public int BufferLength { get; set; } = 2048*2;

        public void Start(string url, string fileName)
        {
            try
            {
                using( var webStream = WebRequest.Create(url).GetResponse().GetResponseStream() )
                using( var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write) )
                {
                    int len;
                    var buffer = new byte[BufferLength];
                    while( (len = webStream.Read(buffer, 0, buffer.Length)) > 0 )
                    {
                        fileStream.Write(buffer, 0, len);
                        fileStream.Flush();
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (WebException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotNetMessages
{
    class Program
    {
        private static Message[] todos = new Message[5];

        private static int count = 0;

        const string hostname = "127.0.0.1";

        const int port = 5000;

        static void Main(string[] args)
        {
            HttpListener http = new HttpListener();
            http.Prefixes.Add($"http://{hostname}:{port}/");
            CancellationToken cancellationToken = new CancellationToken();
            Task t = new Task(() =>
            {
                while (true)
                {
                    HttpListenerContext context = http.GetContext();
                    Task.Run(() => Web(context.Request, context.Response));
                }
            }, cancellationToken);
            http.Start();
            t.Start();
            Console.WriteLine($"http://{hostname}:{port}/");
            while(true);
        }

        private static void Web(HttpListenerRequest req, HttpListenerResponse res)
        {
            switch (req.RawUrl)
            {
                case "/":
                    res.StatusCode = 200;
                    res.ContentType = "text/html";
                    res.AddHeader("Charset", "UTF-8");
                    BinaryReader reader = new BinaryReader(new FileStream("index.html", 
                        FileMode.Open, FileAccess.Read));
                    res.OutputStream.Write(reader.ReadBytes((int)reader.BaseStream.Length), 
                        0, (int)reader.BaseStream.Length);
                    res.Close();
                    break;                    
                case "/messages":
                    if (req.HttpMethod == "GET")
                    {
                        res.StatusCode = 200;
                        res.ContentType = "application/json";
                        Message[] temp_todo = new Message[count];
                        Array.Copy(todos, temp_todo, count);
                        res.OutputStream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp_todo)));
                        res.Close();
                        break;
                    }
                    byte[] buffer = new byte[1024];
                    for(int i = 0; ; i++)
                    {
                        int t = req.InputStream.ReadByte();
                        if (t == -1)
                        {
                            Array.Resize(ref buffer, i);
                            break;
                        }
                        buffer[i] = (byte)t;
                    }
                    Message temp = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(buffer));
                    if(count < 5)
                        count++;
                    for (int i = todos.Length - 1; i > 0; i--)
                        todos[i] = todos[i - 1];
                    todos[0] = temp;
                    res.StatusCode = 200;
                    res.Close();
                    break;
                default:
                    res.StatusCode = 404;
                    res.Close();
                    break;
            }
        }

        private static void SendPage(HttpListenerResponse res, string path)
        {
            
        }

        class Message
        {
            public string message = "";
        }
    }
}

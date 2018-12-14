using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections;


namespace clightning_plugin_dotnet
{
    class Program
    {
        static void Main(string[] args)
        {
            string greeting = "";
            Dictionary<string, Func<string, string>> commands = new Dictionary<string, Func<string, string>>();
            commands.Add("init", (empty) => { return "\"ok\""; });
            commands.Add("hello", (name) =>
            {
                string who = name.Equals("") ? "unknown person or bot" : name;
                greeting = $"\"Meaningless clightning plugin presented to {who}\"";
                return greeting;
            });
            commands.Add("getmanifest", (empty) =>
            {
                return "{\"options\":[{\"name\": \"greeting\",\"type\": \"string\",\"default\":\"greeting\",\"description\": \"What name should I call you?\"},],\"rpcmethods\":[{\"name\":\"hello\",\"description\": \"Returns a personalized greeting for name\",},{\"name\": \"fail\",\"description\": \"Always returns a failure for testing\"}]}";
            });

            var stdin = Console.OpenStandardInput();
            var stdout = Console.OpenStandardOutput();
            var reader = new System.IO.StreamReader(stdin);
            var writer = new System.IO.StreamWriter(stdout);
            while (true)
            {
                string line;
                StringBuilder builder = new StringBuilder();
                while ((line = reader.ReadLine()) != null)
                {
                    builder.Append(line);
                    try {
                        var j = JsonConvert.DeserializeObject<JObject>(builder.ToString());
                        var method = j.GetValue("method").ToString();
                        if(commands.ContainsKey(method)) {
                            var paramz = j.GetValue("params");
                            var arr = new List<string>{""};
                            try {
                                // this is all I care about for now, but ignore errors if other type
                                // the init message sends the following object
                                //  "params": {"options": {      "greeting": "greeting"    },     
                                //  "configuration": {    "lightning-dir": "/home/richard/.lightning",       "rpc-file":"lightning-rpc"    }  }
                                arr = paramz.ToObject<List<string>>(); 
                            } catch(Exception){}
                            var str = arr.Count > 0  ? arr[0] : "";
                            writer.Write($"{{\"jsonrpc\":\"2.0\",\"id\":\"{j.GetValue("id").ToString()}\",\"result\":{commands.GetValueOrDefault(method)(str)}}}\n");
                            builder.Clear();
                            writer.Flush();  
                            reader.DiscardBufferedData(); 
                        }
                    } catch(Exception) {}
                    System.Threading.Thread.Sleep(50);
                }
                System.Threading.Thread.Sleep(50);
            }
        }
    }
}

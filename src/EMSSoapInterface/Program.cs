using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hprose.RPC;
//using Hprose.RPC.Plugins.Limiter;
using System.Threading.Tasks;
using System.Net;
using System.Xml;

namespace SoapProxy
{
    class Program
    {
        static void Main(string[] args)
        {

            HttpListener server = new HttpListener();
            server.Prefixes.Add(Properties.Settings.Default.url);
            server.Start();
            var service = new Service();
            service.AddInstanceMethods(new MyService()).Bind(server);

            Console.Read();
            server.Stop();

        }
        
        class MyService
        {
            public int Sum(int x, int y)
            {
                return x + y;
            }

            public dynamic Call(Dictionary<string , object> input)
            {
                try
                {
                    string url = input["url"].ToString();
                    string servicename = input["service_name"].ToString();
                    string methodname = input["methodname"].ToString();
                    object[] p;
                    List<object> para = (List<object>)input["para"];
                    p = para.ToArray();
                    var result =  WebServiceUtility.WSHelper.Call(url, servicename, methodname, p);
                    return new 
                    {
                        status = true,
                        message = "执行成功",
                        data = new { value = result,
                        jsonvalue =  ToJson(result.ToString())
                        }

                    };
                }
                catch(Exception Ex)
                {
                    return  new
                    {
                        status = false,
                        message =Ex.Message,
                        data = ""

                    };
                }
            }


            string ToJson(string str)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(str);
                    var json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);
                    return json;
                    //dynamic result =  Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    //return result.First();

                }
                catch
                {
                    return "" ;
                }
                
            }
        }

        public interface IMyService
        {
            Task<int> Sum(int x, int y);
            Task<dynamic> Call(string url,string service_name, string methodname, object[] para);
        }
    }
}

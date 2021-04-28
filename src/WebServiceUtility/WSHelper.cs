using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml.Serialization;


namespace WebServiceUtility
{
    public class WSHelper
    {
        /// <summary>
        /// call WebService
        /// </summary>
        /// <param name="url"></param>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="param">parameter array</param>
        public static object Call(string url ,string serviceName, string methodName , object[] param)
        {
            WebClient web = new WebClient();
            Stream stream = web.OpenRead(url);
            ServiceDescription description = ServiceDescription.Read(stream);
            ServiceDescriptionImporter importer = new ServiceDescriptionImporter
            {
                ProtocolName = "Soap", 
                Style = ServiceDescriptionImportStyle.Client, 
                CodeGenerationOptions = CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateNewAsync
            };

            importer.AddServiceDescription(description, null, null); 
            CodeNamespace nmspace = new CodeNamespace(); 
            nmspace.Name = "TWebService";
            CodeCompileUnit unit = new CodeCompileUnit();
            unit.Namespaces.Add(nmspace);

            ServiceDescriptionImportWarnings warning = importer.Import(nmspace, unit);
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            CompilerParameters parameter = new CompilerParameters();
            parameter.GenerateExecutable = false;
            parameter.GenerateInMemory = true;
            parameter.ReferencedAssemblies.Add("System.dll");
            parameter.ReferencedAssemblies.Add("System.XML.dll");
            parameter.ReferencedAssemblies.Add("System.Web.Services.dll");
            parameter.ReferencedAssemblies.Add("System.Data.dll");
            CompilerResults result = provider.CompileAssemblyFromDom(parameter, unit);
            if (!result.Errors.HasErrors)
            {
                Assembly asm = result.CompiledAssembly;
                Type t = asm.GetType($"TWebService.{serviceName}"); 
                object o = Activator.CreateInstance(t);
                MethodInfo method = t.GetMethod(methodName);
                var resultMessage = method.Invoke(o, param);
                return resultMessage;
            }
            else
            {
                return null;
            }


        }
    }
}

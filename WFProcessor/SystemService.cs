using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace WFProcessor
{
    public class Processor : ServiceBase
    {
        private static string GetCurrentLocation()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }

        public static void Install(string serviceName, string serviceDescription, string description, string [] extraArgs)
        {
            string arguments = string.Format(" -s {0}", string.Join(" ", extraArgs));
            string fullpath = Path.Combine(Processor.GetCurrentLocation(), arguments);

            Process serviceExists = Process.Start("sc",
                string.Format("create {0} binPath= \"{1}\" DisplayName= \"{2}\"",
                              serviceName, fullpath, serviceDescription));
            serviceExists.WaitForExit();

            Process setDescription = Process.Start("sc",
                string.Format("description {0} \"{1}\"", serviceName, description));
            setDescription.WaitForExit();

            Console.WriteLine("Successfuly installed service as " + serviceName);
        }

        public static void Remove(string serviceName)
        {
            Process serviceDelete = Process.Start("sc", string.Format("delete {0}", serviceName));
            serviceDelete.WaitForExit();

            Console.WriteLine("Successfuly deleted service " + serviceName);
        }

        public static void Start(Action<WFProcessorArguments> start, WFProcessorArguments args)
        {
            ServiceBase.Run( new ServiceBase[] { new Processor(start, args) } );
        }

				public Processor(Action<WFProcessorArguments> process, WFProcessorArguments args)
        {
            this.process = process;
            this.args = args;
        }

        private Action<WFProcessorArguments> process;
        private WFProcessorArguments args;

        protected override void OnStart(string[] args)
        {
            this.process(this.args);
            base.OnStart(args);
        }
    }
}

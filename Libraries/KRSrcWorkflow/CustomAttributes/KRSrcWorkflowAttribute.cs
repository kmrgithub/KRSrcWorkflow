using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace KRSrcWorkflow.CustomAttributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class KRSrcWorkflowAttribute : HandlerAttribute
	{
		private List<string> MethodsVisited { get; set; }

        private class KRSrcWorkflowAttributeHandler : ICallHandler
		{
			public int Order { get; set; }
			private KRSrcWorkflowAttribute Parent { get; set; }

			public KRSrcWorkflowAttributeHandler()
				: this(null)
			{
			}

            public KRSrcWorkflowAttributeHandler(KRSrcWorkflowAttribute parent)
			{
				this.Parent = parent;
			}

			public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
			{
				if (input.MethodBase.Name.StartsWith("set_"))
				{
					MethodInfo method = input.Target.GetType().GetMethod("SetProperty");
					MethodInfo generic = method.MakeGenericMethod(input.Inputs[0].GetType());
					generic.Invoke(input.Target, new object[] { input.MethodBase.Name.Substring(4), input.Inputs[0] });
				}
				else if (input.MethodBase.Name.StartsWith("get_") && !this.Parent.MethodsVisited.Contains(input.MethodBase.Name))
				{
					this.Parent.MethodsVisited.Add(input.MethodBase.Name);
					MethodInfo method = input.Target.GetType().GetMethod("GetProperty", new Type[] { typeof(string), typeof(string) });
					MethodInfo generic = method.MakeGenericMethod(((MethodInfo)input.MethodBase).ReturnParameter.ParameterType);
                    object value = generic.Invoke(input.Target, new object[] { input.MethodBase.Name.Substring(4), ((KRSrcWorkflowAttribute)(input.Target.GetType().GetProperty(input.MethodBase.Name.Substring(4)).GetCustomAttributes(typeof(KRSrcWorkflowAttribute), false))[0]).ProcessorDataType });
					if (value != null)
					{
						PropertyInfo pi = input.Target.GetType().GetProperty(input.MethodBase.Name.Substring(4));
						pi.SetValue(input.Target, value, new object[] { });
					}
				}

				return getNext()(input, getNext);
			}
		}

		public string ProcessorDataType { get; set; }

		public override ICallHandler CreateHandler(IUnityContainer container)
		{
            return new KRSrcWorkflowAttributeHandler(this);
		}

        public KRSrcWorkflowAttribute()
		{
			this.ProcessorDataType = string.Empty;
			this.MethodsVisited = new List<string>();
		}
	}

}

using Rye.AspectFlare;
using Rye.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rye.Attributes
{
    /// <summary>
    /// 防止接口重复提交的特性，通常用于表单提交或接口调用，确保同一请求在短时间内不会被重复处理。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ResubmitCheckAttribute : InterceptAttribute, ICallingInterceptor, ICalledInterceptor
    {
        private string _key = string.Empty;
        public void Calling(CallingInterceptContext callingInterceptorContext)
        {
            _key = GetKey(callingInterceptorContext.Parameters);
            ResubmitCheck.CheckSubmiting(_key);
        }

        public void Called(CalledInterceptContext calledInterceptorContext)
        {
            ResubmitCheck.Release(_key);
        }

        private static string GetKey(object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                var sb = new StringBuilder();
                sb.Append(':');
                foreach (var param in parameters)
                {
                    sb.Append(param?.ToJsonString() + "_");
                }
                sb.Length--;
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}

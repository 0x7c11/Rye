using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Rye.Util
{
    public class ResubmitCheck
    {
        private static readonly ConcurrentDictionary<string, object> _RequestLock = new();

        public static void CheckSubmiting(string requestKey)
        {
            if (!_RequestLock.TryAdd(requestKey, 0))
            {
                throw new InvalidOperationException("请求正处于提交中，请稍后");
            }
        }


        public static void Release(string requestKey)
        {
            _RequestLock.TryRemove(requestKey, out var _);
        }
    }
}

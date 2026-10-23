using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

using Rye.AspectFlare;
using Rye.Cache;
using Rye.Cache.Store;

using System;
using System.ComponentModel;
using System.Text;

namespace Rye
{
    /// <summary>
    /// 提供方法数据缓存功能，使用IMemoryStore
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CacheAttribute : InterceptAttribute, ICallingInterceptor, ICalledInterceptor
    {
        private static readonly Lazy<IAppCacheStore> _cacheLazy = new Lazy<IAppCacheStore>(() => App.GetService<IAppCacheStore>());

        private static IAppCacheStore Cache { get => _cacheLazy.Value; }

        public CacheAttribute(CacheScheme scheme)
        {
            Scheme = scheme;
        }

        public CacheAttribute(CacheScheme scheme, string cacheKey)
        {
            Check.NotNullOrEmpty(cacheKey, nameof(cacheKey));
            Scheme = scheme;
            CacheKey = cacheKey;
        }

        public CacheAttribute(CacheScheme scheme, string cacheKey, int cacheSeconds)
        {
            Check.NotNullOrEmpty(cacheKey, nameof(cacheKey));
            Scheme = scheme;
            CacheKey = cacheKey;
            CacheSeconds = cacheSeconds;
        }

        public CacheAttribute(CacheScheme scheme, string cacheKey, int cacheSeconds, bool isSliding)
        {
            Check.NotNullOrEmpty(cacheKey, nameof(cacheKey));
            Scheme = scheme;
            CacheKey = cacheKey;
            CacheSeconds = cacheSeconds;
            IsSliding = isSliding;
        }

        public CacheAttribute(CacheScheme scheme, int cacheSeconds)
        {
            Scheme = scheme;
            CacheSeconds = cacheSeconds;
        }

        public CacheAttribute(CacheScheme scheme, int cacheSeconds, bool isSliding)
        {
            Scheme = scheme;
            CacheSeconds = cacheSeconds;
            IsSliding = isSliding;
        }
        /// <summary>
        /// 缓存方案
        /// </summary>
        public CacheScheme Scheme { get; set; }
        /// <summary>
        /// 缓存Key
        /// </summary>
        public string CacheKey { get; set; }
        /// <summary>
        /// 缓存时间
        /// </summary>
        public int CacheSeconds { get; set; } = 30;
        /// <summary>
        /// 是否是滑动过期
        /// </summary>
        public bool IsSliding { get; set; } = false;

        private string _key = string.Empty;
        public void Calling(CallingInterceptContext callingInterceptorContext)
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                var method = callingInterceptorContext.MethodName;
                if (method.EndsWith("Async"))
                    method = method[0..^5];
                CacheKey = callingInterceptorContext.Owner.GetType().FullName + "." + method;
            }
            _key = GetKey(CacheKey, callingInterceptorContext.Parameters);
            var result = Cache.Get<string>(Scheme, _key);
            if (result != null)
            {
                callingInterceptorContext.HasResult = true;
                callingInterceptorContext.Result = callingInterceptorContext.ReturnType.GetUnNullableType().IsPrimitive ?
                    TypeDescriptor.GetConverter(callingInterceptorContext.ReturnType).ConvertFromInvariantString(result) :
                    (callingInterceptorContext.ReturnType == typeof(string) ?
                    result :
                    result.ToObject(callingInterceptorContext.ReturnType));
            }
        }

        public void Called(CalledInterceptContext calledInterceptorContext)
        {
            if (!Cache.Exists(Scheme, _key))
            {
                var result = calledInterceptorContext.Result ?? calledInterceptorContext.ReturnValue;
                if (result != null)
                {
                    CacheOptionEntry entry = new CacheOptionEntry();
                    entry.Key = _key;
                    if (IsSliding)
                    {
                        entry.SetSlidingExpiration(TimeSpan.FromSeconds(CacheSeconds));
                    }
                    else
                    {
                        entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(CacheSeconds));
                    }
                    Cache.Set(Scheme, entry, result.ToJsonString());
                }
            }
        }

        private static string GetKey(string cacheKey, object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                var sb = new StringBuilder(cacheKey);
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
                return cacheKey;
            }
        }
    }
}

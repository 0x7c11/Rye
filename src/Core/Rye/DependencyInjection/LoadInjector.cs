using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rye.DependencyInjection
{
    internal class LoadInjector
    {
        internal static void Load(IServiceCollection serviceCollection)
        {
            var typeDict = new Dictionary<Type, InjectionAttribute>();
            var classList = new List<Type>();
            var injectionAttrType = typeof(InjectionAttribute);
            foreach (var type in App.ScanTypes)
            {
                var attr = type.GetCustomAttribute<InjectionAttribute>(true);
                if (attr != null)
                {
                    typeDict.Add(type, attr);
                }
                if (!type.IsInterface && !type.IsAbstract && !type.IsEnum && !type.IsPrimitive)
                {
                    classList.Add(type);
                }
            }

            foreach (var keyvalue in typeDict)
            {
                var type = keyvalue.Key;
                var attr = keyvalue.Value;
                if (type.IsInterface || type.IsAbstract)
                {
                    foreach (var classType in classList.Where(d => type.IsAssignableFrom(d)).ToArray())
                    {
                        var classAttr = classType.GetCustomAttribute<InjectionAttribute>() ?? attr;
                        AddServices(serviceCollection,
                            type,
                            classType,
                            classAttr);
                    }
                }
                else
                {
                    AddServices(serviceCollection, type, type, attr);
                }
            }
        }

        private static void AddServices(IServiceCollection serviceCollection, Type serviceType, Type implementationType, InjectionAttribute injectionAttribute)
        {
            if (injectionAttribute.ServiceTypes == null || injectionAttribute.ServiceTypes.Length <= 0)
            {
                AddService(serviceCollection, serviceType, implementationType, injectionAttribute.Lifetime, injectionAttribute.Policy);
            }
            else
            {
                foreach (var type in injectionAttribute.ServiceTypes)
                {
                    AddService(serviceCollection, type, implementationType, injectionAttribute.Lifetime, injectionAttribute.Policy);
                }
            }
        }

        private static void AddService(IServiceCollection serviceCollection, Type serviceType, Type implementationType, ServiceLifetime lifetime, InjectionPolicy policy)
        {
            var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);

            // 自注册（服务类型 == 实现类型）在 Append 策略下不能用 TryAddEnumerable：
            // 它会对这个 descriptor 抛 ArgumentException（Implementation type cannot be 'X'
            // because it is indistinguishable from other services registered for 'X'），
            // 使服务类上的 [Injection] 直接让宿主启动失败。
            // 自注册只可能有一个实现，退化为 TryAdd —— 与 Append「已存在则不重复登记」的语义一致；
            // Skip / Replace 对自注册本来就是安全的（TryAdd / Replace 不做该检查），保持原样。
            if (serviceType == implementationType && policy == InjectionPolicy.Append)
            {
                serviceCollection.TryAdd(descriptor);
                return;
            }

            switch (policy)
            {
                case InjectionPolicy.Append:
                    serviceCollection.TryAddEnumerable(descriptor);
                    break;
                case InjectionPolicy.Skip:
                    serviceCollection.TryAdd(descriptor);
                    break;
                case InjectionPolicy.Replace:
                    serviceCollection.Replace(descriptor);
                    break;
            }
        }
    }
}


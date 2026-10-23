using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SqlSugar;

using System;

namespace Rye.SqlSugarCore.Extensions
{
    /// <summary>
    /// SqlSugar 服务注册扩展
    /// </summary>
    public static class SqlSugarServiceCollectionExtensions
    {
        /// <summary>
        /// 以 Scoped 生命周期注册数据库上下文与 SqlSugar 客户端（以无参构造函数创建上下文，连接串从配置 ConnectionStrings:Default/Reading 读取）
        /// </summary>
        /// <typeparam name="TContext">数据库上下文类型，继承 <see cref="RyeDbContext"/></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSqlSugarDbContext<TContext>(this IServiceCollection services)
            where TContext : RyeDbContext, new()
        {
            return services.AddSqlSugarDbContext<TContext>(_ => new TContext());
        }

        /// <summary>
        /// 以 Scoped 生命周期注册数据库上下文与 SqlSugar 客户端
        /// </summary>
        /// <typeparam name="TContext">数据库上下文类型，继承 <see cref="RyeDbContext"/></typeparam>
        /// <param name="services"></param>
        /// <param name="factory">上下文构造工厂</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlSugarDbContext<TContext>(this IServiceCollection services, Func<IServiceProvider, TContext> factory)
            where TContext : RyeDbContext
        {
            services.TryAddScoped(factory);
            services.TryAddScoped(sp => sp.GetRequiredService<TContext>().Db);
            services.TryAddScoped<ISqlSugarClient>(sp => sp.GetRequiredService<TContext>().Db);
            return services;
        }

        /// <summary>
        /// 添加 SqlSugar 模块（默认以无参构造函数创建上下文）
        /// </summary>
        /// <typeparam name="TContext">数据库上下文类型，继承 <see cref="RyeDbContext"/></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSqlSugarModule<TContext>(this IServiceCollection services)
            where TContext : RyeDbContext, new()
        {
            return services.AddModule(new SqlSugarModule<TContext>(_ => new TContext()));
        }

        /// <summary>
        /// 添加 SqlSugar 模块
        /// </summary>
        /// <typeparam name="TContext">数据库上下文类型，继承 <see cref="RyeDbContext"/></typeparam>
        /// <param name="services"></param>
        /// <param name="factory">上下文构造工厂</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlSugarModule<TContext>(this IServiceCollection services, Func<IServiceProvider, TContext> factory)
            where TContext : RyeDbContext
        {
            return services.AddModule(new SqlSugarModule<TContext>(factory));
        }
    }
}

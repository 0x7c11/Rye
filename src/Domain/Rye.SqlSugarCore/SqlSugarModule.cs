using Microsoft.Extensions.DependencyInjection;

using Rye.Enums;
using Rye.Module;
using Rye.SqlSugarCore.Extensions;

using System;

namespace Rye.SqlSugarCore
{
    /// <summary>
    /// 提供对 SqlSugar 数据库访问的功能模块
    /// <para>以 Scoped 生命周期注册 <typeparamref name="TContext"/> 及其 <see cref="SqlSugarClientExtend"/>，每次请求独享一个 SqlSugarClient</para>
    /// </summary>
    /// <typeparam name="TContext">数据库上下文类型，继承 <see cref="RyeDbContext"/></typeparam>
    public class SqlSugarModule<TContext> : StartupModule where TContext : RyeDbContext
    {
        public override ModuleLevel Level => ModuleLevel.FrameWork;

        public override uint Order => 3;

        private readonly Func<IServiceProvider, TContext> _factory;

        public SqlSugarModule(Func<IServiceProvider, TContext> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public override void ConfigueServices(IServiceCollection services)
        {
            services.AddSqlSugarDbContext(_factory);
        }
    }
}

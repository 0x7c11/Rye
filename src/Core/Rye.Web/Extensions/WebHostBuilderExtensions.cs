using Microsoft.AspNetCore.Builder;

namespace Rye
{
    public static class WebHostBuilderExtensions
    {
        public static WebApplicationBuilder ConfigureApp(this WebApplicationBuilder builder)
        {
            // 存储环境对象
            App.HostEnvironment = builder.Environment;
            WebApp.WebHostEnvironment = builder.Environment;

            // 加载配置
            App.AddConfigureFiles(builder.Configuration, builder.Environment);

            builder.Services.AddRye();

            return builder;
        }
    }
}


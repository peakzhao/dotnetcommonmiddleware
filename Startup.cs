using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Hangfire.Redis;
using StackExchange.Redis;
using Hangfire;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

namespace myApi
{
    public class Startup
    {
        public static ConnectionMultiplexer Redis;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Redis = ConnectionMultiplexer.Connect("127.0.0.1:6379,password=123456,allowAdmin=true");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //添加jwt验证：
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options=> {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,//是否验证Issuer
                        ValidateAudience = true,//是否验证Audience
                        ValidateLifetime = true,//是否验证失效时间
                        ValidateIssuerSigningKey = true,//是否验证SecurityKey
                        ValidAudience = "yourdomain.com",//Audience
                        ValidIssuer = "yourdomain.com",//Issuer，这两项和前面签发jwt的设置一致
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecurityKey"]))//拿到SecurityKey
                    };
                    options.RequireHttpsMetadata = true;
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //注册Swagger生成器，定义一个和多个Swagger 文档
            services.AddSwaggerGen
            (c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = "ZHF-API", Version = "v1" });
                }
            );
            services.AddHangfire(config => config.UseRedisStorage(Redis));
       
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();//注意添加这一句，启用验证

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            //启用中间件服务生成Swagger作为JSON终结点
            app.UseSwagger(d=>{});
            //启用中间件服务对swagger-ui，指定Swagger JSON终结点
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZHF-API V1");
            });

            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions());
            //2分钟后执行
            BackgroundJob.Schedule(() => Console.WriteLine($"ASP.NET Core await LineZero{DateTime.Now}"), TimeSpan.FromMinutes(2));
            //仅执行一次
            BackgroundJob.Enqueue(() => Console.WriteLine($"ASP.NET Core One Start LineZero{DateTime.Now}"));
            //每分钟执行一次
            RecurringJob.AddOrUpdate(() => Console.WriteLine($"ASP.NET Core LineZero"), Cron.Minutely());
            
        }
    }
}

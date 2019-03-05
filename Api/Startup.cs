﻿using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Api
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                // задаём политику CORS, чтобы наше клиентское приложение могло отправить запрос на сервер API
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("http://localhost:5003")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // облегчённая версия MVC Core без движка Razor, DataAnnotations и подобного, сопоставима с Asp.NET 4.5 WebApi
            services.AddMvcCore()
                // добавляем авторизацию, благодаря этому будут работать атрибуты Authorize
                .AddAuthorization(options =>
                    // политики позволяют не работать с Roles magic strings, содержащими перечисления ролей через запятую
                        options.AddPolicy("AdminsOnly", policyUser =>
                        {
                            policyUser.RequireClaim("role", "admin");
                        })
                )
                // добавляется AddMVC, не добавляется AddMvcCore, мы же хотим получать результат в JSON 
                .AddJsonFormatters();


            services.AddAuthentication(o =>
            {
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 

            }).AddJwtBearer(o =>
            {
                o.Authority = "http://localhost:5000";
                o.Audience = "api1";
                o.RequireHttpsMetadata = false;
                //o.TokenValidationParameters = new TokenValidationParameters()
                //{
                //    NameClaimType = "name",
                //    RoleClaimType = "role"
                //};
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);

            // добавляем middleware для CORS 
            app.UseCors("default");

            app.UseAuthentication();

            /*
            // добавляем middleware для заполнения объекта пользователя из OpenId  Connect JWT-токенов
            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                // наш IdentityServer
                Authority = "http://localhost:5000",
                // говорим, что нам не требуется HTTPS при общении с IdentityServer, должно быть true на продуктиве
                // https://docs.microsoft.com/en-us/aspnet/core/api/microsoft.aspnetcore.builder.openidconnectoptions
                RequireHttpsMetadata = false,

                // это значение будет сравниваться со значением поля aud внутри access_token JWT
                ApiName = "api1",

                // можно так написать, если мы хотим разделить наш api на отдельные scopes и всё же сохранить валидацию scope
                // AllowedScopes = { "api1.read", "api1.write" }

                // читать JWT-токен и добавлять claims оттуда в HttpContext.User даже если не используется атрибут Authorize со схемоЙ, соответствующей токену
                AutomaticAuthenticate = true,
                // назначаем этот middleware как используемый для формирования authentication challenge
                AutomaticChallenge = true,

                // требуется для [Authorize], для IdentityServerAuthenticationOptions - значение по умолчанию
                RoleClaimType = "role",
            });
            */

            app.UseMvc();
        }
    }
}

﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Serialization;
using RDD.Application;
using RDD.Application.Controllers;
using RDD.Domain;
using RDD.Domain.Models;
using RDD.Infra.Storage;
using RDD.Web.Serialization;

namespace RDD.Web.Helpers
{
    public static class RddServiceCollectionExtensions
    {
        /// <summary>
        /// Register minimum RDD dependecies. Set up RDD services via Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services"></param>
        public static void AddRdd(this IServiceCollection services)
        {
            // register base services
            services.AddScoped(typeof(ApiHelper<,>))
                .AddSingleton<IContractResolver, CamelCasePropertyNamesContractResolver>()
                .AddScoped<IEntitySerializer, EntitySerializer>()
                .AddScoped(typeof(IAppController<,>), typeof(AppController<,>))
                .AddScoped(typeof(IRestCollection<,>), typeof(RestCollection<,>))
                .AddScoped(typeof(IRepository<>), typeof(Repository<>))

                .TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        /// <summary>
        /// Register RDD middleware in the pipeline request
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRdd(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HttpStatusCodeExceptionMiddleware>();
        }
    }
     
}
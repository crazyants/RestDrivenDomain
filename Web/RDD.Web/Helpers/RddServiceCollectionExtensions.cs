﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RDD.Application;
using RDD.Application.Controllers;
using RDD.Domain;
using RDD.Domain.Models;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;
using RDD.Infra;
using RDD.Infra.Storage;
using RDD.Web.Serialization;
using System;
using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RDD.Domain.Models.Querying;
using RDD.Web.Middleware;
using RDD.Web.Querying;

namespace RDD.Web.Helpers
{
    public static class RddServiceCollectionExtensions
    {
        /// <summary>
        /// Register minimum RDD dependecies. Set up RDD services via Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// IRightsService and IRDDSerialization are missing for this setup to be ready
        /// </summary>
        public static IServiceCollection AddRddCore(this IServiceCollection services)
        {
            // register base services
            services.TryAddScoped<IStorageService, EFStorageService>();
            services.TryAddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));
            services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.TryAddScoped<IPatcherProvider, PatcherProvider>();
            services.TryAddScoped(typeof(IReadOnlyRestCollection<,>), typeof(ReadOnlyRestCollection<,>));
            services.TryAddScoped(typeof(IInstanciator<>), typeof(DefaultInstanciator<>));
            services.TryAddScoped(typeof(IRestCollection<,>), typeof(RestCollection<,>));
            services.TryAddScoped(typeof(IReadOnlyAppController<,>), typeof(ReadOnlyAppController<,>));
            services.TryAddScoped(typeof(IAppController<,>), typeof(AppController<,>));

            services.AddHttpContextAccessor();
            services.TryAddScoped<IHttpContextHelper, HttpContextHelper>();

            services.TryAddScoped<IRightExpressionsHelper, DefaultRightExpressionsHelper>();

            services.TryAddSingleton<IWebFilterParser,WebFilterParser> ();
            services.TryAddSingleton<IPagingParser, PagingParser> ();
            services.TryAddSingleton<IHeaderParser, HeaderParser> ();
            services.TryAddSingleton<IOrberByParser, OrberByParser> ();
            services.TryAddSingleton<QueryParsers> ();
            services.TryAddSingleton<QueryTokens>();

            services.TryAddScoped<IQueryFactory, QueryFactory>();
            services.TryAddScoped<QueryMetadata>();

            services.TryAddSingleton(typeof(ICandidateFactory<,>), typeof(CandidateFactory<,>));

            services.AddOptions<RddOptions>();

            return services;
        }

        /// <summary>
        /// Adds custom right management to filter entities
        /// </summary>
        public static IServiceCollection AddRddRights<TCombinationsHolder, TPrincipal>(this IServiceCollection services)
            where TCombinationsHolder : class, ICombinationsHolder
            where TPrincipal : class, IPrincipal
        {
            services.AddScoped<IRightExpressionsHelper, RightExpressionsHelper>();
            services.AddScoped<IPrincipal, TPrincipal>();
            services.AddScoped<ICombinationsHolder, TCombinationsHolder>();
            return services;
        }

        /// <summary>
        /// Adds Rdd specific serialisation (fields + metadata)
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRddSerialization(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, RddSerializationSetup>());
            services.TryAddScoped<IUrlProvider, UrlProvider>();
            services.Configure<MvcJsonOptions>(jsonOptions =>
            {
                jsonOptions.SerializerSettings.ContractResolver = new SelectiveContractResolver();
            });
            return services;
        }

        public static IServiceCollection AddRdd(this IServiceCollection services)
        {
            return services.AddRddCore()
                .AddRddSerialization();
        }

        /// <summary>
        /// Register RDD middleware in the pipeline request
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRdd(this IApplicationBuilder app)
        {
            return app
                .UseMiddleware<QueryContextMiddleware>()
                .UseMiddleware<HttpStatusCodeExceptionMiddleware>();
        }
    }

    public class RddSerializationSetup : IConfigureOptions<MvcOptions>
    {
        private readonly IOptions<MvcJsonOptions> _jsonOptions;
        private readonly ArrayPool<char> _charPool;

        public RddSerializationSetup(IOptions<MvcJsonOptions> jsonOptions, ArrayPool<char> charPool)
        {
            _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
            _charPool = charPool ?? throw new ArgumentNullException(nameof(charPool));
        }

        public void Configure(MvcOptions options)
        {
            options.OutputFormatters.Add(new MetaSelectiveJsonOutputFormatter(_jsonOptions.Value.SerializerSettings, _charPool));
            options.OutputFormatters.Add(new SelectiveJsonOutputFormatter(_jsonOptions.Value.SerializerSettings, _charPool));
        }
    }

}
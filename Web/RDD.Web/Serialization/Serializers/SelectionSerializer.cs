﻿using RDD.Domain;
using RDD.Domain.Json;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Providers;
using System.Collections.Generic;

namespace RDD.Web.Serialization.Serializers
{
    public class SelectionSerializer : Serializer
    {
        public SelectionSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override IJsonElement ToJson(object entity, SerializationOption options)
        {
            return ToSerializableObject(entity as ISelection, options);
        }

        protected IJsonElement ToSerializableObject(ISelection collection, SerializationOption options)
        {
            var items = collection.GetItems();
            return new JsonObject
            {
                Content = new Dictionary<string, IJsonElement>
                {
                   { "items", SerializerProvider.GetSerializer(items).ToJson(items, options) }
                }
            };
        }
    }
}
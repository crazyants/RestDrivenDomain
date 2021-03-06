﻿using NExtends.Primitives.Strings;
using RDD.Domain.Json;
using System;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace RDD.Domain.Patchers
{
    class DictionaryPatcher : IPatcher
	{
        protected IPatcherProvider Provider { get; set; }

		public DictionaryPatcher(IPatcherProvider provider)
		{
            Provider = provider;
		}

        object IPatcher.InitialValue(PropertyInfo property, object patchedObject)
        {
            return property.GetValue(patchedObject);
        }

        object IPatcher.PatchValue(object patchedObject, Type expectedType, IJsonElement json)
		{
			return PatchValue(patchedObject, expectedType, json as JsonObject);
		}

		public object PatchValue(object patchedObject, Type expectedType, JsonObject json)
		{
			if (json == null)
				return null;

			return PatchValue(patchedObject as IDictionary, expectedType, json);
		}

		protected object PatchValue(IDictionary patchedObject, Type expectedType, JsonObject json)
		{
			if (patchedObject == null)
			{
				patchedObject = (IDictionary)Activator.CreateInstance(expectedType);
			}

			// Si la classe hérite de Dictionary on prend le base type
			if (!expectedType.IsGenericType)
			{
				expectedType = expectedType.BaseType;
			}

			var genericArguments = expectedType.GetGenericArguments();
			foreach (var kvp in json.Content)
			{
				var key = kvp.Key.ChangeType(genericArguments[0], CultureInfo.InvariantCulture);
				var patcher = Provider.GetPatcher(genericArguments[1], kvp.Value);
				var value = patcher.PatchValue(null, genericArguments[1], kvp.Value);
				patchedObject[key] = value;
			}

			return patchedObject;
		}
	}
}
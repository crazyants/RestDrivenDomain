﻿using NExtends.Primitives.Types;
using RDD.Domain.Exceptions;
using RDD.Domain.Json;
using System;
using System.Collections;

namespace RDD.Domain.Patchers
{
    public class PatcherProvider : IPatcherProvider
    {
		public virtual IPatcher GetPatcher(Type expectedType, IJsonElement json)
		{
			if (json is JsonArray)
			{
				if (typeof(IEntityBase).IsAssignableFrom(expectedType.GetEnumerableOrArrayElementType())) { return new EntitiesPatcher(this); }
				return new EnumerablePatcher(this);
			}

			if (typeof(IEntityBase).IsAssignableFrom(expectedType)) { throw new ForbiddenException("It is not permitted to patch a property of type derived from IEntityBase"); }
			if (expectedType.IsSubclassOfInterface(typeof(IDictionary))) { return new DictionaryPatcher(this); }

			if (expectedType == typeof(string) || expectedType.IsValueType || expectedType.GetNullableType().IsValueType) { return new ValuePatcher(); }
			if (expectedType == typeof(object))
			{
				if (json is JsonValue) { return new ValuePatcher(); }
				if (json is JsonObject) { return new DynamicPatcher(); }
			}

			return new ObjectPatcher(this);
		}
	}
}
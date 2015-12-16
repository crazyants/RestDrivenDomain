﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IPrimaryKey
	{
		object GetId();
		void SetId(object id);
	}

	public interface IPrimaryKey<TKey> : IPrimaryKey, IIdable<TKey> { }
}

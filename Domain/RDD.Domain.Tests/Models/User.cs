﻿using RDD.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests.Models
{
	public class User : EntityBase<User, int>
	{
		public override int Id { get; set; }
		public override string Name { get; set; }
		public MailAddress Mail { get; set; }
		public Uri TwitterUri { get; set; }
		public decimal Salary { get; set; }
	}
}

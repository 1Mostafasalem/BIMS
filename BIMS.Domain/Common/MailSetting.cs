﻿namespace BIMS.Domain.Common
{
	public class MailSetting
	{
		public string Email { get; set; } = null!;
		public string DisplayName { get; set; } = null!;
		public string Password { get; set; } = null!;
		public string Host { get; set; } = null!;
		public int Port { get; set; }

	}
}

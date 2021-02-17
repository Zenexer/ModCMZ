using System;
using System.Linq;


namespace ModCMZ
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			/*
			AppDomainSetup setup = new AppDomainSetup()
			{
				ApplicationName = "ModCMZ.Injection",
				ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
			};

			AppDomain domain = AppDomain.CreateDomain(setup.ApplicationName, null, setup);
			SecondaryDomain secondary = (SecondaryDomain)domain.CreateInstanceAndUnwrap(typeof(Program).Assembly.FullName, typeof(SecondaryDomain).FullName);
			secondary.Run(args);
			*/
			new SecondaryDomain().Run(args);
		}
	}
}


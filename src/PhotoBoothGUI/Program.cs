#region PhotoBooth - MIT - (c) 2014 Patrick Bronneberg
/*
  PhotoBooth - an application to control a DIY photobooth

  Permission is hereby granted, free of charge, to any person obtaining
  a copy of this software and associated documentation files (the
  "Software"), to deal in the Software without restriction, including
  without limitation the rights to use, copy, modify, merge, publish,
  distribute, sublicense, and/or sell copies of the Software, and to
  permit persons to whom the Software is furnished to do so, subject to
  the following conditions:

  The above copyright notice and this permission notice shall be
  included in all copies or substantial portions of the Software.
  
  Copyright 2014 Patrick Bronneberg
*/
#endregion

using com.prodg.photobooth.domain;
using Gtk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Serilog;

namespace com.prodg.photobooth
{
	static class Program
	{
		static IServiceProvider? value;
		
		[STAThread]
		public static async Task Main (string[] args)
		{			
			var services = new ServiceCollection();
			
			ConfigureServices(services);
			services.AddSingleton<MainWindow>();
			value = services.BuildServiceProvider();
		
   			Application.Init ();
			PhotoBoothHost photoBoothHost = value.GetRequiredService<PhotoBoothHost>();
			await photoBoothHost.StartAsync(new CancellationToken());

			MainWindow win = value.GetRequiredService<MainWindow>();
			win.Show ();
			
			Application.Run ();
		}

		private static void ConfigureServices(ServiceCollection services)
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables()
				.Build();
			
			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration)
				.CreateLogger();
			
			services.AddSingleton<HttpClient>(provider => new HttpClient());
			services.AddSingleton<IConfiguration>(provider => configuration);
			services.AddLogging(configure =>
			{
				configure.AddConsole();
				configure.AddSerilog(dispose: false);
			});
			services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ConsoleLoggerProvider>());
			services.AddPhotoBooth(configuration);
			services.AddSingleton<PhotoBoothHost>();
		}
	}
}

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

using Gtk;
using Microsoft.Extensions.Logging;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace com.prodg.photobooth.common
{
	public class TextBoxLogger : ILogger
	{
		private TextBuffer buffer;
		private TextIter insertIter;
		private const string Warning = "Warning";
		private const string Error = "Error";

		public TextBoxLogger(TextBuffer buffer)
		{
			this.buffer = buffer;
			insertIter = buffer.StartIter;

			TextTag warningTag = new TextTag(Warning);
			warningTag.Foreground = "orange";
			buffer.TagTable.Add(warningTag);

			TextTag errorTag = new TextTag(Error);
			errorTag.Foreground = "red";
			buffer.TagTable.Add(errorTag);
		}

		public bool IsEnabled(LogLevel logLevel) => true;

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			var logString = formatter(state, exception);
			switch (logLevel)
			{
				case LogLevel.Trace:
				case LogLevel.Debug:
				case LogLevel.Information:
					Gtk.Application.Invoke((b, c) => { buffer.Insert(ref insertIter, logString + "\n"); });
					break;
				case LogLevel.Warning:
					Gtk.Application.Invoke((b, c) =>
					{
						buffer.InsertWithTagsByName(ref insertIter, "WARNING: " + logString + "\n", Warning);
					});
					break;
				case LogLevel.Error:
					Gtk.Application.Invoke((b, c) =>
					{
						buffer.InsertWithTagsByName(ref insertIter, "Error: " + logString + "\n", Error);
					});
					break;
				case LogLevel.Critical:
					break;
				case LogLevel.None:
					break;
				default:
					throw new NotSupportedException($"loglevel {logLevel}");
			}
		}
	}
}
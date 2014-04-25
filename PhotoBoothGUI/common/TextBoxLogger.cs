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

using System;
using Gtk;

namespace com.prodg.photobooth.common
{
	public class TextBoxLogger: ILogger
	{
		private TextBuffer buffer;
		private TextIter insertIter;
		private const string Warning = "Warning";
		private const string Error = "Error";

		public TextBoxLogger(TextBuffer buffer)
		{
			this.buffer = buffer;
			insertIter = buffer.StartIter;

			TextTag warningTag  = new TextTag (Warning);
			warningTag.Foreground = "orange";
			buffer.TagTable.Add (warningTag);

			TextTag errorTag  = new TextTag (Error);
			warningTag.Foreground = "red";
			buffer.TagTable.Add (errorTag);
		}

		public void LogInfo(string logString)
		{
			buffer.Insert(ref insertIter, logString+"\n");
		}

		public void LogWarning(string logString)
		{
			buffer.InsertWithTagsByName(ref insertIter, "WARNING: "+logString+"\n", Warning);
		}

		public void LogError(string logString)
		{
			buffer.InsertWithTagsByName(ref insertIter, "Error: "+logString+"\n", Error);
		}

		public void LogException(string logString, Exception exception)
		{
			buffer.InsertWithTagsByName(ref insertIter, "EXCEPTION: "+logString+"\n", Error);
			buffer.InsertWithTagsByName (ref insertIter, exception.ToString()+"\n", Error);
		}
	}
}
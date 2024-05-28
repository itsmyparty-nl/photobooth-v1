#region PhotoBooth - MIT - (c) 2015 Patrick Bronneberg
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

using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace com.prodg.photobooth.domain.converters
{
    public class ImageConverter : JsonConverter<Image>
    {
        public override Image? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var bytes = reader.GetBytesFromBase64();
            return Image.Load(bytes);
        }

        public override void Write(Utf8JsonWriter writer, Image value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToBase64String(JpegFormat.Instance));
        }
    }
}
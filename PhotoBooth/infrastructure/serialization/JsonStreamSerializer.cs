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

using System.IO;
using Newtonsoft.Json;

namespace com.prodg.photobooth.infrastructure.serialization
{
    public class JsonStreamSerializer : IStreamSerializer
    {
        private readonly JsonSerializer serializer;

        public string Type
        {
            get { return "json"; }
        }

        public JsonStreamSerializer()
        {
            serializer = new JsonSerializer
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                MissingMemberHandling = MissingMemberHandling.Error,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
        }
        
        public void Serialize(Stream stream, object obj)
        {
            try
            {
                using (var streamWriter = new StreamWriter(stream))
                using (var jsonTextWriter = new JsonTextWriter(streamWriter))
                {
                    serializer.Serialize(jsonTextWriter, obj);
                }
            }
            catch (JsonException e)
            {
                throw new IOException(e.Message, e);
            }
        }

        public T Deserialize<T>(Stream stream) where T : class
        {
            try
            {
                using (var streamReader = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    return serializer.Deserialize<T>(jsonTextReader);
                }
            }
            catch (JsonException e)
            {
                throw new IOException(e.Message, e);
            }
        }
    }
}

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
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.infrastructure.serialization
{
    public class JsonStreamSerializer : IStreamSerializer
    {
        private readonly ILogger<JsonStreamSerializer> _logger;
        private JsonSerializerOptions? _options;

        public string Type => "json";

        public JsonStreamSerializer(ILogger<JsonStreamSerializer> logger)
        {
            _logger = logger;
            
            _logger.LogDebug("Creating JSon serializer");

            _options = new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                IncludeFields = true
            };
        }
        
        public void Serialize(Stream stream, object? obj)
        {
            _logger.LogDebug("Serializing stream to JSON");

            try
            {
                JsonSerializer.Serialize(stream, obj, _options);
            }
            catch (JsonException e)
            {
                throw new IOException(e.Message, e);
            }
        }

        public T? Deserialize<T>(Stream stream) where T : class
        {
            _logger.LogDebug("Deserializing stream to JSON");

            try
            {
                return JsonSerializer.Deserialize<T>(stream);
            }
            catch (JsonException e)
            {
                throw new IOException(e.Message, e);
            }
        }
    }
}

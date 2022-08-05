#region PhotoBooth - MIT - (c) 2014 Patrick Bronneberg
/*
  PhotoBooth - an application to control a DIY photobooth
  Filters based on WebGLImageFilter - MIT Licensed
                   2013, Dominic Szablewski - phoboslab.org
 

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

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace com.prodg.photobooth.domain.image
{
    public static class FilterFactory
    {
        public static ColorMatrix Create(FilterType transformationMatrix)
        {
            switch (transformationMatrix)
            {
                case FilterType.Original:
                    return ColorMatrix.Identity;
                case FilterType.Polaroid:
                    return KnownFilterMatrices.PolaroidFilter;
                case FilterType.DarkGrayscale:
                    return KnownFilterMatrices.BlackWhiteFilter;
                case FilterType.Sepia:
                    return KnownFilterMatrices.CreateSepiaFilter(0.5f);
                case FilterType.Brownie:
                    return ColorMatrix.Identity;
                case FilterType.DesaturateLuminance:
                    return KnownFilterMatrices.CreateSaturateFilter(0.5f) *
                           KnownFilterMatrices.CreateLightnessFilter(0.5f);
                case FilterType.Kodachrome:
                    return KnownFilterMatrices.KodachromeFilter;
                case FilterType.Technicolor:
                    return KnownFilterMatrices.LomographFilter;
                case FilterType.VintagePinhole:
                    return KnownFilterMatrices.LomographFilter;
                case FilterType.AlternativePolaroid:
                    return KnownFilterMatrices.PolaroidFilter;
                default:
                    throw new NotSupportedException("The provided transformation matrix is not supported");
            }
        }
    }
}

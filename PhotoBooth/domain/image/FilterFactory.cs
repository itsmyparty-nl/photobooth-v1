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

using System;
using System.Drawing.Imaging;

namespace com.prodg.photobooth.domain.image
{
    public static class FilterFactory
    {
        public static ColorMatrix Create(FilterType transformationMatrix)
        {
            switch (transformationMatrix)
            {
                case FilterType.Original:
                    return new ColorMatrix(
                        new[]
                        {
                            new[] {1f, 0, 0, 0, 0},
                            new[] {0, 1f, 0, 0, 0},
                            new[] {0, 0, 1f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {0, 0, 0, 0, 1f}
                        });
                case FilterType.Polaroid:
                    return new ColorMatrix(
                        new[]
                        {
                            new[] {1.438f, -0.062f, -0.062f, 0, 0},
                            new[] {-0.122f, 1.378f, -0.122f, 0, 0},
                            new[] {-0.016f, -0.016f, 1.483f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {-0.03f, 0.05f, -0.02f, 0, 1f}
                        });
                case FilterType.DarkGrayscale:
                    return new ColorMatrix(
                        new[]
                        {
                            new[] {.3f, .3f, .3f, 0, 0},
                            new[] {.59f, .59f, .59f, 0, 0},
                            new[] {.11f, .11f, .11f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {0, 0, 0, 0, 1f}
                        });
                case FilterType.Sepia:
                    return new ColorMatrix(
                        new[]
                        {
                            new[] {0.393f, 0.349f, 0.272f, 0, 0},
                            new[] {0.769f, 0.686f, 0.534f, 0, 0},
                            new[] {0.189f, 0.168f, 0.131f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {0, 0, 0, 0, 1f}
                        });
                case FilterType.Brownie:
                    return new ColorMatrix(
                        new[]
                        {
                            new[]
                            {0.5997023498159715f,-0.037703249837783157f, 0.24113635128153335f, 0, 0},
                            new[]
                            {0.3455324304839126f, 0.8609577587992641f, -0.07441037908422492f, 0, 0},
                            new[]
                            {-0.2708298674538042f, 0.15059552388459913f, 0.44972182064877153f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {47.43192855600873f/255f, -36.96841498319127f/255f, -7.562075277591283f/255f, 0, 1f}
                        });
                case FilterType.DesaturateLuminance:
                    return new ColorMatrix(
                        new[]
                        {
                            new[] {0.2764723f, 0.2764723f, 0.2764723f, 0, 0},
                            new[] {0.9297080f, 0.9297080f, 0.9297080f, 0, 0},
                            new[] {0.0938197f, 0.0938197f, 0.0938197f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {-37.1f/255f, -37.1f/255f, -37.1f/255f, 0, 1f}
                        });
                case FilterType.Kodachrome:
                    return new ColorMatrix(
                        new[]
                        {
                             new[]
                            {1.1285582396593525f, -0.16404339962244616f, -0.16786010706155763f, 0, 0},
                            new[]
                            {-0.3967382283601348f, 1.0835251566291304f, -0.5603416277695248f, 0, 0},
                            new[]
                            {-0.03992559172921793f, -0.05498805115633132f, 1.6014850761964943f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {63.72958762196502f/255f, 24.732407896706203f/255f, 35.62982807460946f/255f, 0, 1f}
                        });
                case FilterType.Technicolor:
                    return new ColorMatrix(
                        new[]
                        {
                            new[]
                            {1.9125277891456083f, -0.3087833385928097f, -0.231103377548616f, 0, 0},
                            new[]
                            {-0.8545344976951645f, 1.7658908555458428f, -0.7501899197440212f, 0, 0},
                            new[]
                            {-0.09155508482755585f, -0.10601743074722245f, 1.847597816108189f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {11.793603434377337f/255f, -70.35205161461398f/255f, 30.950940869491138f/255f, 0, 1f}
                        });
                case FilterType.VintagePinhole:
                    return new ColorMatrix(
                        new[]
                        {
                            new[]
                            {0.6279345635605994f, 0.02578397704808868f, 0.0466055556782719f, 0, 0},
                            new[]
                            {0.3202183420819367f, 0.6441188644374771f, -0.0851232987247891f, 0, 0},
                            new[]
                            {-0.03965408211312453f, 0.03259127616149294f, 0.5241648018700465f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {9.651285835294123f/255f, 7.462829176470591f/255f, 5.159190588235296f/255f, 0, 1f}
                        });
                case FilterType.AlternativePolaroid:
                    return new ColorMatrix(
                        new[]
                        {
                            new[] {1.438f, -0.122f ,-0.016f , 0, 0},
                            new[] {-0.062f, 1.378f, -0.016f, 0, 0},
                            new[] {-0.062f, -0.122f, 1.483f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {0, 0, 0, 0, 1f}
                        });

                case FilterType.FrozenBlue:
                    return new ColorMatrix(
                        new[]
                        {
                            new[] {0.8f, 0,     -0.2f, 0, 0},
                            new[] {0,    1f, 1.25f, 0, 0},
                            new[] {0,    0 , 0.8f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {0.05f, 0.05f, 0.05f, 0, 1f}
                        });
                case FilterType.FrozenIce:
                    return new ColorMatrix(
                        new[]
                        {
                            new[] {0.8f, 0.2f,     -0.2f, 0, 0},
                            new[] {0,    1f, 1.25f, 0, 0},
                            new[] {0,    0.2f , 0.8f, 0, 0},
                            new[] {0, 0, 0, 1f, 0},
                            new[] {0.05f, 0.05f, 0.05f, 0, 1f}
                        });
                default:
                    throw new NotSupportedException("The provided transformation matrix is not supported");
            }
        }
    }
}

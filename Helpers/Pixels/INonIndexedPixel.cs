﻿using System;
using System.Drawing;

namespace ImageBank.Helpers.Pixels
{
    public interface INonIndexedPixel
    {
        // components
        Int32 Alpha { get; }
        Int32 Red { get; }
        Int32 Green { get; }
        Int32 Blue { get; }

        // higher-level values
        Int32 Argb { get; }
        UInt64 Value { get; set; }

        // color methods
        Color GetColor();
        void SetColor(Color color);
    }
}

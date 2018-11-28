﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

using SixLabors.Memory;

namespace SixLabors.ImageSharp.Processing.Processors.Transforms
{
    /// <content>
    /// Contains <see cref="MosaicKernelMap"/>
    /// </content>
    internal partial class ResizeKernelMap
    {
        /// <summary>
        /// Memory-optimized <see cref="ResizeKernelMap"/> where repeating rows are stored only once.
        /// </summary>
        private sealed class MosaicKernelMap : ResizeKernelMap
        {
            private readonly int period;

            private readonly int cornerInterval;

            public MosaicKernelMap(
                MemoryAllocator memoryAllocator,
                IResampler sampler,
                int sourceSize,
                int destinationSize,
                float ratio,
                float scale,
                int radius,
                int period,
                int cornerInterval)
                : base(
                    memoryAllocator,
                    sampler,
                    sourceSize,
                    destinationSize,
                    (cornerInterval * 2) + period,
                    ratio,
                    scale,
                    radius)
            {
                this.cornerInterval = cornerInterval;
                this.period = period;
            }

            internal override string Info => base.Info + $"|period:{this.period}|cornerInterval:{this.cornerInterval}";

            protected override void Initialize()
            {
                // Build top corner data + one period of the mosaic data:
                int startOfFirstRepeatedMosaic = this.cornerInterval + this.period;

                for (int i = 0; i < startOfFirstRepeatedMosaic; i++)
                {
                    ResizeKernel kernel = this.BuildKernel(i, i);
                    this.kernels[i] = kernel;
                }

                // Copy the mosaics:
                int bottomStartDest = this.DestinationSize - this.cornerInterval;
                for (int i = startOfFirstRepeatedMosaic; i < bottomStartDest; i++)
                {
                    float center = ((i + .5F) * this.ratio) - .5F;
                    int left = (int)MathF.Ceiling(center - this.radius);
                    ResizeKernel kernel = this.kernels[i - this.period];
                    this.kernels[i] = kernel.AlterLeftValue(left);
                }

                // Build bottom corner data:
                int bottomStartData = this.cornerInterval + this.period;
                for (int i = 0; i < this.cornerInterval; i++)
                {
                    ResizeKernel kernel = this.BuildKernel(bottomStartDest + i, bottomStartData + i);
                    this.kernels[bottomStartDest + i] = kernel;
                }
            }
        }
    }
}
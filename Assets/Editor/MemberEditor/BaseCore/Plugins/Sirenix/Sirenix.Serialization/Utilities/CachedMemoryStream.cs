//-----------------------------------------------------------------------
// <copyright file="CachedMemoryStream.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using Sirenix.Utilities;
    using System.IO;

    internal sealed class CachedMemoryStream : ICacheNotificationReceiver
    {
        public const int INITIAL_CAPACITY = 1024 * 1; // Initial capacity of 1 kb
        public const int MAX_CAPACITY = 1024 * 32; // Max of 32 kb cached stream size

        public MemoryStream MemoryStream { get; private set; }

        public CachedMemoryStream()
        {
            this.MemoryStream = new MemoryStream(INITIAL_CAPACITY);
        }

        public void OnFreed()
        {
            this.MemoryStream.SetLength(0);
            this.MemoryStream.Position = 0;

            if (this.MemoryStream.Capacity > MAX_CAPACITY)
            {
                this.MemoryStream.Capacity = MAX_CAPACITY;
            }
        }

        public void OnClaimed()
        {
            this.MemoryStream.SetLength(0);
            this.MemoryStream.Position = 0;
        }
    }
}
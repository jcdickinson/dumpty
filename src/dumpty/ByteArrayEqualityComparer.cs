using System.Collections.Generic;

namespace Dumpty
{
    /// <summary>
    /// Represents equality operations for <see cref="Byte[]"/>.
    /// </summary>
    public sealed class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        /// <summary>
        /// The default instance
        /// </summary>
        public static readonly ByteArrayEqualityComparer Default = new ByteArrayEqualityComparer();

        /// <summary>
        /// Determines if the specified byte arrays contain the same bytes.
        /// </summary>
        /// <param name="x">The first comparand.</param>
        /// <param name="y">The second comparand.</param>
        /// <returns>A value indicating whether the byte arrays are equal.</returns>
        public bool Equals(byte[] x, byte[] y)
        {
            return NativeMethods.Equals(x, y);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(byte[] obj)
        {
            if (obj == null) return 0;
            unchecked
            {
                var hash = 17;
                foreach (var value in obj)
                    hash = hash * 23 + value;
                return hash;
            }
        }
    }
}

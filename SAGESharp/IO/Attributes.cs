﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
using System;

namespace SAGESharp.IO
{
    /// <summary>
    /// Specifies a property that should be serialized/deserialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class SerializablePropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new attribute to mark a serializable property.
        /// </summary>
        /// 
        /// <param name="binaryOrder">The binary order of property (see <see cref="BinaryOrder"/>).</param>
        public SerializablePropertyAttribute(byte binaryOrder) => BinaryOrder = binaryOrder;

        /// <summary>
        /// Initializes a new attribute to mark a serializable property.
        /// </summary>
        /// 
        /// <param name="binaryOrder">The binary order of property (see <see cref="BinaryOrder"/>).</param>
        /// <param name="name">The name of the property in text form (see <see cref="Name"/>).</param>
        public SerializablePropertyAttribute(byte binaryOrder, string name) : this(binaryOrder) => Name = name;

        /// <summary>
        /// The order to serialize/deserialize the property as binary data.
        /// </summary>
        /// 
        /// <remarks>
        /// A single class/struct should not have duplicated values for <see cref="BinaryOrder"/>.
        /// </remarks>
        public byte BinaryOrder { get; }

        /// <summary>
        /// The name of the property when serialized in text form.
        /// </summary>
        public string Name { get; }
    }

    /// <summary>
    /// Specifies a property should be followed by a padding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class RightPaddingAttribute : Attribute
    {
        /// <summary>
        /// Initailizes a new attribute to mark a property with right padding.
        /// </summary>
        /// 
        /// <param name="size">The size of the padding (in bytes).</param>
        public RightPaddingAttribute(byte size) => Size = size;

        /// <summary>
        /// The length of the padding in bytes.
        /// </summary>
        public byte Size { get; }
    }

    /// <summary>
    /// Specifies the value for a string property is stored as an offset and with a
    /// single byte for the string length in the binary file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class OffsetStringAttribute : Attribute
    {
    }

    /// <summary>
    /// Specifies the value for a string property is stored inline (no offset) with a fixed length.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class InlineStringAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new attribute to mark a string property inline (with no offset) and a fixed length.
        /// </summary>
        /// 
        /// <param name="length">The fixed length of the string.</param>
        public InlineStringAttribute(byte length) => Length = length;

        /// <summary>
        /// The fixed length for the string.
        /// </summary>
        public byte Length { get; }
    }

    /// <summary>
    /// Specifies a list (or a list of the given class) should read/write twice its length in a binary file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class DuplicateEntryCountAttribute : Attribute
    {
    }
}

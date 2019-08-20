﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
using Konvenience;
using System;
using System.Collections.Generic;

namespace SAGESharp.IO
{
    #region Interfaces
    /// <summary>
    /// Represents a node with data (with children nodes) in the tree.
    /// </summary>
    internal interface IDataNode
    {
        /// <summary>
        /// Writes <paramref name="value"/> to the given <paramref name="binaryWriter"/>.
        /// </summary>
        /// 
        /// <param name="binaryWriter">The output binary writer were the object will be written.</param>
        /// <param name="value">The value to be written.</param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// If either <paramref name="binaryWriter"/> or <paramref name="value"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> is not of the expected type.
        /// </exception>
        void Write(IBinaryWriter binaryWriter, object value);

        /// <summary>
        /// Returns the list of edges connecting to child nodes.
        /// </summary>
        IReadOnlyList<IEdge> Edges { get; }
    }

    /// <summary>
    /// Represents a node that will write its contents at a later time.
    /// </summary>
    internal interface IOffsetNode
    {

        /// <summary>
        /// Writes <paramref name="value"/> to the given <paramref name="binaryWriter"/>.
        /// </summary>
        /// 
        /// <param name="binaryWriter">The output binary writer were the object will be written.</param>
        /// <param name="value">The value to be written.</param>
        /// 
        /// <returns>The position where the offset was written.</returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// If either <paramref name="binaryWriter"/> or <paramref name="value"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> is not of the expected type.
        /// </exception>
        uint Write(IBinaryWriter binaryWriter, object value);

        /// <summary>
        /// The actual node that will write the contents of the object.
        /// </summary>
        IDataNode ChildNode { get; }
    }

    /// <summary>
    /// Represents a node with a single child but several entries of the same child node.
    /// </summary>
    internal interface IListNode : IOffsetNode
    {
        /// <summary>
        /// Retrieves the amount of entries in the given <paramref name="list"/>.
        /// </summary>
        /// 
        /// <param name="list">The input list.</param>
        /// 
        /// <returns>The amount of object ins the input list.</returns>
        /// 
        /// <exception cref="ArgumentNullException">If <paramref name="list"/> is null.</exception>
        int GetListCount(object list);

        /// <summary>
        /// Retrieves the element with <paramref name="index"/> in the given <paramref name="list"/>.
        /// </summary>
        /// 
        /// <param name="list">The input list.</param>
        /// <param name="index">The index of the element.</param>
        /// 
        /// <returns>
        /// The element with the given <paramref name="index"/> in the given <paramref name="list"/>.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">If <paramref name="list"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is not valid for the input list.</exception>
        object GetListEntry(object list, int index);
    }

    /// <summary>
    /// Represents an edge connecting a <see cref="IDataNode"/> to
    /// a node (ex: <see cref="IDataNode"/>, <see cref="IOffsetNode"/>).
    /// </summary>
    internal interface IEdge
    {
        /// <summary>
        /// The child node that the edge is connecting.
        /// </summary>
        object ChildNode { get; }

        /// <summary>
        /// Extracts from the given <paramref name="value"/> a child value that is represented by the child node.
        /// </summary>
        /// 
        /// <param name="value">The object represented by the parent node.</param>
        /// 
        /// <returns>The child value represented by the child node.</returns>
        object ExtractChildValue(object value);
    }

    /// <summary>
    /// Represents an object that can write a value using an SLB graph.
    /// </summary>
    internal interface ITreeWriter
    {
        /// <summary>
        /// Writes <paramref name="value"/> to <paramref name="binaryWriter"/>
        /// using the SLB graph that starts in the given <paramref name="rootNode"/>.
        /// </summary>
        /// 
        /// <param name="binaryWriter">The binary writer that will be used to write the value.</param>
        /// <param name="value">The value to be written.</param>
        /// <param name="rootNode">The root node of the SLB graph that will be used to write the value.</param>
        /// 
        /// <returns>A list containing the position of the offsets written to <paramref name="binaryWriter"/>.</returns>
        /// 
        /// <exception cref="ArgumentNullException">If any argument is null.</exception>
        IReadOnlyList<uint> Write(IBinaryWriter binaryWriter, object value, IDataNode rootNode);
    }
    #endregion

    #region Implementations
    internal sealed class PrimitiveTypeDataNode<T> : IDataNode where T : struct
    {
        private readonly Action<IBinaryWriter, object> write;

        public PrimitiveTypeDataNode()
        {
            if (TypeIs<byte>())
            {
                write = (binaryWriter, value) => binaryWriter.WriteByte((byte)value);
            }
            else if (TypeIs<short>())
            {
                write = (binaryWriter, value) => binaryWriter.WriteInt16((short)value);
            }
            else if (TypeIs<ushort>())
            {
                write = (binaryWriter, value) => binaryWriter.WriteUInt16((ushort)value);
            }
            else if (TypeIs<int>())
            {
                write = (binaryWriter, value) => binaryWriter.WriteInt32((int)value);
            }
            else if (TypeIs<uint>())
            {
                write = (binaryWriter, value) => binaryWriter.WriteUInt32((uint)value);
            }
            else if (TypeIs<float>())
            {
                write = (binaryWriter, value) => binaryWriter.WriteFloat((float)value);
            }
            else if (TypeIs<double>())
            {
                write = (binaryWriter, value) => binaryWriter.WriteDouble((double)value);
            }
            else if (TypeIs<SLB.Identifier>())
            {
                write = (binaryWriter, value) => binaryWriter.WriteUInt32((SLB.Identifier)value);
            }
            else
            {
                throw BadTypeException.For<T>($"Type {typeof(T).Name} is not a valid primitive.");
            }
        }

        public IReadOnlyList<IEdge> Edges => new List<IEdge>();

        public void Write(IBinaryWriter binaryWriter, object value)
        {
            Validate.ArgumentNotNull(nameof(binaryWriter), binaryWriter);
            Validate.ArgumentNotNull(nameof(value), value);
            Validate.Argument(IsOfType(value), $"Cannot write value of type {value.GetType().Name} as type {typeof(T).Name}.");

            write(binaryWriter, value);
        }

        private static bool TypeIs<U>()
        {
            if (typeof(T).IsEnum)
            {
                return Enum.GetUnderlyingType(typeof(T)) == typeof(U);
            }
            else
            {
                return typeof(T) == typeof(U);
            }
        }

        private static bool IsOfType(object value) => typeof(T) == value.GetType();
    }

    internal class TreeWriter : ITreeWriter
    {
        private class QueueEntry
        {
            public QueueEntry(IDataNode node, object value)
            {
                Node = node;
                Value = value;
            }

            public QueueEntry(IDataNode node, object value, uint offsetPosition) : this(node, value)
            {
                OffsetPosition = offsetPosition;
            }

            public IDataNode Node { get; }

            public object Value { get; }

            public uint? OffsetPosition { get; }
        }

        private readonly Action<IBinaryWriter, uint> offsetWriter;

        private readonly Queue<QueueEntry> queue = new Queue<QueueEntry>();

        private readonly List<uint> offsets = new List<uint>();

        public TreeWriter(Action<IBinaryWriter, uint> offsetWriter)
        {
            this.offsetWriter = offsetWriter;
        }

        public IReadOnlyList<uint> Write(IBinaryWriter binaryWriter, object value, IDataNode rootNode)
        {
            Validate.ArgumentNotNull(nameof(binaryWriter), binaryWriter);
            Validate.ArgumentNotNull(nameof(value), value);
            Validate.ArgumentNotNull(nameof(rootNode), rootNode);

            queue.Clear();
            offsets.Clear();

            Enqueue(rootNode, value);

            while (queue.IsNotEmpty())
            {
                QueueEntry entry = queue.Dequeue();

                ProcessOffset(binaryWriter, entry.OffsetPosition);
                ProcessDataNode(binaryWriter, entry.Node, entry.Value);
            }

            return offsets;
        }

        private void ProcessOffset(IBinaryWriter binaryWriter, uint? offsetPosition)
        {
            if (!offsetPosition.HasValue)
            {
                return;
            }

            offsetWriter(binaryWriter, offsetPosition.Value);
            offsets.Add(offsetPosition.Value);
        }

        private void ProcessNode(IBinaryWriter binaryWriter, object node, object value)
        {
            if (node is IDataNode dataNode)
            {
                ProcessDataNode(binaryWriter, dataNode, value);
            }
            else if (node is IOffsetNode offsetNode)
            {
                ProcessOffsetNode(binaryWriter, offsetNode, value);
            }
            else
            {
                throw new NotImplementedException($"Type {node.GetType().Name} is an unknown node type");
            }
        }

        private void ProcessDataNode(IBinaryWriter binaryWriter, IDataNode node, object value)
        {
            node.Write(binaryWriter, value);

            foreach (IEdge edge in node.Edges)
            {
                object childValue = edge.ExtractChildValue(value);
                ProcessNode(binaryWriter, edge.ChildNode, childValue);
            }
        }

        private void ProcessOffsetNode(IBinaryWriter binaryWriter, IOffsetNode node, object value)
        {
            uint offsetPosition = node.Write(binaryWriter, value);

            if (node is IListNode listNode)
            {
                ProcessListNode(listNode, value, offsetPosition);
            }
            else
            {
                Enqueue(node.ChildNode, value, offsetPosition);
            }
        }

        private void ProcessListNode(IListNode node, object list, uint offsetPosition)
        {
            int count = node.GetListCount(list);
            if (count == 0)
            {
                return;
            }

            Enqueue(node.ChildNode, node.GetListEntry(list, 0), offsetPosition);

            for (int n = 1; n < count; ++n)
            {
                Enqueue(node.ChildNode, node.GetListEntry(list, n));
            }
        }

        private void Enqueue(IDataNode node, object value)
        {
            queue.Enqueue(new QueueEntry(node, value));
        }

        private void Enqueue(IDataNode node, object value, uint offsetPosition)
        {
            queue.Enqueue(new QueueEntry(node, value, offsetPosition));
        }
    }
    #endregion
}
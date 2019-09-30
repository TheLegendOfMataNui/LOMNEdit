﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
using FluentAssertions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using SAGESharp.SLB;
using System.Collections.Generic;

namespace SAGESharp.IO
{
    class DefaultPropertyBinarySerializerFactoryTests
    {
        private readonly IBinarySerializerFactory serializerFactory = Substitute.For<IBinarySerializerFactory>();

        private readonly IPropertyBinarySerializerFactory factory = new DefaultPropertyBinarySerializerFactory();

        [SetUp]
        public void Setup()
        {
            serializerFactory.ClearSubstitute();
        }

        [Test]
        public void Test_Getting_Serializers_For_A_Valid_Class()
        {
            var serializers = factory.GetPropertySerializersForType<CustomClass>(serializerFactory);

            serializers
                .Should()
                .HaveCount(4);

            serializers[0]
                .Should()
                .BeOfType<DefaultPropertyBinarySerializer<CustomClass, int>>();

            serializers[1]
                .Should()
                .BeOfType<DefaultPropertyBinarySerializer<CustomClass, string>>();

            serializers[2]
                .Should()
                .BeOfType<DefaultPropertyBinarySerializer<CustomClass, IList<byte>>>();

            serializers[3]
                .Should()
                .BeOfType<IdentifierPropertyBinarySerializer<CustomClass>>();

            serializerFactory.Received().GetSerializerForType<int>();
            serializerFactory.Received().GetSerializerForType<string>();
            serializerFactory.Received().GetSerializerForType<IList<byte>>();
        }

        // Proporsefuly unordered
        class CustomClass
        {
            [SerializableProperty(2)]
            public string String { get; set; }

            [SerializableProperty(1)]
            public int Int { get; set; }

            [SerializableProperty(3)]
            public IList<byte> List { get; set; }

            [SerializableProperty(4)]
            public Identifier Identifier { get; set; }

            public CustomClass IgnoredValue { get; set; }
        }

        [Test]
        public void Test_Getting_Serializers_For_A_Class_With_No_Annotations() => factory
            .Invoking(f => f.GetPropertySerializersForType<ClassWithNoAnnotations>(serializerFactory))
            .Should()
            .Throw<BadTypeException>()
            .WithMessage($"Type has no property annotated with {nameof(SerializablePropertyAttribute)}")
            .And
            .Type
            .Should()
            .Be(typeof(ClassWithNoAnnotations));

        class ClassWithNoAnnotations
        {
            public int Int { get; set; }
        }

        [Test]
        public void Test_Reading_A_Class_With_An_Annotated_Property_With_No_Setter() => factory
            .Invoking(f => f.GetPropertySerializersForType<ClassWithAnnotatedPropertyWithNoSetter>(serializerFactory))
            .Should()
            .Throw<BadTypeException>()
            .WithMessage($"Property {nameof(ClassWithAnnotatedPropertyWithNoSetter.Int)} doesn't have a setter")
            .And
            .Type
            .Should()
            .Be(typeof(ClassWithAnnotatedPropertyWithNoSetter));

        class ClassWithAnnotatedPropertyWithNoSetter
        {
            [SerializableProperty(1)]
            public int Int { get; }
        }

        [Test]
        public void Test_Reading_A_Class_With_Properties_With_Duplicated_Attribute_Order() => factory
            .Invoking(f => f.GetPropertySerializersForType<ClassWithPropertiesWithDuplicatedAttributeOrder>(serializerFactory))
            .Should()
            .Throw<BadTypeException>()
            .WithMessage("Type has more than one property with the same order")
            .And
            .Type
            .Should()
            .Be(typeof(ClassWithPropertiesWithDuplicatedAttributeOrder));

        class ClassWithPropertiesWithDuplicatedAttributeOrder
        {
            [SerializableProperty(1)]
            public int Int { get; set; }

            [SerializableProperty(1)]
            public byte Byte { get; set; }
        }
    }
}
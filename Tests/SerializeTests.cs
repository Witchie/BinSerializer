﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using ThirtyNineEighty.BinarySerializer;

namespace Tests
{
  [TestClass]
  public class SerializeTests
  {
    [Type("NullTestType")]
    class NullTestType
    {
      [Field("a")]
      public string StrField;
      
      [Field("b")]
      public int IntField;

      [Field("c")]
      public NullTestType NullField;
    }
    
    [TestMethod]
    public void NullTest()
    {
      var input = new NullTestType();
      input.StrField = "str value";
      input.IntField = 255;
      input.NullField = null;

      var stream = new MemoryStream();
      BinSerializer.Serialize(stream, input);
      stream.Position = 0;
      var output = BinSerializer.Deserialize<NullTestType>(stream);

      Assert.AreEqual(input.StrField, output.StrField);
      Assert.AreEqual(input.IntField, output.IntField);
      Assert.AreEqual(input.NullField, output.NullField);
    }

    [Type("StructContainerType")]
    class StructContainerType
    {
      [Field("a")]
      public StructType StructField;
    }

    [Type("StructType")]
    struct StructType
    {
      [Field("b")]
      public int IntField;

      [Field("a")]
      public float FloatField;
    }

    [TestMethod]
    public void UserDefinedStructTest()
    {
      var input = new StructContainerType();
      input.StructField = new StructType();
      input.StructField.IntField = 10;
      input.StructField.FloatField = 0.55f;

      var stream = new MemoryStream();
      BinSerializer.Serialize(stream, input);
      stream.Position = 0;
      var output = BinSerializer.Deserialize<StructContainerType>(stream);

      Assert.AreEqual(input.StructField.IntField, output.StructField.IntField);
      Assert.AreEqual(input.StructField.FloatField, output.StructField.FloatField);
    }

    [Type("CycleReferenceType")]
    class CycleReferenceType
    {
      [Field("a")]
      public CycleReferenceType Field;
    }

    [TestMethod]
    public void CycleReferenceTest()
    {
      var input = new CycleReferenceType();
      input.Field = input;

      var stream = new MemoryStream();
      BinSerializer.Serialize(stream, input);
      stream.Position = 0;
      var output = BinSerializer.Deserialize<CycleReferenceType>(stream);

      Assert.AreEqual(ReferenceEquals(input, input.Field), ReferenceEquals(output, output.Field));
    }

    [Type("InterfaceType")]
    class InterfaceType
    {
      [Field("a")]
      public IInterface Field;
    }

    interface IInterface
    {
      int Field { get; }
    }

    [Type("InterfaceImpl")]
    class InterfaceImpl : IInterface
    {
      [Field("a")]
      private int _field = 100;

      public int Field { get { return _field; } }
    }

    [TestMethod]
    public void InterfaceTest()
    {
      var input = new InterfaceType();
      input.Field = new InterfaceImpl();

      var stream = new MemoryStream();
      BinSerializer.Serialize(stream, input);
      stream.Position = 0;
      var output = BinSerializer.Deserialize<InterfaceType>(stream);

      Assert.AreEqual(((InterfaceImpl)input.Field).Field, ((InterfaceImpl)output.Field).Field);
    }

    [Type("ArrayType")]
    class ArrayType
    {
      [Field("a")]
      public int[] ArrayField;
    }

    [TestMethod]
    public void ArrayTest()
    {
      var input = new ArrayType();
      input.ArrayField = new[] { 1, 3, 3, 7 };

      var stream = new MemoryStream();
      BinSerializer.Serialize(stream, input);
      stream.Position = 0;
      var output = BinSerializer.Deserialize<ArrayType>(stream);

      Assert.AreEqual(input.ArrayField.Length, output.ArrayField.Length);

      for (int i = 0; i < input.ArrayField.Length; i++)
        Assert.AreEqual(input.ArrayField[i], output.ArrayField[i]);
    }

    [Type("GenericType")]
    class GenericType<T>
    {
      [Field("a")]
      public T Field;
    }
    
    [TestMethod]
    public void GenericTypeTest()
    {
      var input = new GenericType<GenericType<int>>();
      input.Field = new GenericType<int>();
      input.Field.Field = 500;

      var stream = new MemoryStream();
      BinSerializer.Serialize(stream, input);
      stream.Position = 0;
      var output = BinSerializer.Deserialize<GenericType<GenericType<int>>>(stream);

      Assert.AreEqual(input.Field.Field, output.Field.Field);
    }
  }
}

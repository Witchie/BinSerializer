﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Text;

namespace ThirtyNineEighty.BinarySerializer.Types
{
  sealed class GenericSerializerTypeInfo : SerializerTypeInfo
  {
    [SecurityCritical]
    public GenericSerializerTypeInfo(BinTypeDescription description, BinTypeVersion version, BinTypeProcess process) 
      : base(description, version, process)
    {
      if (!Type.IsGenericType)
        throw new ArgumentException("Type must be generic.");

      if (!Type.IsGenericTypeDefinition)
        throw new ArgumentException("Generic type must be opened.");

      if (IsGenericTypeId(TypeId))
        throw new ArgumentException("Type id must be declared as non generic");
    }

    public override MethodInfo GetWriter(Type notNormalizedType)
    {
      var writer = base.GetWriter(notNormalizedType);
      return GetMethod(writer, notNormalizedType);
    }

    public override MethodInfo GetReader(Type notNormalizedType)
    {
      var reader = base.GetReader(notNormalizedType);
      return GetMethod(reader, notNormalizedType);
    }

    public override MethodInfo GetSkiper(Type notNormalizedType)
    {
      var skiper = base.GetSkiper(notNormalizedType);
      return GetMethod(skiper, notNormalizedType);
    }

    private static MethodInfo GetMethod(MethodInfo method, Type notNormalizedType)
    {
      if (method == null)
        return null;

      if (method.IsGenericMethodDefinition)
      {
        var genericArgs = notNormalizedType.GetGenericArguments();
        return method.MakeGenericMethod(genericArgs);
      }
      return method;
    }

    // Must be called read under SerializerTypes read lock
    [SecuritySafeCritical]
    public override Type GetType(string notNormalizedTypeId)
    {
      var index = 0;
      var types = new Type[Type.GenericTypeParameters.Length];

      foreach (var genericArgumentId in EnumerateGenericTypeIds(notNormalizedTypeId))
        types[index++] = SerializerTypes.GetTypeImpl(genericArgumentId);

      return Type.MakeGenericType(types);
    }

    // Must be called read under SerializerTypes read lock
    [SecuritySafeCritical]
    public override string GetTypeId(Type notNormalizedType)
    {
      if (notNormalizedType.ContainsGenericParameters)
        throw new ArgumentException(string.Format("{0} conatins generic parameters.", Type));

      var builder = new StringBuilder();
      builder.Append(TypeId);
      builder.Append('[');

      var argumentsCount = notNormalizedType.GenericTypeArguments.Length;
      for (int i = 0; i < argumentsCount; i++)
      {
        var genericTypeArgument = notNormalizedType.GenericTypeArguments[i];
        var typeId = SerializerTypes.GetTypeIdImpl(genericTypeArgument);

        builder.Append(typeId);

        var isLast = i == argumentsCount - 1;
        if (!isLast)
          builder.Append(",");
      }

      builder.Append("]");

      return builder.ToString();
    }

    [SecurityCritical]
    private static bool IsGenericTypeId(string typeId)
    {
      var index1 = typeId.IndexOf('[');
      if (index1 < 0)
        return false;

      var index2 = typeId.LastIndexOf(']');
      if (index2 < 0)
        return false;

      return index2 > index1;
    }

    [SecurityCritical]
    private static IEnumerable<string> EnumerateGenericTypeIds(string genericTypeId)
    {
      var index1 = genericTypeId.IndexOf('[');
      var index2 = genericTypeId.LastIndexOf(']');

      if (index1 < 0 || index2 < 0)
        throw new ArgumentException("Is not a generic typeId");

      var builder = new StringBuilder();
      var bracketCounter = 1;

      for (int i = index1 + 1; i <= index2; i++)
      {
        var ch = genericTypeId[i];

        if (ch == '[')
          bracketCounter++;
        if (ch == ']')
          bracketCounter--;

        if (bracketCounter == 0)
        {
          yield return builder.ToString();
        }
        else
        {
          if (ch != ',' || bracketCounter != 1)
            builder.Append(ch);
          else
          {
            yield return builder.ToString();
            builder.Clear();
          }
        }
      }
    }
  }
}
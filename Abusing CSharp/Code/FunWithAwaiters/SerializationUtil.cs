// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace FunWithAwaiters
{
    public static class SerializationUtil
    {
        public static void SerializeFields(object source, Stream stream, Func<FieldInfo, bool> predicate)
        {
            var formatter = new BinaryFormatter();
            foreach (var field in source.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(predicate))
            {
                object value = field.GetValue(source) ?? NullToken.Instance;
                formatter.Serialize(stream, field.Name);
                formatter.Serialize(stream, value);
            }
        }

        public static void DeserializeFields(object target, Stream stream)
        {
            var formatter = new BinaryFormatter();
            long length = stream.Length;
            while (stream.Position != length)
            {
                string name = (string) formatter.Deserialize(stream);
                object value = formatter.Deserialize(stream);
                if (value is NullToken)
                {
                    value = null;
                }
                target.GetType()
                        .GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .SetValue(target, value);
            }
        }

        [Serializable]
        private class NullToken
        {
            public static readonly NullToken Instance = new NullToken();

            // No need to actually make this a singleton...
        }
    }
}

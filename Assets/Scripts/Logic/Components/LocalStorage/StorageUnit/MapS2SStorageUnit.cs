﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GameLogic.Components
{
    public class MapS2SStorageUnit : LocalStorageUnit
    {
        public static readonly short ValueType = 4;

        public override short StorageValueType
        {
            get { return ValueType; }
        }

        public MapS2SStorageUnit(Dictionary<string,string> value)
        {
            Value = value;
        }

        public static KeyValuePair<string, LocalStorageUnit> ParseData(byte[] dataBuff, ref int offset)
        {
            var keyDataLength = BitConverter.ToInt32(dataBuff, offset);
            offset += 4;

            var key = Encoding.UTF8.GetString(dataBuff, offset, keyDataLength);
            offset += keyDataLength;
 
            var valueCount = BitConverter.ToInt32(dataBuff, offset);
            offset += 4;

            var value = new Dictionary<string,string>(valueCount);
            for (int i = 0; i < valueCount; ++i)
            {
                var dataKeyLength = BitConverter.ToInt32(dataBuff, offset);
                offset += 4;

                var datakey = Encoding.UTF8.GetString(dataBuff, offset, dataKeyLength);
                offset += dataKeyLength;

                var dataValueLength = BitConverter.ToInt32(dataBuff, offset);
                offset += 4;

                var dataValue = Encoding.UTF8.GetString(dataBuff, offset, dataValueLength);
                offset += dataValueLength;
                
                value.Add(datakey,dataValue);
            }

            return new KeyValuePair<string, LocalStorageUnit>(key, new MapS2SStorageUnit(value));
        }

        public override int ToBinaryData(string key, ref byte[] dataBuff, int offset)
        {
            var typeBytes = BitConverter.GetBytes(ValueType);
            dataBuff = CheckDataBuffer(dataBuff, offset, typeBytes.Length);
            AddDataToBuffer(dataBuff, offset, typeBytes);
            offset += typeBytes.Length;

            var keyBytes = Encoding.UTF8.GetBytes(key);
            dataBuff = CheckDataBuffer(dataBuff, offset, keyBytes.Length + 4);
            AddDataToBuffer(dataBuff, offset, BitConverter.GetBytes(keyBytes.Length));
            offset += 4;
            AddDataToBuffer(dataBuff, offset, keyBytes);
            offset += keyBytes.Length;

            var value = Value as Dictionary<string, string>;

            var countBytes = BitConverter.GetBytes(value.Count);
            dataBuff = CheckDataBuffer(dataBuff, offset, countBytes.Length);
            AddDataToBuffer(dataBuff, offset, countBytes);
            offset += countBytes.Length;

            var enumerator = value.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var valueBytes = Encoding.UTF8.GetBytes(enumerator.Current.Key);
                dataBuff = CheckDataBuffer(dataBuff, offset, valueBytes.Length + 4);
                AddDataToBuffer(dataBuff, offset, BitConverter.GetBytes(valueBytes.Length));
                offset += 4;
                AddDataToBuffer(dataBuff, offset, valueBytes);
                offset += valueBytes.Length;

                valueBytes = Encoding.UTF8.GetBytes(enumerator.Current.Value);
                dataBuff = CheckDataBuffer(dataBuff, offset, valueBytes.Length + 4);
                AddDataToBuffer(dataBuff, offset, BitConverter.GetBytes(valueBytes.Length));
                offset += 4;
                AddDataToBuffer(dataBuff, offset, valueBytes);
                offset += valueBytes.Length;
            }
            return offset;
        }
    }
}
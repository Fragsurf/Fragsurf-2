using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Network;

namespace Fragsurf.Shared.Packets
{
    public class CustomPacket : IBasePacket
    {
        public struct DataEntry
        {
            public DataType Type;
            public int IntValue;
            public float FloatValue;
            public byte ByteValue;
            public bool BoolValue;
            public string StringValue;
            public Vector3 Vector3Value;
            public byte[] BytesValue;
        }

        public enum DataType
        {
            Int,
            Float,
            Byte,
            Bool,
            String,
            Vector3,
            Bytes
        }

        public string Label = string.Empty;

        public SendCategory Sc { get; set; } = SendCategory.UI_Important;
		public bool DisableAutoPool => false;
        public int ByteSize => 0;

        public List<DataEntry> DataEntries { get; private set; } = new List<DataEntry>(255);
        public int Index { get; set; }

        public void Reset()
        {
            DataEntries.Clear();
            Label = string.Empty;
            Index = 0;
        }

        public void Read(NetBuffer buffer)
        {
            DataEntries.Clear();

            Label = buffer.ReadString();
            byte entryCount = buffer.ReadByte();
            for(int i = 0; i < entryCount; i++)
            {
                var entry = new DataEntry();
                entry.Type = (DataType)buffer.ReadByte();
                switch(entry.Type)
                {
                    case DataType.Bool:
                        entry.BoolValue = buffer.ReadBoolean();
                        break;
                    case DataType.Int:
                        entry.IntValue = buffer.ReadInt32();
                        break;
                    case DataType.Float:
                        entry.FloatValue = buffer.ReadSingle();
                        break;
                    case DataType.Byte:
                        entry.ByteValue = buffer.ReadByte();
                        break;
                    case DataType.String:
                        entry.StringValue = buffer.ReadString();
                        break;
                    case DataType.Vector3:
                        entry.Vector3Value = buffer.ReadVector3();
                        break;
                    case DataType.Bytes:
                        var byteCount = buffer.ReadInt32();
                        entry.BytesValue = buffer.ReadBytes(byteCount);
                        break;
                }

                DataEntries.Add(entry);
            }
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(Label);
            buffer.Write((byte)DataEntries.Count);
            foreach(DataEntry entry in DataEntries)
            {
                buffer.Write((byte)entry.Type);

                switch(entry.Type)
                {
                    case DataType.Bool:
                        buffer.Write(entry.BoolValue);
                        break;
                    case DataType.Int:
                        buffer.Write(entry.IntValue);
                        break;
                    case DataType.Float:
                        buffer.Write(entry.FloatValue);
                        break;
                    case DataType.Byte:
                        buffer.Write(entry.ByteValue);
                        break;
                    case DataType.String:
                        buffer.Write(entry.StringValue);
                        break;
                    case DataType.Vector3:
                        buffer.Write(entry.Vector3Value);
                        break;
                    case DataType.Bytes:
                        buffer.Write(entry.BytesValue.Length);
                        buffer.Write(entry.BytesValue);
                        break;
                }
            }
        }

        public void AddInt(int value)
        {
            DataEntries.Add(new DataEntry() { Type = DataType.Int, IntValue = value });
        }

        public void AddFloat(float value)
        {
            DataEntries.Add(new DataEntry() { Type = DataType.Float, FloatValue = value });
        }

        public void AddByte(byte value)
        {
            DataEntries.Add(new DataEntry() { Type = DataType.Byte, ByteValue = value });
        }

        public void AddBool(bool value)
        {
            DataEntries.Add(new DataEntry() { Type = DataType.Bool, BoolValue = value });
        }

        public void AddString(string value)
        {
            DataEntries.Add(new DataEntry() { Type = DataType.String, StringValue = value });
        }

        public void AddVector3(Vector3 value)
        {
            DataEntries.Add(new DataEntry() { Type = DataType.Vector3, Vector3Value = value });
        }

        public void AddBytes(byte[] value)
        {
            DataEntries.Add(new DataEntry() { Type = DataType.Bytes, BytesValue = value });
        }

        public int GetInt(int index = -1)
        {
            if(index != -1)
            {
                if (index >= DataEntries.Count)
                    return 0;
                return DataEntries[index].IntValue;
            }
            Index++;
            return DataEntries[Index - 1].IntValue;
        }

        public float GetFloat(int index = -1)
        {
            if(index != -1)
            {
                if (index >= DataEntries.Count)
                    return 0;
                return DataEntries[index].FloatValue;
            }
            Index++;
            return DataEntries[Index - 1].FloatValue;
        }

        public byte GetByte(int index = -1)
        {
            if(index != -1)
            {
                if (index >= DataEntries.Count)
                    return 0;
                return DataEntries[index].ByteValue;
            }
            Index++;
            return DataEntries[Index - 1].ByteValue;
        }

        public byte[] GetBytes(int index = -1)
        {
            if(index != -1)
            {
                if (index >= DataEntries.Count)
                {
                    return null;
                }
            }
            Index++;
            return DataEntries[Index - 1].BytesValue;
        }

        public bool GetBool(int index = -1)
        {
            if(index != -1)
            {
                if (index >= DataEntries.Count)
                    return false;
                return DataEntries[index].BoolValue;
            }
            Index++;
            return DataEntries[Index - 1].BoolValue;
        }

        public string GetString(int index = -1)
        {
            if(index != -1)
            {
                if (index >= DataEntries.Count)
                    return string.Empty;
                return DataEntries[index].StringValue;
            }
            Index++;
            return DataEntries[Index - 1].StringValue;
        }

        public Vector3 GetVector3(int index = -1)
        {
            if(index != -1)
            {
                if (index >= DataEntries.Count)
                    return Vector3.zero;
                return DataEntries[index].Vector3Value;
            }
            Index++;
            return DataEntries[Index - 1].Vector3Value;
        }

    }
}

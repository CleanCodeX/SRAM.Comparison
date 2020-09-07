using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using SramCommons.Attributes;
using SramCommons.Models.Structs;

namespace SramCommons.Extensions
{
    public static class StructExtensions
    {
        /// <summary>
        /// Convert the bytes to a structure in host-endian format (little-endian on PCs).
        /// To use with big-endian data, reverse all of the data bytes and create a struct that is in the reverse order of the data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static T ToStructureHostEndian<T>(this byte[] buffer) where T : struct
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T))!;
            handle.Free();
            return stuff;
        }

        /// <summary>
        /// Converts the struct to a byte array in the endianness of this machine.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="structure">The structure.</param>
        /// <returns></returns>
        public static byte[] ToBytesHostEndian<T>(this T structure) 
        {
            var size = Marshal.SizeOf(structure);
            var buffer = new byte[size];
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(structure!, handle.AddrOfPinnedObject(), true);
            handle.Free();
            return buffer;
        }

        public static T ByteArrayToStructure<T>(this byte[] bytes) where T : struct
            => (T)ByteArrayToStructure(bytes, typeof(T))!;

        public static object? ByteArrayToStructure(this byte[] bytes, Type type)
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var stuff = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
            handle.Free();
            return stuff;
        }

        public static T ByteArrayToStructureBigEndian<T>(this byte[] bytes) where T : struct
            => (T)ByteArrayToStructureBigEndian(bytes, typeof(T))!;

        public static object? ByteArrayToStructureBigEndian(this byte[] bytes, Type type)
        {
            var instance = bytes.ByteArrayToStructure(type);

            var fieldInfos = instance!.GetType().GetFields();
            foreach (var fi in fieldInfos)
            {
                if (fi.FieldType == typeof(byte[]))
                    continue;

                if (fi.IsDefined(typeof(FixedBufferAttribute)))
                    continue;
                if (fi.IsDefined(typeof(NoByteReorderingAttribute)))
                    continue;
                if (fi.IsDefined(typeof(IgnoreDataMemberAttribute)))
                    continue;
                if (fi.IsDefined(typeof(CompilerGeneratedAttribute)))
                    continue;

                if (fi.FieldType.IsDefined(typeof(NoByteReorderingAttribute)))
                    continue;
                if (fi.FieldType.IsDefined(typeof(IgnoreDataMemberAttribute)))
                    continue;

                if (fi.FieldType == typeof(ThreeByteUint))
                {
                    //var threeByte = (ThreeByteUint)fi.GetValue(instance);

                    //var b32 = BitConverter.GetBytes(threeByte.Value3Byte);
                    //var b32R = b32.Reverse().ToArray();

                    //threeByte.ValueByte1 = b32R[1];
                    //threeByte.ValueByte2 = b32R[2];
                    //threeByte.ValueByte3 = b32R[3];

                    //fi.SetValueDirect(__makeref(instance), threeByte);
                }
                else if (fi.FieldType.IsValueType && !fi.FieldType.IsPrimitive)
                {
                    var value = fi.GetValue(instance);
                    var structBytes = value.ToBytesHostEndian();

                    if (fi.FieldType.IsDefined(typeof(ReverseBytesOnlyAttribute)))
                    {
                        Array.Reverse(structBytes);

                        value = structBytes.ByteArrayToStructure(fi.FieldType);
                    }
                    else
                        value = structBytes.ByteArrayToStructureBigEndian(fi.FieldType);

                    fi.SetValue(instance, value);
                }
                else if (fi.FieldType == typeof(short))
                {
                    // TODO
                }
                else if (fi.FieldType == typeof(int))
                {
                    // TODO
                }
                else if (fi.FieldType == typeof(long))
                {
                    // TODO
                }
                else if (fi.FieldType == typeof(char))
                {
                    var i16 = (char)fi.GetValue(instance)!;
                    var b16 = BitConverter.GetBytes(i16);
                    var b16R = b16.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(instance), BitConverter.ToChar(b16R, 0));
                }
                else if (fi.FieldType == typeof(ushort))
                {
                    var i16 = (ushort)fi.GetValue(instance)!;
                    var b16 = BitConverter.GetBytes(i16);
                    var b16R = b16.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(instance), BitConverter.ToUInt16(b16R, 0));
                }
                else if (fi.FieldType == typeof(uint))
                {
                    var i32 = Convert.ToUInt32(fi.GetValue(instance));
                    var b32 = BitConverter.GetBytes(i32);
                    var b32R = b32.Reverse().ToArray();
                    var newValue = BitConverter.ToUInt32(b32R, 0);

                    fi.SetValueDirect(__makeref(instance), newValue);
                }
                else if (fi.FieldType == typeof(ulong))
                {
                    var i64 = Convert.ToUInt64(fi.GetValue(instance));
                    var b64 = BitConverter.GetBytes(i64);
                    var b64R = b64.Reverse().ToArray();
                    var newValue = BitConverter.ToUInt64(b64R, 0);

                    fi.SetValueDirect(__makeref(instance), newValue);
                }
            }
            return instance;
        }
    }
}

using System.Runtime.InteropServices;

namespace SramCommons.Helpers
{
    public static class Serializer
    {
#if false
        public static byte[] Serialize<T>(T data)
                where T : struct
            {
                var formatter = new BinaryFormatter();
                var stream = new MemoryStream();
                formatter.Serialize(stream, data);
                return stream.ToArray();
            }

            public static T Deserialize<T>(byte[] array)
                where T : struct
            {
                var stream = new MemoryStream(array);
                var formatter = new BinaryFormatter();
                return (T) formatter.Deserialize(stream);
            }
#else

        public static byte[] Serialize<T>(T s)
        {
            var size = Marshal.SizeOf(typeof(T));
            var array = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(s!, ptr, true);
            Marshal.Copy(ptr, array, 0, size);
            Marshal.FreeHGlobal(ptr);
            return array;
        }

        public static T Deserialize<T>(byte[] array)
        {
            var size = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(array, 0, ptr, size);
            var s = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return s!;
        }
#endif
    }
}
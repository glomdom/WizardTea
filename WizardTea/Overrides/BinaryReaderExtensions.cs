namespace WizardTea;

internal static class BinaryReaderExtensions {
    internal static T[] ReadArray<T>(this BinaryReader reader, uint length, Func<T> readMethod) {
        var array = new T[length];

        for (var i = 0; i < length; i++) {
            array[i] = readMethod();
        }

        return array;
    }
    
    internal static T[] ReadArray<T>(this BinaryReader reader, int length, Func<T> readMethod) {
        return ReadArray(reader, (uint)length, readMethod);
    }

    internal static T ReadEnum<T>(this BinaryReader reader) where T : Enum {
        var underlyingType = Enum.GetUnderlyingType(typeof(T));

        object value = underlyingType switch {
            not null when underlyingType == typeof(byte) => reader.ReadByte(),
            not null when underlyingType == typeof(sbyte) => reader.ReadSByte(),
            not null when underlyingType == typeof(short) => reader.ReadInt16(),
            not null when underlyingType == typeof(ushort) => reader.ReadUInt16(),
            not null when underlyingType == typeof(int) => reader.ReadInt32(),
            not null when underlyingType == typeof(uint) => reader.ReadUInt32(),
            not null when underlyingType == typeof(long) => reader.ReadInt64(),
            not null when underlyingType == typeof(ulong) => reader.ReadUInt64(),

            _ => throw new NotSupportedException($"Unsupported enum underlying type: {underlyingType}"),
        };

        return (T)Enum.ToObject(typeof(T), value);
    }

}
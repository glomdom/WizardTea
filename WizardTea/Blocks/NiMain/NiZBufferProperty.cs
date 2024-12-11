using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public class NiZBufferProperty : NiProperty {
    private ushort _bitfield;

    public ZBufferFlags Flags {
        get => (ZBufferFlags)(_bitfield & 0b_0000_0000_0011_1111);
        set => _bitfield = (ushort)((_bitfield & ~0b_0000_0000_0011_1111) | (ushort)value);
    }

    public TestFunction TestFunc {
        get => (TestFunction)((_bitfield >> 2) & 0b_1111);
        set => _bitfield = (ushort)((_bitfield & ~0b_0000_0000_0011_1100) | ((ushort)value << 2));
    }

    public NiZBufferProperty(NifStream stream, NifHeader header) : base(stream, header) {
        _bitfield = stream.ReadUInt16();
    }
}

[Flags]
public enum ZBufferFlags : ushort {
    ZBuffer_Test = 0b_0000_0001,
    ZBuffer_Write = 0b_0000_0010,
    ZBuffer_TestFunc = 0b_0000_0100,
}

public enum TestFunction : uint {
    TEST_ALWAYS = 0,
    TEST_LESS = 1,
    TEST_EQUAL = 2,
    TEST_LESS_EQUAL = 3,
    TEST_GREATER = 4,
    TEST_NOT_EQUAL = 5,
    TEST_GREATER_EQUAL = 6,
    TEST_NEVER = 7
}
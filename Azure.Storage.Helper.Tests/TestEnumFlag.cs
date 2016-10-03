using System;

namespace Euyuil.Azure.Storage.Helper.Tests
{
    [Flags]
    public enum TestEnumFlag
    {
        None = 0,

        Foo = 1 << 0,

        Bar = 1 << 1,

        Baz = 1 << 2,
    }
}

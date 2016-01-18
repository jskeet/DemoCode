// Variation: abstract class with internal abstract member, can only be derived
// from within the same assembly.

public class UnsealedPrivateConstructor
{
    public static UnsealedPrivateConstructor Foo { get; } = new Derived1();
    public static UnsealedPrivateConstructor Bar { get; } = new Derived2(1);
    public static UnsealedPrivateConstructor Baz { get; } = new Derived2(2);

    public int Value { get; }

    private UnsealedPrivateConstructor(int value)
    {
        Value = value;
    }

    // We could have static factory methods here, too...

    private class Derived1 : UnsealedPrivateConstructor
    {
        internal Derived1() : base(10)
        {
        }
    }

    private class Derived2 : UnsealedPrivateConstructor
    {
        // This might be used in some virtual operation, for example.
        // But the type is still immutable.
        private int OtherValue { get; }

        internal Derived2(int otherValue) : base(5)
        {
            OtherValue = Value;
        }
    }
}

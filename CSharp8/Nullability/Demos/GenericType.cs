namespace Demos
{
    class GenericType<T>
    {
        public T? Value { get; set; }

        // This feels reasonable
        public GenericType(T value) => Value = value;

        public GenericType()
        {
            // This silences the compiler, but it's not clear why.
            // default(string) is still null... and it's also the default
            // value for the field!
            Value = default(T);
        }
    }

    class BangWithGenericClass
    {
        static void Main()
        {
            var nullable = new GenericType<string?>();
            string? nullableString = nullable.Value;

            var nonNullable = new GenericType<string>();
            string nonNullableString = nonNullable.Value;

            var valueType = new GenericType<int>();
            int x = valueType.Value;

            var nullableValueType = new GenericType<int?>();
            int? y = nullableValueType.Value;

            //var hmm = new GenericType<string>();
            //hmm.Value.Clone();
        }
    }
}

namespace Nullability
{
    public class NotNull<T> where T : notnull
    {
        // Invalid:
        // public T? Foo;
    }

    public class MaybeNullableRef<T> where T : class?
    {
        public void Method(T value) { }
    }

    public class NotNullableRef<T> where T : class
    {
        public void Method(T? value) { }
    }

    class Test
    {
        static void Main()
        {
            var valid1 = new NotNull<int>();
            var valid2 = new NotNull<string>();
            var valid3 = new MaybeNullableRef<string>();
            var valid4 = new MaybeNullableRef<string?>();
            var valid5 = new NotNullableRef<string>();

            // Invalid - but only warnings for 3 of them!
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            var invalid1 = new NotNull<int?>();
            var invalid2 = new NotNull<string?>();
#pragma warning restore CS8714

            // Actual errors...
            //var invalid3 = new MaybeNullableRef<int>();
            //var invalid4 = new MaybeNullableRef<int?>();

#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
            var invalid5 = new NotNullableRef<string?>();
#pragma warning restore CS8634
        }
    }
}

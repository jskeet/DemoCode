namespace System.RuntimeCompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public string ParameterName { get; set; }

        public CallerArgumentExpressionAttribute(string parameterName) =>
            ParameterName = parameterName;
    }
}

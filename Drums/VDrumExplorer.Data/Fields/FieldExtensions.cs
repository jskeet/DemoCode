namespace VDrumExplorer.Data.Fields
{
    public static class FieldExtensions
    {
        public static bool IsEnabled(this IField field, ModuleData data) =>
            field.Condition?.IsEnabled(data, field) ?? true;
    }
}

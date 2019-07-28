namespace VDrumExplorer.Data.Json
{
    internal sealed class FieldConditionJson
    {
        /// <summary>
        /// The address of the condition field, relative to the field's parent container.
        /// </summary>
        public HexInt32? Offset { get; set; }
        
        /// <summary>
        /// The required value of the condition field.
        /// </summary>
        public int? RequiredValue { get; set; }
    }
}

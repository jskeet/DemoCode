using System;

namespace MinorFeatures
{
    // Note: this feature is only applicable to structs.
    public class ReadonlyInstanceMembers
    {
        private readonly NotReadonlyStruct field;

        public void CallNormalMethodOnField()
        {
            Console.WriteLine(field.GetValue());
        }

        public void CallReadonlyMethodOnField()
        {
            Console.WriteLine(field.GetValueReadonly());
        }
    }

    public struct NotReadonlyStruct
    {
        int value;

        public void SetValue(int newValue) => value = newValue;
        public readonly int GetValueReadonly() => value;
        public int GetValue() => value;
    }
}

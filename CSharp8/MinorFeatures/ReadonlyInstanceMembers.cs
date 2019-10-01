using System;

namespace MinorFeatures
{
    // Note: this feature is only applicable to structs.
    // IL notes:
    // - ldfld loads the *value* of a field onto the stack
    // - ldflda loads the *home location* of a field onto the stack
    public class ReadonlyInstanceMembers
    {
        private readonly Struct1 field1 = new Struct1(10);
        private readonly Struct2 field2 = new Struct2(20);
        private readonly Struct3 field3 = new Struct3(30);

        public void CallMethods()
        {
            // ldarg.0
            // ldfld valuetype MinorFeatures.Struct1 MinorFeatures.ReadonlyInstanceMembers::field1
            // stloc.0
            // ldloca.s V_0
            // call instance int32 MinorFeatures.Struct1::GetValue()
            field1.GetValue();

            // ldarg.0
            // ldflda valuetype MinorFeatures.Struct2 MinorFeatures.ReadonlyInstanceMembers::field2
            // call instance int32 MinorFeatures.Struct2::GetValue()
            field2.GetValue();

            // ldarg.0
            // ldflda valuetype MinorFeatures.Struct3 MinorFeatures.ReadonlyInstanceMembers::field3
            // call instance int32 MinorFeatures.Struct3::GetValue()
            field3.GetValue();
        }

        public void CallAutoProperties()
        {
            int tmp;

            // ldarg.0
            // ldflda valuetype MinorFeatures.Struct1 MinorFeatures.ReadonlyInstanceMembers::field1
            // call instance int32 MinorFeatures.Struct1::get_Value()
            tmp = field1.Value;

            // ldarg.0
            // ldflda valuetype MinorFeatures.Struct2 MinorFeatures.ReadonlyInstanceMembers::field2
            // call instance int32 MinorFeatures.Struct2::get_Value()
            tmp = field2.Value;

            // ldarg.0
            // ldflda valuetype MinorFeatures.Struct3 MinorFeatures.ReadonlyInstanceMembers::field3
            // call instance int32 MinorFeatures.Struct3::get_Value()
            tmp = field3.Value;
        }

        public void CallManualProperties()
        {
            int tmp;

            // ldarg.0
            // ldfld valuetype MinorFeatures.Struct1 MinorFeatures.ReadonlyInstanceMembers::field1
            // stloc.1
            // ldloca.s V_1
            // call instance int32 MinorFeatures.Struct1::get_Value2()
            tmp = field1.Value2;

            // ldarg.0
            // ldflda valuetype MinorFeatures.Struct2 MinorFeatures.ReadonlyInstanceMembers::field2
            // call instance int32 MinorFeatures.Struct2::get_Value()
            tmp = field2.Value2;

            // ldarg.0
            // ldflda valuetype MinorFeatures.Struct3 MinorFeatures.ReadonlyInstanceMembers::field3
            // call instance int32 MinorFeatures.Struct3::get_Value2()
            tmp = field3.Value2;
        }
    }

    public struct Struct1
    {
        public int Value { get; }
        public int Value2 => Value;
        public int GetValue() => Value;

        public Struct1(int value) => Value = value;
    }

    public struct Struct2
    {
        public int Value { get; }
        
        // All of these have read-only getters
        public readonly int Value2 => Value;
        public readonly int Value3 { get { return Value; } }
        public int Value4
        { 
            readonly get { return Value; }
            set { throw new InvalidOperationException("You can't really set this"); }
        }

        // This has a non-read-only getter, but a read-only setter.
        public int Value5
        {
            get { return Value; }
            readonly set { throw new InvalidOperationException("You can't really set this"); }
        }

        // Both the getter and setter are read-only 
        public readonly int Value6
        {
            get { return Value; }
            set { throw new InvalidOperationException("You can't really set this"); }
        }

        public readonly int GetValue() => Value;

        public Struct2(int value) => Value = value;
    }

    public readonly struct Struct3
    {
        public int Value { get; }
        public int Value2 => Value;
        public int GetValue() => Value;

        public Struct3(int value) => Value = value;
    }

    public struct NotReadonlyStruct
    {
        int value;

        public void SetValue(int newValue) => value = newValue;
        public readonly int GetValueReadonly() => value;
        public int GetValue() => value;
    }
}

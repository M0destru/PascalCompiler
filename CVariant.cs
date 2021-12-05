
namespace PascalCompiler
{
    abstract class CVariant
    {
        public EValueType ValueType { get; set; }
    }

    class IntegerVariant : CVariant
    {
        public int IntegerValue { get; set; }
        public IntegerVariant(int value)
        {
            ValueType = EValueType.Integer;
            IntegerValue = value;
        }

        public override string ToString()
        {
            return $"{IntegerValue}";
        }
    }

    class RealVariant : CVariant
    {
        public double RealValue { get; set; }

        public RealVariant(double value)
        {
            ValueType = EValueType.Real;
            RealValue = value;
        }

        public override string ToString()
        {
            return $"{RealValue}";
        }
    }

    class StringVariant : CVariant
    {
        public string StringValue { get; set; }

        public StringVariant(string value)
        {
            ValueType = EValueType.String;
            StringValue = value;
        }

        public override string ToString()
        {
            return $"{StringValue}";
        }
    }

    class BooleanVariant : CVariant
    {
        public bool BoolValue { get; set; }

        public BooleanVariant(bool value)
        {
            ValueType = EValueType.Boolean;
            BoolValue = value;
        }

        public override string ToString()
        {
            return $"{BoolValue}";
        }
    }
}

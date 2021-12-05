
namespace PascalCompiler
{
    class CVariable
    {
        public string Name { get; set; }
        public CType Type { get; set; }

        public CVariable (string varName, CType varType)
        {
            Name = varName;
            Type = varType;
        }
    }

    abstract class CType
    {
        public EValueType Type { get; set; }

        /* тип fromType приводим к этому типу */
        public abstract bool isDerivedFrom(CType fromType);
    }

    class CIntType: CType
    {
        public CIntType ()
        {
            Type = EValueType.Integer;
        }

        public override bool isDerivedFrom(CType fromType)
        {
            if (Type == fromType.Type || fromType.Type == EValueType.Unknown)
                return true;
            return false;
        }
    }

    class CRealType: CType
    {
        public CRealType ()
        {
            Type = EValueType.Real;
        }

        public override bool isDerivedFrom(CType fromType)
        {
            if (Type == fromType.Type || fromType.Type == EValueType.Integer || fromType.Type == EValueType.Unknown)
                return true;
            return false;
        }
    }

    class CStringType: CType
    {
        public CStringType ()
        {
            Type = EValueType.String;
        }

        public override bool isDerivedFrom(CType fromType)
        {
            if (Type == fromType.Type || fromType.Type == EValueType.Unknown)
                return true;
            return false;
        }
    }

    class CBooleanType: CType
    {
        public CBooleanType ()
        {
            Type = EValueType.Boolean;
        }

        public override bool isDerivedFrom(CType fromType)
        {
            if (Type == fromType.Type || fromType.Type == EValueType.Unknown)
                return true;
            return false;
        }
    }

    class CUnknownType : CType
    {
        public CUnknownType()
        {
            Type = EValueType.Unknown;
        }

        public override bool isDerivedFrom(CType fromType)
        {
            if (Type == fromType.Type)
                return true;
            return false;
        }
    }
}

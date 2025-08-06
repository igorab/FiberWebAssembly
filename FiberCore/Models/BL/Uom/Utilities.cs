using System.Reflection;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace BSFiberCore.Models.BL.Uom
{
    internal class Utilities
    {

    }

    public static class Extensions
    {

        static public string GetDescription(Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null)
                return enumValue.ToString();

            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }

            return enumValue.ToString();
        }

        static public Enum[] GetValues(Enum enumeration)
        {
            FieldInfo[] fields = enumeration.GetType().GetFields(BindingFlags.Static | BindingFlags.Public);
            Enum[] enumerations = new Enum[fields.Length];

            for (var i = 0; i < fields.Length; i++)
                enumerations[i] = (Enum)fields[i].GetValue(enumeration);

            return enumerations;
        }


    }

    [Description("Единицы длины")]
    public enum LengthUnits
    {
        [Description("мм")]
        mm = 0,
        [Description("см")]
        cm = 1,
        [Description("м")]
        m = 2,
    }

    [Description("Единицы Силы")]
    public enum ForceUnits
    {
        [Description("кг")]
        kg = 0,
        [Description("Т")]
        t = 1,
        [Description("Н")]
        n = 2,
        [Description("кН")]
        kn = 3
    }

    [Description("Единицы Момента силы")]
    public enum MomentOfForceUnits
    {
        [Description("кг*мм")]
        kgBymm = 0,
        [Description("кг*см")]
        kgBycm = 1,
        [Description("кг*м")]
        kgBym = 2,

        [Description("Т*мм")]
        tBymm = 3,
        [Description("Т*см")]
        tBycm = 4,
        [Description("Т*м")]
        tBym = 5,

        [Description("Н*мм")]
        nBymm = 6,
        [Description("Н*см")]
        nBycm = 7,
        [Description("Н*м")]
        nBym = 8,

        [Description("кН*мм")]
        knBymm = 9,
        [Description("кН*см")]
        knBycm = 10,
        [Description("кН*м")]
        knBym = 11
    }
}

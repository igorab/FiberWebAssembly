using System.ComponentModel;

namespace BSFiberCore.Models.BL.Lib
{
    [Description("Тип предела текучести")]
    public enum TypeYieldStress
    {
        [Description("Не определено")]
        None = 0,
        [Description("Физический")]
        Physical = 1,
        [Description("Условный")]
        Offset = 2,
    }
}

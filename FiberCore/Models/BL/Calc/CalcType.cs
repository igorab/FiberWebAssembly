using System.ComponentModel;

namespace BSFiberCore.Models.BL.Calc
{
    [Description("Тип расчета")]
    public enum CalcType
    {
        [Description("Проверка сечения")]
        Section = 0,
        [Description("Статическое равновесие")]
        Static = 1,
        [Description("Нелинейная деформационная модель")]
        Nonlinear = 2,
        [Description("Расчет балки")]
        BeamCalc = 3
    }

}

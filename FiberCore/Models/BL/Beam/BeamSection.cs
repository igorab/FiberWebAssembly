using System.ComponentModel;

namespace BSFiberCore.Models.BL.Beam
{
    /// <summary>
    /// Сечение балки
    /// </summary>
    [Flags, Description("Сечение балки")]
    public enum BeamSection
    {
        /// <summary>
        /// Произвольное сечение
        /// </summary>
        [Description("Cечение произвольное")]
        Any = 0,
        /// <summary>
        /// Тавр с верхней полкой
        /// </summary>
        [Description("Тавровое сечение")]
        TBeam = 1,
        /// <summary>
        /// Двутавр
        /// </summary>
        [Description("Двутавровое сечение")]
        IBeam = 2,
        /// <summary>
        /// Кольцо
        /// </summary>
        [Description("Кольцевое сечение")]
        Ring = 3,
        /// <summary>
        /// Прямоугольник
        /// </summary>
        [Description("Прямоугольное сечение")]
        Rect = 4,
        /// <summary>
        /// Тавр с нижней полкой
        /// </summary>
        [Description("Тавр нижняя полка")]
        LBeam = 5
    }
}

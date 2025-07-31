namespace BSFiberCore.Models.BL
{
    public interface IMaterial
    {
        /// <summary>
        /// Наименование материала
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Модуль упругости
        /// </summary>
        double E_young { get; }
    }


    /// <summary>
    /// Тип диаграм состояния
    /// </summary>
    public enum DeformDiagramType
    {
        /// <summary>
        /// трехлинейная диаграмма
        /// </summary>
        D3Linear = 0,
        /// <summary>
        /// двухлинейная диаграмма
        /// </summary>
        D2Linear = 1,
        DNonlinear = 2
    }
}

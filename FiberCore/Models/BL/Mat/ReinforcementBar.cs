namespace BSFiberCore.Models.BL.Mat
{
    /// <summary>
    /// Стержень арматуры
    /// </summary>
    public class ReinforcementBar
    {
        /// <summary>
        /// Номер стержня арматуры
        /// </summary> 
        public int IndexOfBar { get; set; }

        /// <summary>
        /// Напряжение в арматуре
        /// </summary>
        public double Sig { get; set; }

        /// <summary>
        /// Относительная деформация в арматуре
        /// </summary>
        public double Eps { get; set; }

        /// <summary>
        /// Диаметр стержня арматуры
        /// </summary>
        public double Diameter { get; set; }

        /// <summary>
        /// Класс Арматуры
        /// </summary>
        public string Type { get; set; }

    }
}

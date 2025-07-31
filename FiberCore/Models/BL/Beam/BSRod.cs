using BSFiberCore.Models.BL.Mat;
using System;
using System.Security.Policy;

namespace BSFiberCore.Models.BL.Beam
{
    /// <summary>
    /// Тип арматуры : продольная / поперечная
    /// </summary>
    public enum RebarLTType
    {
        Longitudinal = 0,
        Transverse = 1
    }


    /// <summary>
    /// Арматурные стержни - расстановка
    /// </summary>
    public class BSRod
    {
        /// <summary>
        /// Номер стержня
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Координата X Ц.Т.
        /// </summary>
        public double CG_X { get; set; }

        /// <summary>
        /// Координата Y Ц.Т.
        /// </summary>
        public double CG_Y { get; set; }

        /// <summary>
        /// Номинальный диаметр стержня, см
        /// </summary>
        public string Dnom { get; set; }

        /// <summary>
        /// Диаметр стержня, см
        /// </summary>
        public double D { get; set; }

        /// <summary>
        /// Тип сечения балки
        /// </summary>
        public BeamSection SectionType { get; set; }

        /// <summary>
        /// Площадь стержня
        /// </summary>
        public double As { get => Math.PI * Math.Pow(D, 2) / 4; }

        /// <summary>
        /// Расстояние до ц.т. растянутой арматуры
        /// </summary>
        public double a { get; set; }

        /// <summary>
        /// Расстояние до ц.т. сжатой арматуры
        /// </summary>
        public double a1 { get; set; }

        /// <summary>
        /// Коэффициент упругости
        /// </summary>
        public double Nu { get; set; }

        /// <summary>
        /// Тип продольная/поперечная
        /// </summary>
        public RebarLTType LTType { get; set; }
       
        /// <summary>
        /// Материал
        /// </summary>
        public BSMatRod MatRod { get; set; }
    }

    public class NdmSection
    {
        /// <summary>
        /// Номер стержня
        /// </summary>
        public string Num { get; set; }

        /// <summary>
        /// Номер точки
        /// </summary>        
        public int N { get; set; }

        /// <summary>
        /// Координата точки X 
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Координата точки Y
        /// </summary>
        public double Y { get; set; }
    }
}

using System.Diagnostics;

namespace BSFiberCore.Models.BL.Mat
{
    /// <summary>
    ///  Материал стержня арматуры
    /// </summary>
    public class BSMatRod : IMaterial
    {
        public string Name => "Сталь";
        public double E_young => Es;

        // Класс
        public string RCls { get; set; }

        /// <summary>
        /// модуль упругости, МПа
        /// </summary>
        public double Es { get; set; }

        //Нормативное сопротивление растяжению кг/см2
        public double Rsn { get; set; }

        // Расчетное сопротивление растяжению кг/см2
        public double Rs { get; set; }

        // Расчетное сопротивление растяжению 2 гр, кг/см2
        public double Rs_ser => Rsn;
        
        // Расчетное сопротивление сжатию кг/см2
        public double Rsc { get; set; }

        // Расчетное сопротивление сжатию 2 гр, кг/см2
        public double Rsc_ser => Rsn;

        // Площадь растянутой арматуры
        public double As { get; set; }

        // Площадь сжатой арматуры
        public double As1 { get; set; }
        /// <summary>
        /// расстояние от грани до центра растянутой арматуры, см
        /// </summary>
        public double a_s { get; set; }
        /// <summary>
        /// расстояние от грани до центра сжатой арматуры, см
        /// </summary>
        public double a_s1 { get; set; }

        /// <summary>
        /// расстояние от центра тяжести до центра растянутой арматуры, см
        /// </summary>
        public double h0_t { get; set; }
        /// <summary>
        /// расстояние от центра тяжести сечения до центра сжатой арматуры, см
        /// </summary>
        public double h0_p { get; set; }

        /// <summary>
        /// Флаг, характеризующий нажатую на форме галочку "Армирование"
        /// true - галочка нажата
        /// </summary>
        public bool Reinforcement { get; set; }

        public double SelectedRebarDiameter { get; set; }


    /// <summary>
    /// коэффициент упругости
    /// </summary>
    public double Nju_s { get; set; }

        /// <summary>
        /// СП 6.1.25
        /// </summary>
        /// <param name="diagramType">физический или условный предел текучести </param>
        /// <returns></returns>
        public double Eps_s_ult(DeformDiagramType diagramType) 
        { 
            double esult = 0;

            if (diagramType == DeformDiagramType.D2Linear)
            {
                esult = 0.025;
            }
            else if (diagramType == DeformDiagramType.D3Linear)
            {
                esult = 0.015;
            }

            return esult;
        }

        /// <summary>
        /// Значения относительных деформаций арматуры для арматуры с физическим пределом текучести СП 63 п.п. 6.2.11
        /// </summary>        
        public double epsilon_s() => Es != 0 ? Rs / Es : 0;

        public double e_s0 { get; set; }
        public double e_s2 { get; set; }

        public BSMatRod()
        {

        }

        public BSMatRod(double _Es)
        {
            Es = _Es;
        }


        /// <summary>
        /// Диграмма состояния трехлинейная
        /// </summary>
        /// <param name="_e_s">отн деформация </param>
        /// <param name="_res">СП 63.13 6.2.15 </param>
        /// <returns>Напряжение</returns>
        public double Eps_StateDiagram3L(double _e_s, out int _res, int _group = 1)
        {
            double rs = Rs;
            _res = 0;
            double sigma_s = 0;
            double sigma_s1 = 0.9 * rs;
            double sigma_s2 = 1.1 * rs;
            double e_s0 = rs / Es + 0.002;
            double e_s1 = sigma_s1 / Es;
            
            if (0 <= _e_s && _e_s <= e_s1)
            {
                sigma_s = rs * _e_s;
            }
            else if (e_s1 <= _e_s && _e_s <= e_s2)
            {
                sigma_s = ((1 - sigma_s1 / rs) * (_e_s - e_s1) / (e_s0 - e_s1) + sigma_s1 / rs) * rs;

                if (sigma_s > sigma_s2) sigma_s = sigma_s2;
            }            
            else if (_e_s > e_s2)
            {
                Debug.Assert(true, "Превышена деформация арматуры");
                sigma_s = 0;
            }

            return sigma_s;
        }

        //
        /// <summary>
        /// Диаграмма состояния двухлинейная 
        /// на сжатие и растяжение
        /// </summary>
        /// <param name="_e"></param>
        /// <returns></returns>        
        public double Eps_StDiagram2L(double _e, out int _res, int _group = 1)
        {
            double sgm = 0;
            double rs = Rs;

            _res = 0;

            if (0 < _e && _e < e_s0)
            {
                sgm = Es * _e;
            }
            else if (e_s0 <= _e && _e <= e_s2)
            {
                sgm = rs;
            }
            else if (_e > e_s2) //теоретически это разрыв
            {
                Debug.Assert(true, "Превышен предел прочности (временное сопротивление) ");
                _res = -1;
                if (_group == 1)
                    sgm = 0;
                else if (_group == 2)
                    sgm = Rs_ser;
            }

            return sgm;
        }



        /// <summary>
        /// формула по 6.2.15 из  СП63
        /// </summary>
        /// <param name="_Rs"></param>
        /// <param name="_Es"></param>
        /// <returns></returns>
        public static decimal NumEps_s1(decimal _Rs, decimal _Es)
        {
            // Для трехлинейной диаграммы деформирования
            if (_Rs == 0 || _Es == 0)
                return 0;
            
            return _Rs * 0.9m  / _Es;
        }


        /// <summary>
        /// формула 6.12 из  СП63
        /// </summary>
        /// <param name="_Rs"></param>
        /// <param name="_Es"></param>
        /// <returns></returns>
        public static decimal NumEps_s0(decimal _Rs, decimal _Es)
        {
            // Для трехлинейной диаграммы деформирования
            if (_Rs == 0 || _Es == 0)
                return 0;

            return NumEps_s1(_Rs, _Es) + 0.002m;
        }
    }
}

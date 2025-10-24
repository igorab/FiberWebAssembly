using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScottPlot;
using ScottPlot.Plottables;

namespace BSFiberCore.Models.BL.Draw
{
    /// <summary>
    /// класс для формирования цветового диапазона
    /// </summary>
    public class ColorScale
    {
        /// <summary>
        /// Кол-во участков на которое будет разбит диапазон значений от 0 до maxValue (maxValue)
        /// </summary>
        private int numOfSegments;

        /// <summary>
        /// Список всех значений
        /// </summary>
        private List<double>? values { get; set; }

        // предельные значения
        private double ultMin;
        private double ultMax;

        // максимальное значение насыщенности цвета
        // чем больше число (от 0 до 255), тем более насыщенный цвет
        private int maxValueColor;

        // числовая длина диапазона
        private double deltaPositive;
        private double deltaNegative;
        // цветовая длина диапазона
        private int deltaRGB;


        public ColorScale(List<double>? values, double ultMax, double ultMin)
        {
            this.values = values;
            this.ultMax = ultMax;
            this.ultMin = ultMin;

            numOfSegments = 25;
            maxValueColor = 255;

            double minValue = values?.Min()??0;
            double maxValue = values?.Max()??0;
            deltaPositive = maxValue / numOfSegments;
            deltaNegative = minValue / numOfSegments;
            deltaRGB = maxValueColor / numOfSegments;
        }


        /// <summary>
        /// Раскрасить полигон
        /// </summary>
        /// <param name="measured_value"></param>
        /// <param name="typeOfColor"></param>
        /// <returns></returns>
        public void ColorThePolygon(Polygon poly, double measuredValue, int typeOfColor)
        {
            poly.LineColor = ColorDependLimitValue(measuredValue);
            poly.LineWidth = LineWidthDependLimitValue(measuredValue);
            poly.FillColor = ColorFromScale(measuredValue, typeOfColor);
        }


        /// <summary>
        /// Создать цветовую шкалу
        /// </summary>
        /// <param name="typeOfColor"></param>
        /// <returns></returns>
        public Plot CreateColorScale(int typeOfColor, string xLabel = null)
        {
            List<Bar> positiveBars = new List<Bar>();
            List<Bar> negativeBars = new List<Bar>();

            for (int i = 1; numOfSegments >= i; i++)
            {
                double value0 = deltaPositive * (i - 1);
                double value1 = deltaPositive * i;
                var clr = ColorFromScale(value1, typeOfColor);

                ScottPlot.Bar tmpBar = new Bar() { 
                    Position = 1, 
                    ValueBase = value0, 
                    Value = value1, 
                    FillColor = clr
                };
                positiveBars.Add(tmpBar);

                value0 = deltaNegative * (i - 1);
                value1 = deltaNegative * i;
                tmpBar = new Bar() { 
                    Position = 1, 
                    ValueBase = value0, 
                    Value = value1, 
                    FillColor = ColorFromScale(value1, typeOfColor) 
                };
                negativeBars.Add(tmpBar);
            }
            positiveBars.AddRange(negativeBars);

            Plot myPlot = new Plot();
            ScottPlot.Palettes.Category10 palette = new ScottPlot.Palettes.Category10();
            myPlot.Add.Bars(positiveBars);
            myPlot.Axes.Bottom.TickLabelStyle.IsVisible = false;
            myPlot.Axes.Bottom.MajorTickStyle.Length = 0;
            myPlot.Axes.Bottom.MinorTickStyle.Length = 0;
            myPlot.Grid.XAxisStyle.IsVisible = false;
            myPlot.Grid.YAxisStyle.IsVisible = false;

            if (xLabel != null) myPlot.XLabel(xLabel);
            
            return myPlot;
        }



        /// <summary>
        /// Получить цвет для конкретного значения и типа цветовой политры
        /// </summary>
        /// <param name="measured_value"></param>
        /// <param name="typeOfColor"></param>
        /// <returns></returns>
        private ScottPlot.Color ColorFromScale(double measuredValue, int typeOfColor)
        {
            byte redColor;
            byte greenColor;
            byte blueColor;

            if (measuredValue > 0)
            {
                int m = CalcPositiveRangeNumber(measuredValue);

                switch (typeOfColor)
                {
                    case 2:
                        redColor = (byte)maxValueColor;
                        greenColor = (byte)(maxValueColor/2 + m * deltaRGB / 2);
                        blueColor = (byte)(m * deltaRGB);
                        break;
                    default:
                        redColor = (byte)maxValueColor;
                        greenColor = (byte)(m * deltaRGB);
                        blueColor = (byte)(m * deltaRGB);
                        break;
                }
            }            
            else
            {
                int m = CalcNegativeRangeNumber(measuredValue);

                switch (typeOfColor)
                {
                    case 2:
                        redColor = (byte)(m * deltaRGB);
                        greenColor = (byte)(maxValueColor/2 + m * deltaRGB / 2);
                        blueColor = (byte)maxValueColor;
                        break;
                    default:
                        redColor = (byte)(m * deltaRGB);
                        greenColor = (byte)(m * deltaRGB);
                        blueColor = (byte)maxValueColor;
                        break;
                }
            }
            return new ScottPlot.Color(redColor, greenColor, blueColor);
        }


        /// <summary>
        /// Получить цвет в зависимости от превышения нормативных значений
        /// </summary>
        /// <param name="measuredValue"></param>
        /// <returns></returns>
        private ScottPlot.Color ColorDependLimitValue(double measuredValue)
        {
            if (ultMax > measuredValue && ultMin < measuredValue)
            {
                return Colors.Grey;
            }
            return new ScottPlot.Color(250, 0, 127);

        }


        /// <summary>
        /// Получить толщину линии в зависимости от превышения нормативных значений
        /// </summary>
        /// <param name="measuredValue"></param>
        /// <returns></returns>
        private int LineWidthDependLimitValue(double measuredValue)
        {
            if (ultMax > measuredValue && ultMin < measuredValue)
            {
                return 1;
            }
            return 3;
        }


        /// <summary>
        /// определить номер отрицательного диапазона для значения measuredValue
        /// </summary>
        /// <param name="measuredValue"></param>
        /// <returns></returns>
        private int CalcNegativeRangeNumber(double measuredValue)
        {
            if (values != null && values.Min() <= measuredValue)
            { 
                return (int)Math.Floor((values.Min() - measuredValue) / deltaNegative); 
            }
            else
            { 
                return 0; 
            }
        }


        /// <summary>
        /// определить номер положитеьного диапазона для значения measuredValue
        /// </summary>
        /// <param name="measuredValue"></param>
        /// <returns></returns>
        private int CalcPositiveRangeNumber(double measuredValue)
        {
            if (values != null && values.Max() >= measuredValue)
            { 
                return (int)Math.Floor((values.Max() - measuredValue) / deltaPositive); 
            }
            else
            { 
                return 0; 
            }
        }
        
    }
}

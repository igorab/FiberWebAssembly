namespace FiberCore.Models.BL.Diagram
{
    public class Charting
    {
        public static object ChartImageFormat { get; internal set; }

        public static Chart Chart()
        {
            return new Chart();
        }

        public class Series
        {
        }
       
        internal class ChartArea
        {
        }

        internal class Title
        {
        }
    }
}

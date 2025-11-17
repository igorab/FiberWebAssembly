
namespace FiberCore.Models.BL.Diagram
{
    public class Chart
    {
        public string Name { get; internal set; }
        public string Text { get; internal set; }
        public object ChartAreas { get; internal set; }

        internal void SaveImage(string pictureName)
        {
            
        }
    }
}

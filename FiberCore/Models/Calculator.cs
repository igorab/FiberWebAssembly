namespace BSFiberCore.Models
{
    public class Calculator
    {
        public double Length { get; set; }
        public double Width { get; set; }

        public double CalculatePerimeter()
        {
            return 2 * (Length + Width);
        }

        public double CalculateArea()
        {
            return Length * Width;
        }

    }
}

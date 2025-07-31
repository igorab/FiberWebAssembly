namespace BSFiberCore.Models.BL
{
    public interface ICalc
    {
        bool Calculate();

        Dictionary<string, double> Results();
    }
}

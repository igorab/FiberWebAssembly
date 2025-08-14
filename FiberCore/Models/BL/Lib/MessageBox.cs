namespace BSFiberCore.Models.BL.Lib
{
    public class MessageBox
    {
        public static string Show(string _txt, string _header = "")
        {
            return _header + _txt;
        }
    }
}
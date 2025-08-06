using BSFiberCore.Models.BL.Beam;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace BSFiberCore.Models.BL.Lib
{
    public class BSHelper
    {
        public const double Epsilon = 0.00001d;

        public string UnitLength = Units.L;

        public static double AreaCircle(double _D) => Math.PI * _D * _D / 4d;

        public static double DCircle(double _area) => Math.Sqrt(4 * _area / Math.PI);

        public static double mm2sm(double _mm) => _mm * 0.1d;

        // конвертор сил
        public static double Kg2T(double _kg) => _kg * 0.001d;
        public static double kN2Kgs(double? _kN) => _kN * 101.97162129779284d ?? 0;

        public static double NU2U(double? _N, double _coef = 1.0) => _N * _coef ?? 0;
        public static double MU2U(double? _M, double _coef = 1.0) => _M * _coef ?? 0;

        public static double Kgs2kN(double _kgs, int _rnd = 0) => (_rnd >0) ? Math.Round(_kgs * 0.00980665d, _rnd) : _kgs * 0.00980665d;        

        // конвертор моментов
        public static double Kgsm2Tm(double _kgsm) => _kgsm * 0.00001d;
        public static double kgssm2kNsm(double? _kgssm) => _kgssm * 0.00980665d ?? 0;
        public static double kgssm2Nmm(double? _kgssm) => _kgssm * 98.0665 ?? 0;
        public static double kNsm2kgssm(double? _kNsm) => _kNsm * 101.97162129779284d ?? 0;
        public static double kNm2kgssm(double? _kNm) => _kNm * 10197.16212978d ?? 0;
        public static double Nmm2kgssm(double? _Nmm) => _Nmm * 0.010197162d ?? 0;

        // конвертор напряжений 

        public static double MPA2kgsm2(double _mpa) => 10.197162d * _mpa ;
        public static double MPA2kgsm2(double? _mpa) => 10.197162d * _mpa ?? 0;
        public static double MPA2kNsm2(double? _mpa) => 0.1d * _mpa ?? 0;
        public static double kNsm2toMPa(double _KNsm2) => 10d * _KNsm2 ;

        // конвертор сопротивлений
        public static double RU2U(double _kgssm2, double _coef = 1.0) => _kgssm2 * _coef /* 0.00980664999999998d*/;

        public static double Kgssm2ToKNsm2(double _kgssm2, int _dec) => Math.Round(_kgssm2 * 0.00980664999999998d ,_dec) ;

        /// <summary>
        /// конвертор напряжений
        /// </summary>        
        public static double SigU2U(double? _KNsm2, double _coef = 1.0) => _KNsm2 * _coef /*101.97162129779d*/ ?? 0;

        public static double Kgsm2MPa(double? _val, int _dec = 2) => Math.Round( 0.098067d * _val ?? 0, _dec);

        public static double Dec2Dbl(decimal _val, int _dec=2) => Math.Round( (double)_val, _dec);

        public const string Concrete = "Бетон";
        
        public const string FiberConcrete = "Фибробетон";

        public const string Rebar = "Арматура"; // TO Do утвердить нейминг константы

        public const string TwoLineDiagram = "Двухлинейная";

        public const string ThreeLineDiagram = "Трехлинейная";

        public const string IgnoreHumidity = "Не учитывать";
        
        /// <summary>
        /// физический предел текучести
        /// </summary>
        public const string PhysicalYieldStress = "physical";
        /// <summary>
        /// условный предел текучести
        /// </summary>
        public const string OffsetYieldStress = "offset";

        /// <summary>
        /// Является ли сеченние составленным из прямоугольников
        /// </summary>
        /// <param name="_BeamSection">Тип сечения</param>
        /// <returns></returns>
        public static bool IsRectangled(BeamSection _BeamSection)
        {
            return _BeamSection == BeamSection.Rect || 
                   _BeamSection == BeamSection.TBeam || 
                   _BeamSection == BeamSection.LBeam || 
                   _BeamSection == BeamSection.IBeam;
        }

        /// <summary>
        /// Является ли сечение тавровым
        /// </summary>
        /// <param name="_BeamSection">Тип сечения</param>
        /// <returns></returns>
        public static bool IsITL(BeamSection _BeamSection)
        {
            return _BeamSection == BeamSection.TBeam ||
                   _BeamSection == BeamSection.LBeam ||
                   _BeamSection == BeamSection.IBeam;
        }

        
        public static string MakeImageSrcData(Image _image, string _filename = "img.png")
        {
            if (_image == null) return "";

            string _img = "";
           
            try
            {
                using (Image img = _image)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Png);

                        byte[] imgBytes = ms.ToArray();
                        string _extension = Path.GetExtension(_filename).Replace(".", "").ToLower();

                        _img = String.Format("\"data:image/{0};base64, {1}\" alt = \"{2}\" ", _extension, Convert.ToBase64String(imgBytes), _filename);
                    }
                }
            }
            catch (Exception _e)
            {
                _img = _e.Message;
            }

            return _img;
        }


        public static string ImgResource(BeamSection _bs, bool _useReinforcement = false)
        {
            string _img = "";

            switch (_bs)
            {
                case BeamSection.Rect:
                    if (_useReinforcement)
                        _img = "Rect_Rods.PNG";
                    else
                        _img = "FiberBeton.PNG";
                    break;
                case BeamSection.TBeam:
                    _img = "TBeam.jpg";
                    break;
                case BeamSection.LBeam:
                    _img = "LBeam.jpg";
                    break;
                case BeamSection.IBeam:
                    if (_useReinforcement)
                        _img = "IBeamArm.PNG";
                    else
                        _img = "IBeam.PNG";
                    break;
                case BeamSection.Ring:
                    _img = "Ring.PNG";
                    break;
            }

            //_img = "Rect_N.PNG";

            return _img;
        }


        public static string EnumDescription(Enum myEnumVariable)
        {
            string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;
            return desc; 
        }

        public static double ToDouble(string _txtNum)
        {
            NumberFormatInfo formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
            double d_num;
            try
            {
                d_num = Convert.ToDouble(_txtNum, formatter);
            }
            catch (System.FormatException)
            {
                formatter.NumberDecimalSeparator = ",";
                d_num = Convert.ToDouble(_txtNum, formatter);
            }
            return d_num;
        }
    }

    public static class EnumHelper
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }
    }

    public static class StringExtensions
    {
        private static char open = '[';
        private static char close = ']';
        public static string[] Brackets(this string str)
        {
            //Set up vars
            StringBuilder[] builders = new StringBuilder[str.Count(x => x == open)];
            for (int h = 0; h < builders.Count(); h++)
                builders[h] = new StringBuilder();
            string[] results = new string[builders.Count()];
            bool[] tracker = new bool[builders.Count()];
            int haveOpen = 0;
            //loop up string
            for (int i = 0; i < str.Length; i++)
            {
                //if opening bracket
                if (str[i] == open)
                    tracker[haveOpen++] = true;
                //loop over tracker
                for (int j = 0; j < tracker.Length; j++)
                    if (tracker[j])
                        //if in this bracket append to the string
                        builders[j].Append(str[i]);
                //if closing bracket
                if (str[i] == close)
                    tracker[Array.FindLastIndex<bool>(tracker, p => p == true)] = false;
            }
            for (int i = 0; i < builders.Length; i++)
                results[i] = builders[i].ToString();
            return results;
        }
    }


}

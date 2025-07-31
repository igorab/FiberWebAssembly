namespace BSFiberCore.Models.BL.Uom
{
    /// <summary>
    /// класс описывающий ед изм длины
    /// </summary>
    public class LengthMeasurement
    {
        /// <summary>
        /// Установленные пользователем ед изм
        /// </summary>
        private LengthUnits _customUnit;
        /// <summary>
        /// Ед изм Установленные в модели
        /// </summary>
        private LengthUnits _modelUnit;

        public LengthUnits CustomUnit
        { 
            get { return _customUnit; }
            set { _customUnit = value; }
        }


        public LengthUnits ModelUnit
        {
            get { return _modelUnit; }
            set { _modelUnit = value; }
        }


        /// <summary>
        /// Список с названиями ед измерения из LengthUnits (Description)
        /// </summary>
        public static List<string> ListOfName
        { 
            get 
            {
                List<string> tmpList = new List<string>(); 
                foreach (var tmp in (LengthUnits[])Enum.GetValues(typeof(LengthUnits)))
                {
                    string res = Extensions.GetDescription(tmp);
                    tmpList.Add(res);
                }
                return tmpList;
            }
            //private set { _listOfLength = value; }
        }

        /// <summary>
        /// Список значений из LengthUnits
        /// </summary>
        public static List<LengthUnits> ListOfValue
        {
            get
            {
                //List<int> tmpListOld = new List<int>();
                //foreach (var tmp in (LengthUnits[])Enum.GetValues(typeof(LengthUnits)))
                //{
                //    string res = Extensions.GetDescription(tmp);
                //    tmpListOld.Add(1);
                //}

                List<LengthUnits> tmpList = new List<LengthUnits>();
                foreach (var tmp in (LengthUnits[])Enum.GetValues(typeof(LengthUnits)))
                {
                    tmpList.Add(tmp);
                }
                return tmpList;
            }
        }


        public LengthMeasurement()
        {
            _customUnit = 0;
            _modelUnit = 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelUnitOfMeasurement">Ед измерения модели, в которую будут конвертироваться значения перед расчетом</param>
        public LengthMeasurement(LengthUnits modelUnitOfMeasurement)
        {
            _customUnit = 0;
            _modelUnit = modelUnitOfMeasurement;
        }

        #region Методы перевода ед измерения
        public static double mmTocm(double inputValue) => inputValue / 10;
        public static double mmTom(double inputValue) => inputValue / 1000;

        public static double cmTomm(double inputValue) => inputValue * 10;
        public static double cmTom(double inputValue) => inputValue / 100;

        public static double mTomm(double inputValue) => inputValue * 1000;
        public static double mTocm(double inputValue) => inputValue * 100;
        #endregion


        public double CustomToModelUnit(double customValue)
        {
            return ConvertValue(_customUnit, _modelUnit, customValue);
        }


        public double ModelToCustomUnit(double customValue)
        {
            return ConvertValue(_modelUnit, _customUnit, customValue);
        }


        /// <summary>
        /// Универсальная функция для перевода из одной ед изм в другую
        /// </summary>
        /// <param name="inputUnit">текущая ед измерения значения value</param>
        /// <param name="outputUnit">ед измерения в которую будет осуществлен перевод</param>
        /// <param name="value">значение</param>
        /// <returns></returns>
        public static double ConvertValue(LengthUnits inputUnit, LengthUnits outputUnit, double value)
        {
            if (inputUnit == outputUnit)
            { return value; }

            double res;
            if (inputUnit == LengthUnits.mm && outputUnit == LengthUnits.cm)
                res = mmTocm(value);
            else if (inputUnit == LengthUnits.mm && outputUnit == LengthUnits.m)
                res = mmTom(value);
            else if (inputUnit == LengthUnits.cm && outputUnit == LengthUnits.mm)
                res = cmTomm(value);
            else if (inputUnit == LengthUnits.cm && outputUnit == LengthUnits.m)
                res = cmTom(value);
            else if (inputUnit == LengthUnits.m && outputUnit == LengthUnits.mm)
                res = mTomm(value);
            else if (inputUnit == LengthUnits.m && outputUnit == LengthUnits.cm)
                res = mTocm(value);
            else { res = 0; }
            return res;
        }

        public static LengthUnits DefineEnumValue(string description)
        {
            LengthUnits res = new LengthUnits();
            for (int i = 0; i < ListOfName.Count; i++)
            {
                if (ListOfName[i] == description)
                {
                    res = (LengthUnits)ListOfValue[i];
                    break;
                }
            }
            return res;
        }

        //public static void GetNameFromEnum(LengthUnits enumValue)
        //{


        //    foreach (var tmp in (LengthUnits[])Enum.GetValues(typeof(LengthUnits)))
        //    {
        //        string res = Extensions.GetDescription(tmp);
        //        tmpList.Add(res);
        //    }

        //}
    }


    /// <summary>
    /// Единицы измерения Силы
    /// </summary>
    public class ForceMeasurement
    {
        /// <summary>
        /// Установленные пользователем ед изм
        /// </summary>
        private ForceUnits _customUnit;
        /// <summary>
        /// Ед изм Установленные в модели
        /// </summary>
        private ForceUnits _modelUnit;


        public ForceUnits CustomUnit
        {
            get { return _customUnit; }
            set { _customUnit = value; }
        }

        public ForceUnits ModelUnit
        {
            get { return _modelUnit; }
            set { _modelUnit = value; }
        }

        public static List<string> ListOfName
        {
            get
            {
                List<string> tmpList = new List<string>();
                foreach (var tmp in (ForceUnits[])Enum.GetValues(typeof(ForceUnits)))
                {
                    string res = Extensions.GetDescription(tmp);
                    tmpList.Add(res);
                }
                return tmpList;
            }
        }

        /// <summary>
        /// Список значений из ForceUnits
        /// </summary>
        public static List<ForceUnits> ListOfValue
        {
            get
            {
                List<ForceUnits> tmpList = new List<ForceUnits>();
                foreach (var tmp in (ForceUnits[])Enum.GetValues(typeof(ForceUnits)))
                {
                    tmpList.Add(tmp);
                }
                return tmpList;
            }
        }

        public ForceMeasurement()
        {
            _customUnit = 0;
            _modelUnit = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelUnitOfMeasurement">Ед измерения модели, в которую будут конвертироваться значения перед расчетом</param>
        public ForceMeasurement(ForceUnits modelUnitOfMeasurement)
        {
            _customUnit = 0;
            _modelUnit = modelUnitOfMeasurement;
        }

        #region Методы перевода ед измерения
        public static double kgTot(double inputValue) => inputValue / 1000;
        public static double kgTon(double inputValue) => inputValue * 9.81;
        public static double kgTokn(double inputValue) => inputValue * 9.81 / 1000;

        public static double tTokg(double inputValue) => inputValue * 1000;
        public static double tTon(double inputValue) => inputValue * 9.81 * 1000;
        public static double tTokn(double inputValue) => inputValue * 9.81;

        public static double nTokg(double inputValue) => inputValue / 9.81;
        public static double nTot(double inputValue) => inputValue / (1000 * 9.81);
        public static double nTokn(double inputValue) => inputValue / 1000;

        public static double knTokg(double inputValue) => inputValue / 9.81 *1000 ;
        public static double knTot(double inputValue) => inputValue / 9.81;
        public static double knTon(double inputValue) => inputValue * 1000;
        #endregion

        public double CustomToModelUnit(double customValue)
        {
            return ConvertValue(_customUnit, _modelUnit, customValue);
        }

        public double ModelToCustomUnit(double customValue)
        {
            return ConvertValue(_modelUnit, _customUnit, customValue);
        }

        /// <summary>
        /// Универсальная функция для перевода из одной ед изм в другую
        /// </summary>
        /// <param name="inputUnit">текущая ед измерения значения value</param>
        /// <param name="outputUnit">ед измерения в которую будет осуществлен перевод</param>
        /// <param name="value">значение</param>
        /// <returns></returns>
        public static double ConvertValue(ForceUnits inputUnit, ForceUnits outputUnit, double value)
        {
            if (inputUnit == outputUnit)
            { return value; }

            double res;
            if (inputUnit == ForceUnits.kg && outputUnit == ForceUnits.t)
                res = kgTot(value);
            else if (inputUnit == ForceUnits.kg && outputUnit == ForceUnits.n)
                res = kgTon(value);
            else if (inputUnit == ForceUnits.kg && outputUnit == ForceUnits.kn)
                res = kgTokn(value);

            else if (inputUnit == ForceUnits.t && outputUnit == ForceUnits.kg)
                res = tTokg(value);
            else if (inputUnit == ForceUnits.t && outputUnit == ForceUnits.n)
                res = tTon(value);
            else if (inputUnit == ForceUnits.t && outputUnit == ForceUnits.kn)
                res = tTokn(value);

            else if (inputUnit == ForceUnits.n && outputUnit == ForceUnits.kg)
                res = nTokg(value);
            else if (inputUnit == ForceUnits.n && outputUnit == ForceUnits.t)
                res = nTot(value);
            else if (inputUnit == ForceUnits.n && outputUnit == ForceUnits.kn)
                res = nTokn(value);

            else if (inputUnit == ForceUnits.kn && outputUnit == ForceUnits.kg)
                res = knTokg(value);
            else if (inputUnit == ForceUnits.kn && outputUnit == ForceUnits.t)
                res = knTot(value);
            else if (inputUnit == ForceUnits.kn && outputUnit == ForceUnits.n)
                res = knTon(value);
            else { res = 0; }
            return res;
        }

        public static ForceUnits DefineEnumValue(string description)
        {
            ForceUnits res = new ForceUnits();
            for (int i = 0; i < ListOfName.Count; i++)
            {
                if (ListOfName[i] == description)
                {
                    res = (ForceUnits)ListOfValue[i];
                    break;
                }
            }
            return res;
        }
    }

    /// <summary>
    /// Единицы измерения Момента силы
    /// </summary>
    public class MomentOfForceMeasurement
    {

        private ForceMeasurement _forceMeasure;
        private LengthMeasurement _lengthMeasure;

        /// <summary>
        /// Установленные пользователем ед изм
        /// </summary>
        private MomentOfForceUnits _customUnit;
        /// <summary>
        /// Ед изм Установленные в модели
        /// </summary>
        private MomentOfForceUnits _modelUnit;


        public MomentOfForceUnits CustomUnit
        {
            get { return _customUnit; }
            set { _customUnit = value; }
        }

        public MomentOfForceUnits ModelUnit
        {
            get { return _modelUnit; }
            set { _modelUnit = value; }
        }

        public static List<string> ListOfName
        {
            get
            {
                List<string> tmpList = new List<string>();
                foreach (var tmp in (MomentOfForceUnits[])Enum.GetValues(typeof(MomentOfForceUnits)))
                {
                    string res = Extensions.GetDescription(tmp);
                    tmpList.Add(res);
                }
                return tmpList;
            }
        }

        /// <summary>
        /// Список значений из MomentOfForceUnits
        /// </summary>
        public static List<MomentOfForceUnits> ListOfValue
        {
            get
            {
                List<MomentOfForceUnits> tmpList = new List<MomentOfForceUnits>();
                foreach (var tmp in (MomentOfForceUnits[])Enum.GetValues(typeof(MomentOfForceUnits)))
                {
                    tmpList.Add(tmp);
                }
                return tmpList;
            }
        }


        public MomentOfForceMeasurement()
        {
            _customUnit = 0;
            _modelUnit = 0;

            _forceMeasure = new ForceMeasurement();
            _lengthMeasure = new LengthMeasurement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelUnitOfMeasurement">Ед измерения модели, в которую будут конвертироваться значения перед расчетом</param>
        public MomentOfForceMeasurement(MomentOfForceUnits modelUnitOfMeasurement)
        {
            _customUnit = 0;
            _modelUnit = modelUnitOfMeasurement;
            _forceMeasure = new ForceMeasurement();
            _lengthMeasure = new LengthMeasurement();
        }


        #region Методы перевода ед измерения

        #endregion


        public double CustomToModelUnit(double customValue)
        {
            return ConvertValue(_customUnit, _modelUnit, customValue);
        }

        public double ModelToCustomUnit(double customValue)
        {
            return ConvertValue(_modelUnit, _customUnit, customValue);
        }


        /// <summary>
        /// Универсальная функция для перевода из одной ед изм в другую
        /// </summary>
        /// <param name="inputUnit">текущая ед измерения значения value</param>
        /// <param name="outputUnit">ед измерения в которую будет осуществлен перевод</param>
        /// <param name="value">значение</param>
        /// <returns></returns>
        private double ConvertValue(MomentOfForceUnits inputUnit, MomentOfForceUnits outputUnit, double value)
        {
            string inputStr = Extensions.GetDescription(inputUnit);
            string[] inputUnitArray = inputStr.Split('*');

            string outputStr = Extensions.GetDescription(outputUnit);
            string[] outputUnitArray = outputStr.Split('*');

            ForceUnits inputForce = ForceMeasurement.DefineEnumValue(inputUnitArray[0]);
            ForceUnits outputForce = ForceMeasurement.DefineEnumValue(outputUnitArray[0]);
            double newValue = ForceMeasurement.ConvertValue(inputForce, outputForce, value);

            LengthUnits inputLen = LengthMeasurement.DefineEnumValue(inputUnitArray[1]);
            LengthUnits outputLen = LengthMeasurement.DefineEnumValue(outputUnitArray[1]);
            double res = LengthMeasurement.ConvertValue(inputLen, outputLen, newValue);

            return res;
        }


        public static MomentOfForceUnits DefineEnumValue(string description)
        {
            MomentOfForceUnits res = new MomentOfForceUnits();
            for (int i = 0; i < ListOfName.Count; i++)
            {
                if (ListOfName[i] == description)
                {
                    res = (MomentOfForceUnits)ListOfValue[i];
                    break;
                }
            }
            return res;
        }
    }
}

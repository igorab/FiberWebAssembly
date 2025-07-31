namespace BSFiberCore.Models.BL.Uom
{
    /// <summary>
    /// Класс для работы с еденицами измерения
    /// </summary>
    public class LameUnitConverter
    {
        private LengthMeasurement _lengthMeasurement;

        private ForceMeasurement _forceMeasurement;

        private MomentOfForceMeasurement _momentOfForceMeasurement;

        public LameUnitConverter()
        {
            // Задаем значение единиц измерения длины в которых проводятся расчеты
            _lengthMeasurement = new LengthMeasurement(LengthUnits.m);
            _forceMeasurement = new ForceMeasurement(ForceUnits.kg);
            _momentOfForceMeasurement = new MomentOfForceMeasurement(MomentOfForceUnits.kgBycm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelUnitsMeasurement">задаются расчетные значения ед измерения в строгом порядке</param>
        public LameUnitConverter(List<Enum> modelUnitsMeasurement)
        {
            _lengthMeasurement = new LengthMeasurement((LengthUnits)modelUnitsMeasurement[0]);
            _forceMeasurement = new ForceMeasurement((ForceUnits)modelUnitsMeasurement[1]);
            _momentOfForceMeasurement = new MomentOfForceMeasurement((MomentOfForceUnits)modelUnitsMeasurement[2]);
        }

        #region Change Custom or model Unit Measurement
        public void ChangeCustomUnitLength(int index )
        {
            _lengthMeasurement.CustomUnit = (LengthUnits)index;
        }
        public void ChangeCustomUnitForce(int index)
        {
            _forceMeasurement.CustomUnit = (ForceUnits)index;
        }
        public void ChangeCustomUnitMomentOfForce(int index)
        {
            _momentOfForceMeasurement.CustomUnit = (MomentOfForceUnits)index;
        }
        public void ChangeModelUnitLength(int index)
        {
            _lengthMeasurement.ModelUnit = (LengthUnits)index;
        }
        public void ChangeModelUnitForce(int index)
        {
            _forceMeasurement.ModelUnit = (ForceUnits)index;
        }
        public void ChangeModelUnitMomentOfForce(int index)
        {
            _momentOfForceMeasurement.ModelUnit = (MomentOfForceUnits)index;
        }
        #endregion

        #region Convert units measurement (Custom to model or model to custom)
        public double СonvertLength(double input)
        {
            return _lengthMeasurement.CustomToModelUnit(input);
        }

        public double ConvertRevertLength(double input)
        {
            return _lengthMeasurement.ModelToCustomUnit(input);
        }

        public double ConvertForce(double input)
        {
            return _forceMeasurement.CustomToModelUnit(input);
        }

        public double ConvertRevertForce(double input)
        {
            return _forceMeasurement.ModelToCustomUnit(input);
        }

        public double ConvertMomentOfForce(double input)
        {
            return _momentOfForceMeasurement.CustomToModelUnit(input);
        }

        public double ConvertRevertMomentOfForce(double input)
        {
            return _momentOfForceMeasurement.ModelToCustomUnit(input);
        }
        #endregion

        #region get set

        /// <summary>
        /// Получить название пользовательских ед измерения 
        /// </summary>
        /// <returns></returns>
        public string GetCustomNameLengthUnit()
        {
            return Extensions.GetDescription(_lengthMeasurement.CustomUnit);
        }

        public string GetCustomNameForceUnit()
        {
            return Extensions.GetDescription(_forceMeasurement.CustomUnit);
        }

        public string GetCustomNameMomentOfForceUnit()
        {
            return Extensions.GetDescription(_momentOfForceMeasurement.CustomUnit);
        }

        public string GetModelNameLengthUnit()
        {
            return Extensions.GetDescription(_lengthMeasurement.ModelUnit);
        }

        public string GetModelNameForceUnit()
        {
            return Extensions.GetDescription(_forceMeasurement.ModelUnit);
        }

        public string GetModelNameMomentOfForceUnit()
        {
            return Extensions.GetDescription(_momentOfForceMeasurement.ModelUnit);
        }

        #endregion

        #region методы в которых используются достаточно сомнительные механизмы ветвления
        // такие способы решения задачи были выбраны исходя из простоты внедрения в существующий код 

        /// <summary>
        ///  конвертации нагрузок из пользовательских ед в расчетные
        /// </summary>
        /// <param name="effortsName"> Название нагрузки (название колонки)</param>
        /// <param name="effortsValue">значение нагрузки</param>
        public double ConvertEfforts(string effortsName, double effortsValue)
        {
            double newValue = 0; 
            // что по говнокоду?
            if (effortsName.Contains("M"))
            {
                // перевод Момента силы из пользовательских ед в расчетные
                newValue = this.ConvertMomentOfForce(effortsValue);
            }
            else
            {
                // перевод Силы из пользовательских ед в расчетные
                newValue = this.ConvertForce(effortsValue);
            }
            return newValue;

        }

        /// <summary>
        ///  конвертации нагрузок расчетных единиц в пользовательские
        /// </summary>
        /// <param name="effortsName"> Название нагрузки (название колонки)</param>
        /// <param name="effortsValue">значение нагрузки</param>
        public double ConvertRevertEfforts(string effortsName, double effortsValue)
        {
            double newValue = 0;
            if (effortsName.Contains("M")) // говнокод
            {
                // перевод Момента силы из пользовательских ед в расчетные
                newValue = this.ConvertRevertMomentOfForce(effortsValue);
            }
            else
            {
                // перевод Силы из пользовательских ед в расчетные
                newValue = this.ConvertRevertForce(effortsValue);
            }
            return newValue;

        }


        /// <summary>
        /// Перевод из расчетных ед изм в ед установленные пользователем
        /// </summary>
        /// <param name="nameUnitMeasurment">расчетные ед изм</param>
        /// <param name="value">значение</param>
        /// <returns></returns>
        public double ConvertEffortsForReport(string str, double value, out string nameCustomlMeaserment)
        {
            nameCustomlMeaserment = "";
            string[] strArray = str.Split('[', ']');

            if (strArray.Length >= 2)
            {
                string nameUnitMeasurment = strArray[1];
                string nameModelMeaserment = this.GetModelNameForceUnit();
                if (nameModelMeaserment == nameUnitMeasurment)
                {
                    nameCustomlMeaserment = this.GetCustomNameForceUnit();
                    // перевод Силы из расчетных ед изм в пользовательские
                    return this.ConvertRevertForce(value);
                }

                nameModelMeaserment = this.GetModelNameMomentOfForceUnit();
                if (nameModelMeaserment == nameUnitMeasurment)
                {
                    nameCustomlMeaserment = this.GetCustomNameMomentOfForceUnit();
                    // Перевод момента силы из расчетных ед изм в пользовательские
                    return this.ConvertRevertMomentOfForce(value);
                }
            }

            return value;
        }

        /// <summary>
        /// Замена ед измерения в headerText для сил
        /// </summary>
        /// <param name="headerText"></param>
        /// <returns></returns>
        public string ChangeHT4ForForce(string headerText)
        {
            // Только для сил
            string[] stringArray = headerText.Split(',');
            if (stringArray[0].Contains("M"))  // говнокод
            { return headerText; }
            string nameUnitMeasurement = this.GetCustomNameForceUnit();
            return stringArray[0] + ", " + nameUnitMeasurement;

        }

        /// <summary>
        /// Замена ед измерения в headerText для момента сил
        /// </summary>
        /// <param name="headerText"></param>
        /// <returns></returns>
        public string ChangeHTForMomentOfForce(string headerText)
        {
            // Только для моментов сил
            string[] stringArray = headerText.Split(',');
            if (stringArray[0].Contains('M')) // говнокод
            {
                string nameUnitMeasurement = this.GetCustomNameMomentOfForceUnit();
                return  stringArray[0] + ", " + nameUnitMeasurement;
            }
            return headerText;
        }
        #endregion
    }
}

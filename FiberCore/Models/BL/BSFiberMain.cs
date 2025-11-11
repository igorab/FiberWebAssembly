using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Calc;
using BSFiberCore.Models.BL.Draw;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Mat;
using BSFiberCore.Models.BL.Ndm;
using BSFiberCore.Models.BL.Rep;
using BSFiberCore.Models.BL.Sec;
using BSFiberCore.Models.BL.Tri;
using BSFiberCore.Models.BL.Uom;
using FiberCore.Services;
using MathNet.Numerics.Distributions;
using System.Data;
using System.Drawing;

namespace BSFiberCore.Models.BL
{
    public class BSFiberMain
    {
        private BSFiberCalculation BSFibCalc;
        private LameUnitConverter _UnitConverter;
        private MeshSectionSettings? _beamSectionMeshSettings { get; set; }

        private double[] sz;

        public Dictionary<string, double> MNQ { private get; set; } 
        public bool UseReinforcement { get; set; } = false;
        public BeamSection BeamSection { get; set; }
        public BSMatFiber MatFiber { get; set; }
        public FiberCalculator Fiber { get; internal set; }
        public Rebar? Rebar { get; internal set; }
        public List<string> m_Message { get; private set; }
        public Dictionary<string, double> m_CalcResults2Group { get; private set; }
        public List<Elements> FiberConcrete { private get; set; }
        public List<BSFiberBeton> Bft3Lst {private get; set; }
        public List<FiberBft> BftnLst {private get; set; }
        public List<Beton> BfnLst { private get; set; }
        public List<Rebar>? m_Rebar {private  get; set; }
        public List<RebarDiameters>? m_RebarDiameters {private  get; set; }

        BSSectionChart SectionChart;

        // Mesh generation
        // площади элементов (треугольников)
        private List<double> triAreas;
        // координаты центра тяжести элементов (треугольников)
        private List<TriangleNet.Geometry.Point> triCGs;

        public BSFiberMain()
        {
            _UnitConverter = new LameUnitConverter();
            SectionChart = new BSSectionChart();
            MNQ  = new Dictionary<string, double>();
        }

        /// <summary>
        ///  размеры балки (поперечное сечение + длина)
        /// </summary>
        /// <param name="_length">Длина балки </param>
        /// <returns>массив размеров </returns>
        private double[] BeamSizes(double _length = 0)
        {
            double[] sz = new double[2];
            
            double bf = Fiber.bf, hf = Fiber.hf, bw = Fiber.bw, hw = Fiber.hw, b1f = Fiber.b1f, h1f = Fiber.h1f;
            double r1 = Fiber.R1, r2 = Fiber.R2;

            if (BeamSection == BeamSection.Any)
            {
                return sz;
            }
            else if (BeamSection == BeamSection.Rect)
            {
                sz = new double[] { Fiber.b, Fiber.h, Fiber.Length };
            }
            else if (BeamSection == BeamSection.TBeam)
            {
                sz = new double[] { bf, hf, bw, hw, b1f, h1f, _length };
            }
            else if (BeamSection == BeamSection.LBeam)
            {
                sz = new double[] { bf, hf, bw, hw, b1f, h1f, _length };
            }
            else if (BeamSection == BeamSection.IBeam)
            {
                sz = new double[] { bf, hf, bw, hw, b1f, h1f, _length };
            }
            else if (BeamSection == BeamSection.Ring)
            {
                sz = new double[] { r1, r2, _length };
            }

            return sz;
        }

        /// <summary>
        ///  Размеры балки
        /// </summary>        
        private double[] BeamWidtHeight(out double _w, out double _h, out double _area)
        {
            double[] sz = BeamSizes();

            if (BeamSection == BeamSection.Rect)
            {
                _w = sz[0];
                _h = sz[1];
                _area = _w * _h;
            }
            else if (BeamSection == BeamSection.Ring)
            {
                _w = Math.Max(sz[0], sz[1]);
                _h = Math.Max(sz[0], sz[1]);
                _area = Math.PI * Math.Pow(Math.Abs(sz[1] - sz[0]), 2) / 4.0;
            }
            else if (BSHelper.IsITL(BeamSection))
            {
                //_w = Math.Max(sz[0], sz[4]);
                _w = sz[2];
                _h = sz[1] + sz[3] + sz[5];
                _area = sz[0] * sz[1] + sz[2] * sz[3] + sz[4] * sz[5];
            }
            else if (BeamSection == BeamSection.Any)
            {
                _w = 0;
                _h = 0;
                _area = 0;
            }
            else
            {
                throw new Exception("Не определен тип сечения");
            }

            return sz;
        }

        public BSFiberCalc_MNQ FibCalc_MNQ(Dictionary<string, double> _MNQ, double[] _prms)
        {
            BSFiberCalc_MNQ fibCalc = BSFiberCalc_MNQ.Construct(BeamSection);

            fibCalc.MatFiber = MatFiber;
            fibCalc.UseRebar = UseReinforcement;
            fibCalc.Rebar         = new Rebar() { };
            fibCalc.BetonType     = new BetonType() { };
            fibCalc.UnitConverter = _UnitConverter;            
            fibCalc.SetSize(sz);
            fibCalc.SetParams(_prms);
            fibCalc.SetEfforts(_MNQ);
            fibCalc.SetN_Out();

            return fibCalc;
        }

        /// <summary>
        /// Расчеты стельфибробетона по предельным состояниям второй группы
        /// 1) Расчет предельного момента образования трещин
        /// 2) Расчет ширины раскрытия трещины
        /// </summary>        
        public BSFiberCalc_Cracking FiberCalculate_Cracking(Dictionary<string, double> _MNQ)
        {
            bool calcOk;
            try
            {
                BSBeam bsBeam = BSBeam.construct(BeamSection);

                bsBeam.SetSizes(BeamSizes());
                
                BSFiberCalc_Cracking calc_Cracking = new BSFiberCalc_Cracking(_MNQ)
                {
                    Beam = bsBeam,
                    typeOfBeamSection = BeamSection
                };

                double selectedDiameter = 10;
                double Es = 0;
                // задать тип арматуры
                calc_Cracking.MatRebar = new BSMatRod(Es)
                {
                    RCls = "",
                    Rs = 0,
                    e_s0 = 0,
                    e_s2 = 0,
                    As = 0,
                    As1 = 0,
                    a_s = 0,
                    a_s1 = 0,
                    Reinforcement = true,
                    SelectedRebarDiameter = selectedDiameter
                };

                // SetFiberMaterialProperties();

                calc_Cracking.MatFiber = MatFiber;

                calcOk = calc_Cracking.Calculate();

                if (m_Message == null) m_Message = new List<string>();
                m_Message.AddRange(calc_Cracking.Msg);

                if (calcOk)
                    m_CalcResults2Group = calc_Cracking.Results();

                return calc_Cracking;
            }
            catch (Exception _e)
            {
                MessageBox.Show("Ошибка в расчете: " + _e.Message);
            }

            return null;
        }

        public void InitSize()
        {
            sz = BeamWidtHeight(out double w, out double h, out double area);
        }
                       
        private void SelectedFiberBetonValues(string fib_i, string bft3n, ref double numRfbt3n, ref double numRfbt2n)
        {            
            try
            {                
                var beton = Bft3Lst.FirstOrDefault(fbt => fbt.Name == bft3n);
                if (beton == null)
                    return;

                var bet_name = beton.Name;
                bet_name = bet_name.Replace("i", fib_i);

                var getQuery = FiberConcrete.Where(f => f.BT == bet_name);
                if (getQuery?.Count() > 0)
                {
                    Elements? fib = getQuery?.First();

                    numRfbt3n = BSHelper.MPA2kgsm2(fib?.Rfbt3n);
                    numRfbt2n = BSHelper.MPA2kgsm2(fib?.Rfbt2n);
                }
                else
                {
                    numRfbt3n = 0;
                    numRfbt2n = 0;
                }
            }
            catch (Exception _e) 
            {
                Console.WriteLine(_e.Message);
                numRfbt3n = 0;
                numRfbt2n = 0;
            }
        }

        private void BftnSelectedValue(string  bft_n, ref double numRfbt_n)
        {
            try
            {
                FiberBft? bft = BftnLst.FirstOrDefault(bft => bft.ID == bft_n);

                numRfbt_n = BSHelper.MPA2kgsm2(bft?.Rfbtn);
            }
            catch 
            {
                numRfbt_n = 0;
            }
        }

        /// <summary>
        /// выбрать по классу бетона на сжатие
        /// </summary>
        /// <param name="bf_n"></param>
        /// <param name="_betonTypeId">тип: тяжелый / легкий</param>
        /// <param name="_airHumidityId">влажность</param>
        /// <param name="numRfb_n"></param>
        /// <param name="numE_beton"></param>
        /// <param name="B_class">класс бетона</param>
        private async Task<(double numRfb_n, double numE_beton, double B_class)>  BfnSelectedValue(string bf_n, int _betonTypeId, int _airHumidityId)
        {
            double numRfb_n = 0, numE_beton = 0, B_class =  0;

            try
            {                                
                Beton? bfn = BfnLst.FirstOrDefault(bfn => bfn.BT == bf_n);

                if (bfn != null)
                {
                    string betonClass = Convert.ToString(bfn.BT);
                    B_class = bfn.B;

                    if (string.IsNullOrEmpty(betonClass)) return (0,0,0);

                    Beton bt = new Beton();
                    bt = await MaterialServices.HeavyBetonTableFindAsync(betonClass, _betonTypeId);

                    if (bt.Rbn != 0)
                        numRfb_n = BSHelper.MPA2kgsm2(bt.Rbn);

                    double fi_b_cr = 0;

                    if (_airHumidityId >= 0 && _airHumidityId <= 3 && bt.B >= 10)
                    {
                        int iBClass = (int)Math.Round(bt.B, MidpointRounding.AwayFromZero);

                        fi_b_cr = BSFiberLib.CalcFi_b_cr(_airHumidityId, iBClass);
                    }

                    if (bt.Eb != 0)
                    {
                        double _eb = BSHelper.MPA2kgsm2(bt.Eb * 1000);
                        numE_beton = _eb / (1.0 + fi_b_cr);
                    }
                }
            }
            catch 
            {
                numRfb_n = 0;
                numE_beton = 0;
                B_class = 0;
            }

            return (numRfb_n, numE_beton, B_class);
        }

        /// <summary>
        ///  классы материала
        /// </summary>
        public async Task SelectMaterialFromList()
        {                                 
            double rfbt3n = 0, rfbt2n = 0;
            double rfbtn  = 0;
            double rfbn = 0;
            double Eb = 0;
            double Bclass = 0;

            var beton_index = Fiber.BetonIndex;
            var b_ft3 = Fiber.Bft3;
            
            SelectedFiberBetonValues(beton_index, b_ft3, ref rfbt3n, ref rfbt2n);

            BftnSelectedValue(Fiber.Bft, ref rfbtn);

            (rfbn, Eb, Bclass) = await BfnSelectedValue(Fiber.Bfb, 0, 1);
            
            MatFiber = new BSMatFiber(Fiber.Ef, Fiber.mu_fv, Fiber.Efbt, Fiber.Eb, Fiber.Yft, Fiber.Yb, Fiber.Yb1, Fiber.Yb2, Fiber.Yb3, Fiber.Yb5)
            {
                B = Bclass,
                Rfbt3n = rfbt3n,
                Rfbt2n = rfbt2n,
                Rfbtn  = rfbtn,
                Rfbn   = rfbn,                
            };

            Rebar = RebarSelectedValues(Fiber.A_Rs);
        }

        /// <summary>
        /// Расчет прочности сечения на действие момента
        /// </summary>        
        public BSFiberReportData FiberCalculate_M(double _M, double[] _prms)
        {
            bool calcOk;
            BSFiberReportData reportData = new BSFiberReportData();

            try
            {                
                BSFibCalc = BSFiberCalculation.Construct(BeamSection, UseReinforcement);
                BSFibCalc.MatFiber = MatFiber;
                InitRebar(BSFibCalc);

                BSFibCalc.SetParams(_prms);
                BSFibCalc.SetSize(sz);
                BSFibCalc.Efforts = new Dictionary<string, double> { { "My", _M } };

                calcOk = BSFibCalc.Calculate();
                if (calcOk)
                    reportData.InitFromBSFiberCalculation(BSFibCalc, _UnitConverter);

                // расчет по второй группе предельных состояний
                var FibCalcGR2 = FiberCalculate_Cracking(BSFibCalc.Efforts);
                reportData.Messages.AddRange(FibCalcGR2.Msg);
                reportData.CalcResults2Group = FibCalcGR2.Results();

                return reportData;
            }
            catch (Exception _e)
            {
                MessageBox.Show("Ошибка в расчете: " + _e.Message);
                return reportData;
            }
        }

        /// <summary>
        ///  Задать армирование
        /// </summary>
        /// <param name="bSFibCalc"></param>
        private void InitRebar(BSFiberCalculation bSFibCalc)
        {
            double[] matRod = { Fiber.Rs, Fiber.Rsc, Fiber.As, Fiber.A1s, Fiber.Es, Fiber.a_cm, Fiber.a1_cm };

            if (bSFibCalc is BSFiberCalc_RectRods)
            {
                BSFiberCalc_RectRods _bsCalcRods = (BSFiberCalc_RectRods)bSFibCalc;

                _bsCalcRods.SetLTRebar(matRod);
            }
            else if (bSFibCalc is BSFiberCalc_IBeamRods)
            {
                BSFiberCalc_IBeamRods _bsCalcRods = (BSFiberCalc_IBeamRods)bSFibCalc;

                //TODO refactoring
                _bsCalcRods.GetLTRebar(matRod);
            }
        }

        private Dictionary<string, double> DictCalcParams(BeamSection _beamSection)
        {                                                                                   
            // деформации                
            double numEps_s_ult = 0;           
            double lgth = 0, coeflgth = 0;
           
            Dictionary<string, double> D = new Dictionary<string, double>()
            {
                // enforces
                ["N"]  = MNQ.ContainsKey("N") ? -MNQ["N"] : 0,
                ["My"] = MNQ.ContainsKey("My") ? MNQ["My"] : 0,
                ["Mx"] = MNQ.ContainsKey("Mx") ? MNQ["Mx"] : 0,
                ["Qx"] = MNQ.ContainsKey("Qx") ? MNQ["Qx"] : 0,
                ["Qy"] = MNQ.ContainsKey("Qy") ? MNQ["Qy"] : 0,
                //
                //length
                ["lgth"] = lgth,
                ["coeflgth"] = coeflgth,
                //
                //section size
                ["b"] = 0,
                ["h"] = 0,

                ["bf"] = 0,
                ["hf"] = 0,
                ["bw"] = 0,
                ["hw"] = 0,
                ["b1f"] = 0,
                ["h1f"] = 0,

                ["r1"] = 0,
                ["R2"] = 0,
                //
                //Mesh
                ["ny"] = 0, //_beamSectionMeshSettings.NY,
                ["nz"] = 0, //_beamSectionMeshSettings.NX, // в алгоритме плосткость сечения YOZ

                // beton
                ["Eb0"] = MatFiber.Eb, // сжатие
                ["Ebt"] = MatFiber.Efbt, // растяжение

                // - нормативные
                ["Rbcn"]  = MatFiber.Rfbn,
                ["Rbtn"]  = MatFiber.Rfbtn,
                ["Rbt2n"] = MatFiber.Rfbt2n,
                ["Rbt3n"] = MatFiber.Rfbt3n,
                // - расчетные 
                ["Rbc"]  = MatFiber.Rfb,
                ["Rbt"]  = MatFiber.Rfbt,
                ["Rbt2"] = MatFiber.Rfbt2,
                ["Rbt3"] = MatFiber.Rfbt3,
                // - деформации
                // сжатие
                ["ebc0"]   = 0,
                ["ebc2"]   = 0.0035d,
                ["eb_ult"] = 0.0035d,

                // растяжение
                ["ebt0"] = 0,
                ["ebt1"] = 0,
                ["ebt2"] = 0,
                ["ebt3"] = 0,
                ["ebt_ult"] = 0.015d,
                // арматура steel                
                ["Es0"] = Fiber.Es,
                // нормативные 
                ["Rscn"] = Rebar.Rsn,
                ["Rstn"] = Rebar.Rsn,
                // расчетные
                ["Rsc"] = Rebar.Rsc,
                ["Rst"] = Rebar.Rs,
                // деформации
                ["esc2"] = 0,
                ["est2"] = 0,
                ["es_ult"] = numEps_s_ult,
                // коэффициенты надежности
                ["Yft"] = Fiber.Yft,
                ["Yb"]  = Fiber.Yb,
                ["Yb1"] = Fiber.Yb1,
                ["Yb2"] = Fiber.Yb2,
                ["Yb3"] = Fiber.Yb3,
                ["Yb5"] = Fiber.Yb5
            };

            double[] beam_sizes = sz; // BeamSizes();

            double b = 0;
            double h = 0;

            if (_beamSection == BeamSection.Rect)
            {
                b = beam_sizes[0];
                h = beam_sizes[1];
            }
            else if (BSHelper.IsITL(_beamSection))
            {
                D["bf"] = beam_sizes[0];
                D["hf"] = beam_sizes[1];
                D["bw"] = beam_sizes[2];
                D["hw"] = beam_sizes[3];
                D["b1f"] = beam_sizes[4];
                D["h1f"] = beam_sizes[5];

                b = D["bf"];
                h = D["hf"] + D["hw"] + D["h1f"];
            }
            else if (_beamSection == BeamSection.Ring)
            {
                D["r1"] = beam_sizes[0];
                D["R2"] = beam_sizes[1];

                b = 2 * D["R2"];
                h = 2 * D["R2"];
            }

            D["b"] = b;
            D["h"] = h;

            return D;
        }

        private void InitStrengthFactorsFromForm(double[] prms)
        {
            int idx = -1;
            if (prms.Length >= 8)
            {
                prms[++idx] = 0; // Convert.ToDouble(numRfbt3n.Value);
                prms[++idx] = 0; // Convert.ToDouble(numRfb_n.Value);
                prms[++idx] = Fiber.Yft;
                prms[++idx] = Fiber.Yb;
                prms[++idx] = Fiber.Yb1;
                prms[++idx] = Fiber.Yb2;
                prms[++idx] = Fiber.Yb3;
                prms[++idx] = Fiber.Yb5;
                prms[++idx] = 0;
            }
        }

        /// <summary>
        /// Расчет на действие поперечных сил действующих по двум направлениям
        /// </summary>
        private Dictionary<string, double> FiberCalculate_QxQy(Dictionary<string, double> _MNQ, double[] _sz)
        {
            double[] prms = new double[9];

            InitStrengthFactorsFromForm(prms);

            var betonType = BSQuery.BetonTypeFind(0);

            BSFiberCalc_QxQy fiberCalc = new BSFiberCalc_QxQy();
            fiberCalc.MatFiber  = MatFiber;
            fiberCalc.UseRebar  = UseReinforcement;
            fiberCalc.Rebar     = Rebar;// m_SectionChart.Rebar; // поперечная амрматура из полей в контроле m_SectionChart
            fiberCalc.BetonType = betonType;
            fiberCalc.UnitConverter = _UnitConverter;
            // fiberCalc.SetFiberFromLoadData(fiber);
            fiberCalc.SetSize(_sz);
            fiberCalc.SetParams(prms);
            fiberCalc.SetEfforts(_MNQ);
            fiberCalc.SetN_Out();

            bool calcOk = fiberCalc.Calculate();

            if (calcOk)
                fiberCalc.Msg.Add("Расчет успешно выполнен!");
            else
                fiberCalc.Msg.Add("Расчет по наклонному сечению на действие Q не выполнен!");

            Dictionary<string, double> xR = fiberCalc.Results();

            return xR;
        }


        private NDMSetup NDMSetupInitFormValues()
        {
            NDMSetup ndmSetup = BSData.LoadNDMSetup();

            _beamSectionMeshSettings = new MeshSectionSettings(ndmSetup.N, ndmSetup.M, ndmSetup.MinAngle, ndmSetup.MaxArea);

            return ndmSetup;
        }


        /// <summary>
        /// Расчет по НДМ 
        /// </summary>
        /// <param name="_beamSection">Тип сечения</param>
        /// <returns></returns>
        public BSCalcResultNDM CalcNDM(BeamSection _beamSection)
        {            
            double[] sz = BeamSizes(/*length*/);

            if (_beamSection == BeamSection.Any)
            {
                sz[0] = Fiber.Width;
                sz[1] = Fiber.Length;
            }

            Dictionary<string, double> resQxQy = FiberCalculate_QxQy(MNQ, sz);

            // данные с формы:
            Dictionary<string, double> _D = DictCalcParams(_beamSection);

            NDMSetupInitFormValues();

            // расчет:
            CalcNDM calcNDM = new CalcNDM(_beamSection) { Dprm = _D };

            Dictionary<string, double>? resGroup2 = null;

            if (_beamSection == BeamSection.Any ||
                _beamSection == BeamSection.Ring)
            {
                calcNDM.RunGroup1();

                calcNDM.CalcRes.b = Fiber.Width;
                calcNDM.CalcRes.h = Fiber.Length;

                resGroup2 = FiberCalculateGroup2(calcNDM.CalcRes);
            }
            else if (BSHelper.IsRectangled(_beamSection))
            {
                calcNDM.RunGroup1();

                resGroup2 = FiberCalculateGroup2(calcNDM.CalcRes);
            }
            else
            {
                calcNDM.Run();
            }

            BSCalcResultNDM calcRes = new BSCalcResultNDM();
            if (calcNDM.CalcRes != null)
                calcRes = calcNDM.CalcRes;

            if (resGroup2 != null && calcRes != null)
                calcRes.SetRes2Group(resGroup2, true, true);

            calcRes.ResQxQy = resQxQy;
            //calcRes.ImageStream = ImageStream;
            //calcRes.Coeffs = Coeffs;
            calcRes.UnitConverter = _UnitConverter;

            return calcRes;
        }

        /// <summary>
        /// Покрыть сечение сеткой
        /// </summary>
        public string GenerateMesh(ref TriangleNet.Geometry.Point _CG)
        {
            string pathToSvgFile;
            double[] sz = BeamWidtHeight(out double b, out double h, out double area);

            BSMesh.Nx = _beamSectionMeshSettings.NX ?? 10;
            BSMesh.Ny = _beamSectionMeshSettings.NY ?? 10;
            BSMesh.MinAngle = _beamSectionMeshSettings.MinAngle;
            Tri.Tri.MinAngle = _beamSectionMeshSettings.MinAngle;
            BSMesh.MaxArea = _beamSectionMeshSettings.MaxArea;

            BSMesh.FilePath = Path.Combine(Environment.CurrentDirectory, "Templates");
            Tri.Tri.FilePath = BSMesh.FilePath;

            if (BeamSection == BeamSection.Rect)
            {
                List<double> rect = new List<double> { 0, 0, b, h };

                pathToSvgFile = BSMesh.GenerateRectangle(rect);
                Tri.Tri.Mesh  = BSMesh.Mesh;
                // сместить начало координат из левого нижнего угла в центр тяжести
                Tri.Tri.Oxy = _CG;

                _ = Tri.Tri.CalculationScheme();
            }
            else if (BSHelper.IsITL(BeamSection))
            {
                List<PointF> pts;
                BSSection.IBeam(sz, out pts, out PointF _center, out PointF _left);
                _CG = new TriangleNet.Geometry.Point(_center.X, _center.Y);

                pathToSvgFile = Tri.Tri.CreateSectionContour(pts, BSMesh.MaxArea);
                _ = Tri.Tri.CalculationScheme();
            }
            else if (BeamSection == BeamSection.Ring)
            {
                _CG = new TriangleNet.Geometry.Point(0, 0);

                double r = sz[0];
                double R = sz[1];

                if (r > R)
                    throw BSBeam_Ring.RadiiError();

                BSMesh.Center = _CG;
                pathToSvgFile = BSMesh.GenerateRing(R, r, true);

                Tri.Tri.Mesh = BSMesh.Mesh;
                _ = Tri.Tri.CalculationScheme();
            }
            else if (BeamSection == BeamSection.Any)
            {
                pathToSvgFile = SectionChart.GenerateMesh(BSMesh.MaxArea);
            }
            else
            {
                throw new Exception("Не задано сечение");
            }

            // площади треугольников
            triAreas = Tri.Tri.triAreas;
            // центры тяжести треугольников
            triCGs = Tri.Tri.triCGs;

            return pathToSvgFile;
        }

        /// <summary>
        /// Расчет по 2 группе предельных состояний
        /// </summary>
        /// <param name="calcRes"></param>
        /// <returns></returns>
        private Dictionary<string, double>? FiberCalculateGroup2(BSCalcResultNDM calcRes)
        {
            bool calcOk;

            try
            {
                BSBeam bsBeam;

                if (BeamSection == BeamSection.Any)
                {
                    bsBeam = new BSBeam(calcRes.Area, calcRes.W_s, calcRes.I_s, calcRes.Jy, calcRes.Jx, calcRes.Sx, calcRes.Sy);
                    bsBeam.b = calcRes.b;
                    bsBeam.h = calcRes.h;

                }
                else
                {
                    bsBeam = BSBeam.construct(BeamSection);
                    bsBeam.SetSizes(BeamSizes());
                }

                //Dictionary<string, double> MNQ = GetEffortsForCalc();

                BSFiberCalc_Cracking calc_Cracking = new BSFiberCalc_Cracking(MNQ)
                {
                    Beam = bsBeam,
                    typeOfBeamSection = BeamSection
                };

                double _As = calcRes.As_t;
                double h0 = calcRes.h0_t;
                double _a_s = Fiber.a_cm;

                double _As1 = calcRes.As1_p;
                double h01 = calcRes.h01_p;
                double _a_s1 = Fiber.a1_cm; 

                // задать тип арматуры
                calc_Cracking.MatRebar = new BSMatRod(Fiber.Es)
                {
                    RCls  = Fiber.A_Rs,
                    Rs    = Fiber.Rs,
                    e_s0  = 0,
                    e_s2  = 0,
                    As    = _As,
                    As1   = _As1,
                    a_s   = _a_s,
                    h0_t  = h0,
                    a_s1  = _a_s1,
                    h0_p  = h01,
                    Reinforcement = UseReinforcement
                };

                //SetFiberMaterialProperties();

                calc_Cracking.MatFiber = MatFiber;

                calcOk = calc_Cracking.CalculateNDN();

                var msgs = calc_Cracking.Msg;

                Dictionary<string, double> res = calc_Cracking.ResGr2();

                return res;
            }
            catch (Exception _e)
            {
                MessageBox.Show("Ошибка в расчете: " + _e.Message);
            }

            return null;
        }
        
        // арматура
        public Rebar? RebarSelectedValues(string _item)
        {            
            // настройки из БД
            Rebar? dbRebar = m_Rebar?.Where(x => x.ID == _item).FirstOrDefault();
            double numEsValue = BSHelper.MPA2kgsm2(dbRebar?.Es);

            return dbRebar;
        }

        /// <summary>
        /// диаграмма деформирования
        /// </summary>
        /// <returns></returns>
        private string DiagramPlot()
        {
            // create a plot and fill it with sample data
            ScottPlot.Plot myPlot = new();
            double[] dataX = ScottPlot.Generate.Consecutive(100);
            double[] dataY = ScottPlot.Generate.RandomWalk(100);
            myPlot.Add.Scatter(dataX, dataY);

            // render the plot as a PNG and encode its bytes in HTML
            byte[] imgBytes = myPlot.GetImageBytes(600, 400, ScottPlot.ImageFormat.Png);
            string b64 = Convert.ToBase64String(imgBytes);
            string png = $"<img src='data:image/png;base64,{b64}'>";
            string html = $"{png}";

            return html;
        }

        public void CreatePictureForHeaderReport(List<BSCalcResultNDM> calcResults)
        {
            List<string> pathToPictures = new List<string>();
            string pathToPicture = DiagramPlot(); 
            //// Диаграмма деформирования
            
            //    // собрать данные
            //    DataForDeformDiagram data = ValuesForDeformDiagram();
            //    // определить vm
            //    CalcDeformDiagram calculateDiagram = new CalcDeformDiagram(data.typesDiagram, data.resists, data.elasticity);
            //    Chart deformDiagram = calculateDiagram.CreteChart();
            //    pathToPicture = CalcDeformDiagram.SaveChart(deformDiagram);
            pathToPictures.Add(pathToPicture);
            
            calcResults[0].PictureForHeaderReport = pathToPictures;
        }


        public void CreatePictureForBodyReport(List<BSCalcResultNDM> calcResultsNDM)
        {
            for (int i = 0; i < calcResultsNDM.Count ; i++)
            {
                BSCalcResultNDM calcResNDM = calcResultsNDM[i];

                List<string> pictures = new List<string>();
                
                // изополя сечения по деформации                                
                MeshDraw mDraw = CreateMosaic(1, calcResNDM.Eps_B, calcResNDM.Eps_S, calcResNDM.Eps_fbt_ult, calcResNDM.Eps_fb_ult, calcResNDM.Rs);

                string? htmlScaleDeform = "";
                string htmlPlotDeform = mDraw.SaveToPNG("Относительные деформации", ref htmlScaleDeform);
                pictures.Add(htmlScaleDeform);
                pictures.Add(htmlPlotDeform);
                                                     
                // изополя сечения по напряжению                                                
                // не самое элегантное решение, чтобы не рисовать ограничивающие рамки, в случае превышения нормативных значений
                double ultMaxValue = calcResNDM.Sig_B?.Max()??0 + 1;
                double ultMinValue = calcResNDM.Sig_B?.Min()??0 - 1;

                string? htmlScale = "";
                MeshDraw mDrawStress = CreateMosaic(2, calcResNDM.Sig_B, calcResNDM.Sig_S, ultMaxValue, ultMinValue, BSHelper.kgssm2kNsm(calcResNDM.Rs));
                string htmlPlotStress = mDrawStress.SaveToPNG("Напряжения", ref htmlScale);

                pictures.Add(htmlScale);
                pictures.Add(htmlPlotStress);
                                                
                if (pictures.Count > 0)
                {
                    calcResNDM.PictureForBodyReport = pictures;
                }
            }
        }


        /// <summary>
        ///  Разбиение сечения на конечные элементы
        /// </summary>
        /// <param name="_valuesB">значения для бетона</param>
        /// <param name="_valuesB">значения для арматуры</param>
        private MeshDraw CreateMosaic(int _Mode = 0,
                                List<double>? _valuesB = null,
                                List<double>? _valuesS = null,
                                double _ultMax = 0,
                                double _ultMin = 0,
                                double _ultRs = 0,
                                double _e_st_ult = 0,
                                double _e_s_ult = 0)
        {
            MeshDraw mDraw = null;

            double[] sz = BeamSizes();

            if (BSHelper.IsRectangled(BeamSection))
            {
                int nx = 10; 
                int ny = 10;

                if (_beamSectionMeshSettings != null)
                {
                    nx = _beamSectionMeshSettings.NX ?? 10;                 
                    ny = _beamSectionMeshSettings.NY ?? 10;
                }

                mDraw = new MeshDraw(nx, ny);
                mDraw.MosaicMode = _Mode;
                mDraw.UltMax = _ultMax;
                mDraw.UltMin = _ultMin;
                mDraw.Rs_Ult = _ultRs;
                mDraw.e_st_ult = _e_st_ult;
                mDraw.e_s_ult = _e_s_ult;
                mDraw.Values_B = _valuesB;
                mDraw.Values_S = _valuesS;
                mDraw.colorsAndScale = new ColorScale(_valuesB, _ultMax, _ultMin);
                mDraw.CreateRectanglePlot1(sz, BeamSection);
                mDraw.DrawReinforcementBar(BeamSection);

            }
            else if (BeamSection == BeamSection.Ring)
            {
                TriangleNet.Geometry.Point cg = new TriangleNet.Geometry.Point();
                _ = GenerateMesh(ref cg);

                mDraw = new MeshDraw(Tri.Tri.Mesh);
                mDraw.MosaicMode = _Mode;
                mDraw.UltMax = _ultMax;
                mDraw.UltMin = _ultMin;
                mDraw.Rs_Ult = _ultRs;
                mDraw.e_st_ult = _e_st_ult;
                mDraw.e_s_ult = _e_s_ult;
                mDraw.Values_B = _valuesB;
                mDraw.Values_S = _valuesS;
                mDraw.colorsAndScale = new ColorScale(_valuesB, _ultMax, _ultMin);
                mDraw.PaintSectionMesh();
                mDraw.DrawReinforcementBar(BeamSection);

            }
            else if (BeamSection == BeamSection.Any) //заданное пользователем сечение
            {
                //TriangleNet.Geometry.Point cg = new TriangleNet.Geometry.Point();
                SectionChart.GenerateMesh(0);

                mDraw = new MeshDraw(Tri.Tri.Mesh);
                mDraw.MosaicMode = _Mode;
                mDraw.UltMax = _ultMax;
                mDraw.UltMin = _ultMin;
                mDraw.Rs_Ult = _ultRs;
                mDraw.e_st_ult = _e_st_ult;
                mDraw.e_s_ult = _e_s_ult;
                mDraw.Values_B = _valuesB;
                mDraw.Values_S = _valuesS;
                mDraw.colorsAndScale = new ColorScale(_valuesB, _ultMax, _ultMin);
                mDraw.PaintSectionMesh();
                mDraw.DrawReinforcementBar(BeamSection);
            }

            return mDraw;
        }
    }   
}

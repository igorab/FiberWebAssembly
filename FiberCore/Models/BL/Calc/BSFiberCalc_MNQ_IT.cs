using BSFiberCore.Models.BL.Beam;

namespace BSFiberCore.Models.BL.Calc
{
    public class BSFiberCalc_MNQ_IT_T: BSFiberCalc_MNQ_IT
    {
        public override BeamSection BeamSectionType() => BeamSection.TBeam;
    }

    public class BSFiberCalc_MNQ_IT_L : BSFiberCalc_MNQ_IT
    {
        public override BeamSection BeamSectionType() => BeamSection.LBeam;
    }


    // Расчет балки двутаврового сечения на внецентренное сжатие
    public class BSFiberCalc_MNQ_IT : BSFiberCalc_MNQ
    {
        public BSBeam_IT beam { get; set; }

        public override BeamSection BeamSectionType() => BeamSection.IBeam;
        
        public BSFiberCalc_MNQ_IT()
        {
            this.beam = new BSBeam_IT();

            base.m_Beam = this.beam;
        }

        public override bool Calculate()
        {
            if (N_Out)
            {
                Calculate_N_Out();                
            }
            else if (Shear)
            {
                m_ImgCalc = "Incline_Q.PNG";

                // Расчет на действие поперечной силы вдоль оси X
                _ = Calculate_Qcx(b, h);
                
                // Расчет на действие моментов относительно оси Y
                _ = Calculate_Mc(b, h);                
            }
            else if (UseRebar)
            {
                Calculate_N_Rods();
            }
            else
            {
                Calculate_N();
            }
            
            return true;
        }
       
        public override void SetSize(double[] _t)
        {
            beam.SetSizes(_t);
            base.m_Beam = this.beam;
            LngthCalc0 = beam.Length;
            b = beam.Width;
            h = beam.Height;
            I = beam.Jx();
            A = beam.Area();
            y_t = beam.y_t;
        }
    }
}

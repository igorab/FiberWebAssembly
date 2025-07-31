using System.Drawing;

namespace BSFiberCore.Models.BL.Tri
{
    public class MeshRect
    {
        private readonly int ny;
        private readonly int nz;

        private List<double> Ab;
        private List<double> y0b;
        private List<double> z0b;

        public List<RectangleF> rectangleFs { get; private set; }

        public MeshRect(int _ny, int _nz)
        {
            this.ny = _ny;
            this.nz = _nz;
            Ab = new List<double>();
            y0b = new List<double>();
            z0b = new List<double>();

            rectangleFs = new List<RectangleF>();
        }

        public int Rectangle(double _b, double _h, double _y0 = 0, double _z0 = 0)
        {
            // количество элементов сечения
            int n = ny * nz;
            double sy = _b / ny;
            double sz = _h / nz;
            // площадь 1 элемента
            double Ab1 = sy * sz;

            //заполнить массив площадей элементов            
            for (int i = 0; i < n; i++)
                Ab.Add(Ab1);

            //заполнить массив привязок бетонных эл-в к вспомогательной оси y0            
            for (int iz = 0; iz < nz; iz++)            
                for (int iy = 0; iy < ny; iy++)                
                    y0b.Add(iy * sy + sy / 2.0 + _y0);
                            
            //заполнить массив привязок бетонных эл-в к вспомогательной оси z0            
            for (int iz = 0; iz < nz; iz++)            
                for (int iy = 0; iy < ny; iy++)                
                    z0b.Add(iz * sz + sz / 2.0 + _z0);
                            
            for (int iz = 0; iz < nz; iz++)
            {
                for (int iy = 0; iy < ny; iy++)
                {
                    float bottom = (float)( iz * sz + _z0);
                    float left = (float)(iy * sy + _y0);

                    float top = (float)(iz * sz + sz + _z0);
                    float right = (float)(iy * sy + sy + _y0);

                    RectangleF rectangleF = RectangleF.FromLTRB(left, top, right, bottom);

                    rectangleFs.Add(rectangleF);
                }
            }

            return n;
        }

        public int IBeamSection(double _bf, double _hf, double _bw, double _hw, double _b1f, double _h1f)
        {
            int n1 = 0, n2 = 0, n3 = 0;

            if (_bf > 0 && _hf > 0)
                n1 = Rectangle(_bf, _hf, -_bf / 2.0, 0);

            n2 = Rectangle(_bw, _hw, -_bw / 2.0, _hf);

            if (_b1f > 0 && _h1f > 0)
                n3 = Rectangle(_b1f, _h1f, -_b1f / 2.0, _hf + _hw);

            return n1 + n2 + n3;
        }
    }
}

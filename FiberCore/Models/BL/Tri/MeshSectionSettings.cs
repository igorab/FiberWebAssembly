namespace BSFiberCore.Models.BL.Tri
{
    public class MeshSectionSettings
    {
        private int? _nX;
        private int? _nY;
        private double _minAngle;
        private double _maxArea;

        public int? NX
        {
            get { return _nX; }
            set { _nX = value; }
        }

        public int? NY
        {
            get { return _nY; }
            set { _nY = value; }
        }

        public double MinAngle
        {
            get { return _minAngle; }
            set { _minAngle = value; }
        }
        public double MaxArea
        {
            get { return _maxArea; }
            set { _maxArea = value; }
        }

        public MeshSectionSettings() { }

        public MeshSectionSettings(int nx, int ny, double minAngle, double maxArea)
        { 
            NX = nx;
            NY = ny;
            MinAngle = minAngle;
            MaxArea = maxArea;        
        }
    }
}

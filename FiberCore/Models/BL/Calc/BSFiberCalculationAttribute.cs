namespace BSFiberCore.Models.BL.Calc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    class BSFiberCalculationAttribute : Attribute
    {
        public string Name { get; set; }
        public string Descr { get; set; }
    }
}

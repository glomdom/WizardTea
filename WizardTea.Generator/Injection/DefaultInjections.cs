namespace WizardTea.Generator.Injection;

public static class DefaultInjections {
    #region struct injectors

    public static void SystemVector3ToXYZ(Injector injector) {
        injector.Register(InjectionPoint.StructEnd,
            ctx => """
                    public static implicit operator ByteVector3(System.Numerics.Vector3 v) {
                        var temp = new ByteVector3();
                        temp.x = (byte)v.X;
                        temp.y = (byte)v.Y;
                        temp.z = (byte)v.Z;
                        
                        return temp;
                    }
                    """);
    }
    
    #endregion
    
    #region global injectors

    public static void NormbyteToByte(Injector injector) {
        injector.Register(
            InjectionPoint.FieldOverride,
            ctx => ctx.CurrentSource.Contains("normbyte")
                ? ctx.CurrentSource.Replace("normbyte", "byte")
                : ctx.CurrentSource
        );
    }

    #endregion

}
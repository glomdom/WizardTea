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

    public static void XYZToSystemVector3(Injector injector) {
        injector.Register(
            InjectionPoint.StructEnd,
            ctx => """
                   public static implicit operator System.Numerics.Vector3(ByteVector3 v) {
                       var temp = new System.Numerics.Vector3();
                       temp.X = v.x;
                       temp.Y = v.y;
                       temp.Z = v.z;
                       
                       return temp;
                   }
                   """);
    }

    public static void CPUToNxDeviceCode(Injector injector) {
        injector.Register(
            InjectionPoint.FieldOverride,
            ctx => ctx.CurrentSource.Replace("CPU", "NxDeviceCode.CPU")
        );
    }

    public static void SCT_RigidBodyToNxCompartmentType(Injector injector) {
        injector.Register(
            InjectionPoint.FieldOverride,
            ctx => ctx.CurrentSource.Replace("SCT_RIGIDBODY", "NxCompartmentType.SCT_RIGIDBODY")
        );
    }

    #endregion

    #region global injectors

    public static void NormbyteToByte(Injector injector) {
        injector.Register(
            InjectionPoint.FieldOverride,
            ctx => ctx.CurrentSource.Replace("normbyte", "byte")
        );
    }

    public static void HFloatToHalf(Injector injector) {
        injector.Register(
            InjectionPoint.FieldOverride,
            ctx => ctx.CurrentSource.Replace("hfloat", "Half")
        );
    }
    
    public static void FloatMaxTokenToValue(Injector injector) {
        injector.Register(
            InjectionPoint.FieldOverride,
            ctx => {
                if (ctx.CurrentSource.Contains("#FLT_MAX#")) {
                    ctx.CurrentSource = ctx.CurrentSource[..^2] + ctx.CurrentSource[^1..];

                    return ctx.CurrentSource.Replace("#FLT_MAX#", "float.MaxValue");
                }

                return ctx.CurrentSource;
            }
        );
    }

    #endregion

}
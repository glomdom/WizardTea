namespace WizardTea.Generator.Injection;

public static class DefaultInjections {

    #region struct injectors

    public static void SystemVector3ToXYZ(Injector injector) {
        injector.Register(InjectionPoint.ItemEnd,
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
            InjectionPoint.ItemEnd,
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

    public static void Vector2ToTexCoord(Injector injector) {
        ValuesFromVec2ToProperties(injector, "TexCoord", "u", "v");
    }

    public static void EndianLittleToEndianType(Injector injector) {
        RegisterOverrideReplacement(injector, "ENDIAN_LITTLE", "EndianType.ENDIAN_LITTLE");
    }

    public static void CPUToNxDeviceCode(Injector injector) {
        RegisterOverrideReplacement(injector, "CPU", "NxDeviceCode.CPU");
    }

    public static void SCT_RigidBodyToNxCompartmentType(Injector injector) {
        RegisterOverrideReplacement(injector, "SCT_RIGIDBODY", "NxCompartmentType.SCT_RIGIDBODY");
    }

    public static void MIP_FMT_DEFAULTToMipMapFormat(Injector injector) {
        RegisterOverrideReplacement(injector, "MIP_FMT_DEFAULT", "MipMapFormat.MIP_FMT_DEFAULT");
    }
    
    public static void ALPHA_DEFAULTToAlphaFormat(Injector injector) {
        RegisterOverrideReplacement(injector, "ALPHA_DEFAULT", "AlphaFormat.ALPHA_DEFAULT");
    }

    public static void WRAP_S_WRAP_TToTexClampMode(Injector injector) {
        RegisterOverrideReplacement(injector, "WRAP_S_WRAP_T", "TexClampMode.WRAP_S_WRAP_T");
    }

    public static void FILTER_TRILERPToTexFilterMode(Injector injector) {
        RegisterOverrideReplacement(injector, "FILTER_TRILERP", "TexFilterMode.FILTER_TRILERP");
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

    public static void BlockTypeIndexToUint(Injector injector) {
        injector.Register(
            InjectionPoint.FieldOverride,
            ctx => ctx.CurrentSource.Replace("BlockTypeIndex", "short")
        );
    }

    public static void Ulittle32ToUint(Injector injector) {
        injector.Register(
            InjectionPoint.FieldOverride,
            ctx => ctx.CurrentSource.Replace("ulittle32", "uint")
        );
    }

    public static void FileVersionToInt(Injector injector) {
        injector.Register(
            InjectionPoint.FieldOverride,
            ctx => ctx.CurrentSource.Replace("FileVersion", "int")
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
    
    public static void XAxisTokenToValue(Injector injector) {
        injector.Register(
            InjectionPoint.FieldOverride,
            ctx => ctx.CurrentSource.Replace("#X_AXIS#", "Vector3.UnitX"));
    }

    public static void Vec2OneTokenToValue(Injector injector) {
        RegisterOverrideReplacement(injector, "#VEC2_ONE#", "Vector2.One");
    }

    public static void NiControllerSequenceTextKeysToNonHide(Injector injector) => RegisterOverrideReplacement(injector, "Text_Keys", "Text_Keys_Controller");
    public static void NiControllerSequenceAccumRootNameToNonHide(Injector injector) => RegisterOverrideReplacement(injector, "Accum_Root_Name", "Accum_Root_Name_Controller");
    public static void NiParticleSystemDataToNonHide(Injector injector) => RegisterOverrideReplacement(injector, "Data", "Data_Particle");

    #endregion

    #region utilities

    private static void RegisterOverrideReplacement(Injector injector, string from, string to) {
        injector.Register(
            InjectionPoint.FieldOverride,
            ctx => ctx.CurrentSource.Replace(from, to)
        );
    }

    private static void ValuesFromVec2ToProperties(Injector injector, string structName, string prop1, string prop2) {
        injector.Register(InjectionPoint.ItemEnd,
            ctx => $@"
                   public static implicit operator {structName}(System.Numerics.Vector2 v) {{
                       var temp = new {structName}();
                       temp.{prop1} = (byte)v.X;
                       temp.{prop2} = (byte)v.Y;
                       
                       return temp;
                   }}
                   ");
    }

    #endregion
}
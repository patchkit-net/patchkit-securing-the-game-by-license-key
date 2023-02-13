using JetBrains.Annotations;

namespace PatchKit.SecuringByTheLicenseKey.Core
{
public class SecuringByLicenseKeyVersion
{
    public static int Major = 1;

    public static int Minor = 0;

    public static int Patch = 0;

    [NotNull]
    public static string Name
    {
        get { return "v" + Major + "." + Minor + "." + Patch; }
    }
}
}
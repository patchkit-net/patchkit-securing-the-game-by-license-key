using PatchKit.SecuringByTheLicenseKey.Core;
using UnityEngine.Assertions;

namespace PatchKit.SecuringByTheLicenseKey.UI
{
public class SetAppSecretWindow : Window
{
    public void ShowWindow(SecuringByLicenseKey securingByLicenseKey)
    {
        var window = (SetAppSecretWindow) GetWindow(
            typeof(SetAppSecretWindow),
            false,
            "SetAppSecret");

        Assert.IsNotNull(window);

        window.Push<SetAppSecretScreen>().Initialize(securingByLicenseKey);
    }
}
}
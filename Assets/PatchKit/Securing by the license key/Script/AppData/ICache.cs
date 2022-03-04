namespace PatchKit.SecuringByTheLicenseKey.AppData
{
    public interface ICache
    {
        void SetValue(string key, int value);
        int GetValue(string key, int defaultValue);
    }
}
using PatchKit.SecuringByTheLicenseKey.Utilities;
using UnityEngine;

namespace PatchKit.SecuringByTheLicenseKey.AppData
{
    public class UnityCache : ICache
    {
        private readonly string _gameName;

        public UnityCache(string gameName)
        {
            _gameName = gameName;
        }

        private string FormatKey(string key)
        {
            return _gameName + "-" + key;
        }

        public void SetValue(string key, int value)
        {
            UnityDispatcher.Invoke(() =>
            {
                PlayerPrefs.SetInt(FormatKey(key), value);
                PlayerPrefs.Save();
            }).WaitOne();
        }

        public int GetValue(string key, int defaultValue)
        {
            int result = defaultValue;
            UnityDispatcher.Invoke(() => result = PlayerPrefs.GetInt(FormatKey(key), defaultValue)).WaitOne();
            return result;
        }
    }
}
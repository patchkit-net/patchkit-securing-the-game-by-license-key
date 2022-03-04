using System;
using System.IO;
using JetBrains.Annotations;
using PatchKit.Api.Models.Keys;
using UnityEngine;

namespace PatchKit.SecuringByTheLicenseKey.AppData
{
public class LocalMetaData
{
    private readonly char[] EndLineJson = new[] {',', '}'};

    private string _filePath;

    public string LicenseKey;
    public AppData Data;

    public LocalMetaData([NotNull] string filename)
    {
        Assert.IsNotNull(filename);

        _filePath = Path.Combine(MakeAppDataPathAbsolute(), filename);
    }

    public string MakeAppDataPathAbsolute()
    {
        string path = Path.GetDirectoryName(Application.dataPath);

        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            path = Path.GetDirectoryName(path);
        }

        // ReSharper disable once AssignNullToNotNullAttribute
        return Path.Combine(path);
    }


    public bool TryLoadDataFromFile()
    {
        try
        {
            Debug.Log("Trying to load data from file...");

            Debug.Log("Loading content from file...");
            var fileContent = File.ReadAllText(_filePath);
            Debug.Log("File content loaded.");
            Debug.Log("fileContent = " + fileContent);

            Debug.Log("Deserializing data...");
            Data = JsonUtility.FromJson<AppData>(fileContent);
            Debug.Log("Data deserialized.");
            LicenseKey = Data.product_key;
            Debug.Log("Data loaded from file. " + LicenseKey);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to load data from file." + e);

            return false;
        }
    }

    public bool FileExist()
    {
        return File.Exists(_filePath);
    }

    public void SaveKey(string key)
    {
        if (key != Data.product_key)
        {
            Debug.Log(string.Format("Saving data to {0}", _filePath));
            try
            {
                if (!FileExist())
                {
                    string data = JsonUtility.ToJson(Data);
                    File.WriteAllText(_filePath, data);
                }
                else
                {
                    var fileContent = File.ReadAllText(_filePath);
                    var index = fileContent.IndexOf("\"product_key\"", StringComparison.Ordinal);
                    if (index > 0)
                    {
                        string newData = fileContent.Substring(0, index);
                        int indexOfEndProductKey =
                            fileContent.Substring(index).IndexOfAny(EndLineJson) + index;
                        newData += "\"product_key\": \"" + Data.product_key + "\"";
                        newData += fileContent.Substring(indexOfEndProductKey);

                        File.WriteAllText(_filePath, newData);
                    }
                    else
                    {
                        fileContent = fileContent.Remove(fileContent.LastIndexOf("}", StringComparison.Ordinal));
                        fileContent += ", \"product_key\": \"" + Data.product_key + "\"}";

                        File.WriteAllText(_filePath, fileContent);
                    }
                }
                Debug.Log("Data saved.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to save data to file. " + e);
                throw;
            }
        }
    }
}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;

public class SecuringByLicenseKey : MonoBehaviour
{
    private const string AppDataFileName = "app_data.json";
    private static bool isFirstStart = true;
    private string _appDataPath;
    private string _filePath;
    private GUIStyle guiStyle;
    private Rect windowRect = new Rect((Screen.width - 300) / 2, (Screen.height - 100) / 2, 300, 100);
    private bool show = false;
    private string _labelMessage;
    private string _contentMessage;
    private string host = "keys2.patchkit.net";
    private string path = "/v2/keys/{key}";
    
    public string AppSecret;

    /// <summary>
    /// Data structure stored in file.
    /// </summary>
    private struct Data
    {
        [DefaultValue("patcher_data")] [JsonProperty("file_id", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string FileId;

        [DefaultValue("1.0")] [JsonProperty("version", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Version;

        [DefaultValue("")] [JsonProperty("product_key", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ProductKey;

        [DefaultValue("none")]
        [JsonProperty("product_key_encryption", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ProductKeyEncryption;

        [JsonProperty("_fileVersions")] public Dictionary<string, int> FileVersionIds;
    }

    private Data _data;

    void Start()
    {
#if UNITY_EDITOR
        return;
#endif
        if (isFirstStart)
        {
            isFirstStart = false;
            guiStyle = new GUIStyle();
            guiStyle.alignment = TextAnchor.UpperCenter;
            guiStyle.wordWrap = true;

            Time.timeScale = 0;
            var checkNetwork = GetResponse("", "", host);
            if (checkNetwork == null)
            {
                _labelMessage = "No internet connection";
                _contentMessage = "Your are not connected to internet. Please check your internet connection!";
                show = true;
                return;
            }

            _appDataPath = MakeAppDataPathAbsolute();
            _filePath = Path.Combine(_appDataPath, AppDataFileName);

            Debug.Log(_filePath);
            if (File.Exists(_filePath))
            {
                if (TryLoadDataFromFile())
                {
                    var keyInfo = GetKeyInfo();
                    if (keyInfo != null && keyInfo.StatusCode == HttpStatusCode.OK)
                    {
                        Time.timeScale = 1;
                        return;
                    }
                    else
                    {
                        _labelMessage = "License key is not found";
                        _contentMessage = $"License key is not found. Please start the game using the Launcher.";
                    }
                }
                else
                {
                    _labelMessage = $"Failed to load data from {AppDataFileName}";
                    _contentMessage =
                        $"Failed to load data from {AppDataFileName}. Please start the game using the Launcher.";
                }
            }
            else
            {
                _labelMessage = $"Not found {AppDataFileName}";
                _contentMessage = $"Not found {AppDataFileName}. Please start the game using the Launcher.";
            }
            show = true;
        }
    }

    private HttpWebResponse GetKeyInfo()
    {
        List<string> queryList = new List<string>();
        path = path.Replace("{key}", _data.ProductKey);
        queryList.Add("app_secret=" + AppSecret);

        string query = string.Join("&", queryList.ToArray());
        return GetResponse(path, query, host);
    }

    private HttpWebResponse GetResponse(string path, string query, string host)
    {
        try
        {
            Debug.Log("Getting response for path: '" + path + "' and query: '" + query + "'...");

            Debug.Log(host);
            var uri = new UriBuilder
            {
                Scheme = "https",
                Host = host,
                Path = path,
                Query = query
            }.Uri;
            Debug.Log(uri);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(uri);
            httpWebRequest.Timeout = 15000000; //15s
            return (HttpWebResponse) httpWebRequest.GetResponse();
        }
        catch (WebException e)
        {
            using (WebResponse response = e.Response)
            {
                return (HttpWebResponse) response;
            }
        }
    }


    private static string MakeAppDataPathAbsolute()
    {
        string path = Path.GetDirectoryName(Application.dataPath);

        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            path = Path.GetDirectoryName(path);
        }

        // ReSharper disable once AssignNullToNotNullAttribute
        return Path.Combine(path);
    }


    private bool TryLoadDataFromFile()
    {
        try
        {
            Debug.Log("Trying to load data from file...");

            Debug.Log("Loading content from file...");
            var fileContent = File.ReadAllText(_filePath);
            Debug.Log("File content loaded.");
            Debug.Log("fileContent = " + fileContent);

            Debug.Log("Deserializing data...");
            _data = JsonConvert.DeserializeObject<Data>(fileContent);
            Debug.Log("Data deserialized.");

            Debug.Log("Data loaded from file.");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to load data from file." + e);

            return false;
        }
    }

    void OnGUI()
    {
        if (show)
            windowRect = GUI.Window(0, windowRect, DialogWindow, _labelMessage);
    }

    void DialogWindow(int windowID)
    {
        GUI.Label(new Rect(5, 20, windowRect.width, 40), _contentMessage, guiStyle);

        if (GUI.Button(new Rect(windowRect.width / 4, 60, windowRect.width / 2, 20), "OK"))
        {
            Application.Quit();
            show = false;
        }
    }
}
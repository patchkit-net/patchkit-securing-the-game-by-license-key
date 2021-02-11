using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatchKit
{
    public class SecuringByLicenseKey : MonoBehaviour
    {
        private const string AppDataFileName = "app_data.json";
        private static bool _isFirstStart = true;
        private string _appDataPath;
        private string _filePath;
        private GUIStyle _guiStyle;
        private Rect _windowRectKeyInput = new Rect((Screen.width - 300) / 2, (Screen.height - 160) / 2, 300, 160);
        private Rect _windowRectInfo = new Rect((Screen.width - 300) / 2, (Screen.height - 160) / 2, 300, 100);
        private bool _show = false;
        private bool _notFoundKey = false;
        private string _labelMessage;
        private string _contentMessage;
        private string _tmpKey;
        private int _idWindow = 0;
        private string _host = "keys2.patchkit.net";
        private string _path = "/v2/keys/{key}";
        private AppData _data;

        public Options Option;
        public List<GameObject> Objects;
        public SceneAsset SceneLoad;
        public bool NotExecuteInEditor = false;
        public string AppSecret;

        [Serializable]
        struct AppData
        {
            public string product_key;
        }

        public enum Options
        {
            TimeStopStart,
            EnablesSelectedObjects,
            LoadNewScene,
            CallbackEntryAndExit
        }

        public virtual void OnEntry()
        {
        }

        public virtual void OnExit()
        {
        }

        private void Entry()
        {
            switch (Option)
            {
                case Options.TimeStopStart:
                    Time.timeScale = 0;
                    break;
                case Options.EnablesSelectedObjects:
                    Objects.ForEach(o => o.SetActive(false));
                    break;
                case Options.LoadNewScene:
                    break;
                case Options.CallbackEntryAndExit:
                    OnEntry();
                    break;
            }
        }

        private void Exit()
        {
            switch (Option)
            {
                case Options.TimeStopStart:
                    Time.timeScale = 1;
                    break;
                case Options.EnablesSelectedObjects:
                    Objects.ForEach(o => o.SetActive(true));
                    break;
                case Options.LoadNewScene:
                    SceneManager.LoadScene(SceneLoad.name);
                    break;
                case Options.CallbackEntryAndExit:
                    OnExit();
                    break;
            }
        }

        void Start()
        {
#if UNITY_EDITOR
            if (NotExecuteInEditor)
                return;
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
            if (_isFirstStart)
            {
                _isFirstStart = false;
                _guiStyle = new GUIStyle();
                _guiStyle.alignment = TextAnchor.UpperCenter;
                _guiStyle.wordWrap = true;

                Entry();
                var checkNetwork = GetResponse("", "", _host);
                if (checkNetwork == null)
                {
                    _labelMessage = "No internet connection";
                    _contentMessage = "Your are not connected to internet. Please check your internet connection!";
                    _show = true;
                    return;
                }

                if (string.IsNullOrEmpty(AppSecret))
                {
                    _labelMessage = "App secret Application is empty";
                    _contentMessage = "App secret is empty. Please contact with your developer!";
                    _show = true;
                    return;
                }

                _appDataPath = MakeAppDataPathAbsolute();
                _filePath = Path.Combine(_appDataPath, AppDataFileName);

                Debug.Log(_filePath);
                _idWindow = 1;
                if (File.Exists(_filePath))
                {
                    if (TryLoadDataFromFile())
                    {
                        var keyInfo = GetKeyInfo(_tmpKey);
                        if (keyInfo != null && CheckStatusCode(keyInfo))
                        {
                            return;
                        }
                    }
                    else
                    {
                        _labelMessage = $"Failed to load data from {AppDataFileName}";
                        _contentMessage =
                            $"Failed to load data from {AppDataFileName}. Please enter the license key:";
                    }
                }
                else
                {
                    _labelMessage = $"Not found {AppDataFileName}";
                    _contentMessage = $"Not found {AppDataFileName}. Please enter the license key:";
                }

                _show = true;
            }
        }

        private bool CheckStatusCode(HttpWebResponse keyInfo)
        {
            if (keyInfo != null && keyInfo.StatusCode == HttpStatusCode.OK)
            {
                Exit();
                return true;
            }
            else if (keyInfo != null && keyInfo.StatusCode == (HttpStatusCode) 410)
            {
                _labelMessage = "License key is blocked";
                _contentMessage = "License key is blocked. Please contact with your developer!";
            }
            else if (keyInfo != null && keyInfo.StatusCode == (HttpStatusCode) 404)
            {
                _labelMessage = "License key is not found.";
                _contentMessage = "License key is not found. Please enter the license key:";
            }
            else if (keyInfo != null && keyInfo.StatusCode == (HttpStatusCode) 403)
            {
                _labelMessage = "License key validation service is not available";
                _contentMessage = "License key validation service is not available. Try again.";
            }
            else
            {
                _labelMessage = "Unrecognized server or API error";
                _contentMessage = "Unrecognized server or API error. Please check your internet connection!";
            }

            return false;
        }

        private HttpWebResponse GetKeyInfo(string key)
        {
            List<string> queryList = new List<string>();
            string path = _path;
            path = path.Replace("{key}", key);
            queryList.Add("app_secret=" + AppSecret);

            string query = string.Join("&", queryList.ToArray());
            return GetResponse(path, query, _host);
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
                _data = JsonUtility.FromJson<AppData>(fileContent);
                Debug.Log("Data deserialized.");
                _tmpKey = _data.product_key;
                Debug.Log("Data loaded from file. " + _tmpKey);

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
            if (_show)
            {
                GUI.color = new Color(1, 1, 1, 0.5f); // half transparent
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
                switch (_idWindow)
                {
                    case 0:
                        GUI.Window(_idWindow, _windowRectInfo, DialogWindow, _labelMessage);
                        break;
                    case 1:
                        GUI.Window(_idWindow, _windowRectKeyInput, DialogWindow, _labelMessage);
                        break;
                }
            }
        }

        void DialogWindow(int windowID)
        {
            GUI.Label(new Rect(0, 20, _windowRectKeyInput.width, 40), _contentMessage, _guiStyle);
            if (_notFoundKey)
            {
                GUI.Label(
                    new Rect(_windowRectKeyInput.width / 4, 60, _windowRectKeyInput.width / 2, 20),
                    "License key is not found",
                    new GUIStyle() {normal = new GUIStyleState() {textColor = Color.red}});
            }

            switch (windowID)
            {
                case 0:
                    if (GUI.Button(new Rect(_windowRectKeyInput.width / 4, 60, _windowRectKeyInput.width / 2, 20), "OK"))
                    {
                        Application.Quit();
                        _show = false;
                    }

                    break;
                case 1: // key entry
                    _tmpKey = GUI.TextField(new Rect(_windowRectKeyInput.width / 10, 80, _windowRectKeyInput.width * 0.8f, 20),
                        _tmpKey);
                    if (GUI.Button(new Rect(_windowRectKeyInput.width / 4, 105, _windowRectKeyInput.width / 2, 20), "OK"))
                    {
                        if (CheckStatusCode(GetKeyInfo(_tmpKey)))
                        {
                            _data.product_key = _tmpKey;
                            Exit();
                            _show = false;
                        }
                        else
                        {
                            _notFoundKey = true;
                        }
                    }

                    if (GUI.Button(new Rect(_windowRectKeyInput.width / 4, 130, _windowRectKeyInput.width / 2, 20), "Exit"))
                    {
                        Application.Quit();
                        _show = false;
                    }

                    break;
            }
        }
    }
}
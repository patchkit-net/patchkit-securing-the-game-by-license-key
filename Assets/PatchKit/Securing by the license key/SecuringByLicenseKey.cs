using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.PatchKit.Securing_by_the_license_key
{
    public class SecuringByLicenseKey : MonoBehaviour
    {
        private const string AppDataFileName = "app_data.json";
        private static bool _isFirstStart = true;
        private string _appDataPath;
        private string _filePath;
        private GUIStyle _guiStyle;
        private Rect _windowRect = new Rect((Screen.width - 300) / 2, (Screen.height - 160) / 2, 300, 160);
        private bool _show = false;
        private bool _notFoundKey = false;
        private string _labelMessage;
        private string _contentMessage;
        private string _tmpKey;
        private int _idWindow = 0;
        private string _host = "keys2.patchkit.net";
        private string _path = "/v2/keys/{key}";
        private string _productKey = "product_key";
        
        public Options Option;
        public List<GameObject> Objects;
        public SceneAsset SceneLoad;
        public bool NotExecuteInEditor = false;
        public string AppSecret;
        private JObject _data;

        public enum Options
        { 
            TimeStopStart, 
            EnablesSelectedObjects, 
            LoadNewScene, 
            CallbackEntryAndExit
        }

        public virtual void OnEntry() {}
    
        public virtual void OnExit() {}
    
        private void Entry()
        {
            switch (Option)
            {
                case Options.TimeStopStart:
                    Time.timeScale = 0;
                    break;
                case Options.EnablesSelectedObjects:
                    Objects.ForEach(o=> o.SetActive(false));
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
                    Objects.ForEach(o=> o.SetActive(true)); 
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
           if(NotExecuteInEditor)
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
                
                if(string.IsNullOrEmpty(AppSecret))
                {
                    _labelMessage = "App secret Application is empty";
                    _contentMessage = "App secret is empty. Please contact with your developer!";
                    _show = true;
                    return;
                }
                _appDataPath = MakeAppDataPathAbsolute();
                _filePath = Path.Combine(_appDataPath, AppDataFileName);

                Debug.Log(_filePath);
                if (File.Exists(_filePath))
                {
                    if (TryLoadDataFromFile())
                    {
                        var keyInfo = GetKeyInfo(_tmpKey);
                        if (keyInfo != null && keyInfo.StatusCode == HttpStatusCode.OK)
                        {
                            Exit();
                            return;
                        }
                        else
                        {
                            _labelMessage = "License key is not found";
                            _contentMessage = $"License key is not found. Please enter the license key:";
                            _idWindow = 1;
                        }
                    }
                    else
                    {
                        _labelMessage = $"Failed to load data from {AppDataFileName}";
                        _contentMessage =
                            $"Failed to load data from {AppDataFileName}. Please enter the license key:";
                        _idWindow = 1;
                    }
                }
                else
                {
                    _labelMessage = $"Not found {AppDataFileName}";
                    _contentMessage = $"Not found {AppDataFileName}. Please enter the license key:";
                    _idWindow = 1;
                }
                _show = true;
            }
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
                _data = JObject.Parse(fileContent);
                Debug.Log("Data deserialized.");
                _tmpKey = _data[_productKey]?.ToString();
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

                GUI.color = new Color(1,1,1,0.5f); // half transparent
                GUI.Box (new Rect (0,0,Screen.width,Screen.height), "");
                _windowRect = GUI.Window(_idWindow, _windowRect, DialogWindow, _labelMessage);
            }
        }

        void DialogWindow(int windowID)
        {
            GUI.Label(new Rect(0, 20, _windowRect.width, 40), _contentMessage, _guiStyle);
            if (_notFoundKey)
            {
                GUI.Label(
                    new Rect(_windowRect.width / 4, 60, _windowRect.width / 2, 20),
                    "License key is not found",
                    new GUIStyle() {normal = new GUIStyleState() {textColor = Color.red}});
            }

            switch (windowID)
            {
                case 0:
                    if (GUI.Button(new Rect(_windowRect.width / 4, 60, _windowRect.width / 2, 20), "OK"))
                    {
                        Application.Quit();
                        _show = false;
                    }
                    break;
                case 1: // key entry
                    _tmpKey = GUI.TextField(new Rect(_windowRect.width / 10, 80, _windowRect.width *0.8f, 20), _tmpKey);
                    if (GUI.Button(new Rect(_windowRect.width / 4, 105, _windowRect.width / 2, 20), "OK"))
                    {
                        if (GetKeyInfo(_tmpKey).StatusCode != HttpStatusCode.OK)
                        {
                            _notFoundKey = true;
                        }
                        else
                        {
                            _data[_productKey] = _tmpKey;
                            SaveData();
                            Exit();
                            _show = false;
                        }
                    }
                    if (GUI.Button(new Rect(_windowRect.width / 4, 130, _windowRect.width / 2, 20), "Exit"))
                    {
                        Application.Quit();
                        _show = false;
                    }
                    break;
            }
        }
        
        private void SaveData()
        {
            Debug.Log(string.Format("Saving data to {0}", _filePath));

            CreateDataDir();
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(_data, Formatting.Indented));

            Debug.Log("Data saved.");
        }
        private void CreateDataDir()
        {
            string dirPath = Path.GetDirectoryName(_filePath);
            if (dirPath != null)
            {
                Directory.CreateDirectory(dirPath);
            }
        }
    }
}

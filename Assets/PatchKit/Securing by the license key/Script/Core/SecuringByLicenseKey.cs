using System.Collections.Generic;
using PatchKit.Api.Models.Keys;
using PatchKit.Api.Models.Main;
using PatchKit.SecuringByTheLicenseKey.Api;
using PatchKit.SecuringByTheLicenseKey.AppData;
using PatchKit.SecuringByTheLicenseKey.UI;
using PatchKit.SecuringByTheLicenseKey.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatchKit.SecuringByTheLicenseKey.Core
{
public class SecuringByLicenseKey : MonoBehaviour
{
    private const string AppDataFileName = "app_data.json";

    private bool _isFirstStart
    {
        get => _unityCache.GetValue("isFirstStart", 1) == 1;
        set => _unityCache.SetValue("isFirstStart", value ? 1 : 0);
    }

    private bool _show;
    private int _idWindow = 0;
    private DialogWindow _dialogWindow;
    private ApiConnection _apiConnection;
    private LocalMetaData _localMetaData;
    private InfoMessage _infoMessage;
    private App _app;
    private UnityCache _unityCache;
    
    [SerializeField] 
    public string AppSecret;
    
    [SerializeField] 
    public string SceneLoad;
    
    public Options Option;
    public List<GameObject> Objects;

    public bool NotExecuteInEditor = false;
    public bool AlwaysCheckAfterStarting = false;


    public enum Options
    {
        TimeStopStart,
        EnablesSelectedObjects,
        LoadNewScene,
        CallbackEntryAndExit
    }

    private void Awake()
    {
        UnityDispatcher.Initialize();
        _unityCache = new UnityCache(Application.productName);
    }

    void Start()
    {
#if UNITY_EDITOR
        if (NotExecuteInEditor)
            return;
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
        AppSecret appSecret = new AppSecret(AppSecret);
        _infoMessage = new InfoMessage();
        _apiConnection = new ApiConnection(appSecret, _infoMessage);
        _localMetaData = new LocalMetaData(AppDataFileName);
        _dialogWindow = new DialogWindow(_app, _apiConnection, _infoMessage);

        if (_isFirstStart || AlwaysCheckAfterStarting)
        {
            Entry();

            if (_apiConnection.CheckNetwork())
            {
                _infoMessage.LabelMessage = "No internet connection";
                _infoMessage.ContentMessage =
                    "Your are not connected to internet. Please check your internet connection!";
                _show = true;
                return;
            }

            if (string.IsNullOrEmpty(AppSecret))
            {
                _infoMessage.LabelMessage = "App secret Application is empty";
                _infoMessage.ContentMessage = "App secret is empty. Please contact with your developer!";
                _show = true;
                return;
            }

            _app = _apiConnection.GetAppInfo();

            if (!_app.use_keys)
            {
                Debug.Log("Validating license is not required - application is not using license keys.");
                return;
            }

            _idWindow = 1;
            if (_localMetaData.FileExist())
            {
                if (_localMetaData.TryLoadDataFromFile())
                {
                    var keyInfo = _apiConnection.GetKeyInfo(_localMetaData.LicenseKey);
                    if (keyInfo != null && _apiConnection.CheckStatusCode(keyInfo))
                    {
                        _dialogWindow.LicenseKey = _apiConnection.ParseResponse<LicenseKey>(keyInfo);
                        Exit();
                        return;
                    }
                }
                else
                {
                    _infoMessage.LabelMessage = $"Failed to load data from {AppDataFileName}";
                    _infoMessage.ContentMessage =
                        $"Failed to load data from {AppDataFileName}. Please enter the license key:";
                }
            }
            else
            {
                _infoMessage.LabelMessage = $"Not found {AppDataFileName}";
                _infoMessage.ContentMessage = $"Not found {AppDataFileName}. Please enter the license key:";
            }

            _show = true;
        }
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
                SceneManager.LoadScene(SceneLoad);
                break;
            case Options.CallbackEntryAndExit:
                OnExit();
                break;
        }

        _isFirstStart = false;

        _localMetaData.SaveKey(_dialogWindow.LicenseKey.key);
        _show = false;
        gameObject.SetActive(false);
    }

    void OnGUI()
    {
        if (_show && _dialogWindow.OnGUI(_idWindow))
            Exit();
    }
}
}
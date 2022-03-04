using PatchKit.Api.Models.Keys;
using PatchKit.Api.Models.Main;
using PatchKit.SecuringByTheLicenseKey.Api;
using UnityEditor;
using UnityEngine;

namespace PatchKit.SecuringByTheLicenseKey.UI
{
public class DialogWindow
{
    private Rect _windowRectKeyInput = new Rect((Screen.width - 300) / 2, (Screen.height - 160) / 2, 300, 160);
    private Rect _windowRectInfo = new Rect((Screen.width - 300) / 2, (Screen.height - 160) / 2, 300, 100);
    private bool _notFoundKey = false;
    private bool _isExit;
    private string _tmpKey;
    private GUIStyle _guiStyle;
    private App _app;
    private ApiConnection _apiConnection;
    private InfoMessage _infoMessage;

    public LicenseKey LicenseKey;
    public DialogWindow(App app, ApiConnection apiConnection, InfoMessage infoMessage)
    {
        Assert.IsNotNull(apiConnection);

        _app = app;
        _apiConnection = apiConnection;
        _infoMessage = infoMessage;
        _guiStyle = new GUIStyle();
        _guiStyle.alignment = TextAnchor.UpperCenter;
        _guiStyle.wordWrap = true;
    }

    public bool OnGUI(int idWindow)
    {
        GUI.color = new Color(1, 1, 1, 0.5f); // half transparent
        switch (idWindow)
        {
            case 0:
                GUILayout.Window(idWindow, _windowRectInfo, Window, _infoMessage.LabelMessage);
                break;
            case 1:
                GUILayout.Window(idWindow, _windowRectKeyInput, Window, _infoMessage.LabelMessage);
                break;
        }


        return _isExit;
    }

    void Window(int windowID)
    {
        if (_app.patcher_whitelabel)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(
                    PluginResources.Logo,
                    GUILayout.Height(64),
                    GUILayout.Width(256));
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Label(_infoMessage.ContentMessage, _guiStyle);
        if (_notFoundKey)
        {
            GUILayout.Label("License key is not found",
                new GUIStyle() {normal = new GUIStyleState() {textColor = Color.red}});
        }

        switch (windowID)
        {
            case 0:
                if (GUILayout.Button("OK"))
                {
                    Quit();
                }

                break;
            case 1: // key entry
                _tmpKey = GUILayout.TextField(_tmpKey);
                if (GUILayout.Button("OK"))
                {
                    var keyInfo = _apiConnection.GetKeyInfo(_tmpKey);
                    if (_apiConnection.CheckStatusCode(keyInfo))
                    {
                        LicenseKey = _apiConnection.ParseResponse<LicenseKey>(keyInfo);
                        _isExit = true;
                    }
                    else
                    {
                        _notFoundKey = true;
                    }
                }

                if (GUILayout.Button("Exit"))
                {
                    Quit();
                }

                break;
        }
    }

    void Quit()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
#endif
        Application.Quit();
    }
}
}

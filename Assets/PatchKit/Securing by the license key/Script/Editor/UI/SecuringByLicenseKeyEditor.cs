using PatchKit.SecuringByTheLicenseKey.Core;
using UnityEditor;
using UnityEngine;

namespace PatchKit.SecuringByTheLicenseKey
{
[CustomEditor(typeof(SecuringByLicenseKey))]
[CanEditMultipleObjects]
public class SecuringByLicenseKeyEditor : Editor
{
    private string _appSecret;
    private SecuringByLicenseKey securingByLicenseKey;

    void OnEnable()
    {
        securingByLicenseKey = target as SecuringByLicenseKey;
    }

    public override void OnInspectorGUI()
    {
        securingByLicenseKey.NotExecuteInEditor =
            EditorGUILayout.ToggleLeft("Not Execute In Editor", securingByLicenseKey.NotExecuteInEditor);
        securingByLicenseKey.AlwaysCheckAfterStarting =
            EditorGUILayout.ToggleLeft("Always Check After Starting",
                securingByLicenseKey.AlwaysCheckAfterStarting);
        serializedObject.Update();
        SetAppSecret();
        SetActionMode();
        DisplayLogo();

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
            EditorUtility.SetDirty(securingByLicenseKey);
    }

    private void SetActionMode()
    {
        securingByLicenseKey.Option =
            (SecuringByLicenseKey.Options) EditorGUILayout.EnumPopup("Action Mode:", securingByLicenseKey.Option);

        switch (securingByLicenseKey.Option)
        {
            case SecuringByLicenseKey.Options.TimeStopStart:
                break;

            case SecuringByLicenseKey.Options.EnablesSelectedObjects:
                EnablesSelectedObjects();
                break;

            case SecuringByLicenseKey.Options.LoadNewScene:
                LoadNewScene();
                break;

            case SecuringByLicenseKey.Options.CallbackEntryAndExit:
                break;
        }
    }

    private static void DisplayLogo()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(
                PluginResources.Logo,
                GUILayout.Height(64),
                GUILayout.Width(256));
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SetAppSecret()
    {
        EditorGUI.BeginChangeCheck();

        _appSecret = securingByLicenseKey.AppSecret;
        _appSecret = EditorGUILayout.TextField("App Secret:", _appSecret);
        AppSecretInfo();

        if (EditorGUI.EndChangeCheck())
        {
            var appSecretProperty = serializedObject.FindProperty("AppSecret");
            appSecretProperty.stringValue = _appSecret;
        }
    }

    private void AppSecretInfo()
    {
        string message = AppSecret.GetValidationError(_appSecret);
        if (message != null)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox(
                message,
                MessageType.Error);
            if (GUILayout.Button(
                new GUIContent(
                    PluginResources.Search,
                    "Find your AppSecret"),
                GUILayout.Width(40),
                GUILayout.Height(40)))
            {
                OpenApplicationsWebpage();
            }

            GUILayout.EndHorizontal();
        }
    }

    private void LoadNewScene()
    {
        var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(securingByLicenseKey.SceneLoad);
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        var newScene = EditorGUILayout.ObjectField("Scene:",
            oldScene, typeof(SceneAsset), false) as SceneAsset;
        if (securingByLicenseKey.SceneLoad == null)
            EditorGUILayout.HelpBox("Scene can't be none", MessageType.Error);
        if (EditorGUI.EndChangeCheck())
        {
            var newPath = AssetDatabase.GetAssetPath(newScene);
            var scenePathProperty = serializedObject.FindProperty("SceneLoad");
            scenePathProperty.stringValue = newPath;
        }
    }

    private void EnablesSelectedObjects()
    {
        int newCount = Mathf.Max(0,
            EditorGUILayout.IntField("Number of objects", securingByLicenseKey.Objects.Count));
        while (newCount < securingByLicenseKey.Objects.Count)
            securingByLicenseKey.Objects.RemoveAt(securingByLicenseKey.Objects.Count - 1);
        while (newCount > securingByLicenseKey.Objects.Count) securingByLicenseKey.Objects.Add(null);
        for (int i = 0; i < securingByLicenseKey.Objects.Count; i++)
        {
            securingByLicenseKey.Objects[i] =
                (GameObject) EditorGUILayout.ObjectField(securingByLicenseKey.Objects[i],
                    typeof(GameObject), true);
        }
    }

    private void OpenApplicationsWebpage()
    {
        Application.OpenURL("https://panel.patchkit.net/apps");
    }
}
}
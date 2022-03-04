using PatchKit.SecuringByTheLicenseKey.Building;
using PatchKit.SecuringByTheLicenseKey.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PatchKit.SecuringByTheLicenseKey.UI
{
public class SetAppSecretScreen : Screen
{
    #region GUI

    public override string Title
    {
        get { return "Set AppSecret"; }
    }

    public override Vector2? Size
    {
        get { return new Vector2(400f, 160f); }
    }

    public override void UpdateIfActive()
    {
    }

    public override void Draw()
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
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(
                "Set AppSecret your Application from PatchKit account",
                EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("AppSecret:");
            _newAppSecret = EditorGUILayout.TextField(_newAppSecret);

            if (GUILayout.Button(
                new GUIContent(
                    PluginResources.Search,
                    "Find your AppSecret"),
                GUILayout.Width(20),
                GUILayout.Height(20)))
            {
                Dispatch(() => OpenApplicationsWebpage());
            }
        }
        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(_newAppSecret))
        {
            if (NewAppSecretValidationError != null)
            {
                EditorGUILayout.HelpBox(
                    NewAppSecretValidationError,
                    MessageType.Error);
            }
            else
            {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Submit", GUILayout.Width(100)))
                    {
                        Dispatch(() => Link());
                    }

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Please enter your AppSecret to link your application from PatchKit account.",
                MessageType.Info);
        }

        EditorGUILayout.Separator();
    }

    #endregion

    #region Data

    [SerializeField] private SecuringByLicenseKey SecuringByTheLiceneseKey;

    [SerializeField] private string _newAppSecret;

    #endregion

    #region Logic

    public void Initialize(SecuringByLicenseKey securingByTheLiceneseKey)
    {
        SecuringByTheLiceneseKey = securingByTheLiceneseKey;
    }

    public override void OnActivatedFromTop(object result)
    {
    }

    private string NewAppSecretValidationError
    {
        get { return AppSecret.GetValidationError(_newAppSecret); }
    }

    private void Link()
    {
        SecuringByTheLiceneseKey.AppSecret = _newAppSecret;
        var serializedObject = new SerializedObject(SecuringByTheLiceneseKey);
        serializedObject.Update();
        var appSecretProperty = serializedObject.FindProperty("AppSecret");
        appSecretProperty.stringValue = _newAppSecret;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(SecuringByTheLiceneseKey);

        EditorSceneManager.MarkSceneDirty(PreBuild.SceneWithSecuring);
        EditorSceneManager.SaveScene(PreBuild.SceneWithSecuring);
        Close();
        
        PreBuild.BuildPlayer();
    }

    private void OpenApplicationsWebpage()
    {
        Application.OpenURL("https://panel.patchkit.net/apps");
    }

    #endregion
}
}
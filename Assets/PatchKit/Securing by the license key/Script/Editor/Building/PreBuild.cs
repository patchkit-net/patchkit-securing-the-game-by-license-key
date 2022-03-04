using PatchKit.SecuringByTheLicenseKey.Core;
using PatchKit.SecuringByTheLicenseKey.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatchKit.SecuringByTheLicenseKey.Building
{
[ExecuteInEditMode]
class PreBuild : MonoBehaviour
{
    private static BuildPlayerOptions _buildPlayerOptions;
    public static Scene SceneWithSecuring;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayerHandler);
    }

    public static void BuildPlayerHandler(BuildPlayerOptions options)
    {
        _buildPlayerOptions = options;
        if (CanBuild())
        {
            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
        }
    }

    public static void BuildPlayer()
    {
        if (CanBuild())
        {
            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(_buildPlayerOptions);
        }
    }

    public static bool CanBuild()
    {
        for (int i = 0; i < EditorSceneManager.sceneCountInBuildSettings; i++)
        {
            var activeScene = EditorSceneManager.GetSceneByBuildIndex(i);
            foreach (var gameObject in activeScene.GetRootGameObjects())
            {
                var securingByLicenseKey = gameObject.GetComponent<SecuringByLicenseKey>();
                if (securingByLicenseKey != null)
                {
                    SceneWithSecuring = activeScene;
                    if (AppSecret.GetValidationError(securingByLicenseKey.AppSecret) == null)
                        return true;
                    SetAppSecretWindow window = ScriptableObject.CreateInstance<SetAppSecretWindow>();
                    window.ShowWindow(securingByLicenseKey);
                }
            }
        }

        return false;
    }
}
}
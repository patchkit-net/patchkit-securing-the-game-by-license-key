using UnityEditor;
using UnityEngine;

namespace PatchKit
{
    [CustomEditor(typeof(SecuringByLicenseKey))]
    public class SecuringByLicenseKeyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SecuringByLicenseKey securingByLicenseKey = (SecuringByLicenseKey) target;
            securingByLicenseKey.NotExecuteInEditor =
                EditorGUILayout.Toggle("Not Execute In Editor", securingByLicenseKey.NotExecuteInEditor);
            
            securingByLicenseKey.AppSecret = EditorGUILayout.TextField("App Secret:", securingByLicenseKey.AppSecret);
            if (string.IsNullOrEmpty(securingByLicenseKey.AppSecret))
                EditorGUILayout.HelpBox("App Secret can't be empty", MessageType.Error);

            securingByLicenseKey.Option =
                (SecuringByLicenseKey.Options) EditorGUILayout.EnumPopup("Action mode:", securingByLicenseKey.Option);

            switch (securingByLicenseKey.Option)
            {
                case SecuringByLicenseKey.Options.TimeStopStart:
                    break;
                
                case SecuringByLicenseKey.Options.EnablesSelectedObjects:
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
                    break;
                
                case SecuringByLicenseKey.Options.LoadNewScene:
                    securingByLicenseKey.SceneLoad = (SceneAsset) EditorGUILayout.ObjectField("Scene:",
                        securingByLicenseKey.SceneLoad, typeof(SceneAsset), false);
                    if (securingByLicenseKey.SceneLoad == null)
                        EditorGUILayout.HelpBox("Scene can't be none", MessageType.Error);
                    break;
                
                case SecuringByLicenseKey.Options.CallbackEntryAndExit:
                    break;
            }
            
            if(GUI.changed)
                EditorUtility.SetDirty(securingByLicenseKey);
        }
    }
}

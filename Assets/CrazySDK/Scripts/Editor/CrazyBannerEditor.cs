using UnityEditor;

namespace CrazyGames
{
    [CustomEditor(typeof(CrazyBanner))]
    public class CrazyBannerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = (CrazyBanner)target;

            EditorGUI.BeginChangeCheck();
            var newValue = (CrazyBanner.BannerSize)EditorGUILayout.EnumPopup("Banner size", script.Size);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Change Banner Size");
                script.Size = newValue;
                EditorUtility.SetDirty(script);
            }
        }
    }
}

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class RemoveMeshCollidersTool : MonoBehaviour
{
    

    public void RemoveMeshColliders()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            MeshCollider[] colliders = obj.GetComponentsInChildren<MeshCollider>();
            foreach (MeshCollider collider in colliders)
            {
                Undo.DestroyObjectImmediate(collider);
            }
        }
        Debug.Log("Removed all MeshColliders from selected objects.");
    }
}

[CustomEditor(typeof(RemoveMeshCollidersTool))]
public class RemoveMeshCollidersToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RemoveMeshCollidersTool script = (RemoveMeshCollidersTool)target;
        if (GUILayout.Button("Remove Mesh Colliders"))
        {
            script.RemoveMeshColliders();
        }
    }
}
#endif

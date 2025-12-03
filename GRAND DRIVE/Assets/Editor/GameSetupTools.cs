using UnityEngine;
using UnityEditor;

public class GameSetupTools : EditorWindow
{
    [MenuItem("GRAND DRIVE/Setup Game Managers")]
    public static void SetupGameManagers()
    {
        string goName = "GameManagers";
        GameObject go = GameObject.Find(goName);

        if (go == null)
        {
            go = new GameObject(goName);
            Undo.RegisterCreatedObjectUndo(go, "Create GameManagers");
            Debug.Log($"Created new GameObject: {goName}");
        }
        else
        {
            Debug.Log($"Found existing GameObject: {goName}");
        }

        // Add TrajectoryManager
        if (go.GetComponent<TrajectoryManager>() == null)
        {
            Undo.AddComponent<TrajectoryManager>(go);
            Debug.Log("Added TrajectoryManager component.");
        }

        // Add MinimapSetup
        if (go.GetComponent<MinimapSetup>() == null)
        {
            Undo.AddComponent<MinimapSetup>(go);
            Debug.Log("Added MinimapSetup component.");
        }

        Selection.activeGameObject = go;
        Debug.Log("✅ Game Managers Setup Complete!");
    }
}

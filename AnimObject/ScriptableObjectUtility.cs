using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

public static class ScriptableObjectUtility
{

    /// <summary>
    //	This makes it easy to create, name and place unique new ScriptableObject asset files.
    // http://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset
    /// </summary>
    public static T CreateAsset<T>() where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        asset.name = "New " + asset.ToString() + asset.GetInstanceID() + ".asset";

#if UNITY_EDITOR
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + asset.ToString() + asset.GetInstanceID() + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        //EditorUtility.FocusProjectWindow();
        //Selection.activeObject = asset;
#endif
        return asset;
    }

    public static void DeleteAsset(AnimObject ao)
    {
        string path = "Assets/Resources/AnimObject Assets" + "/New " + ao.ToString() + ao.GetInstanceID() + ".asset";

#if UNITY_EDITOR
        AssetDatabase.DeleteAsset(path);
#endif
    }
}
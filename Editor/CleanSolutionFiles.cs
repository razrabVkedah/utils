using System.IO;
using UnityEditor;
using UnityEngine;

public static class CleanSolutionFiles
{
    [MenuItem("Rusleo/Clean Solution Files")]
    public static void Clean()
    {
        var root = Directory.GetParent(Application.dataPath)?.FullName;

        if (string.IsNullOrEmpty(root))
        {
            Debug.LogError("Root path not found");
            return;
        }

        var csprojFiles = Directory.GetFiles(root, "*.csproj");
        var slnFiles = Directory.GetFiles(root, "*.sln");

        foreach (var file in csprojFiles)
        {
            File.Delete(file);
            Debug.Log($"Deleted: {file}");
        }

        foreach (var file in slnFiles)
        {
            File.Delete(file);
            Debug.Log($"Deleted: {file}");
        }

        Debug.Log("Solution files cleaned. Reopen project.");
    }
}
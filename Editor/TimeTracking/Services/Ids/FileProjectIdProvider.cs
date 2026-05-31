using System;
using System.IO;
using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Ids
{
    public sealed class FileProjectIdProvider : IProjectIdProvider
    {
        private const string FolderPath = "ProjectSettings/RusleoTimeTracking";
        private const string FileName = "project_id.txt";

        public ProjectId GetProjectId()
        {
            var fullPath = GetFullPath();

            if (File.Exists(fullPath))
            {
                var value = File.ReadAllText(fullPath).Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    return new ProjectId(value);
            }

            var id = Guid.NewGuid().ToString("N");
            EnsureFolderExists();
            File.WriteAllText(fullPath, id);

            AssetDatabase.Refresh();

            return new ProjectId(id);
        }

        private static string GetFullPath()
        {
            return Path.Combine(FolderPath, FileName);
        }

        private static void EnsureFolderExists()
        {
            if (Directory.Exists(FolderPath))
                return;

            Directory.CreateDirectory(FolderPath);
        }
    }
}
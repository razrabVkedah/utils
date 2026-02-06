using System.Security.Cryptography;
using System.Text;
using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Ids
{
    public sealed class ProjectIdProvider : IProjectIdProvider
    {
        public ProjectId GetProjectId()
        {
            var cloudId = CloudProjectSettings.projectId;

            if (!string.IsNullOrWhiteSpace(cloudId))
                return new ProjectId(cloudId);

            var path = Application.dataPath;
            return new ProjectId(HashShort(path));
        }

        private static string HashShort(string input)
        {
            using var sha1 = SHA1.Create();
            var bytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
            var hash = sha1.ComputeHash(bytes);

            var sb = new StringBuilder(24);
            for (var i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));

            return sb.ToString().Substring(0, 12);
        }
    }
}
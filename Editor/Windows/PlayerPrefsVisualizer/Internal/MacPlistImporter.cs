using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using UnityEditor;

namespace Rusleo.Utils.Editor.Windows.PlayerPrefsVisualizer.Internal
{
    internal sealed class MacPlistImporter : IImporter
    {
        public string DisplayName => "macOS Plist";

        public int TryImportIntoIndex(PrefIndex index)
        {
            try
            {
                var company = PlayerSettings.companyName;
                var product = PlayerSettings.productName;
                var plistPath =
                    Environment.ExpandEnvironmentVariables(
                        $"%HOME%/Library/Preferences/unity.{company}.{product}.plist");
                if (!File.Exists(plistPath))
                {
                    EditorUtility.DisplayDialog("Plist", "No plist found.", "OK");
                    return 0;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "/usr/libexec/PlistBuddy",
                    Arguments = $"-c 'Print' \"{plistPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                var output = p.StandardOutput.ReadToEnd();
                var error = p.StandardError.ReadToEnd();
                p.WaitForExit();
                if (p.ExitCode != 0) throw new Exception(error);

                int imported = 0;
                using var sr = new StringReader(output);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("Dict")) continue;
                    var eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    var name = line[..eq].Trim();
                    var val = line[(eq + 1)..].Trim().Trim(';').Trim();
                    if (name == "__ppv_index") continue;

                    PrefType t;
                    string raw;
                    if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    {
                        t = PrefType.Int;
                        raw = val;
                    }
                    else if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    {
                        t = PrefType.Float;
                        raw = val;
                    }
                    else
                    {
                        if (val.Length >= 2 && val.StartsWith("\"") && val.EndsWith("\""))
                            val = val[1..^1];
                        t = PrefType.String;
                        raw = val;
                    }

                    index.AddOrUpdate(new PrefRecord(name, t, raw));
                    imported++;
                }

                index.Save();
                return imported;
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Plist Import Failed", e.Message, "OK");
                return 0;
            }
        }
    }
}
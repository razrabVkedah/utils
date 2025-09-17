using System;
using System.Globalization;
using System.Text;
using Microsoft.Win32;
using UnityEditor;

namespace Rusleo.Utils.Editor.Windows.PlayerPrefsVisualizer.Internal
{
internal sealed class WinRegistryImporter : IImporter
    {
        public string DisplayName => "Windows Registry";

        public int TryImportIntoIndex(PrefIndex index)
        {
            try
            {
                var company = PlayerSettings.companyName;
                var product = PlayerSettings.productName;
                var path = $"Software{company}{product}";
                using var key = Registry.CurrentUser.OpenSubKey(path, false);
                if (key == null) { EditorUtility.DisplayDialog("Registry", "No registry key found.", "OK"); return 0; }

                var valueNames = key.GetValueNames();
                int imported = 0;
                foreach (var name in valueNames)
                {
                    if (name == "__ppv_index") continue;
                    var kind = key.GetValueKind(name);
                    PrefType type; string raw;
                    switch (kind)
                    {
                        case RegistryValueKind.DWord:
                            type = PrefType.Int; raw = ((int)key.GetValue(name, 0)).ToString();
                            break;
                        case RegistryValueKind.QWord:
                            type = PrefType.Int; raw = Convert.ToInt32((long)key.GetValue(name, 0L)).ToString();
                            break;
                        case RegistryValueKind.String:
                        case RegistryValueKind.ExpandString:
                            var s = (string)key.GetValue(name, string.Empty);
                            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                            { type = PrefType.Float; raw = s; }
                            else { type = PrefType.String; raw = s; }
                            break;
                        case RegistryValueKind.Binary:
                            var bytes = (byte[])key.GetValue(name, Array.Empty<byte>());
                            raw = Encoding.UTF8.GetString(bytes);
                            type = PrefType.String;
                            break;
                        default: continue;
                    }
                    index.AddOrUpdate(new PrefRecord(name, type, raw));
                    imported++;
                }
                index.Save();
                return imported;
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Registry Import Failed", e.Message, "OK");
                return 0;
            }
        }
    }
}
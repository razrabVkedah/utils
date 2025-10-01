using System.Collections.Generic;
using UnityEngine;

namespace Rusleo.Graphics
{
    public class SceneWatchEntries : MonoBehaviour
    {
        [Header("Config")] public List<WatchEntry> entries = new();
    }
}
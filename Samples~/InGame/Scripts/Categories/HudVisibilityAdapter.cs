using Rusleo.Utils.Runtime.Hud;
using UnityEngine;

namespace Rusleo.Graphics
{
    public class HudVisibilityAdapter : MonoBehaviour
    {
        public void SetVisibility(bool visible)
        {
            HudService.TrySetVisible(visible);
        }
    }
}
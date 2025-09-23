using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;

namespace Rusleo.Graphics
{
    public enum ShadowQualityLevel
    {
        Off,
        Low,
        High
    }

    public class GraphicsRuntimeController : MonoBehaviour
    {
        [Header("Optional bindings (можно не заполнять)")] [SerializeField]
        Volume globalVolume; // глобальный Volume (если пусто — найдём/создадим)

        [SerializeField] bool createVolumeIfMissing = true;
        [SerializeField] float defaultVolumePriority = 9999f;

        UniversalRenderPipelineAsset _urp;
        VolumeProfile _profile;

        void Awake()
        {
            // Получаем текущий URP Asset
            _urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (_urp == null)
            {
                Debug.LogError("[GraphicsRuntimeController] URP Asset не найден. Убедись, что проект использует URP.");
            }

            // Находим или создаём глобальный Volume + профиль
            EnsureGlobalVolume();
        }

        #region Shadows

        public void SetShadows(ShadowQualityLevel level)
        {
            if (_urp == null) return;

            // switch (level)
            // {
            //     case ShadowQualityLevel.Off:
            //         _urp.supportsMainLightShadows = false;
            //         _urp.supportsSoftShadows = false;
            //         _urp.mainLightShadowmapResolution = ShadowResolution._512; // не важно, т.к. выключены
            //         _urp.shadowCascadeCount = 1;
            //         _urp.shadowDistance = 0f;
            //         break;
            //
            //     case ShadowQualityLevel.Low:
            //         _urp.supportsMainLightShadows = true;
            //         _urp.supportsSoftShadows = false;
            //         _urp.mainLightShadowmapResolution = ShadowResolution._1024;
            //         _urp.shadowCascadeCount = 1;
            //         _urp.shadowDistance = 35f;
            //         break;
            //
            //     case ShadowQualityLevel.High:
            //         _urp.supportsMainLightShadows = true;
            //         _urp.supportsSoftShadows = true;
            //         _urp.mainLightShadowmapResolution = ShadowResolution._2048; // можно 4096, если нужно
            //         _urp.shadowCascadeCount = 4;
            //         _urp.shadowDistance = 80f;
            //         break;
            // }

            // Применяем к активным камерам флаг рендеринга теней (на всякий случай)
            ApplyToAllCameras(camData =>
            {
                // В URP флагов "shadows on/off" на камере нет, поэтому полагаемся на asset.
                // Оставлено пустым намеренно.
            });
        }

        #endregion

        #region Post-processing (global enable + отдельные эффекты)

        /// <summary>Глобально включает/выключает постобработку на всех активных камерах (UniversalAdditionalCameraData.postProcessing).</summary>
        public void SetPostProcessingEnabled(bool enabled)
        {
            ApplyToAllCameras(camData => camData.renderPostProcessing = enabled);
        }

        public void SetBloom(bool enabled)
        {
            if (!EnsureProfile()) return;
            if (_profile.TryGet(out Bloom bloom))
            {
                bloom.active = enabled;
            }
            else if (enabled)
            {
                var bloomNew = _profile.Add<Bloom>(true);
                bloomNew.active = true;
            }
        }

        #endregion

        #region LOD & Draw Distance

        /// <summary>Настройка LOD (обычно 0.3 — 2.0). 1.0 — дефолт.</summary>
        public void SetLodBias(float bias)
        {
            bias = Mathf.Clamp(bias, 0.1f, 4f);
            QualitySettings.lodBias = bias;
        }

        /// <summary>Дистанция прорисовки (far clip) для всех активных камер + дистанция теней URP.</summary>
        public void SetDrawDistance(float meters)
        {
            var dist = Mathf.Max(10f, meters);
            ApplyToAllCameras(camData =>
            {
                var cam = camData.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.farClipPlane = dist;
                }
            });

            if (_urp != null)
            {
                // Дистанцию теней лучше держать <= общей дальности, иначе лишние расчёты вне видимого объёма
                _urp.shadowDistance = Mathf.Min(_urp.shadowDistance, dist);
            }
        }

        #endregion

        #region Helpers

        void EnsureGlobalVolume()
        {
            if (globalVolume == null)
            {
                // Пытаемся найти существующий глобальный volume с максимальным приоритетом
                var volumes = FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                globalVolume = volumes
                    .Where(v => v != null && v.isGlobal)
                    .OrderByDescending(v => v.priority)
                    .FirstOrDefault();
            }

            if (globalVolume == null && createVolumeIfMissing)
            {
                var go = new GameObject("Global Volume (Runtime)");
                globalVolume = go.AddComponent<Volume>();
                globalVolume.isGlobal = true;
                globalVolume.priority = defaultVolumePriority;
                DontDestroyOnLoad(go);
            }

            if (globalVolume != null)
            {
                if (globalVolume.profile == null)
                {
                    globalVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
                }

                _profile = globalVolume.profile;
            }
        }

        bool EnsureProfile()
        {
            if (_profile != null) return true;

            EnsureGlobalVolume();
            if (_profile == null)
            {
                Debug.LogError("[GraphicsRuntimeController] Не удалось получить/создать VolumeProfile.");
                return false;
            }

            return true;
        }

        void ApplyToAllCameras(System.Action<UniversalAdditionalCameraData> apply)
        {
            var cams = Camera.allCameras;
            for (var i = 0; i < cams.Length; i++)
            {
                var cam = cams[i];
                if (cam == null) continue;
                if (!cam.TryGetComponent<UniversalAdditionalCameraData>(out var data))
                {
                    data = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                }

                apply?.Invoke(data);
            }
        }

        #endregion
    }
}
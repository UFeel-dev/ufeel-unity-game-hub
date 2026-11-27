using UnityEngine;
using UnityEngine.Rendering;

namespace GameHub.Hub
{
    public sealed class RenderSettingsSnapshot
    {
        private readonly bool fog;
        private readonly Color fogColor;
        private readonly FogMode fogMode;
        private readonly float fogDensity;
        private readonly float fogStart;
        private readonly float fogEnd;
        private readonly AmbientMode ambientMode;
        private readonly Color ambientSkyColor;
        private readonly Color ambientEquatorColor;
        private readonly Color ambientGroundColor;
        private readonly float ambientIntensity;
        private readonly SphericalHarmonicsL2 ambientProbe;
        private readonly Material skybox;
        private readonly DefaultReflectionMode reflectionMode;
        private readonly Texture customReflection;
        private readonly float reflectionIntensity;

        private RenderSettingsSnapshot()
        {
            fog = RenderSettings.fog;
            fogColor = RenderSettings.fogColor;
            fogMode = RenderSettings.fogMode;
            fogDensity = RenderSettings.fogDensity;
            fogStart = RenderSettings.fogStartDistance;
            fogEnd = RenderSettings.fogEndDistance;
            ambientMode = RenderSettings.ambientMode;
            ambientSkyColor = RenderSettings.ambientSkyColor;
            ambientEquatorColor = RenderSettings.ambientEquatorColor;
            ambientGroundColor = RenderSettings.ambientGroundColor;
            ambientIntensity = RenderSettings.ambientIntensity;
            ambientProbe = RenderSettings.ambientProbe;
            skybox = RenderSettings.skybox;
            reflectionMode = RenderSettings.defaultReflectionMode;
            customReflection = RenderSettings.customReflectionTexture;
            reflectionIntensity = RenderSettings.reflectionIntensity;
        }

        public static RenderSettingsSnapshot Capture()
        {
            return new RenderSettingsSnapshot();
        }

        public void Apply()
        {
            RenderSettings.fog = fog;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.ambientProbe = ambientProbe;
            RenderSettings.skybox = skybox;
            RenderSettings.defaultReflectionMode = reflectionMode;
            RenderSettings.customReflectionTexture = customReflection;
            RenderSettings.reflectionIntensity = reflectionIntensity;
        }
    }
}

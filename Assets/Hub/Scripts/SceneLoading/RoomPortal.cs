using GameHub.Common;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameHub.Hub
{
    public class RoomPortal : MonoBehaviour
    {
        private const string HubLayerName = "Hub";
        private const string TextureProperty = "_MainTex";
        private const float MinClipDistance = 0.05f;

        [SerializeField] private Camera viewer;
        [SerializeField] private Transform hubRoot;
        [SerializeField] private MeshRenderer screen;

        private Camera? portalCamera;
        private RenderTexture? texture;
        private Vector3 offset;
        private RenderSettingsSnapshot? hubSettings;
        private RenderSettingsSnapshot? roomSettings;
        private int hubMask;
        private int viewerMask;

        private void Awake()
        {
            int hubLayer = LayerMask.NameToLayer(HubLayerName);
            hubMask = 1 << hubLayer;
            SetLayer(hubRoot, hubLayer);
            foreach (Light hubLight in hubRoot.GetComponentsInChildren<Light>(true))
                hubLight.cullingMask = hubMask;

            viewerMask = viewer.cullingMask;
            viewer.cullingMask = hubMask;
            screen.enabled = false;
        }

        private void OnDestroy()
        {
            Close();
        }

        private void LateUpdate()
        {
            if (portalCamera == null)
                return;

            if (texture == null || texture.width != Screen.width || texture.height != Screen.height)
                CreateTexture();

            Transform view = viewer.transform;
            portalCamera.transform.SetPositionAndRotation(view.position + offset, view.rotation);
            portalCamera.fieldOfView = viewer.fieldOfView;
            portalCamera.projectionMatrix = viewer.projectionMatrix;
            ClipAtPortal();
        }

        public void Open(Vector3 roomEntry, GameObject[] roomObjects, RenderSettingsSnapshot hub, RenderSettingsSnapshot room)
        {
            offset = roomEntry - HubLayout.HubEntry;
            hubSettings = hub;
            roomSettings = room;
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
            RenderPipelineManager.endCameraRendering += OnEndCamera;
            foreach (GameObject roomObject in roomObjects)
            {
                foreach (Light roomLight in roomObject.GetComponentsInChildren<Light>(true))
                    roomLight.cullingMask &= ~hubMask;
            }

            GameObject cameraObject = new(nameof(RoomPortal));
            cameraObject.transform.SetParent(transform, false);
            portalCamera = cameraObject.AddComponent<Camera>();
            portalCamera.CopyFrom(viewer);
            portalCamera.cullingMask = viewerMask & ~hubMask;
            portalCamera.depth = viewer.depth - 1f;
            CreateTexture();
            screen.enabled = true;
        }

        public void Enter()
        {
            FirstPersonController player = viewer.GetComponentInParent<FirstPersonController>();
            player.Teleport(player.transform.position + offset);
            viewer.cullingMask = viewerMask & ~hubMask;
            Close();
        }

        public void Close()
        {
            if (portalCamera != null)
                Destroy(portalCamera.gameObject);

            portalCamera = null;
            screen.enabled = false;
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            RenderPipelineManager.endCameraRendering -= OnEndCamera;
            hubSettings = null;
            roomSettings = null;
            ReleaseTexture();
        }

        private void OnBeginCamera(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (renderingCamera == portalCamera)
                roomSettings?.Apply();
        }

        private void OnEndCamera(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (renderingCamera == portalCamera)
                hubSettings?.Apply();
        }

        private static void SetLayer(Transform root, int layer)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = layer;
        }

        private void ClipAtPortal()
        {
            if (portalCamera == null)
                return;

            Vector3 planePoint = transform.position + offset;
            Vector3 cameraPosition = portalCamera.transform.position;
            Vector3 normal = transform.forward * Mathf.Sign(Vector3.Dot(transform.forward, planePoint - cameraPosition));
            if (Mathf.Abs(Vector3.Dot(normal, planePoint - cameraPosition)) < MinClipDistance)
                return;

            Matrix4x4 worldToCamera = portalCamera.worldToCameraMatrix;
            Vector3 cameraPoint = worldToCamera.MultiplyPoint(planePoint);
            Vector3 cameraNormal = worldToCamera.MultiplyVector(normal).normalized;
            Vector4 plane = new(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPoint, cameraNormal));
            portalCamera.projectionMatrix = viewer.CalculateObliqueMatrix(plane);
        }

        private void CreateTexture()
        {
            if (portalCamera == null)
                return;

            ReleaseTexture();
            texture = new RenderTexture(Screen.width, Screen.height, 24);
            portalCamera.targetTexture = texture;
            screen.material.SetTexture(TextureProperty, texture);
        }

        private void ReleaseTexture()
        {
            if (texture == null)
                return;

            if (portalCamera != null)
                portalCamera.targetTexture = null;

            texture.Release();
            Destroy(texture);
            texture = null;
        }
    }
}

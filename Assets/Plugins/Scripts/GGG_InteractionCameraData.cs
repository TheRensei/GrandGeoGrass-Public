using UnityEngine;

namespace GrandGeoGrass
{

    [RequireComponent(typeof(Camera))]
    public class GGG_InteractionCameraData : MonoBehaviour
    {
        [SerializeField] Camera cam = null;
        [SerializeField] RenderTexture rend = null;
        [SerializeField] LayerMask interactionLayer = 0;

        Vector4 data = Vector4.zero;

        void SetCameraSettings()
        {
            if (cam == null)
                cam = this.GetComponent<Camera>();
            cam.transform.rotation = Quaternion.Euler(90,180f,0);
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = interactionLayer;

            rend = cam.targetTexture;

            if (rend == null)
                Debug.LogError("Render texture was not assignet to the camera!", this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SetCameraSettings();
        }
#endif

        private void Start()
        {
            SetCameraSettings();
        }
        // Update is called once per frame
        void LateUpdate()
        {
            data = new Vector4(cam.transform.position.x, cam.transform.position.z, cam.orthographicSize);
            Shader.SetGlobalVector("_InteractionCameraData", data);
            Shader.SetGlobalTexture("_InteractionRenderTex", rend);
        }
    }
}
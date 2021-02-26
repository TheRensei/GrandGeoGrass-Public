using UnityEngine;

namespace GrandGeoGrass
{
    public class GGG_GlobalWindSettings : MonoBehaviour
    {
        [SerializeField] Vector2 direction = new Vector2(1, 1);
        [SerializeField] float speed = 0.01f;
        [SerializeField] float strength = 1f;

        Vector4 windData;

#if UNITY_EDITOR
        private void OnValidate()
        {
            direction = new Vector2(transform.forward.x, transform.forward.z);
            windData = new Vector4(-direction.x, -direction.y, strength, speed);
            Shader.SetGlobalVector("_WindData", windData);
        }

#endif
        //Change to Update on value change and maybe create a scriptable object holding settings
        //Doesn't need to be updated at all if once set so this is temporary 
        void Update()
        {
            direction = new Vector2(transform.forward.x, transform.forward.z);
            windData = new Vector4(-direction.x, -direction.y, strength, speed);
            Shader.SetGlobalVector("_WindData", windData);
        }
    }
}

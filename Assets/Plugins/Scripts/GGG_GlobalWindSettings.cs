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

        private void OnDrawGizmos()
        {
            // Vector3 pos = this.transform.position;
            // pos = (this.transform.forward*0.5f);
            // Gizmos.DrawCube(pos, new Vector3(0.2f, 0.2f, 1));
            // Gizmos.DrawCube(pos, new Vector3(0.2f, 0.2f, 0.2f));

            Vector3 pos = Vector3.zero;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one) * Matrix4x4.Rotate(Quaternion.Euler(0, -45, 0));
            pos.x += 0.3f;
            Gizmos.DrawCube(pos, new Vector3(0.2f,0.2f, 0.5f));
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one) * Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
            pos.x -= 0.6f;
            Gizmos.DrawCube(pos, new Vector3(0.2f,0.2f, 0.5f));
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            pos = Vector3.zero;
            Gizmos.DrawCube(pos, new Vector3(0.2f,0.2f, 1f));
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickParticleSpawn : MonoBehaviour
{
    Camera cam;
    public ParticleSystem particles;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                particles.transform.position = hit.point;
                particles.Play();
            }
        }
    }
}

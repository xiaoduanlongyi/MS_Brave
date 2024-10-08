using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    Transform cam;
    Vector3 cameraStartPos;
    float distance_x; //distance between camera starting position and current position
    float distance_y;

    GameObject[] backgrounds;
    Material[] mat;
    float[] backspeed;

    float farthestBack;

    [Range(0f, 0.5f)]
    public float parallexSpeed;

    void Start()
    {
        cam = Camera.main.transform;
        cameraStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backspeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
        }
        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++) //find the farthest background
        {
            if (backgrounds[i].transform.position.z - cam.position.z > farthestBack)
            {
                farthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backCount; i++) //set the speed of backgrounds
        {
            backspeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    private void LateUpdate()
    {
        distance_x = cam.position.x - cameraStartPos.x;
        distance_y = cam.position.y - cameraStartPos.y;
        transform.position = new Vector3(cam.position.x + 5, cam.position.y, 0); //background move with camera

        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backspeed[i] * parallexSpeed;
            mat[i].SetTextureOffset("_MainTex", new Vector2(distance_x, distance_y) * speed);
        }
    }
}

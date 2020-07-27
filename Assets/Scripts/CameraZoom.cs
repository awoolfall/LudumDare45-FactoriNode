using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] BoxCollider2D BackgroundCollider;
    static float ScrollSpeed = 50.0f;
    // Update is called once per frame

    void Update()
    {
        float Scroll = Input.GetAxis("Mouse ScrollWheel");
        print(Scroll);
        if (Scroll != 0.0f) {
            Vector2 MousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Camera.main.orthographicSize -= Time.deltaTime * (ScrollSpeed * Camera.main.orthographicSize) * Scroll;
            Camera.main.orthographicSize = Mathf.Max(Mathf.Abs(Camera.main.orthographicSize), 5);
            float BoxSize = Camera.main.orthographicSize * 10;
            BackgroundCollider.size = new Vector2(BoxSize, BoxSize);
        }
    }
}

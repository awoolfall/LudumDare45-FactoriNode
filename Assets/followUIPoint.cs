using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followUIPoint : MonoBehaviour
{
	public Transform uiAnchor;

    // Update is called once per frame
    void Update()
    {
		Vector3 followPos = Camera.main.ScreenToWorldPoint(uiAnchor.position);
		followPos.z = transform.position.z;
		transform.position = followPos;
	}
}

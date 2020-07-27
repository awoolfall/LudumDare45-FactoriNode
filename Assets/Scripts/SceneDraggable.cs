using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDraggable : MonoBehaviour
{
	public bool Dragging
	{
		set {
			dragging = value;
			if (dragging) {
				mouseLastPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			}
		}
	}

	Vector2 mouseLastPos;
	private bool dragging = false;

	public static SceneDraggable instance;
	private void Awake()
	{
		instance = this;
	}

	// Update is called once per frame
	void Update()
    {
		if (dragging && Input.GetMouseButtonUp(1)) {
			Dragging = false;
		}
		if (dragging)
		{
			Vector2 curScreenPoint = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			Vector3 worldDiff = Camera.main.ScreenToWorldPoint(mouseLastPos) - Camera.main.ScreenToWorldPoint(curScreenPoint);
			Camera.main.transform.position += worldDiff;
            mouseLastPos = curScreenPoint;
		}
	}

	private void OnMouseOver()
	{
		if (Input.GetMouseButtonDown(1)) {
			Dragging = true;
		}
		if (Input.GetMouseButtonDown(0)) {
			/* Start selection box */
			SelectController.self.deselectNodes();
			SelectController.self.StartSelectionBox();
		}
	}
}

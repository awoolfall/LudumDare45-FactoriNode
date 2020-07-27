using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDraggable : MonoBehaviour {

	public bool Dragging
	{
		set {
			dragging = value;
		}
	}

	public bool followUI = false;
	public Transform uiAnchor;

	Vector3 positionOffset;
	private bool dragging = false;

    // Update is called once per frame
    void Update()
    {
		if (dragging)
		{
			SelectController.self.dragSelected();
		}

		if (followUI)
		{
			Vector3 followPos = Camera.main.ScreenToWorldPoint(uiAnchor.position);
			followPos.z = -1f;
			transform.position = followPos; 
		}

	}

	public void performDrag()
	{
		Vector2 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
		Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + positionOffset;
		transform.position = new Vector3 (curPosition.x, curPosition.y, -.5f);
		Collider2D[] NearNodes = Physics2D.OverlapAreaAll(curPosition + new Vector3(10, 10), curPosition + new Vector3(-10, -10), ~LayerMask.NameToLayer("Node"));
		foreach (Collider2D Col in NearNodes) {
			if (SelectController.self.isSelected(Col.gameObject.GetComponent<Node> ())) continue;
			if (Mathf.Abs(Col.transform.position.x - transform.position.x) < 0.25f) {
				transform.position = new Vector3(Col.transform.position.x, transform.position.y, transform.position.z);
			}
			if (Mathf.Abs(Col.transform.position.y - transform.position.y) < 0.25f) {
				transform.position = new Vector3(transform.position.x, Col.transform.position.y, transform.position.z);
			}
		}
	}

	public void Release()
	{
		Vector3 pos = transform.position;
		pos.z = 0;
		transform.position = pos;
	}

	public void setPositionOffset()
	{
		positionOffset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
	}

	private void OnMouseOver()
	{
		if (Input.GetMouseButtonDown(1))
		{
			SceneDraggable.instance.Dragging = true;
		}
	}

	private void OnMouseDown()
	{
		if (followUI)
		{
			if (!GameManager.instance.recipeManager.PurchaseNode(GetComponent<Node>()))
			{
				//if we cant afford it player cant take it
				return;
			}
			followUI = false;
		}
		Dragging = true;
		setPositionOffset();
		SelectController.self.prepareDragSelected();
		
		
	}

	private void OnMouseUp()
	{
		Dragging = false;
		SelectController.self.releaseSelected();
	}
}

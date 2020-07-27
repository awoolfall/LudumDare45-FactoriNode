using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIConnectorOut : MonoBehaviour
{
	public LineRenderer line;
	public float lineBendDistance = 0.5f;

	public static UIConnectorOut currentDraggedConnector;

	public UnityEngine.UI.Text Text;
	public UIConnectorIn endPoint = null;

	[SerializeField] Material MatActive;
	[SerializeField] Material MatInactive;
	[SerializeField] Material MatActiveRev;

	[HideInInspector]
	public RecipeScriptableObject outputRecipe;

	private void Update()
	{
		if (endPoint)
		{
			Vector3 linePoint = transform.position;
			linePoint.z = -1;
			line.SetPosition(0, linePoint);
			linePoint.x = linePoint.x + lineBendDistance;
			line.SetPosition(1, linePoint);

			linePoint = endPoint.transform.position;
			linePoint.z = -1;
			line.SetPosition(3, linePoint);
			linePoint.x = linePoint.x - lineBendDistance;
			line.SetPosition(2, linePoint);
		}
		if (endPoint != null) {
			if (endPoint.GetNode().Running) {
				if (transform.position.x <= endPoint.transform.position.x) {
					line.material = MatActive;
				} else {
					line.material = MatActiveRev;
				}
			} else {
				line.material = MatInactive;
			}
		} else {
			line.material = MatInactive;
		}
	}

	private void OnMouseOver()
	{
		if (Input.GetMouseButtonDown(1))
		{
			SceneDraggable.instance.Dragging = true;
		}
	}

	public void OnMouseDrag()
	{
		//update line renderer
		if (GetNode().draggable.followUI)
		{
			return;//do nothing if it hasnt been purchased yet
		}
		Debug.Log("ondrag");
		Vector3 linePoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
		linePoint.z = -1;
		line.SetPosition(3, linePoint );
		linePoint.x = linePoint.x - lineBendDistance;
		line.SetPosition(2, linePoint);
	}

	public void OnMouseDown()
	{
		if (GetNode().draggable.followUI)
		{
			return;//do nothing if it hasnt been purchased yet
		}
		if (endPoint)
		{
			endPoint.startPoint = null;
		}
		endPoint = null;

		Vector3 linePoint = transform.position;
		linePoint.z = -1;
		line.SetPosition(0, linePoint);
		linePoint.x = linePoint.x + lineBendDistance;
		line.SetPosition(1, linePoint);

		currentDraggedConnector = this;

		StartCoroutine(LineAfterOneFrame());
		Debug.Log("drag start");
	}

	public void OnMouseUp()
	{
		if (GetNode().draggable.followUI)
		{
			return;//do nothing if it hasnt been purchased yet
		}
		if (UIConnectorIn.currentHoveredConnector)
		{
			if (UIConnectorIn.currentHoveredConnector.GetNode().draggable.followUI)
			{
				//purchase this node
				if (!GameManager.instance.recipeManager.PurchaseNode(UIConnectorIn.currentHoveredConnector.GetNode()))
				{
					//if we cant afford it player cant take it
					return;
				}
				UIConnectorIn.currentHoveredConnector.GetNode().draggable.followUI = false;
			}
			endPoint = UIConnectorIn.currentHoveredConnector;
			endPoint.startPoint = this;
		}
		else
		{
			line.enabled = false;
			line.SetPosition(0, transform.position);
			line.SetPosition(1, transform.position);
			line.SetPosition(2, transform.position);
			line.SetPosition(3, transform.position);

		}

		Debug.Log("drag end");
		currentDraggedConnector = null;
	}

	private Node node = null;
	public Node GetNode()
	{
		//cache those components
		if (!node)
		{
			node = GetComponentInParent<Node>();
		}
		return node;
	}

	IEnumerator LineAfterOneFrame()
	{
		yield return null;
		line.enabled = true;
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIConnectorIn : MonoBehaviour
{

	public static UIConnectorIn currentHoveredConnector;

	public UnityEngine.UI.Text Text;
	public UIConnectorOut startPoint = null;

	[HideInInspector]
	public RecipeScriptableObject inputRecipe;


	private void OnMouseEnter()
	{
		Debug.Log("entered");
		if(startPoint == null)
		{
			if (UIConnectorOut.currentDraggedConnector && UIConnectorOut.currentDraggedConnector.outputRecipe == inputRecipe)
			{
				currentHoveredConnector = this;
			}
		}
		
	}

	private void OnMouseExit()
	{
		Debug.Log("exit");
		
		currentHoveredConnector = null;

	}

	private UIConnectorOut tempOut = null;
	private void OnMouseDown()
	{
		if (startPoint)
		{
			tempOut = startPoint;
			startPoint.OnMouseDown();
		}
	}

	private void OnMouseUp()
	{
		if (tempOut)
		{
			tempOut.OnMouseUp();
			tempOut = null;
		}
	}
	private void OnMouseDrag()
	{
		if (tempOut)
		{
			tempOut.OnMouseDrag();
		}
	}

	private void OnMouseOver()
	{
		if (Input.GetMouseButtonDown(1))
		{
			SceneDraggable.instance.Dragging = true;
		}
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

}

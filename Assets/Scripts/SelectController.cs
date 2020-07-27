using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectController : MonoBehaviour
{
    public static SelectController self;

    private List<Node> SelectedList = new List<Node> ();
    [SerializeField] private RectTransform SelectBox;

    private bool IsSelecting = false;
    Vector2 SelectingStartingPos;

    void Start()
    {
        SelectController.self = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsSelecting) {
            if (Input.GetMouseButtonUp(0)) {
                StopSelecting();
            }

            Vector2 Diff = new Vector2(Input.mousePosition.x - SelectingStartingPos.x, Input.mousePosition.y - SelectingStartingPos.y);
            SelectBox.localScale = new Vector3(Mathf.Abs(SelectBox.localScale.x) * (Diff.x > 0.0 ? 1 : -1), Mathf.Abs(SelectBox.localScale.y) * (Diff.y > 0.0 ? -1 : 1), SelectBox.localScale.z);
            float Scale = 1.0f/Mathf.Abs(SelectBox.transform.localScale.x);
            Diff.Scale(new Vector2(Scale, -Scale));
            SelectBox.sizeDelta = new Vector2(Mathf.Abs(Diff.x), Mathf.Abs(Diff.y));
        } else {
            if (Input.GetKeyDown(KeyCode.D)) {
                duplicateNodeSelection();
            }
            if (Input.GetKeyDown(KeyCode.Q)) {
                deleteNodeSelection();
            }
        }
    }

    private void StopSelecting()
    {
        IsSelecting = false;
        SelectBox.GetComponent<UnityEngine.UI.Image> ().enabled = false;
        Vector2 Vec2 = new Vector2(SelectBox.sizeDelta.x, -SelectBox.sizeDelta.y);
        Vec2.Scale(SelectBox.localScale);
        Vector3 BotRight = Camera.main.ScreenToWorldPoint(SelectBox.anchoredPosition + Vec2);
        Vector3 TopLeft = Camera.main.ScreenToWorldPoint(SelectBox.anchoredPosition);

        Debug.DrawLine(TopLeft, BotRight, Color.red, 4);

        Collider2D[] hits = Physics2D.OverlapAreaAll(TopLeft, BotRight, ~LayerMask.NameToLayer("Node"));
        foreach (Collider2D hit in hits) {
            if (!hit.gameObject.GetComponent<Node> ().IsPreviewNode) {
                addNodeToSelected(hit.gameObject.GetComponent<Node> ());
            }
        }
    }

    public void StartSelectionBox()
    {
        IsSelecting = true;
        SelectBox.GetComponent<UnityEngine.UI.Image> ().enabled = true;
        SelectingStartingPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        SelectBox.anchoredPosition = SelectingStartingPos;
    }

    public void setNodeAsSelected(Node node) 
    {
        if (!SelectedList.Contains(node)) {
            deselectNodes();
            addNodeToSelected(node);
        }
    }

    public void addNodeToSelected(Node node)
    {
        if (!SelectedList.Contains(node)) {
            SelectedList.Add(node);
            print("node selected: now at " + SelectedList.Count + " nodes");
            node.setAsSelected(true);
            DupePanel.setDupeCost(getPriceOfSelected());
        }
    }

    public void deselectNodes()
    {
        print("nodes deselected");
        foreach (Node n in SelectedList) {
            n.setAsSelected(false);
        }
        SelectedList.Clear();
        DupePanel.setDupeCost(0);
    }


    List<Node> newSelectionList = new List<Node> ();

    void recursiveDoop(Node doopedNode, Node originalNode)
    {
        for (int index = 0; index < originalNode.InConnections.Count; index++){
            UIConnectorIn i = originalNode.InConnections[index];
            if (i.startPoint != null) {
                if (SelectedList.Contains(i.startPoint.GetNode())) {
                    Node newNode = i.startPoint.GetNode().duplicateNodeSuchThatThisNodeStillExistsButThereNowExistsAnotherIdenticalOneBesideItAlsoSetTheOutputToLinkToTheInConnectionSuppliedInTheFunctionParametersIfItIsNotNull(doopedNode.InConnections[index]);
                    SelectedList.Remove(i.startPoint.GetNode());
                    newSelectionList.Add(newNode);
                    recursiveDoop(newNode, i.startPoint.GetNode());
                }
            }
        }
    }

    public void duplicateNodeSelection()
    {
        if (SelectedList.Count == 0) return;
        if (GameManager.GetCurrentMoney() < this.getPriceOfSelected()) return;

        GameManager.instance.CurrentMoney -= this.getPriceOfSelected();

        foreach (Node n in SelectedList) {
            n.setAsSelected(false);
        }
        newSelectionList.Clear();
        while (SelectedList.Count > 0) {
            Node endNode = SelectedList[0];
            if (endNode.OutConnection != null) {
                if (endNode.OutConnection.endPoint != null) {
                    while (SelectedList.Contains(endNode.OutConnection.endPoint.GetNode())) {
                        endNode = endNode.OutConnection.endPoint.GetNode();
                        if (endNode.OutConnection == null) break;
                        if (endNode.OutConnection.endPoint == null) break;
                    }
                }
            }
            
            if (!endNode.IsPreviewNode) {
                Node newNode = endNode.duplicateNodeSuchThatThisNodeStillExistsButThereNowExistsAnotherIdenticalOneBesideItAlsoSetTheOutputToLinkToTheInConnectionSuppliedInTheFunctionParametersIfItIsNotNull(null);
                newSelectionList.Add(newNode);
                recursiveDoop(newNode, endNode);
            }
            SelectedList.Remove(endNode);
        }

        foreach (Node n in newSelectionList) {
            addNodeToSelected(n);
        }
    }

    public void deleteNodeSelection()
    {
        GameManager.instance.CurrentMoney += (getPriceOfSelected() / 2);
        foreach (Node n in SelectedList) {
            if (!n.IsPreviewNode) {
                Destroy(n.gameObject);
            }
        }
        SelectedList.Clear();
    }

    public bool isSelected(Node node)
    {
        return SelectedList.Contains(node);
    }

    public void prepareDragSelected()
    {
        foreach (Node n in SelectedList) {
            n.GetComponent<UIDraggable> ().setPositionOffset();
        }
    }

    public void dragSelected()
    {
        foreach (Node n in SelectedList) {
            n.GetComponent<UIDraggable> ().performDrag();
        }
    }

	public void releaseSelected()
	{
		foreach (Node n in SelectedList)
		{
			n.GetComponent<UIDraggable>().Release();
		}
	}

    public int getPriceOfSelected()
    {
        int sum = 0;
        foreach (Node n in SelectedList) {
            sum += n.Recipe.getPrice();
        }
        return sum;
    }
}

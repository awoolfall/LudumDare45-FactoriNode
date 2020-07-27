using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
	public static List<Node> allNodes = new List<Node>();

    public UnityEngine.UI.Text TitleText;
    public Transform inPivot;
    public Transform outPivot;
	public Transform moneyPivot;
	public UnityEngine.UI.Text moneyText;
    public SpriteRenderer Background;
    public SpriteRenderer ItemImage;
    public SpriteRenderer RunningImage;
	public Transform Gear1;
	public Transform Gear2;
	public float rotationRate = 3;

    public GameObject InPrefab;
    public GameObject OutPrefab;

    public List<UIConnectorIn> InConnections = new List<UIConnectorIn> ();
    public UIConnectorOut OutConnection;

	public UIDraggable draggable;
	public bool IsPreviewNode = false;

	public bool Running
	{
		get
		{
			return _running;
		}
		set
		{
			if(_running != value)
			{
				_running = value;
				if (_running)
				{
                    RunningImage.enabled = true;
					if (firstRun)
					{
						GameManager.instance.recipeManager.BuiltRecipe(Recipe);
						firstRun = false;
					}
				}
				else
				{
                    RunningImage.enabled = false;
				}
			}
		}
	}
	[SerializeField]
	private bool _running = false;
	private bool firstRun = true;

	[HideInInspector]
    public RecipeScriptableObject Recipe;

    // yes, these are hard coded
    float background_base_height = 2.1f;
    float col_base_height = 0.16f;
    Color selectedColor = new Color(1, 0.935f, 0.875f);
    Color unselectedColor = Color.white;


    void OnDestroy()
    {
        foreach (UIConnectorIn i in InConnections) {
            if (i.startPoint != null) {
                i.startPoint.endPoint = null;
                i.startPoint.line.enabled = false;
                i.startPoint = null;
            }
        }
		if (OutConnection != null) {
			if (OutConnection.endPoint != null) {
				OutConnection.endPoint.startPoint = null;
				OutConnection.endPoint = null;
			}
		}
    }

	private void Update()
	{
		//rotate cogs
		if (Running)
		{
	
			Gear1.Rotate(new Vector3(0, 0, (rotationRate * Time.deltaTime)), Space.Self);
			Gear2.Rotate(new Vector3(0, 0, -(rotationRate * Time.deltaTime)), Space.Self);
		}
	}

	void OnMouseOver()
    {

    }

    void OnMouseDown()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
            SelectController.self.addNodeToSelected(this);
        } else {
            SelectController.self.setNodeAsSelected(this);
        }
    }

    public void setAsSelected(bool Selected)
    {
        if (Selected) {
            Background.color = selectedColor;
        } else {
            Background.color = unselectedColor;
        }
    }

    public void generateNode(RecipeScriptableObject pRecipe)
    {
        foreach (UIConnectorIn i in InConnections) {
            Destroy(i);
        }
        InConnections.Clear();

        int ingredients_count = Mathf.Max(pRecipe.ingredients.Count-1, 0);

        Background.size = new Vector2(Background.size.x, background_base_height + (ingredients_count * 0.5f));
        BoxCollider2D col = GetComponent<BoxCollider2D> ();
        col.size = new Vector2(col.size.x, col_base_height + (ingredients_count * 0.5f));
        col.offset = new Vector2(0, -(ingredients_count * 0.25f));

        for (int i = 0; i < pRecipe.ingredients.Count; i++) {
            RecipeScriptableObject ingredient = pRecipe.ingredients[i];
            GameObject go = Instantiate(InPrefab, (inPivot.position + new Vector3(0, -0.5f * i, -0.1f)), Quaternion.identity);
            InConnections.Add(go.GetComponent<UIConnectorIn> ());
            go.transform.SetParent(inPivot, true);
            go.GetComponent<UIConnectorIn> ().Text.text = ingredient.name;
			go.GetComponent<UIConnectorIn>().inputRecipe = pRecipe.ingredients[i];
		}

        TitleText.text = pRecipe.GetFullName();
		if (pRecipe.IsFinalNode)
		{
			Destroy(OutConnection.gameObject);
		}
		else
		{
			OutConnection.Text.text = pRecipe.name;
			OutConnection.outputRecipe = pRecipe;
		}
		this.gameObject.name = pRecipe.GetFullName() + "_" + (Time.frameCount % 9999999);
        Recipe = pRecipe;
		
    }

    public Node duplicateNodeSuchThatThisNodeStillExistsButThereNowExistsAnotherIdenticalOneBesideItAlsoSetTheOutputToLinkToTheInConnectionSuppliedInTheFunctionParametersIfItIsNotNull(UIConnectorIn connection)
    {
        Vector3 Offset = new Vector3(2.0f, -2.0f, 0.0f);
        Node node = Instantiate<Node> (this, this.transform.position + Offset, Quaternion.identity);

        node.GetComponentsInChildren<UIConnectorIn> (false, node.InConnections);

        foreach (UIConnectorIn i in node.InConnections) {
            if (i != null) {
                i.startPoint = null;
            }
        }

		if (!Recipe.IsFinalNode)
		{
			node.OutConnection.endPoint = null;
			node.OutConnection.line.enabled = false;
		}

        if (connection != null) {
            node.OutConnection.endPoint = connection;
            connection.startPoint = node.OutConnection;
            node.OutConnection.line.enabled = true;
        }

        node.gameObject.name = Recipe.GetFullName() + "_" + Time.frameCount;
        Node.allNodes.Add(node);
        return node;
    }

	//Split the ticks calls across a bunch of frames for optimisation
	public IEnumerator TickDelayCoroutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		TickNode();
	}

	public void TickNode()
	{
		
		//Check if can run
		if(InConnections.Count == 0)
		{
			//This is a raw resource generator
			Running = true;
			//Debug.Log("tick " + Recipe.GetFullName() + " " + Running);
			return;
		}
		bool allIngredientsRunning = true;
		foreach (var ingredientConnection in InConnections)
		{
			if (ingredientConnection.startPoint)
			{
				if (!ingredientConnection.startPoint.GetNode().Running)
				{
					allIngredientsRunning = false;
					break;
				}
			}
			else
			{
				allIngredientsRunning = false;
				break;
			}
		}
		Running = allIngredientsRunning;
		if (Running && Recipe.IsFinalNode)
		{
			//Gimme dat moneeeeeeeey
			GameManager.instance.CurrentMoney += Recipe.GetMoneyPerTick();
			StartCoroutine(MoneyLerpCoroutine(Recipe.GetMoneyPerTick()));


			if(Recipe.name == "Exit Button")
			{
				GameManager.instance.youWinLabel.SetActive(true);
			}
		}

		//Debug.Log("tick " + Recipe.GetFullName() + " " + Running);


	}

	IEnumerator MoneyLerpCoroutine(int moneyGained)
	{
		float timepassed = 0;
		moneyText.text = "+$"+ moneyGained;
		moneyText.transform.position = moneyPivot.position;
		Color moneyTextColor = moneyText.color;
		moneyTextColor.a = 1;
		moneyText.color = moneyTextColor;
		moneyText.gameObject.SetActive(true);
		while (true)
		{
			timepassed += Time.deltaTime;
			float t = timepassed / (GameManager.instance.tickRate - 0.25f) ;
			Vector3 endPos = moneyPivot.position + Vector3.up * 2;
			moneyText.transform.position = Vector3.Lerp(moneyPivot.position, endPos, t);
			moneyTextColor.a = Mathf.Lerp(1, 0, t);
			moneyText.color = moneyTextColor;
			yield return null;
		}
	}






}

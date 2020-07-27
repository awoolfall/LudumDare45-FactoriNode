using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
#endif

public class RecipeList : MonoBehaviour
{
	public int startTier = 0;
	public int currentTier = 0;
	public int tiersAboveCurrentAllowed;
	public List<RecipeScriptableObject> allRecipes;

	
	private HashSet<RecipeScriptableObject> builtRecipes = new HashSet<RecipeScriptableObject>();
	private HashSet<RecipeScriptableObject> unlockedRecipes = new HashSet<RecipeScriptableObject>();
	//private Dictionary<RecipeScriptableObject, bool> unlockedTracker = new Dictionary<RecipeScriptableObject, bool>();
	//all unlocked recipies of the highest unlocked tier
	private Dictionary<RecipeScriptableObject, GameObject> displayedRecipes = new Dictionary<RecipeScriptableObject, GameObject>();

	public GameObject nodePrefab;
	public GameObject recipeButtonPrefab;

	public RectTransform rootGrid;
	public Transform contentRoot;
	public Transform sellableRoot;
	public Transform nodeSpawnPos;

	[SerializeField] Image PullArrow;
	[SerializeField] Transform PullArrowRoot;
    
	/// <summary>
	/// call this when production finally works on a recipe
	/// </summary>
	public void BuiltRecipe(RecipeScriptableObject recipe)
	{
		if (!builtRecipes.Contains(recipe))
		{
			builtRecipes.Add(recipe);

			UnlockRecipe(recipe);
			foreach (var usedinRecipe in recipe.usedIn)
			{
				UnlockRecipe(usedinRecipe);
			}

			if (recipe.tier > currentTier)
			{
				currentTier = recipe.tier;
				OnTierIncreased();
			}
			UpdateDisplayedRecipes();


		}
	}

	public void OnTierIncreased()
	{
		foreach (var recipe in builtRecipes)
		{
			foreach (var usedinRecipe in recipe.usedIn)
			{
				UnlockRecipe(usedinRecipe);
			}
		}
	}

	public void UnlockRecipe(RecipeScriptableObject recipe)
	{
		//check if tier allows for recipe to be unlocked
		if (recipe.tier <= currentTier + tiersAboveCurrentAllowed)
		{
			if (!unlockedRecipes.Contains(recipe))
			{
				//unlock this recipe
				unlockedRecipes.Add(recipe);
				//unlock all children
				foreach (var childRecipe in recipe.ingredients)
				{
					UnlockRecipe(childRecipe);
				}
				
			}
		}
	}

	public void Start()
	{
		foreach (var recipe in allRecipes)
		{
			if (recipe.tier == startTier)
			{
				BuiltRecipe(recipe);
			}
		}

		UpdateDisplayedRecipes();

	}

	void UpdateDisplayedRecipes()
	{
		foreach (var recipe in unlockedRecipes)
		{
			//if its not already displaying display it
			if (!displayedRecipes.ContainsKey(recipe))
			{
				GameObject go;
				if (recipe.IsFinalNode)
				{
					//Debug.Log("display final " + recipe.name);
					go = GameObject.Instantiate(recipeButtonPrefab, sellableRoot);
				}
				else
				{
					//Debug.Log("display " + recipe.name);
					go = GameObject.Instantiate(recipeButtonPrefab, contentRoot);
				}
				displayedRecipes.Add(recipe, go);
				go.name = recipe.name;
				RecipeButton button = go.GetComponentInChildren<RecipeButton>();
				
				if (recipe.IsFinalNode)
				{
					button.nameLabel.text = recipe.name + " +" + recipe.GetMoneyPerTick() + "/t";
				}
				else
				{
					button.nameLabel.text = recipe.name;
				}
				button.priceLabel.text = "$" + recipe.getPrice();
				button.button.onClick.AddListener(delegate { SelectRecipe(recipe); });
				button.buttonImage.color = button.newRecipeColor;
				
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(rootGrid);
	}

	UIDraggable LastSpawnedObject = null;
	public void SelectRecipe(RecipeScriptableObject recipe)
	{
		if (LastSpawnedObject)
		{
			//the last spawned object didnt get bought
			if(LastSpawnedObject.followUI)
			{
				Destroy(LastSpawnedObject.gameObject);
			}
		}
		displayedRecipes[recipe].GetComponent<RecipeButton>().buttonImage.color = displayedRecipes[recipe].GetComponent<RecipeButton>().defaultColor;
		Vector3 spawnPos = Camera.main.ScreenToWorldPoint(nodeSpawnPos.position);
		spawnPos.z = -.5f;
		GameObject go = Instantiate(nodePrefab, spawnPos, Quaternion.identity);
		go.GetComponent<Node> ().generateNode(recipe);
		go.GetComponent<Node> ().IsPreviewNode = true;
		LastSpawnedObject = go.GetComponent<UIDraggable>();
		LastSpawnedObject.followUI = true;
		LastSpawnedObject.uiAnchor = nodeSpawnPos;
		if (GameManager.GetCurrentMoney() >= recipe.getPrice()) {
			PullArrow.enabled = true;
		} else {
			PullArrow.enabled = false;
		}
	}

	public bool PurchaseNode(Node nodeToPurchase)
	{
		
		if (GameManager.instance.CurrentMoney >= nodeToPurchase.Recipe.getPrice())
		{
			Node.allNodes.Add(nodeToPurchase);
			nodeToPurchase.IsPreviewNode = false;
			Debug.Log("Purchased " + nodeToPurchase.Recipe);
			GameManager.instance.CurrentMoney -= nodeToPurchase.Recipe.getPrice();

			PullArrow.enabled = false;

			//temp 
			//BuiltRecipe(nodeToPurchase.Recipe);
			return true;
		}
		else
		{
			return false;
		}
		
	}

	public void SortByString(string term)
	{
		Debug.Log("Sorting by " + term);
		term = term.ToLower().Replace(" ", string.Empty);
		if(term == "")
		{
			foreach (var recipeButton in displayedRecipes.Values)
			{
				recipeButton.SetActive(true);
			}
			return;
		}
		foreach (var recipeButton in displayedRecipes.Values)
		{
			if (recipeButton.name.ToLower().Replace(" ", string.Empty).Contains(term))
			{
				recipeButton.SetActive(true);
			}
			else
			{
				recipeButton.SetActive(false);
			}
		}
	}

	[ContextMenu("LoadAllRecipes")]
	public void LoadAllRecipes()
	{
#if UNITY_EDITOR
		allRecipes.Clear();
		string path = Application.dataPath + "/Recipes";
		string[] filePaths = Directory.GetFiles(path,searchPattern:"*",  searchOption:SearchOption.AllDirectories);
		
		foreach(string s in filePaths)
		{
			string assetPath = s.Substring(Application.dataPath.Length - 6);
			RecipeScriptableObject recipe = AssetDatabase.LoadAssetAtPath<RecipeScriptableObject>(assetPath);
			if (recipe)
				allRecipes.Add(recipe);
		}

		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#endif
	}

	[ContextMenu("Clean Recipes")]
	public void CleanRecipes()
	{

		foreach (var recipe in allRecipes)
		{
			recipe.ingredients.RemoveAll(item => item == null);
			recipe.usedIn.RemoveAll(item => item == null);
			recipe.OnValidate();
		}
	}

	[ContextMenu("Calculate Cached MPT")]
	public void CalculateAllMPT()
	{
		foreach (var recipe in allRecipes)
		{
			recipe.RecursiveCalculateMoneyPerTick();
		}
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewRecipe", menuName = "CreateRecipe", order = 1)]
public class RecipeScriptableObject : ScriptableObject
{

	public enum ProductionType
	{
		Refinery,
		Miner,
		ProductionPlant,
		AssemblyLine,
		Factory,
		LumberMill,
		ProcessingPlant,
		ConstructionYard,
		Sifter,
		Centrifuge,
		ChemicalMixer,


	}

	public ProductionType type;
	public int tier;

	/// <summary>
	/// This is only relevant for base resources
	/// </summary>
	public int baseCost;

	int IngredientsLength { get { return ingredients.Count; } }
	public List<RecipeScriptableObject> ingredients = new List<RecipeScriptableObject>();
	[SerializeField, HideInInspector]
	private List<RecipeScriptableObject> pastIngredients = new List<RecipeScriptableObject>();


	[Header("-DONT MODIFY-")]
	[ReadOnly]
	public List<RecipeScriptableObject> usedIn;

	public bool IsFinalNode
	{
		get
		{
			return (usedIn.Count == 0);
		}
	}

	public bool IsRawResource
	{
		get
		{
			return (ingredients.Count == 0);
		}
	}

	/// <summary>
	/// returns true if the item was found in its dependancies
	/// </summary>
	public bool SearchFor(RecipeScriptableObject item)
	{
		if(item == this)
		{
			return true;
		}
		foreach(var ingredient in ingredients)
		{
			if (ingredient && ingredient.SearchFor(item))
			{
				return true;
			}
		}
		return false;
	}

	public int getPrice()
	{
		//TODO: make more complex
		return 75 * (int)Mathf.Pow((tier + 1),1.5f);
	}

	public int GetMoneyPerTick()
	{
		if(cachedMoneyPerTick > 0) {
			return cachedMoneyPerTick;
		}
		else
		{
			return RecursiveCalculateMoneyPerTick();
		}
	}

	[SerializeField]
	private int cachedMoneyPerTick = -1;
	public int RecursiveCalculateMoneyPerTick()
	{
		if (IsRawResource)
		{
			return baseCost;
		}
		else
		{
			int oldValue = cachedMoneyPerTick;
			cachedMoneyPerTick = 0;
			foreach (var recipe in ingredients)
			{
				cachedMoneyPerTick += recipe.RecursiveCalculateMoneyPerTick();
			}
			cachedMoneyPerTick += 5 * tier;
#if UNITY_EDITOR
			if(oldValue != cachedMoneyPerTick) 
				EditorUtility.SetDirty(this);
#endif
			return cachedMoneyPerTick;
		}
	}

	public void OnValidate()
	{
		//check whats changed in the ingredients
		List<RecipeScriptableObject> added = new List<RecipeScriptableObject>(ingredients);
		List<RecipeScriptableObject> removed = new List<RecipeScriptableObject>(pastIngredients);

		foreach (var ingredient in pastIngredients)
		{
			added.Remove(ingredient);
		}
		foreach(var ingredient in ingredients)
		{
			removed.Remove(ingredient);
		}

		//for each item just added
		foreach (var item in added)
		{
			if (item)
			{
				//Debug.Log("added " + item.name);
				if (item.SearchFor(this))
				{
					Debug.LogError("Circular Recipe Detected");
					ingredients = pastIngredients;
					return;
				}
				if (item.usedIn != null && !item.usedIn.Contains(this))
				{
					item.usedIn.Add(this);
#if UNITY_EDITOR
					EditorUtility.SetDirty(item);
#endif
				}
			}
		}
		//for each item just removed
		foreach(var item in removed)
		{
			if (item)
			{
				//Debug.Log("removed " + item.name);
				if (!ingredients.Contains(item))
				{
					item.usedIn.Remove(this);
#if UNITY_EDITOR
					EditorUtility.SetDirty(item);
#endif
				}
			}
		}
		pastIngredients = new List<RecipeScriptableObject>(ingredients);
	}

	public string GetFullName()
	{
		return this.name + " " + this.type.ToString();
	}
}

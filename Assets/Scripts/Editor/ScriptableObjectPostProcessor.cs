using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScriptableObjectPostProcessor : UnityEditor.AssetModificationProcessor
{
	void OnWillDeleteAsset(string name, RemoveAssetOptions options)
	{
		Debug.Log(name);
	}
}

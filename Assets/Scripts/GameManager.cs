using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;
	public RecipeList recipeManager;

	public Text moneyLabel;
	public Text deltaMoneyLabel;

	public GameObject youWinLabel;

	public float tickRate;

	Coroutine MoneyLerp = null;
	Coroutine DeltaMoneyNumerator = null;

	int _currentMoney = 5000;
	public int CurrentMoney
	{
		get
		{
			return _currentMoney;
		}
		set
		{
			_currentMoney = value;
			moneyLabel.text = "$" + _currentMoney.ToString("n0");
			//if (MoneyLerp != null) {
			//	StopCoroutine(MoneyLerp);
			//}
			//MoneyLerp = StartCoroutine(LerpToNewMoney(value));
		}
	}

	private void Awake()
	{
		instance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		moneyLabel.text = "$" + _currentMoney.ToString("n0");
		deltaMoneyLabel.text = "+$0";
		StartCoroutine(TickLoop());
		//StartCoroutine(lerpDeltaMoney(0, 0));
	}

	

	/// <summary>
	/// Main Game Loop
	/// </summary>
	/// <returns></returns>
	IEnumerator TickLoop()
	{
		int ticksPassed = 0;
		int moneylastloop = CurrentMoney;
		int oldDelta = 0;
		while (true)
		{
			Debug.Log("Starting Tick: " + ticksPassed);
			
			yield return new WaitForSeconds(tickRate);
			//CurrentMoney += 10;//extra money each tick
			float totalTickTime = tickRate - (tickRate * 0.1f);//leave a little breathing room at the end of a tick
			for (int i = 0; i < Node.allNodes.Count; i++)
			{
				if (Node.allNodes[i])
				{
					Node.allNodes[i].StartCoroutine(Node.allNodes[i].TickDelayCoroutine(totalTickTime * ( i / (float)Node.allNodes.Count) ));
				}
			}
			ticksPassed++;

			int newDelta = Mathf.Max((CurrentMoney - moneylastloop), 0);
			deltaMoneyLabel.text = "+$" + newDelta.ToString("n0");
			//if (DeltaMoneyNumerator != null) StopCoroutine(DeltaMoneyNumerator);
			//DeltaMoneyNumerator = StartCoroutine(lerpDeltaMoney(newDelta, oldDelta));
			moneylastloop = CurrentMoney;
			oldDelta = newDelta;
		}
	}

	IEnumerator lerpDeltaMoney(int toValue, int oldValue)
	{
		while (oldValue != toValue) {
            oldValue = (int)Mathf.Lerp(oldValue, toValue, 15f * Time.deltaTime);
            if (Mathf.Abs(oldValue - toValue) <= 5) {
                oldValue = toValue;
            } 
            deltaMoneyLabel.text = "+$" + Mathf.Max(oldValue, 0) + "/t";
            yield return null;
        }
		deltaMoneyLabel.text = "+$" + Mathf.Max(oldValue, 0) + "/t";
		MoneyLerp = null;
	}

	public static int GetCurrentMoney()
	{
		return GameManager.instance.CurrentMoney;
	}

	IEnumerator LerpToNewMoney(int NewValue)
	{
		while (_currentMoney != NewValue) {
            _currentMoney = (int)Mathf.Lerp(_currentMoney, NewValue, 15f * Time.deltaTime);
            if (Mathf.Abs(_currentMoney - NewValue) <= 5) {
                _currentMoney = NewValue;
            } 
            moneyLabel.text = "$" + _currentMoney;
            yield return null;
        }
		MoneyLerp = null;
	}

}

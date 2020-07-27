using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DupePanel : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Text CostText;

    static DupePanel self;

    float CurrentValue = 0.0f;

    Color DupeColorGood = Color.blue;
    Color DupeColorBad = Color.red;

    void Start()
    {
        DupePanel.self = this;
    }

    void Update()
    {
        if (CurrentValue < 1) {
            DupePanel.self.CostText.color = Color.gray;
        } else {
            DupePanel.self.CostText.color = (((int)CurrentValue <= GameManager.GetCurrentMoney() ? DupeColorGood : DupeColorBad));
        }
    }

    void _setDupeCost(int value) {
        StopAllCoroutines();
        StartCoroutine(LerpToNewValue(value));
    }

    public static void setDupeCost(int value)
    {
        DupePanel.self._setDupeCost(value);
    }

    public void OnDupeButton()
    {
        SelectController.self.duplicateNodeSelection();
    }

    IEnumerator LerpToNewValue(int newValue)
    {
        while (CurrentValue != newValue) {
            CurrentValue = Mathf.Lerp(CurrentValue, (float)newValue, 15f * Time.deltaTime);
            if (Mathf.Abs(CurrentValue - (float)newValue) < 0.25f) {
                CurrentValue = newValue;
            } 
            DupePanel.self.CostText.text = "$" + ((int)CurrentValue).ToString("n0");
            yield return null;
        }
    }
}

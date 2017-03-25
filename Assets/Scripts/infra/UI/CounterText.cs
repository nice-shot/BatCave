using UnityEngine;
using UnityEngine.UI;
using System;

namespace Infra.UI {
[RequireComponent(typeof(Text))]
public abstract class CounterText : MonoBehaviour {

    public string format;

    protected int displayedValue;

    private Text textComponent;

    protected virtual void Awake() {
        textComponent = GetComponent<Text>();
    }

    protected virtual void OnEnable() {
        displayedValue = GetTarget();
        UpdateDisplayedValue();
    }

    public int GetCurrentValue() {
        return displayedValue;
    }

    protected abstract int GetTarget();

    protected void UpdateDisplayedValue() {
        textComponent.text = GetDisplayedValueText();
    }

    private string GetDisplayedValueText() {
        if (String.IsNullOrEmpty(format)) {
            return displayedValue.ToString();
        }
        return displayedValue.ToString(format);
    }

    protected void Update() {
        int target = GetTarget();
        if (target != displayedValue) {
            displayedValue = target;
            UpdateDisplayedValue();
        }
    }

}
}

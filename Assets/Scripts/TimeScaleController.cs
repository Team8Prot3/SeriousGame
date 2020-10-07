using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TimeScaleController : MonoBehaviour
{
    public Slider slider;
    public Text scaleText;

    void Start()
    {
        slider.onValueChanged.AddListener((float value) => OnSliderValueChanged(value));
    }
    private void OnSliderValueChanged(float value)
    {
        Time.timeScale = value;
        scaleText.text = value.ToString();
    }
}

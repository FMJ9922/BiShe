using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundVolumn : MonoBehaviour
{
    [SerializeField] Slider slider;

    private void Awake()
    {
        slider.onValueChanged.AddListener(SetVolumn);
        if (PlayerPrefs.HasKey("SoundVolumn"))
        {
            slider.value = PlayerPrefs.GetFloat("SoundVolumn");
        }
    }

    private void SetVolumn(float volumn)
    {
        PlayerPrefs.SetFloat("SoundVolumn", volumn/5);
        SoundManager.Instance.Volumn = volumn/5;
    }

}

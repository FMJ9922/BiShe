using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicVolumn : MonoBehaviour
{
    [SerializeField]Slider slider;

    private void Awake()
    {
        slider.onValueChanged.AddListener(SetVolumn);
        if (PlayerPrefs.HasKey("MusicVolumn"))
        {
            slider.value = PlayerPrefs.GetFloat("MusicVolumn")*5;
        }
    }

    private void SetVolumn(float volumn)
    {
        PlayerPrefs.SetFloat("MusicVolumn", volumn/5);
        MusicManager.Instance.Volumn = volumn/5;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicVolumn : MonoBehaviour
{
    [SerializeField]Slider slider;

    private void Start()
    {
        slider.onValueChanged.AddListener(SetVolumn);
    }

    private void SetVolumn(float volumn)
    {
        PlayerPrefs.SetFloat("MusicVolumn", volumn/5);
        MusicManager.Instance.Volumn = volumn/5;
    }

}

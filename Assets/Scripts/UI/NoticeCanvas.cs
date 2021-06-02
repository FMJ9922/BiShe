using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoticeCanvas : MonoBehaviour
{
    [SerializeField] Text text;

    public void SetText(string context)
    {
        text.text = context.Replace('|', '\n');
    }
}

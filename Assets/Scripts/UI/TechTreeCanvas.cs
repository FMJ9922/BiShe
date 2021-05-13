using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechTreeCanvas : CanvasBase
{
    [SerializeField] private GameObject mainCanvas;
    public override void InitCanvas()
    {
        mainCanvas.SetActive(false);
    }

    public override void OnOpen()
    {
        mainCanvas.SetActive(true);
    }

    public override void OnClose()
    {
        mainCanvas.SetActive(false);
    }
}

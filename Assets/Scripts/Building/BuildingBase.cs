using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBase : MonoBehaviour
{
    //占地长宽
    [SerializeField]
    private int Length;

    [SerializeField]
    private int Width;

    [SerializeField]
    private BundlePrimaryType BundlePrimaryType;

    [SerializeField]
    private BundleSecondaryType BundleSecondaryType;

    public string BundleName => string.Format("{0}.{1}", BundlePrimaryType.ToString(), BundleSecondaryType.ToString());

    public string PfbName => this.gameObject.name;

    public Vector2Int Size => new Vector2Int(Length, Width);

    public bool buildFlag = false;
     
    public virtual void OnConfirmBuild()
    {
        buildFlag = true;
    }
}

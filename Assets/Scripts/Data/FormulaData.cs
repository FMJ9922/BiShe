using System.Collections.Generic;

[System.Serializable]
public class FormulaData 
{
    public int ID;//配方ID
    public string Describe;//配方描述
    public List<int> InputItemID;//生产消耗物品
    public List<int> InputNum;//消耗量/周
    public List<int> OutputItemID;//生产产出物品
    public List<int> ProductNum;//产品数量
    public int ProductTime;//生产时长
}

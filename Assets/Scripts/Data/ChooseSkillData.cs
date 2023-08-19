using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChooseSkillData
{
    public int Id;//技能唯一指定id
    public string NameIds;//技能名字
    public List<int> BuffList;//技能增益生效buff id
    public List<int> BuffValueList;//技能数值(0-1000)
    public int Stage;//所属阶段
    public int RefSkill;//依赖技能
}

[System.Serializable]
public class SkillBuffData
{
    public int Id;//buffId
    public string NameIds;//buff名字
}

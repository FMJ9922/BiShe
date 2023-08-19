using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class SkillChooseItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _addText;

    private Action<int> _onClick;
    private ChooseSkillData _data;

    public void Init(ChooseSkillData data,Action<int> callback)
    {
        _data = data;
        _titleText.text = Localization.Get(_data.NameIds);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < data.BuffList.Count; i++)
        {
            var buffData = DataManager.GetSkillBuffData(data.BuffList[i]);
            sb.Append(buffData.NameIds);
            sb.Append('\n');
        }
        _descriptionText.text = sb.ToString();
        sb.Clear();
        for (int i = 0; i < data.BuffValueList.Count; i++)
        {
            sb.Append("+"+(data.BuffValueList[i]/10)+"%");
            sb.Append('\n');
        }
        _addText.text = sb.ToString();
        _onClick = callback;
    }

    public void OnClick()
    {
        _onClick.Invoke(_data.Id);
    }
}

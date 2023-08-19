using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChooseSkillCanvas : CanvasBase
    {
        [SerializeField] private GameObject mainCanvas;
        [SerializeField] private Transform _btnParent;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroup;
        
        
        #region 实现基类
        public override void InitCanvas()
        {
            mainCanvas.SetActive(false);
        }

        public override void OnOpen()
        {
            mainCanvas.SetActive(true);
            GameManager.Instance.PauseGame();
            RefreshChoose(0);
        }

        public override void OnClose()
        {
            if (mainCanvas.activeInHierarchy)
            {
                mainCanvas.SetActive(false);
                GameManager.Instance.ContinueGame();
            }
        }

        private List<ChooseSkillData> GetCurChooseSkillDatas()
        {
            return new List<ChooseSkillData>(DataManager.Instance.ChooseSkillArray);
        }

        private List<ChooseSkillData> _curChooseList;
        private void RefreshChoose(int index)
        {
            _curChooseList= GetCurChooseSkillDatas();
            InitGridItem(OnItemInit, _curChooseList.Count, _btnParent);
            _horizontalLayoutGroup.CalculateLayoutInputHorizontal();
        }

        private void OnItemInit(int index,GameObject obj)
        {
            var skillChooseItem = obj.GetComponent<SkillChooseItem>();
            skillChooseItem.Init(_curChooseList[index],OnClick);
        }

        public void OnClick(int skillId)
        {
            Debug.Log("选择了"+skillId);
            OnClose();
        }

        #endregion
        
        
    }
}

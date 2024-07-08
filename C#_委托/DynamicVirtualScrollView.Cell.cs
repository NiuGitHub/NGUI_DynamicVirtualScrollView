using System;
using UnityEngine;
public partial class DynamicVirtualScrollView : UIScrollView
{
    public class DynamicVirtualScrollViewCell : MonoBase
    {
        private const string NAME_APPEND = "_";
        private int m_CellSeqIndex = 0;
        public virtual int cellWidth { get; set; }
        public virtual int cellHeight { get; set; }
        private object m_Data;
        public object data => m_Data;
        public DynamicVirtualScrollViewCell(GameObject go) : base(go)
        {

        }
        public void SetCellSeqIndex(int idx)
        {
            m_CellSeqIndex = idx;
            CSStringBuilder.Clear();
            gameObject.name = CSStringBuilder.Append(GetType().Name, NAME_APPEND, idx).ToString();
        }
        private Action<int, object> m_ClickCallBackFunc = null;
        private Action<DynamicVirtualScrollViewCell> m_IninCallBackFunc = null;
        public void SetCallBackFunc(Action<int, object> clickCb, Action<DynamicVirtualScrollViewCell> initCb)
        {
            m_ClickCallBackFunc = clickCb;
            m_IninCallBackFunc = initCb;
        }
        protected bool bSelect = false;

        public void OnSelectChange(int selectIdx)
        {
            bSelect = m_CellSeqIndex == selectIdx;
        }
        public void SetData(object dt)
        {
            m_Data = dt;
            m_IninCallBackFunc?.Invoke(this);
        }
        protected void DispatchClickCallBack()
        {
            if (!bSelect)
            {
                m_ClickCallBackFunc?.Invoke(m_CellSeqIndex, m_Data);
            }
        }
        protected virtual void OnClick(GameObject go)
        {
            DispatchClickCallBack();
        }
        public override void Destroy()
        {
            m_ClickCallBackFunc = null;
            m_IninCallBackFunc = null;
            base.Destroy();
        }
    }
}




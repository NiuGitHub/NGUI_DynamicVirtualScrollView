using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
public partial class DynamicVirtualScrollView : UIScrollView
{
    public class DynamicVirtualScrollViewCell : MonoBase
    {
        private const string NAME_APPEND = "_";
        private int m_CellSeqIndex = 0;
        public virtual int cellWidth { get; set; }
        public virtual int cellHeight { get; set; }
        public DynamicVirtualScrollViewCell() : base()
        {
        }
        public DynamicVirtualScrollViewCell(GameObject go) : base(go)
        {

        }
        public void SetCellSeqIndex(int idx)
        {
            m_CellSeqIndex = idx;
            CSStringBuilder.Clear();
            gameObject.name = CSStringBuilder.Append(GetType().Name, NAME_APPEND, idx).ToString();
        }
        private Action<int> m_ClickCallBackFunc = null;

        public void SetClickCallBackFunc(Action<int> cb)
        {
            m_ClickCallBackFunc = cb;
        }
        protected bool bSelect = false;

        public void OnSelectChange(int selectIdx)
        {
            bSelect = m_CellSeqIndex == selectIdx;
        }
        public void SetData(object dt)
        {

        }
        protected void DispatchClickCallBack()
        {
            if (!bSelect)
            {
                m_ClickCallBackFunc?.Invoke(m_CellSeqIndex);
            }
        }
        protected virtual void OnClick(GameObject go)
        {
            DispatchClickCallBack();
        }
        public override void Destroy()
        {
            m_ClickCallBackFunc = null;
            base.Destroy();
        }
    }
}




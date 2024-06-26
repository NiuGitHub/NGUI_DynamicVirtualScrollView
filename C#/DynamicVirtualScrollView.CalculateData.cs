using NPOI.OpenXmlFormats.Vml;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

using UnityEngine.UI;
public partial class DynamicVirtualScrollView : UIScrollView
{
    private class UIRectangle
    {
        float x;
        float y;
        float width;
        float height;
        public UIRectangle(float x, float y, float width, float height)
        {
            SetRect(x, y, width, height);
        }
        public UIRectangle()
        {
            SetRect(0, 0, 0, 0);
        }
        public void SetRect(float x, float y, float width, float height)
        {
            this.x = x - width / 2; this.y = y - height / 2; this.width = width; this.height = height;
        }
        public bool Intersects(UIRectangle rect)
        {
            return !(rect.x > x + width || rect.x + rect.width < x || rect.y > y + height || rect.y + rect.height < y);
        }
        public override string ToString()
        {
            return $"{x},{y},{width},{height}";
        }
    }

    private class ShowCellData : IFastArray
    {
        int m_Index;
        float m_PosX;
        float m_PosY;
        int m_Width;
        int m_Height;
        int m_RealWidth;
        int m_RealHeight;
        bool m_CalculatedreRealSize = false;
        object m_Data;
        UIRectangle m_Rect = new UIRectangle();
        public int index { get { return m_Index; } }
        public float posX { get { return m_PosX; } }
        public float posY { get { return m_PosY; } }
        public int width { get { return m_RealWidth; } }
        public int height { get { return m_RealHeight; } }
        public UIRectangle rect { get { return m_Rect; } }
        public bool calculatedreRealSize { get { return m_CalculatedreRealSize; } }

        DynamicVirtualScrollViewCell m_Cell;
        DynamicVirtualScrollView m_Owner;
        public ShowCellData(DynamicVirtualScrollView owner)
        {
            m_Owner = owner;
        }
        public bool CheckInBound(UIRectangle viewRect)
        {
            return m_Rect.Intersects(viewRect);
        }
        public void Init(int idx, float posX, float posY, int width, int height, object data)
        {
            m_Index = idx;
            m_CalculatedreRealSize = false;
            m_Width = width;
            m_Height = height;
            m_RealWidth = width;
            m_RealHeight = height;
            SetPos(posX, posY);
            m_Data = data;
            m_Cell?.SetData(data);
            m_Cell?.OnSelectChange(m_Owner.curSelectedIdx);
        }
        public void SetPos(float posX, float posY)
        {
            m_PosX = posX;
            m_PosY = posY;
            if (m_Cell != null && m_Cell.transform != null)
            {
                m_Cell.transform.localPosition = new Vector3(m_PosX, m_PosY, 0);
            }
            m_Rect.SetRect(posX, posY, width, height);
        }
        public void ReturnCellToPool()
        {
            if (m_Cell != null)
            {
                m_Owner.ReturnCell(m_Cell);
                m_Cell = null;
            }
        }
        public void GetCellFromPool()
        {
            if (m_Cell == null)
            {
                m_Cell = m_Owner.CreateCell();
                SetCellData();
            }
        }
        private void SetCellData()
        {
            if (m_Cell == null)
            {
                return;
            }
            m_Cell.cellHeight = m_Width;
            m_Cell.cellWidth = m_Height;
            m_Cell.SetCellSeqIndex(m_Index);
            m_Cell.SetData(m_Data);
            if (!m_CalculatedreRealSize)
            {
                m_CalculatedreRealSize = true;
                m_RealWidth = m_Cell.cellWidth;
                m_RealHeight = m_Cell.cellHeight;
            }
            m_Cell.OnSelectChange(m_Owner.curSelectedIdx);
            SetPos(m_PosX, m_PosY);
        }
        public void Destroy()
        {
            ReturnCellToPool();
            m_Data = null;
            m_Owner = null;
        }
    }
}

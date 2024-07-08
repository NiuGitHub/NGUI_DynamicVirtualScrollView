using System;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public partial class DynamicVirtualScrollView : UIScrollView
{
    #region ???????????????
    public enum eLayoutType
    {
        Horizontal = 0,
        Vertical = 1,
    }
    #region ??????
    /// <summary>
    /// ???????? 
    /// </summary>
    [SerializeField]
    private eLayoutType m_LayoutType = eLayoutType.Horizontal;
    [SerializeField]
    private Vector2 m_Spacing = Vector2.zero;
    #endregion

    [SerializeField]
    private GameObject m_Content;
    /// <summary>
    /// ?????????
    /// </summary>
    [SerializeField]
    private GameObject m_ItemPrefab;
    [SerializeField]
    private UIWidget.Pivot m_ItemPivot = UIWidget.Pivot.Center;
    [SerializeField]
    private UIWidget.Pivot m_ContentPivot = UIWidget.Pivot.TopLeft;
    [SerializeField]
    private Vector2Int m_CellSize = Vector2Int.zero;

    /// <summary>
    /// ?????????????
    /// </summary>
    [SerializeField]
    public int perLineItemNum = 99999;
    #endregion

    #region ??§ÕUIScrollView
    public override Bounds bounds => mBounds;
    public override void SetDragAmount(float x, float y, bool updateScrollbars)
    {
        base.SetDragAmount(x, y, updateScrollbars);
        UpdateShowView();
    }
    #endregion
    public int curSelectedIdx { get; private set; }
    private bool m_bIsDynamicCellSize = false;
    private bool m_bIsDynamicCheck = true;
    private readonly Type[] VIEW_CTOR_TYPES = new Type[] { typeof(GameObject) };
    private List<ShowCellData> m_ShowDragData = new List<ShowCellData>();
    private List<DynamicVirtualScrollViewCell> m_HideList = new List<DynamicVirtualScrollViewCell>();
    private List<DynamicVirtualScrollViewCell> m_ShowList = new List<DynamicVirtualScrollViewCell>();
    private List<object> m_data;
    private Action<int, object> m_OnClick;
    private Action<DynamicVirtualScrollViewCell> m_OnInit;
    public void InitViewCell()
    {
        onMomentumMove -= UpdateShowView;
        onMomentumMove += UpdateShowView;
        Vector4 rect = panel.baseClipRegion;
        mBounds = new Bounds(new Vector2(rect.x, rect.y), new Vector2(rect.z, rect.w));
        curSelectedIdx = 0;
        m_ItemPrefab?.CustomActive(false);
    }
    public void SetCallBackFunc(Action<int, object> clickCb, Action<DynamicVirtualScrollViewCell> initCb)
    {
        m_OnClick = clickCb;
        m_OnInit = initCb;
    }
    public void SetSelectIndex(int idx, bool isMoveToIdx = true)
    {
        OnCellClick(idx, m_data[idx]);
        if (isMoveToIdx)
        {
            MoveToIndex(idx);
        }
    }
    public void MoveToIndex(int idx)
    {
        if (m_ShowDragData[idx] != null)
        {
            SetDragAmount(m_ShowDragData[idx].posX, m_ShowDragData[idx].posY, false);
        }
    }
    public void SetData(List<object> data)
    {
        if (m_bIsDynamicCellSize)
        {
            ClearShow();
        }
        m_bIsDynamicCheck = true;
        m_data = data;
        var len = data.Count;
        var cellWidth = m_CellSize.x;
        var cellHeight = m_CellSize.y;
        var spacingX = m_Spacing.x;
        var spacingY = m_Spacing.y;
        if (m_LayoutType == eLayoutType.Horizontal)
        {
            var mw = len % perLineItemNum; ;
            var maxWidth = Math.Max(0, mw * cellWidth + (mw - 1) * spacingX);
            UpdateBoundsSize(maxWidth);
        }
        else
        {
            var mh = Mathf.CeilToInt(len * 1f / perLineItemNum);
            var maxHeight = Math.Max(0, mh * cellHeight + (mh - 1) * spacingY);
            UpdateBoundsSize(maxHeight);
        }
        UnityEngine.Debug.Log($" mBounds {mBounds.ToString()} {mBounds.size}");
        ShowCellData item;
        var num = m_ShowDragData.Count - len;
        while (num > 0)
        {
            item = m_ShowDragData[0];
            item.Destroy();
            num--;
            m_ShowDragData.RemoveAt(0);
        }

        while (num < 0)
        {
            item = new ShowCellData(this);
            num++;
            m_ShowDragData.Add(item);
        }
        CalculateCellPosition();
        UpdateShowView();
    }
    private Vector2 GetPosByIdx(int idx)
    {
        int x = idx % perLineItemNum;
        int y = Mathf.FloorToInt(idx * 1f / perLineItemNum);
        var cellWidth = m_CellSize.x + m_Spacing.x;
        var cellHeight = m_CellSize.y + m_Spacing.y;
        Vector2 pos;
        var startX = -panel.width / 2f + m_CellSize.x / 2f;
        var startY = panel.height / 2f - m_CellSize.y / 2f;
        pos = new Vector2(startX + cellWidth * x, startY - cellHeight * y);
        if (m_ContentPivot != UIWidget.Pivot.TopLeft)
        {
            var maxIdx = m_ShowDragData.Count - 1;
            var maxX = 0;
            var maxY = 0;
            Vector2 po = NGUIMath.GetPivotOffset(m_ContentPivot);
            float fx, fy;
            if (m_LayoutType == eLayoutType.Horizontal)
            {
                maxX = maxIdx % perLineItemNum;
                fx = Mathf.Lerp(0f, maxX * cellWidth, po.x);
                fy = Mathf.Lerp(-maxY * cellHeight, 0f, po.y);
            }
            else
            {
                maxY = Mathf.CeilToInt(maxIdx * 1f / perLineItemNum);
                fx = Mathf.Lerp(0f, maxY * cellWidth, po.x);
                fy = Mathf.Lerp(-maxX * cellHeight, 0f, po.y);
            }
            pos.x = pos.x - fx;
            pos.y = pos.y - fy;
        }
        return pos;
    }
    private void CalculateCellPosition()
    {
        var cellWidth = m_CellSize.x + m_Spacing.x;
        var cellHeight = m_CellSize.y + m_Spacing.y;
        for (int i = 0, imax = m_ShowDragData.Count; i < imax; i++)
        {
            Vector2 pos = GetPosByIdx(i);
            m_ShowDragData[i].Init(i, pos.x, pos.y, m_CellSize.x, m_CellSize.y, m_data[i]);
            //UnityEngine.Debug.Log($"idx:{i},posX:{m_ShowDragData[i].posX},posY:{m_ShowDragData[i].posY},GetPosByIdx{pos.ToString()}");
        }
    }
    private void UpdateBoundsSize(float max)
    {
        Vector2 size;
        Vector2 center;
        if (m_LayoutType == eLayoutType.Horizontal)
        {
            size = new Vector2(Math.Max(panel.width, max), panel.height);
            //center = new Vector2((max - panel.width), 0);
            center = (size - panel.GetViewSize()) * 0.5f;
        }
        else
        {
            size = new Vector2(panel.width, Math.Max(panel.height, max));
            //center = new Vector2(0, -(max - panel.width));
            center = (size - panel.GetViewSize()) * -0.5f;
        }
        mBounds.size = size;
        mBounds.center = center;
    }
    public int TestNum = 10;
    [ContextMenu("Test")]
    public void Test()
    {
        if (NGUITools.GetActive(this))
        {
            m_Content.transform.DestroyChildren();
            m_ShowDragData.Clear();
            //InitViewCell<DynamicVirtualScrollViewCell>();
            List<object> list = new List<object>();
            for (int i = 0; i < TestNum; i++)
            {
                list.Add(i);
            }
            SetData(list);
            ResetPosition();
        }
    }

    private Vector2 GetPivot(UIWidget.Pivot pivotTypeEnum)
    {
        Vector2 v2 = Vector2.zero;
        switch (pivotTypeEnum)
        {
            case UIWidget.Pivot.TopLeft:
                v2 = (Vector2.one + Vector2.up + Vector2.left) / 2;
                break;
            case UIWidget.Pivot.Top:
                v2 = (Vector2.one + Vector2.up) / 2;
                break;
            case UIWidget.Pivot.TopRight:
                v2 = (Vector2.one + Vector2.up + Vector2.right) / 2;
                break;
            case UIWidget.Pivot.Left:
                v2 = (Vector2.one + Vector2.left) / 2;
                break;
            case UIWidget.Pivot.Center:
                v2 = Vector2.one / 2;
                break;
            case UIWidget.Pivot.Right:
                v2 = (Vector2.one + Vector2.right) / 2;
                break;
            case UIWidget.Pivot.BottomLeft:
                v2 = (Vector2.one + Vector2.down + Vector2.left) / 2;
                break;
            case UIWidget.Pivot.Bottom:
                v2 = (Vector2.one + Vector2.down) / 2;
                break;
            case UIWidget.Pivot.BottomRight:
                v2 = (Vector2.one + Vector2.down + Vector2.right) / 2;
                break;
            default:
                v2 = Vector2.one / 2;
                break;
        }
        return v2;
    }
    private Vector2 GetPosByPivot(UIWidget.Pivot pivotTypeEnum, int w = 200, int h = 100)
    {
        Vector2 v2 = new Vector2(w, h);
        Vector2 pivot = GetPivot(pivotTypeEnum);
        Vector2 pos = v2 * pivot;
        int opposite = 1;
        float x = opposite * (pos.x - w / 2);
        float y = opposite * (pos.y - h / 2);
        return new Vector2(x, y);
    }
    /// <summary>
    /// ?????????????size ?????????/???§Ø????§³
    /// </summary>
    /// <param name="v"></param>
    public void SetIsDynamicCellSize(bool v)
    {
        if (this.m_LayoutType == eLayoutType.Horizontal)
        {
            m_bIsDynamicCellSize = v;
        }
        else
        {
            m_bIsDynamicCellSize = v && perLineItemNum == 1;
        }
    }
    private void UpdateShowView()
    {
        Vector4 v4 = panel.finalClipRegion;
        UIRectangle showRect = new UIRectangle(v4.x, v4.y, v4.z, v4.w);

        UnityEngine.Debug.Log($"showRect:{showRect.ToString()}");
        foreach (var item in m_ShowDragData)
        {
            var isIn1 = item.CheckInBound(showRect);
            //UnityEngine.Debug.Log($"idx:{item.index},isIn1:{isIn1},rect:{item.rect.ToString()}");
            if (isIn1)
            {
                item.GetCellFromPool();
            }
            else
            {
                item.ReturnCellToPool();
            }
        }

        //??????????????? ????item??????¦Ë??
        if (m_bIsDynamicCellSize && m_bIsDynamicCheck)
        {
            CalculateDynamicSize();
        }
    }
    public override void MoveRelative(Vector3 relative)
    {
        base.MoveRelative(relative);
        UpdateShowView();
    }
    private void CalculateDynamicSize()
    {
        var cellWidth = (int)m_CellSize.x;
        var cellHeight = (int)m_CellSize.y;
        var spacingX = m_Spacing.x;
        var spacingY = m_Spacing.y;

        float offsetWidthOrHeight = 0;
        var hasNull = false;
        for (int i = 0; i < m_ShowDragData.Count; i++)
        {
            var showData = m_ShowDragData[i];
            Vector2 sizeDelta = Vector2.zero;
            if (showData.calculatedreRealSize)
            {
                sizeDelta = new Vector2(cellWidth, cellWidth);
            }
            else
            {
                hasNull = true;
            }
            var pos = GetPosByIdx(i);
            var px = pos.x;
            var py = pos.y;
            if (i > 0)
            {
                var lastShowItem = m_ShowDragData[i - 1];
                if (m_LayoutType == eLayoutType.Horizontal)
                {
                    px = lastShowItem.posX + lastShowItem.width + spacingX + sizeDelta.x;
                }
                else
                {
                    py = lastShowItem.posY - lastShowItem.height - spacingY - sizeDelta.y;
                }
            }
            if (m_LayoutType == eLayoutType.Horizontal)
            {
                var offset = sizeDelta.x - cellWidth;
                offsetWidthOrHeight = offsetWidthOrHeight + offset;
            }
            else
            {
                var offset = sizeDelta.y - cellHeight;
                offsetWidthOrHeight = offsetWidthOrHeight + offset;
            }
            showData.SetPos(px, py);


        }

        m_bIsDynamicCheck = hasNull;
        if (m_LayoutType == eLayoutType.Horizontal)
        {
            var mw = Math.Min(perLineItemNum, m_ShowDragData.Count);
            var maxWidth = mw * cellWidth + (mw - 1) * spacingX + offsetWidthOrHeight;
            UpdateBoundsSize(maxWidth);
        }
        else
        {
            var mh = Convert.ToInt32(Math.Ceiling(m_ShowDragData.Count * 1f / perLineItemNum));
            var maxHeight = mh * cellHeight + (mh - 1) * spacingY + offsetWidthOrHeight;
            UpdateBoundsSize(maxHeight);
        }

    }
    private GameObject ClonePrefabAndInit()
    {
        GameObject go = NGUITools.AddChild(m_Content, m_ItemPrefab);
        var box = GetOrAddComponent<BoxCollider>(go);
        box.size = new Vector2(m_CellSize.x, m_CellSize.y);
        box.center = m_CellSize * (NGUIMath.GetPivotOffset(UIWidget.Pivot.Center) - NGUIMath.GetPivotOffset(m_ItemPivot));
        var drag = GetOrAddComponent<UIDragScrollView>(go);
        drag.scrollView = this;
        go.CustomActive(false);
        return go;
    }
    private T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        var rs = go.GetComponent<T>();
        if (rs == null)
        {
            rs = go.AddComponent<T>();
        }
        return rs;
    }
    private DynamicVirtualScrollViewCell CreateCell()
    {
        DynamicVirtualScrollViewCell item = null;
        if (m_HideList.Count < 1)
        {
            item = new DynamicVirtualScrollViewCell(ClonePrefabAndInit());
            item.SetCallBackFunc(m_OnClick, m_OnInit);
        }
        else
        {
            item = m_HideList[0];
            m_HideList.RemoveAt(0);
        }
        item.SetActive(true);
        m_ShowList.Add(item);
        return item;
    }
    private void ReturnCell(DynamicVirtualScrollViewCell item)
    {
        item.SetActive(false);
        m_ShowList.Remove(item);
        m_HideList.Add(item);
    }
    private void OnCellClick(int idx, object data)
    {
        curSelectedIdx = idx;
        for (int i = 0; i < m_ShowList.Count; i++)
        {
            m_ShowList[i].OnSelectChange(curSelectedIdx);
        }
        m_OnClick?.Invoke(idx, data);
    }

    public void ClearShow()
    {
        ResetPosition();
        curSelectedIdx = 0;
        foreach (var item in m_ShowList)
        {
            item.SetActive(false);
            m_HideList.Add(item);
        }
        m_ShowList.Clear();
        m_ShowDragData.Clear();
        m_data = null;
    }
    public void Destroy()
    {
        m_data = null;
        m_OnClick = null;
        m_OnInit = null;
        foreach (var item in m_ShowDragData)
        {
            item.Destroy();
        }
        foreach (var item in m_ShowList)
        {
            item.Destroy();
        }

        foreach (var item in m_HideList)
        {
            item.Destroy();
        }
        m_ShowDragData.Clear();
        m_ShowList.Clear();
        m_HideList.Clear();
    }

}

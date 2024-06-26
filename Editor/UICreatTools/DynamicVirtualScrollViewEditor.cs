using System;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(DynamicVirtualScrollView))]
public class DynamicVirtualScrollViewEditor : UIScrollViewEditor
{
    SerializedProperty m_Content;
    SerializedProperty m_LayoutType;
    SerializedProperty m_ItemPrefab;
    SerializedProperty m_ItemPivot;
    SerializedProperty m_ContentPivot;
    SerializedProperty perLineItemNum;
    SerializedProperty m_Spacing;
    SerializedProperty m_CellSize;
    void Init()
    {
        m_Content = serializedObject.FindProperty("m_Content");
        if (m_Content.objectReferenceValue == null)
        {
            m_Content.objectReferenceValue = CreateUIObject("Content", (target as DynamicVirtualScrollView).gameObject);
            serializedObject.ApplyModifiedProperties();
        }
        m_LayoutType = serializedObject.FindProperty("m_LayoutType");
        m_ItemPrefab = serializedObject.FindProperty("m_ItemPrefab");
        perLineItemNum = serializedObject.FindProperty("perLineItemNum");
        m_Spacing = serializedObject.FindProperty("m_Spacing");
        m_CellSize = serializedObject.FindProperty("m_CellSize");
        m_ItemPivot = serializedObject.FindProperty("m_ItemPivot");
        m_ContentPivot = serializedObject.FindProperty("m_ContentPivot");
    }

    GUIStyle m_caption;
    GUIStyle caption
    {
        get
        {
            if (m_caption == null)
            {
                m_caption = new GUIStyle { richText = true, alignment = TextAnchor.MiddleCenter };
                m_caption.normal.textColor = Color.green;
            }
            return m_caption;
        }
    }

    GUIStyle m_TipsGUIStyle;

    GUIStyle tipsGUIStyle
    {
        get
        {
            if (m_TipsGUIStyle == null)
            {
                m_TipsGUIStyle = new GUIStyle { richText = false, alignment = TextAnchor.MiddleLeft };
                m_TipsGUIStyle.normal.textColor = Color.cyan;
            }
            return m_TipsGUIStyle;
        }
    }

    GUIStyle m_warningGUIStyle;

    GUIStyle warningGUIStyle
    {
        get
        {
            if (m_warningGUIStyle == null)
            {
                m_warningGUIStyle = new GUIStyle { richText = true, alignment = TextAnchor.MiddleCenter };
                m_warningGUIStyle.normal.textColor = Color.red;
            }
            return m_warningGUIStyle;
        }
    }
    public override void OnInspectorGUI()
    {
        Init();
        serializedObject.Update();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("<b>内容左上对齐 子项锚点为中心点\n为了方便计算</b>", warningGUIStyle);
        EditorGUILayout.LabelField("<b>Additional configs</b>", caption);
        EditorGUILayout.Space(5);
        DrawConfigInfo();
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("<b>For original ScrollRect</b>", caption);
        EditorGUILayout.Space(5);
        //EditorGUILayout.LabelField("Content Origin为Item的锚点", tipsGUIStyle);
        base.OnInspectorGUI();

        EditorGUILayout.EndVertical();

    }


    protected virtual void DrawConfigInfo()
    {
        EditorGUILayout.LabelField("子项内容父节点", tipsGUIStyle);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(m_Content);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField("滑动类型(Movement)=>通过这里控制", tipsGUIStyle);
        EditorGUILayout.PropertyField(m_LayoutType);

    
        EditorGUILayout.LabelField("内容布局(Content Origin)=>通过这里控制", tipsGUIStyle);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(m_ContentPivot);
        EditorGUI.EndDisabledGroup();

        //layoutType.intValue = (int)(VirtualScrollView.eLayoutType)EditorGUILayout.EnumPopup("layoutType", (VirtualScrollView.eLayoutType)layoutType.intValue); 
        EditorGUILayout.LabelField("Item预制体锚点 默认居中", tipsGUIStyle);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(m_ItemPivot);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField("Item预制体", tipsGUIStyle);
        EditorGUILayout.PropertyField(m_ItemPrefab);
        EditorGUILayout.LabelField("Item预制体宽高", tipsGUIStyle);
        EditorGUILayout.PropertyField(m_CellSize);
        EditorGUILayout.LabelField("Item每行最大显示数量", tipsGUIStyle);
        EditorGUILayout.PropertyField(perLineItemNum);
        EditorGUILayout.LabelField("X列间隔 Y行间隔", tipsGUIStyle);
        EditorGUILayout.PropertyField(m_Spacing);

        SerializedProperty contentPivot = serializedObject.FindProperty("contentPivot");
        contentPivot.intValue = m_ContentPivot.intValue;
        SerializedProperty movement = serializedObject.FindProperty("movement");
        movement.intValue = m_LayoutType.intValue == (int)DynamicVirtualScrollView.eLayoutType.Vertical ? (int)UIScrollView.Movement.Vertical : (int)UIScrollView.Movement.Horizontal;
    }





    static Vector2 TopLeft = new Vector2(0, 1);

    const string bgPath = "UI/Skin/Background.psd";
    const string spritePath = "UI/Skin/UISprite.psd";
    const string maskPath = "UI/Skin/UIMask.psd";
    static Color panelColor = new Color(1f, 1f, 1f, 0.392f);
    static Color defaultSelectableColor = new Color(1f, 1f, 1f, 1f);
    static Vector2 thinElementSize = new Vector2(160f, 20f);



    [MenuItem("GameObject/NGUI/DynamicVirtualScrollView", false, 90)]
    static public void AddScrollView(MenuCommand menuCommand)
    {
        InternalAddScrollView<DynamicVirtualScrollView>(menuCommand);
    }
    protected static void InternalAddScrollView<T>(MenuCommand menuCommand) where T : DynamicVirtualScrollView
    {
        GameObject root = CreateUIElementRoot(typeof(T).Name, typeof(DynamicVirtualScrollView));
        root.layer = LayerMask.NameToLayer("UI");
        var rs = root.GetComponent<T>();
        rs.panel.clipping = UIDrawCall.Clipping.SoftClip;
        rs.panel.clipSoftness = Vector2.zero;
        GameObject parent = menuCommand.context as GameObject;
        if (parent != null)
        {
            root.transform.SetParent(parent.transform, false);
        }
        Selection.activeGameObject = root;


    }



    static GameObject CreateScrollbar()
    {
        // Create GOs Hierarchy
        GameObject scrollbarRoot = CreateUIElementRoot("Scrollbar", thinElementSize);
        GameObject sliderArea = CreateUIObject("Sliding Area", scrollbarRoot);
        GameObject handle = CreateUIObject("Handle", sliderArea);

        Image bgImage = scrollbarRoot.AddComponent<Image>();
        bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(bgPath);
        bgImage.type = Image.Type.Sliced;
        bgImage.color = defaultSelectableColor;

        Image handleImage = handle.AddComponent<Image>();
        handleImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(spritePath);
        handleImage.type = Image.Type.Sliced;
        handleImage.color = defaultSelectableColor;

        RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
        sliderAreaRect.sizeDelta = new Vector2(-20, -20);
        sliderAreaRect.anchorMin = Vector2.zero;
        sliderAreaRect.anchorMax = Vector2.one;

        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);

        Scrollbar scrollbar = scrollbarRoot.AddComponent<Scrollbar>();
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImage;
        SetDefaultColorTransitionValues(scrollbar);

        return scrollbarRoot;
    }

    static GameObject CreateUIElementRoot(string name, params Type[] components)
    {
        GameObject child = new GameObject(name, components);
        return child;
    }

    static GameObject CreateUIElementRoot(string name, Vector2 size, params Type[] components)
    {
        GameObject child = new GameObject(name, components);
        return child;
    }

    static GameObject CreateUIObject(string name, GameObject parent, params Type[] components)
    {
        GameObject go = new GameObject(name, components);
        SetParentAndAlign(go, parent);
        return go;
    }

    private static void SetParentAndAlign(GameObject child, GameObject parent)
    {
        if (parent == null)
            return;

        child.transform.SetParent(parent.transform, false);
        SetLayerRecursively(child, parent.layer);
    }

    static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
    }

    static void SetDefaultColorTransitionValues(Selectable slider)
    {
        ColorBlock colors = slider.colors;
        colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
        colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
        colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
    }
}

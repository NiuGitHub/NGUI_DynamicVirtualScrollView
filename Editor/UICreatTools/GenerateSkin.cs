using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenerateSkin
{
    private static T Generate<T>(GameObject parent) where T : Component
    {
        GameObject skinGo = new GameObject();
        skinGo.layer = (int)Layer.UI;
        T result = skinGo.AddComponent<T>();
        skinGo.name = result.GetType().Name;
        SetParent(skinGo);
        SetUIWidget(result as UIWidget);
        return result;
    }
    private static T Generate<T>() where T : Component
    {
        GameObject skinGo = new GameObject();
        skinGo.layer = (int)Layer.UI;
        T result = skinGo.AddComponent<T>();
        skinGo.name = result.GetType().Name;
        SetParent(skinGo);
        SetUIWidget(result as UIWidget);
        skinGo.transform.localPosition = Vector3.zero;
        skinGo.transform.localEulerAngles = Vector3.zero;
        skinGo.transform.localScale = Vector3.one;
        return result;
    }
    private static void SetParent(GameObject skinGo, GameObject parent)
    {
        skinGo.transform.parent = parent?.transform;
    }
    private static void SetParent(GameObject skinGo, bool IsPanel = false)
    {
        GameObject uiRoot = null;
        if (Selection.gameObjects.Length > 0)
        {
            uiRoot = Selection.gameObjects[0];
        }
        SetParent(skinGo, uiRoot);
    }
    private static void SetUIWidget<T>(T widget) where T : UIWidget
    {
        if (widget == null)
        {
            return;
        }
        widget.depth = widget.parent ? widget.parent.transform.childCount - 1 : 0;
    }
    private static void SetUIWidget(UIWidget widget, UIWidget parent)
    {
        if (widget == null)
        {
            return;
        }
        widget.depth = parent.depth + parent.transform.childCount;
    }
    [MenuItem("GameObject/NGUI/PanelSkin", false, 1)]
    private static void GeneratePanelSkin()
    {
        GameObject skinGo = Generate<UIPanel>().gameObject;
        skinGo.AddComponent<ScriptBinder>();
        SetParent(skinGo, GameObject.Find("UI Root"));
    }
    [MenuItem("GameObject/NGUI/ComponentSkin", false, 0)]
    private static void GenerateComponentSkin()
    {
        Generate<ScriptBinder>();
    }
    [MenuItem("GameObject/NGUI/Label", false, -1)]
    static UILabel GenerateUILabel()
    {
        var rs = Generate<UILabel>();
        rs.ambigiousFont = NGUISettings.ambigiousFont;
        rs.text = rs.name;
        return rs;
    }
    [MenuItem("GameObject/NGUI/Sprite", false, -2)]
    static void GenerateUISprite()
    {
        var rs = Generate<UISprite>();
        rs.atlas = NGUISettings.atlas;
        rs.spriteName = NGUISettings.selectedSprite;
    }
    [MenuItem("GameObject/NGUI/Only Sprite Button", false, -3)]
    static void GenerateUISpriteButton()
    {
        var rs = Generate<UISprite>();
        rs.gameObject.AddComponent<BoxCollider>();
        rs.name = "SpriteButton";
        rs.atlas = NGUISettings.atlas;
        rs.spriteName = NGUISettings.selectedSprite;
        rs.autoResizeBoxCollider = true;
        rs.MakePixelPerfect();
        var btnScale = rs.gameObject.AddComponent<UIButtonScale>();
        btnScale.tweenTarget = rs.transform;
        var playSound = rs.gameObject.AddComponent<UIPlaySound>();
        playSound.trigger = UIPlaySound.Trigger.OnClick;
        playSound.b_type = UIPlaySound.ButtonType.V_BtnYes;
    }
    [MenuItem("GameObject/NGUI/Sprite Button", false, -4)]
    static void GenerateUISpriteButtonAndLabel()
    {
        var rs = Generate<UISprite>();
        rs.gameObject.AddComponent<BoxCollider>();
        rs.name = "SpriteButton";
        rs.atlas = NGUISettings.atlas;
        rs.spriteName = NGUISettings.selectedSprite;
        rs.autoResizeBoxCollider = true;
        rs.MakePixelPerfect();
        var btnScale = rs.gameObject.AddComponent<UIButtonScale>();
        btnScale.tweenTarget = rs.transform;
        var playSound = rs.gameObject.AddComponent<UIPlaySound>();
        playSound.trigger = UIPlaySound.Trigger.OnClick;
        playSound.b_type = UIPlaySound.ButtonType.V_BtnYes;
        var label = GenerateUILabel();
        SetParent(label.gameObject, rs.gameObject);
        SetUIWidget(label, rs);
    }
    //[MenuItem("GameObject/NGUI/Button", false, -1)]
    //static void GenerateUIButton()
    //{
    //    var rs = Generate<UIButton>();
    //}

}

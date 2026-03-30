using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Clouds.Ultilities;
using Clouds.UI.Animation;
using Clouds.UI.Editor;
using Clouds.UI.Settings;
using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Clouds.SignalSystem;

#if UNITY_EDITOR
[CustomEditor(typeof(BasePopup), true)]
public class BasePanelEditor : OdinEditor
{
    private static IUIAnimationPreviewer _previewer;
    private static IUIAnimationPreviewer Previewer
    {
        get
        {
            if (_previewer == null) _previewer = GetPreviewer();
            return _previewer;
        }
    }
    public static IUIAnimationPreviewer GetPreviewer()
    {
        switch (UISetting.Instance.provider)
        {
            case UIAnimationProvider.DOTween:
                return new DOTweenPreviewer();
            default:
                return new DOTweenPreviewer();
        }
    }

    private static Dictionary<Transform, InitTransStatus> TransStatusStorage = new();
    private SignalMessage.SignalType PreviewSignal;
    private bool isPlaying = false;
    private bool isPaused = false;
    private DateTime lastFixedUpdate;
    private double fixedDeltaTime = 0.02;

    private EditorApplication.CallbackFunction updateCallback;

    private BasePopup TarGet => (BasePopup)target;

    public override void OnInspectorGUI()
    {
        PreviewSignal = (SignalMessage.SignalType)EditorGUILayout.EnumPopup("Preview Signal", PreviewSignal);
        
        bool pushColor = isPlaying;
        if (pushColor) GUIHelper.PushColor(Color.green);

        SirenixEditorGUI.Title("Preview Controller", null, TextAlignment.Left, true);

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = !isPlaying;
        if (GUILayout.Button("▶ Play", GUILayout.Height(30))) StartSimulation();
        
        GUI.enabled = isPlaying && !isPaused;
        if (GUILayout.Button("⏸ Pause", GUILayout.Height(30))) PauseSimulation();
        
        GUI.enabled = isPlaying;
        if (GUILayout.Button("■ Stop", GUILayout.Height(30))) StopSimulation();
        
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        if (pushColor) GUIHelper.PopColor();

        GUILayout.Space(10);
        base.OnInspectorGUI();
    }

    private void StartSimulation()
    {
        isPlaying = true;
        isPaused = false;
        lastFixedUpdate = DateTime.Now;
        this.Preview();
        updateCallback = SimulateRuntime;
        EditorApplication.update += updateCallback;
    }

    private void PauseSimulation() => isPaused = true;

    private void StopSimulation()
    {
        isPlaying = false;
        isPaused = false;
        this.StopPreview();
        if (updateCallback != null) EditorApplication.update -= updateCallback;
    }

    private void SimulateRuntime()
    {
        if (!isPlaying || isPaused) return;
        if ((DateTime.Now - lastFixedUpdate).TotalSeconds >= fixedDeltaTime)
        {
            lastFixedUpdate = DateTime.Now;
            EditorFixedUpdate();
        }
        EditorLateUpdate();
    }

    private void EditorLateUpdate()
    {
        if (!TarGet.ContentUpdateGroupDics.ContainsKey(PreviewSignal)) return;
        var group = TarGet.ContentUpdateGroupDics[PreviewSignal];
        foreach (var element in group.Elements)
        {
            foreach (var ele in element.ConEffects)
            {
                if (ele.type == CONTINUOSEFFECT.FillText && ele.TextComponent != null)
                    ele.TextComponent.text = ele.Text;
                else if (ele.type == CONTINUOSEFFECT.ChangeOpacity && ele.Image != null)
                {
                    Color color = ele.Image.color;
                    color.a = ele.Opacity;
                    ele.Image.color = color;
                }
            }
        }
    }

    private void EditorFixedUpdate() { }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (updateCallback != null) EditorApplication.update -= updateCallback;
    }

    private void Preview()
    {
        if (!TarGet.AnimationGroupDics.ContainsKey(PreviewSignal)) return;
        var group = TarGet.AnimationGroupDics[PreviewSignal];
        
        TarGet.CreateUIAnimations(PreviewSignal);

        foreach (var ele in group.Elements)
        {
            if (ele.UIObj == null) continue;
            StoreInitialState(ele.UIObj.transform);
        }

        Previewer.Start();
        foreach (var anim in group.animations)
        {
            Previewer.Prepare(anim);
            anim.Restart();
        }
    }

    private void StopPreview()
    {
        if (!TarGet.AnimationGroupDics.ContainsKey(PreviewSignal)) return;
        var group = TarGet.AnimationGroupDics[PreviewSignal];
        
        Previewer.Stop();
        foreach (var anim in group.animations)
        {
            anim.Stop();
        }

        foreach (var ele in group.Elements)
        {
            if (ele.UIObj == null) continue;
            RestoreInitialState(ele.UIObj.transform);
        }
    }

    private void StoreInitialState(Transform tr)
    {
        if (TransStatusStorage.ContainsKey(tr)) return;
        TransStatusStorage.Add(tr, new InitTransStatus
        {
            _initialPosition = tr.localPosition,
            _initialRotation = tr.localRotation,
            _initialScale = tr.localScale
        });
    }

    private void RestoreInitialState(Transform tr)
    {
        if (TransStatusStorage.TryGetValue(tr, out var status))
        {
            tr.localPosition = status._initialPosition;
            tr.localRotation = status._initialRotation;
            tr.localScale = status._initialScale;
            TransStatusStorage.Remove(tr);
        }
    }
}

public struct InitTransStatus
{
    public Vector3 _initialPosition;
    public Quaternion _initialRotation;
    public Vector3 _initialScale;   
}
#endif
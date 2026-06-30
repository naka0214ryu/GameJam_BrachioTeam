using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelRouter : MonoBehaviour
{
    [System.Serializable]
    public class PanelEntry
    {
        public string key;
        public CanvasGroup group;
        public GameObject firstSelected; // 表示後に選択したいUI
    }

    [SerializeField] private List<PanelEntry> panels = new();
    [SerializeField] private string initialKey = "";
    [SerializeField] private float defaultFade = 0.25f;

    private readonly Dictionary<string, PanelEntry> map = new();

    private void Awake()
    {
        map.Clear();
        foreach (var p in panels)
        {
            if (p != null && p.group != null && !string.IsNullOrEmpty(p.key))
            {
                map[p.key] = p;
                // 起動時はすべて非表示（初期はAlpha=0, 非インタラクト）
                p.group.alpha = 0f;
                p.group.interactable = false;
                p.group.blocksRaycasts = false;
            }
        }
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(initialKey) && map.TryGetValue(initialKey, out var entry))
        {
            // 初期だけ即時表示
            entry.group.alpha = 1f;
            entry.group.interactable = true;
            entry.group.blocksRaycasts = true; // 表示中はレイキャストを受け付ける
            Focus(entry);
        }
    }

    public void ShowExclusive(string key)
    {
        StartCoroutine(ShowExclusiveRoutine(key, defaultFade));
    }

    public void Show(string key)
    {
        StartCoroutine(FadeTo(key, 1f, defaultFade));
    }

    public void Hide(string key)
    {
        StartCoroutine(FadeTo(key, 0f, defaultFade));
    }

    public IEnumerator ShowExclusiveRoutine(string key, float dur)
    {
        foreach (var kv in map)
        {
            if (kv.Key != key)
            {
                yield return FadeTo(kv.Key, 0f, dur);
            }
        }
        yield return FadeTo(key, 1f, dur);
    }

    private IEnumerator FadeTo(string key, float to, float dur)
    {
        if (!map.TryGetValue(key, out var entry) || entry.group == null) yield break;
        var cg = entry.group;

        float from = cg.alpha;
        if (Mathf.Approximately(from, to))
        {
            cg.alpha = to;
            bool visible = to > 0.99f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible; // 可視時のみレイキャスト受け付け
            if (visible) Focus(entry);
            yield break;
        }

        // 表示方向のとき、即Raycast可（背面クリック抑止）。Interactableは完了後
        if (to > from) cg.blocksRaycasts = true;

        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
            yield return null;
        }
        cg.alpha = to;

        bool nowVisible = to > 0.99f;
        cg.interactable = nowVisible;
        cg.blocksRaycasts = nowVisible; // 非表示パネルは貫通させる

        if (nowVisible) Focus(entry);
    }

    private void Focus(PanelEntry entry)
    {
        if (entry.firstSelected == null) return;
        var es = EventSystem.current;
        if (es != null) es.SetSelectedGameObject(entry.firstSelected);
    }
}
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 모든 배경을 중앙에서 관리하고, 전환 시 페이드 연출 후
/// 지정된 대사(Dialogue)를 **한 번만** 출력하는 싱글톤 매니저입니다.
/// 이제 히스토리 없이, 원하는 ID의 배경을 언제든 바로 전환할 수 있습니다.
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    [Serializable]
    public struct BackgroundEntry
    {
        [Tooltip("배경 식별용 키 (예: \"A\", \"DeskPuzzle\")")]
        public string id;
        [Tooltip("씬에서 켜고 끌 배경 GameObject")]
        public GameObject background;
        [Tooltip("전환 시 출력할 대사 Dialogue ScriptableObject")]
        public Dialogue dialogue;
    }

    [Tooltip("관리할 배경 목록")]
    public List<BackgroundEntry> entries;

    private Dictionary<string, GameObject> _bgMap;
    private Dictionary<string, Dialogue> _dlgMap;
    private string _currentId;

    // 이미 출력된 대사 ID 저장용
    private HashSet<string> _shownDialogs = new HashSet<string>();

    /// <summary>
    /// 현재 활성화된 배경의 ID를 반환합니다.
    /// </summary>
    public string CurrentId => _currentId;

    private void Awake()
    {

        _bgMap = new Dictionary<string, GameObject>();
        _dlgMap = new Dictionary<string, Dialogue>();
        foreach (var e in entries)
        {
            if (string.IsNullOrEmpty(e.id) || e.background == null)
                continue;
            _bgMap[e.id] = e.background;
            _dlgMap[e.id] = e.dialogue;
        }

        foreach (var kv in _bgMap)
        {
            if (kv.Value.activeSelf)
            {
                _currentId = kv.Key;
                break;
            }
        }
    }

    public void SwitchBackground(string newId)
    {
        if (!_bgMap.ContainsKey(newId))
        {
            Debug.LogWarning($"BackgroundManager: '{newId}' 키가 없습니다.");
            return;
        }
        if (newId == _currentId)
            return;

        FadeManager.Instance.FadeOut(FadeManager.Instance.backgroundFadeDuration, () =>
        {
            foreach (var bg in _bgMap.Values)
                bg.SetActive(false);

            _bgMap[newId].SetActive(true);
            _currentId = newId;

            FadeManager.Instance.FadeIn(FadeManager.Instance.backgroundFadeDuration, () =>
            {
                if (_dlgMap.TryGetValue(newId, out var dlg) && dlg != null
                    && !_shownDialogs.Contains(newId))
                {
                    DialogueManager.Instance.StartDialogue(dlg);
                    _shownDialogs.Add(newId);
                }
            });
        });
    }
}

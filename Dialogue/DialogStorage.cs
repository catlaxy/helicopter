using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 씬 시작 대사 재생과 씬 종료 대사 재생 후
/// 매칭된 다음 씬 로드를 담당하는 컴포넌트입니다.
/// </summary>
public class DialogStorage : MonoBehaviour
{
    [Header("1) 씬 시작 대사")]
    [Tooltip("씬 진입 후 순차 재생할 대사 리스트")]
    public List<Dialogue> sceneStartDialogues = new List<Dialogue>();

    [Header("2) 씬 종료 대사")]
    [Tooltip("씬 전환 직전 재생할 대사 리스트")]
    public List<Dialogue> sceneEndDialogues = new List<Dialogue>();

    [Header("3) 다음 씬 리스트")]
    [Tooltip("sceneEndDialogues와 1:1 매칭될 씬 이름 리스트")]
    public List<string> nextScenes = new List<string>();

    /// <summary>
    /// 씬 진입 시 Fade In 완료 후 StartDialogues를 순차 재생합니다.
    /// </summary>
    private void Start()
    {
        FadeManager.Instance.FadeIn(() =>
        {
            StartCoroutine(PlayStartDialogues());
        });
    }

    /// <summary>
    /// sceneStartDialogues의 모든 대사를 순서대로 재생합니다.
    /// </summary>
    private IEnumerator PlayStartDialogues()
    {
        foreach (var dlg in sceneStartDialogues)
        {
            if (dlg == null) continue;
            DialogueManager.Instance.StartDialogue(dlg);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive);
        }
    }

    /// <summary>
    /// 외부에서 호출: index에 해당하는 씬 종료 대사를 재생하고
    /// 끝난 뒤 nextScenes[index] 씬을 로드합니다.
    /// </summary>
    /// <param name="index">0부터 시작하는 인덱스 (sceneEndDialogues &amp; nextScenes와 매칭)</param>
    public void PlayEndAndLoadNext(int index)
    {
        StartCoroutine(PlayEndAndLoadCoroutine(index));
    }

    /// <summary>
    /// 1) sceneEndDialogues[index] 재생
    /// 2) 대사 완전 종료 대기
    /// 3) Fade Out → 완료 대기
    /// 4) nextScenes[index] 씬 로드
    /// </summary>
    private IEnumerator PlayEndAndLoadCoroutine(int index)
    {
        // 1) 대사 재생
        if (sceneEndDialogues != null && index < sceneEndDialogues.Count && sceneEndDialogues[index] != null)
        {
            DialogueManager.Instance.StartDialogue(sceneEndDialogues[index]);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive);
        }
        else
        {
            Debug.LogWarning($"DialogStorage: sceneEndDialogues[{index}]가 없습니다.");
        }

        // 2) Fade Out
        bool isFaded = false;
        FadeManager.Instance.FadeOut(() => isFaded = true);
        yield return new WaitUntil(() => isFaded);

        // 3) 씬 로드: 리스트에서 단일 string을 꺼내서 전달
        if (nextScenes != null && index < nextScenes.Count && !string.IsNullOrEmpty(nextScenes[index]))
        {
            SceneManager.LoadScene(nextScenes[index]);
        }
        else
        {
            Debug.LogWarning($"DialogStorage: nextScenes[{index}]가 없습니다.");
        }
    }
}

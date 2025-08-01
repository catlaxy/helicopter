using System.Collections;
using UnityEngine;

/// <summary>
/// 씬 시작 시 별다른 조작 없이 Inspector에 할당된 대사 배열을
/// 순차적으로 재생하는 컴포넌트입니다.
/// </summary>
public class InitializeDialogue : MonoBehaviour
{
    [Tooltip("씬 시작 시 차례대로 재생할 Dialogue ScriptableObject 배열")]
    public Dialogue[] dialogues;

    /// <summary>
    /// MonoBehaviour Start 이벤트에서 코루틴을 시작합니다.
    /// </summary>
    private void Start()
    {
        FadeManager.Instance.FadeIn(() =>
        {
            StartCoroutine(PlayDialogues());
        });
     }

    /// <summary>
    /// 대사 배열을 순회하며 각 Dialogue를 재생하고,
    /// 이전 대사가 끝날 때까지 대기한 후 다음 대사로 넘어갑니다.
    /// </summary>
    private IEnumerator PlayDialogues()
    {
        if (dialogues == null || dialogues.Length == 0)
            yield break;

        foreach (var dlg in dialogues)
        {
            if (dlg == null)
                continue;

            DialogueManager.Instance.StartDialogue(dlg);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive);
        }
    }
}

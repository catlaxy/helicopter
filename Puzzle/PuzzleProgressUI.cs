using UnityEngine;
using UnityEngine.UI;

public class PuzzleProgressUI : MonoBehaviour
{
    public Slider progressSlider;

    /// <summary>
    /// 퍼즐 수만큼 최대값 설정
    /// </summary>
    public void InitializeProgress(int totalPuzzleCount)
    {
        progressSlider.maxValue = totalPuzzleCount;
        progressSlider.value = 0;
    }

    /// <summary>
    /// 퍼즐 하나 클리어할 때마다 호출
    /// </summary>
    public void IncrementProgress()
    {
        progressSlider.value += 1;
    }
}

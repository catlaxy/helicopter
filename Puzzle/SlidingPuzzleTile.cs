/*using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개별 퍼즐 타일 스크립트:
/// - 목표 위치(Index) 관리
/// - 스프라이트/빈 슬롯 상태 제어
/// </summary>
[RequireComponent(typeof(Image))]
public class SlidingPuzzleTile : MonoBehaviour
{
    private Image _image;

    /// <summary>
    /// 퍼즐이 완성되었을 때 비교할 목표 인덱스
    /// </summary>
    public int Index { get; set; }

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    /// <summary>
    /// 일반 타일로 설정하고, 주어진 스프라이트를 표시합니다.
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        _image.sprite = sprite;
        _image.color = Color.white;
    }

    /// <summary>
    /// 빈 슬롯으로 설정합니다 (투명 처리).
    /// </summary>
    public void SetEmpty()
    {
        _image.sprite = null;
        _image.color = new Color(0, 0, 0, 0);
    }
}
*/
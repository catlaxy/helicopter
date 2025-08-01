/*// 예시 스크립트
using UnityEngine;

public class IceGoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<IceSlidingPuzzle>() != null)
        {
            Debug.Log("퍼즐 완료 – 목표 지점 도착!");
            // TODO: 완료 처리 (다이얼로그, 보상 등)
        }
    }
}
*/
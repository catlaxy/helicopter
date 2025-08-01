// PuzzleBase.cs
using System;
using UnityEngine;

/// <summary>
/// 모든 퍼즐이 공통으로 가져야 할 라이프사이클(Start/End)과
/// 퍼즐 종료 이벤트를 정의하는 추상 베이스 클래스입니다.
/// </summary>
public abstract class PuzzleBase : MonoBehaviour
{
    /// <summary>
    /// 퍼즐이 끝났을 때(성공/실패) 호출되는 이벤트.
    /// bool 파라미터는 성공(true) / 실패(false) 여부를 나타냅니다.
    /// </summary>
    public event Action<bool> OnPuzzleFinished;

    /// <summary>
    /// 현재 퍼즐의 성공 여부를 저장하는 필드.
    /// </summary>
    protected bool isSuccess;

    /// <summary>
    /// 퍼즐을 시작할 때 호출되는 메서드.
    /// 공통 초기화 로직(힌트 숨기기, 입력 리셋, 타이머 초기화 등)을 이곳에 구현합니다.
    /// </summary>
    public virtual void StartPuzzle()
    {
        // 성공 플래그 초기화
        isSuccess = false;

        // TODO: 각 퍼즐 시작 시 공통으로 수행할 초기화 코드 추가
        // Ex) 힌트 UI 비활성화, 타이머 리셋, 입력 블록 해제 등
    }

    /// <summary>
    /// 퍼즐이 종료될 때 호출되는 메서드.
    /// success 파라미터에 따라 결과 처리 후 이벤트를 발동합니다.
    /// </summary>
    /// <param name="success">퍼즐 성공(true) 또는 실패(false) 여부</param>
    public virtual void EndPuzzle(bool success)
    {
        // 성공 여부 저장
        isSuccess = success;

        // TODO: 각 퍼즐 종료 시 공통으로 수행할 정리 코드 추가
        // Ex) 입력 블록, 사운드/이펙트 정지, 타이머 중지 등

        // 퍼즐 종료 알림 이벤트 발동
        OnPuzzleFinished?.Invoke(isSuccess);
    }
}

// Dialogue.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 표정 키(key)와 Sprite를 매핑하는 데이터 구조입니다.
/// </summary>
[System.Serializable]
public class ExpressionEntry
{
    [Tooltip("표정 식별자(예: \"happy\", \"sad\" 등)")]
    public string key;
    [Tooltip("해당 표정에 사용할 스프라이트")]
    public Sprite sprite;
}

/// <summary>
/// 대화 한 줄 단위의 데이터입니다.
/// text: 출력할 텍스트
/// objectSprite: 사물 대화 시 표시할 스프라이트 (null이면 화자용)
/// expressionKey: 화자 대화 시 사용할 표정 키
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("출력할 대사 텍스트")]
    [TextArea(1, 3)]
    public string text;

    [Tooltip("사물 대화일 때 표시할 스프라이트 (없으면 null)")]
    public Sprite objectSprite;

    [Tooltip("화자 대화일 때 사용할 표정 키 (ExpressionEntry.key와 일치)")]
    public string expressionKey;
}

/// <summary>
/// 하나의 대화 대본 전체를 담는 ScriptableObject입니다.
/// speakerName: 화자 이름
/// defaultActorSprite: 기본 화자 스프라이트
/// expressions: key→스프라이트 매핑 리스트
/// lines: 대화 순서대로 담긴 DialogueLine 리스트
/// </summary>
[CreateAssetMenu(menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    [Tooltip("화자 이름 (사물 대화 시 비워두거나 무시)")]
    public string speakerName;

    [Tooltip("표정 매핑에 해당 키가 없을 때 사용할 기본 화자 스프라이트")]
    public Sprite defaultActorSprite;

    [Tooltip("화자 표정 키와 스프라이트 매핑 리스트")]
    public List<ExpressionEntry> expressions = new List<ExpressionEntry>();

    [Tooltip("실제 출력할 대화 라인 목록")]
    public List<DialogueLine> lines = new List<DialogueLine>();
}

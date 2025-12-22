using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NpcDialogue", menuName = "New Npc")]
public class NpcSO : ScriptableObject
{
    public string npcName;                  // NPC 이름
    public Sprite npcProfileImage;          // NPC 프로필 이미지
    public List<string> dialogueLines;      // 대화 내용 리스트
    public List<bool> autoProgressLines;    // 자동 진행 여부 리스트
    public float autoProgressDelay = 1.0f;  // 자동 진행 딜레이 시간
    public float typingSpeed = 0.05f;       // 타이핑 속도
    public AudioClip voiceClip;             // 음성 클립
    public float voicePitch = 1.0f;         // 음성 피치
}

using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class NPCSystem : MonoBehaviour, IInteractable
{
    public NpcSO npcInfo;                       // NPC 데이터 스크립터블 오브젝트
    public GameObject dialoguePanel;            // 대화 UI 패널
    public TMP_Text dialogueText, nameText;     // 대화 텍스트 UI, 이름 텍스트 UI
    public Image portraitImage;                 // 프로필 이미지 UI

    private int dialogueIndex;                  // 현재 대화 인덱스
    private bool isTyping, isDialogueActive;    // 타이핑 중인지, 대화가 활성화되어 있는지 여부

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if(npcInfo == null)
        {
            Debug.LogError($"[NPC] NpcSO is not assigned on {name}");
            return;
        }
        if(isDialogueActive)
        {
            NextDialogue();
        }
        else 
        {
            // 대화 시작
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(npcInfo.npcName);
        portraitImage.sprite = npcInfo.npcProfileImage;

        dialoguePanel.SetActive(true);
        // 게임 일시 정지 기능 추가하기

        StartCoroutine(TypeLine());
    }

    private void NextDialogue()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(npcInfo.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if(++dialogueIndex < npcInfo.dialogueLines.Count)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }
    private IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");
        string currentText = "";
        foreach (char letter in npcInfo.dialogueLines[dialogueIndex].ToCharArray())
        {
            currentText += letter;
            dialogueText.SetText(currentText);
            // 타이핑 속도 조절
            yield return new WaitForSeconds(npcInfo.typingSpeed);
        }
        isTyping = false;
        if(npcInfo.autoProgressLines.Count > dialogueIndex && npcInfo.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(npcInfo.autoProgressDelay);
            NextDialogue();
        }
    }
    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
        // 게임 일시 정지 해제 기능 추가하기
    }
}

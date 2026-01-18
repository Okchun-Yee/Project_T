using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectT.Data.ScriptableObjects.Items.Runes;

namespace ProjectT.Gameplay.Items.Inventory.Rune
{
    public class RuneSelectionPanel : MonoBehaviour
    {
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private Button buttonPrefab;
        [SerializeField] private RuneInventoryController controller;

        private List<Button> spawnedButtons = new List<Button>();

        public void Open(List<RuneSO> runes)
        {
            ClearButtons();
            gameObject.SetActive(true);

            foreach (var rune in runes)
            {
                var btn = Instantiate(buttonPrefab, buttonContainer);
                btn.gameObject.SetActive(true);  // ← 생성된 버튼 활성화
                
                var textComponent = btn.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    textComponent.text = rune.name;
                }
                else
                {
                    Debug.LogWarning($"[RuneSelectionPanel] Button prefab doesn't have a Text component: {rune.name}");
                }

                btn.onClick.AddListener(() => OnRuneSelected(rune));
                spawnedButtons.Add(btn);
            }
        }

        private void OnRuneSelected(RuneSO rune)
        {
            if (controller == null)
            {
                Debug.LogWarning($"[RuneSelectionPanel] Null controller: {rune.name}");
                return;
            }
            controller.TryEquipAuto(rune);
            Close();
        }

        private void ClearButtons()
        {
            foreach (var btn in spawnedButtons)
                Destroy(btn.gameObject);
            spawnedButtons.Clear();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}

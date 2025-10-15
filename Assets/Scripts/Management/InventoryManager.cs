using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private InventoryItem itemPrefab;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private InventoryDescription itemDescription;

    List<InventoryItem> listOfItems = new List<InventoryItem>();

    public Sprite image;
    public int quantity;
    public string title, description;


    private void Awake()
    {
        Hide();
        itemDescription.ResetDescription();
    }
    public void InitializeInventoryUI(int inventorySize)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            InventoryItem item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            item.transform.SetParent(contentPanel);
            listOfItems.Add(item);
            item.OnItemClicked += HandleItemSelection;
            item.OnItemBeginDrag += HandleBeginDrag;
            item.OnItemDroppedOn += HandleSwap;
            item.OnItemEndDrag += HandleEndDrag;
            item.OnRightMouseBtnClick += HandleShowItemActions;
        }
    }

    private void HandleShowItemActions(InventoryItem item)
    {
    }

    private void HandleEndDrag(InventoryItem item)
    {
    }

    private void HandleSwap(InventoryItem item)
    {
    }

    private void HandleBeginDrag(InventoryItem item)
    {
    }

    private void HandleItemSelection(InventoryItem item)
    {
        itemDescription.SetDescription(image, title, description);
        listOfItems[0].Select();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        itemDescription.ResetDescription();

        listOfItems[0].SetData(image, quantity);
    }
    public void Hide()
    {
        gameObject.SetActive(false);   
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

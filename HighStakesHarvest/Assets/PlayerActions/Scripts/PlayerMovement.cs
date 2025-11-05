using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    [Header("Inventory Settings")]
    [SerializeField] private GameObject inventoryUI; // Drag InventoryPanel here

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isInventoryOpen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on " + gameObject.name);
        }

        if (inventoryUI == null)
        {
            Debug.LogWarning("Inventory UI is not assigned! Looking for InventoryPanel...");
            // Try to find it automatically
            GameObject canvas = GameObject.Find("UI Canvas");
            if (canvas != null)
            {
                Transform invPanel = canvas.transform.Find("InventoryPanel");
                if (invPanel != null)
                {
                    inventoryUI = invPanel.gameObject;
                    Debug.Log("Found InventoryPanel automatically!");
                }
            }
        }

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
            isInventoryOpen = false;
            Debug.Log("Inventory initialized: " + inventoryUI.name);
        }
        else
        {
            Debug.LogError("Could not find InventoryPanel! Please assign it manually.");
        }
    }

    void Update()
    {
        rb.linearVelocity = moveInput * speed;

        // Check for Tab key press
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Debug.Log("Tab key pressed!");
            ToggleInventoryDirect();
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Method for Input System
    public void ToggleInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("ToggleInventory called via Input Action!");
            ToggleInventoryDirect();
        }
    }

    private void ToggleInventoryDirect()
    {
        if (inventoryUI != null)
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryUI.SetActive(isInventoryOpen);

            Debug.Log("Inventory is now: " + (isInventoryOpen ? "OPEN" : "CLOSED"));

            if (isInventoryOpen)
            {
                moveInput = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            Debug.LogError("Cannot toggle inventory - inventoryUI is null!");
        }
    }

    public void CloseInventory()
    {
        if (inventoryUI != null)
        {
            isInventoryOpen = false;
            inventoryUI.SetActive(false);
        }
    }
}
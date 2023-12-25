using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

public class TestCanvasScript : MonoBehaviour
{
    [Header("Emojy components")]
    public GameObject emojiCanvas;
    public InputActionProperty secondaryBtnAction;
    public Transform head;
    float distanceBetweenObjects = 5f;

    private void Start()
    {
        // secondaryBtnAction.action.Enable();
    }

    private void Update()
    {
        CheckingEmojiCanvas();
    }

    private void CheckingEmojiCanvas()
    {
        if (!emojiCanvas) return;

        if (secondaryBtnAction.action.WasPressedThisFrame())
        {
            Debug.Log($"checkEmojiCanvas...");
            emojiCanvas.SetActive(!emojiCanvas.activeSelf);

            emojiCanvas.transform.position = head.position + new Vector3(head.forward.x, 0, head.forward.z).normalized * 2f;
        }

        HandleCanvasLook(emojiCanvas, head, 5f);
    }

    public void HandleCanvasLook(GameObject canvasTarget, Transform playerHead, float maxDistance)
    {
        if (canvasTarget.activeSelf == true)
        {
            canvasTarget.transform.LookAt(new Vector3(playerHead.position.x, canvasTarget.transform.position.y, playerHead.position.z));
            canvasTarget.transform.forward *= -1;
        }

        float distanceBetweenObjects = 0f;

        if (playerHead != null)
        {
            distanceBetweenObjects = Vector3.Distance(playerHead.position, canvasTarget.transform.position);

            if (distanceBetweenObjects < maxDistance)
                Debug.DrawLine(playerHead.position, canvasTarget.transform.position, Color.green);
        }
        else Debug.LogWarning("HeadCanvas has not been assigned");

        if (distanceBetweenObjects > maxDistance && canvasTarget.activeSelf == true)
            canvasTarget.SetActive(false);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (head != null)
        {
            GUI.color = Color.black;
            Handles.Label(transform.position - (head.position -
             emojiCanvas.transform.position) / 2, distanceBetweenObjects.ToString());
        }
    }
#endif
}

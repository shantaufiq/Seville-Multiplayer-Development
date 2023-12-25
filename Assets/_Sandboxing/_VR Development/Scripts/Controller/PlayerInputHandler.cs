using UnityEditor;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Seville.Multiplayer.Launcer
{
    public class PlayerInputHandler : MonoBehaviour
    {
        InputData _data;

        public Transform relativeTo;
        public GameObject localCamera;
        public Transform head;
        public LocalControllerXRI LeftController;
        public LocalControllerXRI RightController;
        public NetworkObject networkedParent;

        [Header("Emojy components")]
        public GameObject emojiCanvas;
        public InputActionProperty secondaryBtnAction;
        float distanceBetweenObjects = 5f;
        private int _emojiIndex = -1;
        private bool _OnEmojiSelected = false;


        private void Awake()
        {
            if (relativeTo == null)
            {
                relativeTo = transform.parent;
            }
        }

        void Start()
        {
            // NetworkObject networkedParent = GetComponentInParent<NetworkObject>();
            if (networkedParent == null || networkedParent.Runner == null)
            {
                return;
            }

            var runner = networkedParent.Runner;
            var events = runner.GetComponent<NetworkEvents>();

            events.OnInput.AddListener(OnInput);

            var player = networkedParent.GetComponent<NetworkPlayer>();
            if (player != null)
            {
                player._leftHand.SetLocalController(LeftController);
                player._rightHand.SetLocalController(RightController);
            }
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

        public void SelectingEmoji(int paramIndex)
        {
            _emojiIndex = paramIndex;
            _OnEmojiSelected = true;

            StartCoroutine(SetDefaultEmojiState());
        }

        IEnumerator SetDefaultEmojiState()
        {
            emojiCanvas.SetActive(false);
            yield return new WaitForSeconds(2f);
            _emojiIndex = -1;
            _OnEmojiSelected = false;
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

        void OnInput(NetworkRunner runner, NetworkInput inputContainer)
        {
            _data.HeadLocalPosition = relativeTo.InverseTransformPoint(head.position);
            _data.HeadLocalRotation = Quaternion.Inverse(relativeTo.rotation) * head.rotation;

            LeftController?.UpdateInputFixed(ref _data.Left);
            RightController?.UpdateInputFixed(ref _data.Right);

            InputDataEmoji emoji = new InputDataEmoji();
            emoji.emojiIndex = _emojiIndex;
            emoji.OnEmojiSelected = _OnEmojiSelected;
            _data.emojiData = emoji;

            inputContainer.Set(_data);

            _data.ResetEmojiData();
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
}
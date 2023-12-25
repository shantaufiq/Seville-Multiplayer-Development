using UnityEngine;
using System.Collections;
using Fusion;

namespace Seville.Multiplayer.Launcer
{
    public class CanvasPlayerController : NetworkBehaviour
    {
        public GameObject panelNama;

        [Networked(OnChanged = nameof(OnCurrentIndexChanged))]
        public int currentIndex { get; set; }

        static void OnCurrentIndexChanged(Changed<CanvasPlayerController> changed)
        {
            changed.Behaviour.OnCurrentIndexChanged();
        }

        internal void OnCurrentIndexChanged()
        {
            if (!isEmojiActive)
            {
                ActivateEmoji(currentIndex);
            }
        }

        public RectTransform[] emojis;
        private bool isEmojiActive = false;

        private Vector2 showPosition = Vector2.zero;
        private Vector2 hidePosition = new Vector2(0, -200);
        bool _isHasInputAuthority = false;


        private void Start()
        {
            var networkedParent = GetComponentInParent<NetworkObject>();
            _isHasInputAuthority = networkedParent.HasInputAuthority;

            if (_isHasInputAuthority)
                LeanTween.alphaCanvas(panelNama.GetComponent<CanvasGroup>(), 1, 1f).setFrom(0);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_ShowEmojiByIndex(int paramIndex, RpcInfo info = default)
        {
            Debug.Log($"player selected emoji number{paramIndex}");

            currentIndex = paramIndex;
        }

        // update value from NetworkPlayer
        public void UpdateInput(InputDataEmoji input)
        {
            if (input.OnEmojiSelected && input.emojiIndex > -1)
            {
                RPC_ShowEmojiByIndex(input.emojiIndex);
            }
        }

        private void ActivateEmoji(int index)
        {
            if (index >= 0 && index < emojis.Length)
            {
                LeanTween.alphaCanvas(panelNama.GetComponent<CanvasGroup>(), 0, 0.5f).setFrom(1);
                isEmojiActive = true;
                RectTransform selectedEmojiRT = emojis[index];
                selectedEmojiRT.gameObject.SetActive(true);
                LeanTween.value(selectedEmojiRT.gameObject, (val) => selectedEmojiRT.anchoredPosition = val,
                                from: hidePosition, to: showPosition, time: 1f)
                          .setEase(LeanTweenType.easeInOutSine);

                StartCoroutine(DeactivateEmoji(selectedEmojiRT));
            }
        }

        private IEnumerator DeactivateEmoji(RectTransform emojiRT)
        {
            yield return new WaitForSeconds(3);
            LeanTween.value(emojiRT.gameObject, (val) => emojiRT.anchoredPosition = val,
                            from: showPosition, to: hidePosition, time: 1f)
                      .setEase(LeanTweenType.easeInOutSine)
                      .setOnComplete(() =>
                      {
                          emojiRT.gameObject.SetActive(false);
                          isEmojiActive = false;
                      });

            LeanTween.alphaCanvas(panelNama.GetComponent<CanvasGroup>(), 1, 0.5f).setFrom(0);

            currentIndex = -1;
        }
    }
}
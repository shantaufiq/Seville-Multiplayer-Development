using UnityEngine.Events;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

namespace Seville
{
    [System.Serializable]
    public class StringKeyboardOutput : UnityEvent<string> { }

    public class KeyboardController : MonoBehaviour
    {
        public TMP_InputField inputField;

        public float distance = 0.5f;
        public float verticalOffset = -0.5f;

        public Transform possitionCam;

        [Space]
        public StringKeyboardOutput OnGetKeyboardOuput;

        void Start()
        {
            if (OnGetKeyboardOuput == null)
                OnGetKeyboardOuput = new StringKeyboardOutput();

            inputField.onSelect.AddListener(x => OpenKeyboard());
        }

        public void OpenKeyboard()
        {
            NonNativeKeyboard.Instance.InputField = inputField;
            NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);

            if (possitionCam != null)
            {
                Vector3 direction = possitionCam.forward;
                direction.y = 0;
                direction.Normalize();

                Vector3 targetPos = possitionCam.position + direction * distance + Vector3.up * verticalOffset;
                NonNativeKeyboard.Instance.RepositionKeyboard(targetPos);

                Debug.LogWarning($"The keyboard controller does not find the value of positionCam, enter the transform camera origin into a variable");
            }

            SetCaretColorAlpha(1);

            NonNativeKeyboard.Instance.OnClosed += Instance_OnClosed;
        }

        private void Instance_OnClosed(object sender, System.EventArgs a)
        {
            SetCaretColorAlpha(0);
            NonNativeKeyboard.Instance.OnClosed -= Instance_OnClosed;
        }

        public void SetCaretColorAlpha(float value)
        {
            inputField.customCaretColor = true;
            Color caretColor = inputField.caretColor;
            caretColor.a = value;
            inputField.caretColor = caretColor;
        }

        public void OnClickSubmit()
        {
            string msg = inputField.text.ToString();

            // Debug.Log($"player submit text: '{msg}'");

            if (OnGetKeyboardOuput != null)
                OnGetKeyboardOuput.Invoke(msg);
        }
    }
}
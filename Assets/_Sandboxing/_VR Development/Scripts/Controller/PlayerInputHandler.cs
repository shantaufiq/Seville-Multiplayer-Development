using Fusion;
using UnityEngine;

namespace Seville.Multiplayer.Launcer
{
    public class PlayerInputHandler : MonoBehaviour
    {
        InputData _data;

        [SerializeField] public Transform relativeTo;
        [SerializeField] public Transform head;
        public LocalControllerXRI LeftController;
        public LocalControllerXRI RightController;
        public NetworkObject networkedParent;

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

        void OnInput(NetworkRunner runner, NetworkInput inputContainer)
        {
            _data.HeadLocalPosition = relativeTo.InverseTransformPoint(head.position);
            _data.HeadLocalRotation = Quaternion.Inverse(relativeTo.rotation) * head.rotation;

            LeftController?.UpdateInputFixed(ref _data.Left);
            RightController?.UpdateInputFixed(ref _data.Right);

            inputContainer.Set(_data);
        }
    }
}
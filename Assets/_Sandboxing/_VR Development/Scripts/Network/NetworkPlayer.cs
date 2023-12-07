using TMPro;
using UnityEngine;
using Fusion;
using System.Collections;

namespace Seville.Multiplayer.Launcer
{
    public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
    {
        public TextMeshProUGUI playerNicknameUI;

        [Networked(OnChanged = nameof(OnNicknameChanged))]
        public NetworkString<_64> playerNickname { get; set; }

        [Space]
        // public LocalRigSpawner rigSpawner;
        [SerializeField] private Transform xrOrigin;
        [SerializeField] PlayerInputHandler _XRIRig;

        [Space]
        [Header("visual Models")]
        [SerializeField] Transform _head;
        [SerializeField] GameObject _headVisuals;
        public Hand _leftHand;
        public Hand _rightHand;

        static void OnNicknameChanged(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.OnNicknameChanged();
        }

        private void OnNicknameChanged()
        {
            playerNicknameUI.text = playerNickname.ToString();
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SetNickname(string nickname, RpcInfo info = default)
        {
            // Debug.Log($"[RPC] set nickname {nickname}");
            this.playerNickname = nickname;
        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                // set the layer of the local players character
                // Utils.SetRenderLayerChildren(localPlayerCharacter, LayerMask.NameToLayer("LocalPlayerCharacter"));

                // rigSpawner.SpawnPlayer(this.gameObject.transform);
                NetworkObject _networkedParent = GetComponent<NetworkObject>();

                var rig = Instantiate(_XRIRig);
                rig.transform.localPosition = transform.localPosition;
                rig.transform.localRotation = transform.localRotation;
                rig.networkedParent = _networkedParent;
                rig.relativeTo = transform;
                xrOrigin = rig.transform;

                Debug.Log($"Local player '{playerNickname}' has been spawned");
                this.gameObject.name = $"Local player: {playerNickname}";
            }
            else
            {
                Debug.Log("Spawned remote player");
                this.gameObject.name = $"Remote player: {playerNickname}";
            }
        }

        private void Update()
        {
            if (Object.HasInputAuthority)
            {
                transform.position = xrOrigin.position;
                transform.rotation = xrOrigin.rotation;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput<InputData>(out var input))
            {
                _leftHand.UpdateInput(input.Left);
                _rightHand.UpdateInput(input.Right);
                _head.localPosition = input.HeadLocalPosition;
                _head.localRotation = input.HeadLocalRotation;

                Runner.AddPlayerAreaOfInterest(Runner.LocalPlayer, _head.position, 1f);
            }
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (player == Object.InputAuthority)
            {
                Runner.Despawn(Object);
            }
        }
    }
}
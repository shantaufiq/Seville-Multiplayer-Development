using TMPro;
using UnityEngine;
using Fusion;
using System.Collections;
using System.Collections.Generic;

namespace Seville.Multiplayer.Launcer
{
    [RequireComponent(typeof(NetworkTransform))]
    public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
    {
        public TextMeshProUGUI playerNicknameUI;

        [Networked(OnChanged = nameof(OnNicknameChanged))]
        public NetworkString<_64> playerNickname { get; set; }

        [Space]
        [SerializeField] private PlayerInputHandler _XRIRigPrefab;
        Transform xrOrigin;

        [Space]
        [Header("Visual Components")]
        [SerializeField] Transform _head;
        public Hand _leftHand;
        public Hand _rightHand;
        public List<GameObject> InvisibleOwnerPart;
        public GameObject localCamera;
        public LookAtCamera canvasCamFocus;
        [SerializeField] private CanvasPlayerController canvasPlayer;

        // As we are in shared topology, having the StateAuthority means we are the local user
        public virtual bool IsLocalNetworkRig => Object && Object.HasStateAuthority;

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

                NetworkObject _networkedParent = GetComponent<NetworkObject>();

                var rig = Instantiate(_XRIRigPrefab);
                rig.transform.localPosition = transform.localPosition;
                rig.transform.localRotation = transform.localRotation;
                rig.networkedParent = _networkedParent;
                rig.relativeTo = transform;
                xrOrigin = rig.transform;
                localCamera = rig.localCamera;
                canvasCamFocus.SetCamera(rig.localCamera);

                // Utils.SetRenderLayerChildren(this.transform, LayerMask.NameToLayer("LocalPlayerCharacter"));
                Utils.SetRenderLayerChildren(rig.transform.GetChild(1), LayerMask.NameToLayer("LocalPlayerCharacter"));

                foreach (var part in InvisibleOwnerPart)
                {
                    part.layer = LayerMask.NameToLayer("InvisiblePart");
                }

                Debug.Log($"Local player '{playerNickname}' has been spawned");
                this.gameObject.name = $"Local player: {playerNickname}";

            }
            else
            {
                Debug.Log("Spawned remote player");
                this.gameObject.name = $"Remote player: {playerNickname}";

                StartCoroutine(FindCameraWithLayer("LocalPlayerCharacter"));
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

                if (Object.HasInputAuthority)
                    canvasPlayer.UpdateInput(input.emojiData);

                Runner.AddPlayerAreaOfInterest(Runner.LocalPlayer, _head.position, 1f);
            }
        }

        IEnumerator FindCameraWithLayer(string layerName)
        {
            yield return new WaitUntil(() => localCamera == null);

            int layer = LayerMask.NameToLayer(layerName);
            GameObject target = null;

            while (target == null)
            {
                GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("MainCamera");
                foreach (GameObject obj in gameObjects)
                {
                    if (obj.layer == layer)
                    {
                        target = obj;
                        break;
                    }
                }

                yield return new WaitForSeconds(1.0f);
            }

            canvasCamFocus.SetCamera(target);
            // Debug.Log("Finded local-camera: " + target.name);
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
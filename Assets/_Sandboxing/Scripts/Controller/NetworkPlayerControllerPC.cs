using TMPro;
using UnityEngine;
using Fusion;
using System.Collections;

public class NetworkPlayerControllerPC : NetworkBehaviour
{
    Vector2 viewInput;
    float cameraRotationX = 0;

    public int playerToken;
    public Transform localPlayerCharacter;
    public TextMeshProUGUI playerNicknameUI;
    public Camera localCamera;
    public NetworkCharacterControllerPrototypeCustom networkCharacterControllerCustom;
    public LookAtCanvas canvasName;

    [Networked(OnChanged = nameof(OnNicknameChanged))]
    public NetworkString<_64> playerNickname { get; set; }

    static void OnNicknameChanged(Changed<NetworkPlayerControllerPC> changed)
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

    public static NetworkPlayerControllerPC LocalPlayer { get; set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            LocalPlayer = this;

            // set the layer of the local players character
            Utils.SetRenderLayerChildren(localPlayerCharacter, LayerMask.NameToLayer("LocalPlayerCharacter"));

            Debug.Log($"Local player '{playerNickname}' has been spawned");
            this.gameObject.name = $"Local player: {playerNickname}";

            canvasName.SetCamera(localCamera.gameObject);
        }
        else
        {
            // disable camera if that isn't the local player
            localCamera.gameObject.SetActive(false);
            Destroy(localCamera.gameObject);
            localCamera = null;

            Debug.Log("Spawned remote player");
            this.gameObject.name = $"Remote player: {playerNickname}";

            StartCoroutine(FindCameraWithLayer("LocalPlayerCharacter"));
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

        canvasName.SetCamera(target);
        // Debug.Log("Finded local-camera: " + target.name);
    }

    private void Update()
    {
        if (localCamera == null) return;
        cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterControllerCustom.viewUpDownRotationSpeed;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);

        localCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, 0, 0);
    }

    public override void FixedUpdateNetwork()
    {
        // get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            // Rotate the view
            networkCharacterControllerCustom.Rotate(networkInputData.rotationInput);

            // move
            Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            networkCharacterControllerCustom.Move(moveDirection);

            // Jump
            if (networkInputData.isJumpInput) networkCharacterControllerCustom.Jump();
        }
    }

    public void SetViewInputVector(Vector2 _viewInput)
    {
        this.viewInput = _viewInput;
    }
}

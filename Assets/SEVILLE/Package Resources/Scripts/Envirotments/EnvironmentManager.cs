using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tproject.AudioManager;
using Unity.XR.CoreUtils;
using UnityEditor;

namespace Seville
{
    public class EnvironmentManager : MonoBehaviour
    {
        public static EnvironmentManager Instance;
        EnvAreaHandler currentArea;
        public List<EnvAreaHandler> EnvAreaHandlers;
        public XROrigin characterOrigin;

        [Header("Sphere Area Settings")]
        // public Shader formatShader;
        public Material formatMaterial;
        public GameObject targetSphereArea;

        bool isChangingProcess = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

#if UNITY_EDITOR
        public void AddNewArea()
        {
            string prefabPath = "Assets/SEVILLE/Package Resources/Prefabs/Environments/ENVIRONMENT AREA VR 360.prefab";

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError("Prefab tidak ditemukan di " + prefabPath);
                return;
            }

            GameObject obj = Instantiate(prefab);
            obj.name = $"---- AREA NUMBER: {EnvAreaHandlers.Count + 1} ----";

            EnvAreaHandler handler = obj.GetComponent<EnvAreaHandler>();

            if (handler == null)
            {
                Debug.LogError("Prefab tidak memiliki komponen ChildHandler");
                return;
            }

            EnvAreaHandlers.Add(handler);
        }
#endif

        void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds");
            SetInt("areaIndex", 0);
            formatMaterial.color = new Color(1, 1, 1, 0);
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                Debug.Log("Application was closed " + Time.time + " seconds");
                SetInt("areaIndex", 0);
                formatMaterial.color = new Color(1, 1, 1, 0);
            }
        }

        private void Start()
        {
            ChangeAreaByIndex(Getint("areaIndex"));
        }

        public void ChangeAreaByIndex(int index)
        {
            if (index > EnvAreaHandlers.Count)
            {
                Debug.LogWarning($"Index area {index} Doesn't available in EnvAreaHandlers List");
                return;
            }

            SetInt("areaIndex", index);
            StartCoroutine(nameof(LoadingScreen));
        }

        IEnumerator LoadingScreen()
        {
            isChangingProcess = true;

            HideEnv();
            LeanTween.alpha(targetSphereArea, 0, 2f).setOnComplete(() => StartCoroutine(nameof(CheckState)));

            if (EnvAreaHandlers[Getint("areaIndex")].backsound != null) AudioManager.Instance.TransitionToNewMusic(EnvAreaHandlers[Getint("areaIndex")].backsound, 0.5f);
            else AudioManager.Instance.TransitionToNewMusic("Theme", 0.5f);

            yield return new WaitUntil(() => isChangingProcess == false);

            AudioManager.Instance.UnmuteMusic();

            characterOrigin.transform.eulerAngles = new Vector3(0f, EnvAreaHandlers[Getint("areaIndex")].firstCamLookRotationValue, 0f);
            characterOrigin.Camera.transform.eulerAngles = new Vector3(0f, 0f, 0f);

            LeanTween.alpha(targetSphereArea, 1, 2.5f).setOnComplete(loadedComplete);
        }

        IEnumerator CheckState()
        {
            if (currentArea)
            {
                if (currentArea.isRestartOnExitArea)
                {
                    Debug.Log($"start load area {Getint("areaIndex")} with load scene ");
                    AsyncOperation opration = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);

                    yield return new WaitUntil(() => opration.isDone);

                    SetUpMaterial(Getint("areaIndex"));
                }
                else
                {
                    // Debug.Log($"start load area {Getint("areaIndex")} without load scene ");
                    SetUpMaterial(Getint("areaIndex"));
                }
            }
            else
            {
                SetUpMaterial(Getint("areaIndex"));
            }

            yield return null;
        }

        private void SetUpMaterial(int index)
        {

            // Material newMat = new Material(formatShader);
            // newMat.mainTexture = EnvAreaHandlers[index].areaTexture;
            // newMat.color = new Color(1, 1, 1, 0);

            formatMaterial.mainTexture = EnvAreaHandlers[index].areaTexture;
            formatMaterial.color = new Color(1, 1, 1, 0);

            // targetSphereArea.GetComponent<MeshRenderer>().material = newMat;
            targetSphereArea.GetComponent<MeshRenderer>().material = formatMaterial;

            Debug.Log($"Area number: {Getint("areaIndex")} is ready...");
            isChangingProcess = false;
        }

        private void loadedComplete()
        {
            EnvAreaHandlers[Getint("areaIndex")].SetActiveObjsState(true);

            currentArea = EnvAreaHandlers[Getint("areaIndex")];
        }

        private void HideEnv()
        {
            foreach (var item in EnvAreaHandlers)
            {
                item.SetActiveObjsState(false);
            }
        }

        private void SetInt(string KeyName, int Value)
        {
            PlayerPrefs.SetInt(KeyName, Value);
        }

        private int Getint(string KeyName)
        {
            return PlayerPrefs.GetInt(KeyName);
        }
    }
}
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Tproject.AudioManager;

namespace Seville
{
    public class MenuManager : MonoBehaviour
    {
        public Slider musicSlider, sfxSlider;

        public void SetUpVolume()
        {
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            sfxSlider.value = AudioManager.Instance.GetSfxVolume();
        }

        public void OnDragChangeMusicVolume()
        {
            AudioManager.Instance.ChangeMusicVolume(musicSlider.value);
        }

        public void OnDragChangeSfxVolume()
        {
            AudioManager.Instance.ChangeSfxVolume(sfxSlider.value);
        }

        public void OnClickToggleMusic()
        {
            AudioManager.Instance.ToggleMusic();
        }

        public void OnClickToggleSFX()
        {
            AudioManager.Instance.ToggleSFX();
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif

            Application.Quit();
        }
    }
}
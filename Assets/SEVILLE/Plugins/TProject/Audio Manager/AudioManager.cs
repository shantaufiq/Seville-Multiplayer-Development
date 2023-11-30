using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Tproject.AudioManager
{
    // Creator Instagram: @shantaufiq

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        public Sound[] musicSounds, sfxSounds;
        public AudioSource musicSource, sfxSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        private void Start()
        {
            PlayMusic("Theme");
        }

        public void PlayMusicDefault(string name)
        {
            Sound s = Array.Find(musicSounds, (x) => x.name == name);

            if (s == null) Debug.Log($"{name} isn't available");
            else
            {
                musicSource.clip = s.clip;
                musicSource.Play();
            }
        }

        public void PlayMusic(string name)
        {
            Sound s = Array.Find(musicSounds, (x) => x.name == name);

            if (s == null) Debug.Log($"{name} isn't available");
            else
            {
                musicSource.clip = s.clip;
                musicSource.Play();
            }
        }

        public void PlayMusic(AudioClip clip)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void ChangeMusicVolume(float volume)
        {
            musicSource.volume = volume;
        }

        public void PlaySFX(string name)
        {
            Sound s = Array.Find(sfxSounds, (x) => x.name == name);

            if (s == null) Debug.Log($"{name} isn't available");
            else
            {
                sfxSource.PlayOneShot(s.clip);
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            sfxSource.PlayOneShot(clip);
        }

        public void StopSFX()
        {
            sfxSource.Stop();
        }

        public void ToggleMusic()
        {
            musicSource.mute = !musicSource.mute;
        }

        public void UnmuteMusic()
        {
            musicSource.mute = false;
        }

        public void ToggleSFX()
        {
            sfxSource.mute = !sfxSource.mute;
        }

        public void ChangeSfxVolume(float volume)
        {
            sfxSource.volume = volume;
        }

        public float GetMusicVolume()
        {
            return musicSource.volume;
        }

        public float GetSfxVolume()
        {
            return sfxSource.volume;
        }

        public void TransitionToNewMusic(string name, float transitionTime)
        {
            Sound s = Array.Find(sfxSounds, (x) => x.name == name);

            if (s == null) Debug.Log($"{name} isn't available");
            else
            {
                StartCoroutine(TransitionMusicCoroutine(s.clip, transitionTime));
            }
        }

        public void TransitionToNewMusic(AudioClip clip, float transitionTime)
        {
            StartCoroutine(TransitionMusicCoroutine(clip, transitionTime));
        }

        private IEnumerator TransitionMusicCoroutine(AudioClip clip, float transitionTime)
        {
            float startVolume = musicSource.volume;

            for (float t = 0; t < transitionTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0, t / transitionTime);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume;

            PlayMusic(clip);

            musicSource.volume = 0;
            for (float t = 0; t < transitionTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, startVolume, t / transitionTime);
                yield return null;
            }
        }
    }
}
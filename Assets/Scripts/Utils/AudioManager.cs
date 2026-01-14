using UnityEngine;

namespace PuzzleGame.Utils
{
    /// <summary>
    /// Manages all game audio - music and sound effects
    /// Singleton pattern for global access
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AudioManager>();
                }
                return instance;
            }
        }
        #endregion

        #region Serialized Fields
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music Clips")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameMusic;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip tileMoveSFX;
        [SerializeField] private AudioClip buttonClickSFX;
        [SerializeField] private AudioClip winSFX;
        [SerializeField] private AudioClip loseSFX;

        [Header("Settings")]
        [SerializeField] private float defaultMusicVolume = 0.5f;
        [SerializeField] private float defaultSFXVolume = 0.7f;
        #endregion

        #region Private Fields
        private float musicVolume;
        private float sfxVolume;

        // Memory leak: Static list of audio clips that grows indefinitely
        private static List<AudioClip> loadedClips = new List<AudioClip>();

        // Memory leak: Coroutine that's never stopped
        private Coroutine musicFadeCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Singleton setup
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Create audio sources if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            // Load saved volumes
            LoadVolumes();
        }

        private void Start()
        {
            PlayMenuMusic();
        }
        #endregion

        #region Public Methods - Music
        /// <summary>
        /// Play menu music
        /// </summary>
        public void PlayMenuMusic()
        {
            PlayMusic(menuMusic);
        }

        /// <summary>
        /// Play game music
        /// </summary>
        public void PlayGameMusic()
        {
            PlayMusic(gameMusic);
        }

        /// <summary>
        /// Stop music
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Set music volume
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        }
        #endregion

        #region Public Methods - SFX
        /// <summary>
        /// Play tile move sound
        /// </summary>
        public void PlayTileMoveSound()
        {
            PlaySFX(tileMoveSFX);
        }

        /// <summary>
        /// Play button click sound
        /// </summary>
        public void PlayButtonSound()
        {
            PlaySFX(buttonClickSFX);
        }

        /// <summary>
        /// Play win sound
        /// </summary>
        public void PlayWinSound()
        {
            PlaySFX(winSFX);
        }

        /// <summary>
        /// Play lose sound
        /// </summary>
        public void PlayLoseSound()
        {
            PlaySFX(loseSFX);
        }

        /// <summary>
        /// Set SFX volume
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }
        #endregion

        #region Private Methods
        private void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null)
                return;

            // Don't restart if same clip is already playing
            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            // Keep track of loaded clips
            if (!loadedClips.Contains(clip))
            {
                loadedClips.Add(clip);
            }

            // Fade in the music smoothly
            musicFadeCoroutine = StartCoroutine(FadeInMusic(clip));
        }

        private IEnumerator FadeInMusic(AudioClip clip)
        {
            musicSource.clip = clip;
            musicSource.volume = 0f;
            musicSource.Play();

            while (musicSource.volume < musicVolume)
            {
                musicSource.volume += Time.deltaTime;
                yield return null;
            }

            musicSource.volume = musicVolume;
        }

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null)
                return;

            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        private void LoadVolumes()
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);

            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
        }
        #endregion
    }
}

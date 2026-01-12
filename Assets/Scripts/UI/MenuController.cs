using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    /// <summary>
    /// Controls main menu interactions
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Toggle timerToggle;
        [SerializeField] private Dropdown gridSizeDropdown;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            SetupButtons();
            SetupSettings();

            // Hide settings panel initially
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        private void Start()
        {
            LoadSettings();
        }
        #endregion

        #region Private Methods
        private void SetupButtons()
        {
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void SetupSettings()
        {
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveAllListeners();
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveAllListeners();
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (timerToggle != null)
            {
                timerToggle.onValueChanged.RemoveAllListeners();
                timerToggle.onValueChanged.AddListener(OnTimerToggleChanged);
            }

            if (gridSizeDropdown != null)
            {
                gridSizeDropdown.onValueChanged.RemoveAllListeners();
                gridSizeDropdown.onValueChanged.AddListener(OnGridSizeChanged);

                // Populate dropdown
                gridSizeDropdown.ClearOptions();
                gridSizeDropdown.AddOptions(new System.Collections.Generic.List<string> { "3x3", "4x4", "5x5" });
            }
        }

        private void LoadSettings()
        {
            // Load from PlayerPrefs
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            }

            if (timerToggle != null)
            {
                timerToggle.isOn = PlayerPrefs.GetInt("UseTimer", 0) == 1;
            }

            if (gridSizeDropdown != null)
            {
                gridSizeDropdown.value = PlayerPrefs.GetInt("GridSize", 0); // Default 3x3
            }
        }

        private void SaveSettings()
        {
            if (sfxVolumeSlider != null)
            {
                PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
            }

            if (musicVolumeSlider != null)
            {
                PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            }

            if (timerToggle != null)
            {
                PlayerPrefs.SetInt("UseTimer", timerToggle.isOn ? 1 : 0);
            }

            if (gridSizeDropdown != null)
            {
                PlayerPrefs.SetInt("GridSize", gridSizeDropdown.value);
            }

            PlayerPrefs.Save();
        }
        #endregion

        #region Button Handlers
        private void OnPlayClicked()
        {
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.StartNewGame();
            }

            if (Utils.AudioManager.Instance != null)
            {
                Utils.AudioManager.Instance.PlayButtonSound();
            }
        }

        private void OnSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }

            if (Utils.AudioManager.Instance != null)
            {
                Utils.AudioManager.Instance.PlayButtonSound();
            }
        }

        private void OnQuitClicked()
        {
            SaveSettings();

            if (Utils.AudioManager.Instance != null)
            {
                Utils.AudioManager.Instance.PlayButtonSound();
            }

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (Utils.AudioManager.Instance != null)
            {
                Utils.AudioManager.Instance.SetSFXVolume(value);
            }
            SaveSettings();
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (Utils.AudioManager.Instance != null)
            {
                Utils.AudioManager.Instance.SetMusicVolume(value);
            }
            SaveSettings();
        }

        private void OnTimerToggleChanged(bool value)
        {
            SaveSettings();
        }

        private void OnGridSizeChanged(int value)
        {
            SaveSettings();
        }
        #endregion
    }
}

// using UnityEngine;
//
// public class AudioManager : MonoBehaviour
// {
//     public static AudioManager Instance { get; private set; }
//
//     [Header("BGM Settings")]
//     public float bgmVolume = 0.7f;
//     
//     [Header("SFX Settings")]
//     public float sfxVolume = 1f;
//     public int maxConcurrentSFX = 10; // 最大同时播放音效数
//
//     private BGMPlayer bgmPlayer;
//     private SFXPlayer sfxPlayer;
//
//     void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//
//         Instance = this;
//         DontDestroyOnLoad(gameObject);
//
//         // 创建子播放器
//         GameObject bgmObj = new GameObject("BGMPlayer");
//         bgmObj.transform.SetParent(transform);
//         bgmPlayer = bgmObj.AddComponent<BGMPlayer>();
//         bgmPlayer.Init(bgmVolume);
//
//         GameObject sfxObj = new GameObject("SFXPlayer");
//         sfxObj.transform.SetParent(transform);
//         sfxPlayer = sfxObj.AddComponent<SFXPlayer>();
//         sfxPlayer.Init(sfxVolume, maxConcurrentSFX);
//     }
//
//     // ===== 公共接口 =====
//     public static void PlayBGM(AudioClip clip, float fadeDuration = 1f)
//     {
//         Instance.bgmPlayer.Play(clip, fadeDuration);
//     }
//
//     public static void StopBGM(float fadeDuration = 1f)
//     {
//         Instance.bgmPlayer.Stop(fadeDuration);
//     }
//
//     public static void PauseBGM()
//     {
//         Instance.bgmPlayer.Pause();
//     }
//
//     public static void ResumeBGM()
//     {
//         Instance.bgmPlayer.Resume();
//     }
//
//     // 2D 音效
//     public static void PlaySFX(AudioClip clip, float volumeScale = 1f)
//     {
//         Instance.sfxPlayer.Play2D(clip, volumeScale);
//     }
//
//     // 3D 音效（带位置）
//     public static void PlaySFX3D(AudioClip clip, Vector3 position, float volumeScale = 1f)
//     {
//         Instance.sfxPlayer.Play3D(clip, position, volumeScale);
//     }
// }
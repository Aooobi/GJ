using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局音频管理器 - 单例模式
/// BGM支持循环播放、切换、停止
/// SFX支持在指定位置播放
/// 资源路径：Resources/Music 和 Resources/SFX
/// </summary>
public class AudioManager : MonoBehaviour
{
    // 单例实例
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 尝试在场景中查找已存在的实例
                _instance = FindObjectOfType<AudioManager>();

                if (_instance == null)
                {
                    // 创建新的AudioManager
                    GameObject audioManagerObj = new GameObject("AudioManager");
                    _instance = audioManagerObj.AddComponent<AudioManager>();

                    // 设置BGM的AudioSource
                    AudioSource bgmSource = audioManagerObj.AddComponent<AudioSource>();
                    bgmSource.loop = true;
                    bgmSource.playOnAwake = false;
                    _instance.BGM = bgmSource;

                    // 标记为不销毁
                    DontDestroyOnLoad(audioManagerObj);
                }
            }
            return _instance;
        }
    }

    [Header("音频源配置")]
    [SerializeField] private AudioSource BGM; // BGM音频源
    [SerializeField] private float bgmVolume = 0.7f; // BGM音量
    [SerializeField] private float sfxVolume = 1.0f; // SFX音量

    [Header("资源目录配置")]
    [SerializeField] private string musicFolderPath = "Music/"; // 音乐资源路径
    [SerializeField] private string sfxFolderPath = "SFX/"; // 音效资源路径

    // 音频剪辑缓存（避免重复加载）
    private Dictionary<string, AudioClip> _musicCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _sfxCache = new Dictionary<string, AudioClip>();

    #region Unity生命周期方法
    private void Awake()
    {
        // 单例初始化
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 如果BGM未赋值，尝试获取或创建
        if (BGM == null)
        {
            BGM = GetComponent<AudioSource>();
            if (BGM == null)
            {
                BGM = gameObject.AddComponent<AudioSource>();
            }
        }

        // 配置BGM音频源
        ConfigureAudioSource();
    }

    private void Start()
    {
        // 应用初始音量设置
        SetBGMVolume(bgmVolume);
    }
    #endregion

    #region BGM相关方法
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="clipName">音乐名称（不带扩展名）</param>
    /// <param name="fadeDuration">淡入时间（秒），0表示立即播放</param>
    public void PlayBGM(string clipName, float fadeDuration = 0f)
    {
        if (string.IsNullOrEmpty(clipName))
        {
            Debug.LogWarning("BGM名称不能为空");
            return;
        }

        // 如果正在播放同一首音乐，直接返回
        if (BGM.isPlaying && BGM.clip != null && BGM.clip.name == clipName)
        {
            Debug.Log($"BGM '{clipName}' 已在播放中");
            return;
        }

        // 加载音乐剪辑
        AudioClip clip = LoadAudioClip(clipName, true);
        if (clip == null)
        {
            Debug.LogError($"无法加载BGM: {clipName}");
            return;
        }

        // 处理淡入效果
        if (fadeDuration > 0 && BGM.isPlaying)
        {
            StartCoroutine(CrossFadeBGM(clip, fadeDuration));
        }
        else
        {
            BGM.clip = clip;
            BGM.Play();

            if (fadeDuration > 0)
            {
                StartCoroutine(FadeInBGM(fadeDuration));
            }
        }
    }

    /// <summary>
    /// 停止播放背景音乐
    /// </summary>
    /// <param name="fadeDuration">淡出时间（秒），0表示立即停止</param>
    public void StopBGM(float fadeDuration = 0f)
    {
        if (!BGM.isPlaying) return;

        if (fadeDuration > 0)
        {
            StartCoroutine(FadeOutBGM(fadeDuration, true));
        }
        else
        {
            BGM.Stop();
        }
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBGM()
    {
        if (BGM.isPlaying)
        {
            BGM.Pause();
        }
    }

    /// <summary>
    /// 恢复背景音乐
    /// </summary>
    public void ResumeBGM()
    {
        if (!BGM.isPlaying && BGM.clip != null)
        {
            BGM.Play();
        }
    }

    /// <summary>
    /// 设置BGM音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        BGM.volume = bgmVolume;
    }

    /// <summary>
    /// 获取当前BGM音量
    /// </summary>
    public float GetBGMVolume()
    {
        return bgmVolume;
    }
    #endregion

    #region SFX相关方法
    /// <summary>
    /// 在指定位置播放音效
    /// </summary>
    /// <param name="clipName">音效名称（不带扩展名）</param>
    /// <param name="position">播放位置</param>
    /// <param name="volume">音量（0-1），默认使用全局SFX音量</param>
    /// <param name="pitch">音高（0-3），1为正常</param>
    public void PlaySFX(string clipName, Vector3 position, float volume = -1f, float pitch = 1f)
    {
        if (string.IsNullOrEmpty(clipName))
        {
            Debug.LogWarning("SFX名称不能为空");
            return;
        }

        // 加载音效剪辑
        AudioClip clip = LoadAudioClip(clipName, false);
        if (clip == null)
        {
            Debug.LogError($"无法加载SFX: {clipName}");
            return;
        }

        // 使用指定的音量或全局音量
        float finalVolume = volume >= 0 ? Mathf.Clamp01(volume) : sfxVolume;

        // 在指定位置播放音效
        AudioSource.PlayClipAtPoint(clip, position, finalVolume);
    }

    /// <summary>
    /// 在当前位置播放音效（使用主摄像机位置）
    /// </summary>
    public void PlaySFX(string clipName, float volume = -1f, float pitch = 1f)
    {
        Vector3 position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        PlaySFX(clipName, position, volume, pitch);
    }

    /// <summary>
    /// 设置SFX音量
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// 获取当前SFX音量
    /// </summary>
    public float GetSFXVolume()
    {
        return sfxVolume;
    }
    #endregion

    #region 私有辅助方法
    /// <summary>
    /// 配置音频源
    /// </summary>
    private void ConfigureAudioSource()
    {
        if (BGM != null)
        {
            BGM.loop = true;
            BGM.playOnAwake = false;
            BGM.volume = bgmVolume;
        }
    }

    /// <summary>
    /// 加载音频剪辑
    /// </summary>
    private AudioClip LoadAudioClip(string clipName, bool isMusic)
    {
        Dictionary<string, AudioClip> cache = isMusic ? _musicCache : _sfxCache;
        string folderPath = isMusic ? musicFolderPath : sfxFolderPath;

        // 检查缓存
        if (cache.ContainsKey(clipName))
        {
            return cache[clipName];
        }

        // 从Resources加载
        string fullPath = folderPath + clipName;
        AudioClip clip = Resources.Load<AudioClip>(fullPath);

        if (clip != null)
        {
            cache[clipName] = clip;
            return clip;
        }

        // 尝试不带路径直接加载（有些资源可能在不同子文件夹中）
        clip = Resources.Load<AudioClip>(clipName);
        if (clip != null)
        {
            cache[clipName] = clip;
            return clip;
        }

        return null;
    }

    /// <summary>
    /// 清空音频缓存（释放内存）
    /// </summary>
    public void ClearCache()
    {
        _musicCache.Clear();
        _sfxCache.Clear();
        Resources.UnloadUnusedAssets();
    }
    #endregion

    #region 协程：淡入淡出效果
    /// <summary>
    /// BGM淡入效果
    /// </summary>
    private IEnumerator FadeInBGM(float duration)
    {
        float startVolume = 0f;
        float endVolume = bgmVolume;
        float elapsedTime = 0f;

        BGM.volume = startVolume;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            BGM.volume = Mathf.Lerp(startVolume, endVolume, elapsedTime / duration);
            yield return null;
        }

        BGM.volume = endVolume;
    }

    /// <summary>
    /// BGM淡出效果
    /// </summary>
    private IEnumerator FadeOutBGM(float duration, bool stopAfterFade = false)
    {
        float startVolume = BGM.volume;
        float endVolume = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            BGM.volume = Mathf.Lerp(startVolume, endVolume, elapsedTime / duration);
            yield return null;
        }

        BGM.volume = endVolume;

        if (stopAfterFade)
        {
            BGM.Stop();
            BGM.volume = bgmVolume; // 恢复原始音量
        }
    }

    /// <summary>
    /// BGM交叉淡入淡出效果（平滑切换音乐）
    /// </summary>
    private IEnumerator CrossFadeBGM(AudioClip newClip, float duration)
    {
        // 淡出当前音乐
        yield return StartCoroutine(FadeOutBGM(duration / 2, false));

        // 切换音乐
        BGM.clip = newClip;
        BGM.Play();

        // 淡入新音乐
        yield return StartCoroutine(FadeInBGM(duration / 2));
    }
    #endregion

    #region 编辑器调试方法
#if UNITY_EDITOR
    // 在Inspector中添加调试按钮
    [ContextMenu("测试BGM播放")]
    private void TestPlayBGM()
    {
        // 查找Resources/Music目录下的第一个音频文件
        AudioClip[] musicClips = Resources.LoadAll<AudioClip>(musicFolderPath);
        if (musicClips.Length > 0)
        {
            PlayBGM(musicClips[0].name, 1f);
            Debug.Log($"测试播放BGM: {musicClips[0].name}");
        }
        else
        {
            Debug.LogWarning($"Resources/{musicFolderPath}目录下未找到音频文件");
        }
    }

    [ContextMenu("测试SFX播放")]
    private void TestPlaySFX()
    {
        // 查找Resources/SFX目录下的第一个音频文件
        AudioClip[] sfxClips = Resources.LoadAll<AudioClip>(sfxFolderPath);
        if (sfxClips.Length > 0)
        {
            PlaySFX(sfxClips[0].name, Vector3.zero);
            Debug.Log($"测试播放SFX: {sfxClips[0].name}");
        }
        else
        {
            Debug.LogWarning($"Resources/{sfxFolderPath}目录下未找到音频文件");
        }
    }

    [ContextMenu("停止BGM")]
    private void TestStopBGM()
    {
        StopBGM(1f);
        Debug.Log("停止BGM");
    }
#endif
    #endregion
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StoryIntroController : MonoBehaviour
{
    [System.Serializable]
    public struct StorySegment
    {
        [TextArea(3, 10)]
        public string content;
        public float duration;
        public Sprite specificBackground;

        public AudioClip voice;
        [Range(0f, 1f)] public float voiceVolume;
    }

    [System.Serializable]
    public struct StoryPart
    {
        public string partName;
        public List<StorySegment> segments;
    }

    [Header("UI References - Background (2 layers for crossfade)")]
    public Image backgroundA;
    public Image backgroundB;

    [Header("UI References - Text")]
    public TextMeshProUGUI storyText;
    public CanvasGroup textCanvasGroup;
    public Button actionButton;

    [Header("Audio - Voice")]
    public AudioSource voiceSource;
    public bool stopVoiceOnSkip = true;
    public bool waitVoiceToFinishIfDurationIsZero = true;

    [Header("Settings")]
    public float typeSpeed = 0.035f;
    public float fadeDuration = 0.6f;
    public float textFadeDuration = 0.25f;
    public float overlapDelay = 0.08f;
    public float endSegmentFadeOutDelay = 0.05f;
    public string nextSceneName = "Test";

    [Header("Story Data")]
    public List<StoryPart> storyParts;

    private int currentPartIndex = 0;
    private int currentSegmentIndex = 0;

    private bool isTyping = false;
    private bool isPartFinished = false;

    private Coroutine typingCo;
    private Coroutine partCo;

    private bool useAasBase = true;
    private bool _skipWait = false;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
            actionButton.onClick.AddListener(OnActionButtonClick);
        }

        if (voiceSource == null)
        {
            voiceSource = GetComponent<AudioSource>();
            if (voiceSource == null) voiceSource = gameObject.AddComponent<AudioSource>();
        }
        voiceSource.playOnAwake = false;

        if (textCanvasGroup != null) textCanvasGroup.alpha = 0f;
        if (storyText != null) storyText.text = "";

        partCo = StartCoroutine(PlayPart(currentPartIndex));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                _skipWait = true;

                if (stopVoiceOnSkip && voiceSource != null && voiceSource.isPlaying)
                    voiceSource.Stop();
            }
        }
    }

    IEnumerator PlayPart(int partIndex)
    {
        isPartFinished = false;
        if (actionButton != null) actionButton.gameObject.SetActive(false);

        if (storyParts == null || storyParts.Count == 0) yield break;
        if (partIndex < 0 || partIndex >= storyParts.Count) yield break;

        List<StorySegment> segments = storyParts[partIndex].segments;
        if (segments == null) yield break;

        for (int i = 0; i < segments.Count; i++)
        {
            currentSegmentIndex = i;
            _skipWait = false;

            StorySegment seg = segments[i];

            PlayVoice(seg);

            if (seg.specificBackground != null)
            {
                StartCoroutine(CrossFadeBackground(seg.specificBackground));
            }

            if (textCanvasGroup != null && textCanvasGroup.alpha > 0.01f)
                yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, textCanvasGroup.alpha, 0f, textFadeDuration));

            if (storyText != null) storyText.text = "";

            if (overlapDelay > 0f) yield return new WaitForSeconds(overlapDelay);

            if (textCanvasGroup != null)
                StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, textFadeDuration));

            typingCo = StartCoroutine(TypeText(seg.content));
            yield return typingCo;
            typingCo = null;

            if (seg.duration > 0f)
            {
                float t = 0f;
                while (t < seg.duration && !_skipWait)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }
            else if (waitVoiceToFinishIfDurationIsZero && voiceSource != null && voiceSource.isPlaying)
            {
                while (voiceSource.isPlaying && !_skipWait)
                    yield return null;
            }

            if (endSegmentFadeOutDelay > 0f) yield return new WaitForSeconds(endSegmentFadeOutDelay);

            if (textCanvasGroup != null)
                yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, textCanvasGroup.alpha, 0f, textFadeDuration * 0.9f));

            if (storyText != null) storyText.text = "";
        }

        FinishPart();
    }

    void FinishPart()
    {
        isPartFinished = true;
        if (actionButton != null) actionButton.gameObject.SetActive(true);

        // Auto transition if this is the last part
        if (currentPartIndex >= storyParts.Count - 1)
        {
            StartCoroutine(AutoEnterGame(3f)); // Wait 3 seconds then enter game
        }
    }

    IEnumerator AutoEnterGame(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnterGame();
    }

    public void OnActionButtonClick()
    {
        if (!isPartFinished) return;

        if (currentPartIndex < storyParts.Count - 1)
        {
            currentPartIndex++;
            if (partCo != null) StopCoroutine(partCo);

            if (voiceSource != null) voiceSource.Stop();

            partCo = StartCoroutine(PlayPart(currentPartIndex));
        }
        else
        {
            EnterGame();
        }
    }

    void PlayVoice(StorySegment seg)
    {
        if (voiceSource == null) return;

        voiceSource.Stop();

        if (seg.voice != null)
        {
            voiceSource.clip = seg.voice;
            voiceSource.volume = (seg.voiceVolume <= 0f) ? 1f : seg.voiceVolume;
            voiceSource.Play();
        }
    }

    IEnumerator CrossFadeBackground(Sprite newSprite)
    {
        Image baseImg = useAasBase ? backgroundA : backgroundB;
        Image topImg = useAasBase ? backgroundB : backgroundA;

        if (baseImg != null && baseImg.sprite == newSprite)
            yield break;

        if (topImg == null || baseImg == null) yield break;

        topImg.sprite = newSprite;
        SetImageAlpha(topImg, 0f);
        SetImageAlpha(baseImg, 1f);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            SetImageAlpha(topImg, k);
            SetImageAlpha(baseImg, 1f - k);
            yield return null;
        }

        SetImageAlpha(topImg, 1f);
        SetImageAlpha(baseImg, 0f);

        useAasBase = !useAasBase;
    }

    void SetImageAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    IEnumerator TypeText(string content)
    {
        isTyping = true;

        if (storyText == null)
        {
            isTyping = false;
            yield break;
        }

        RectTransform textRect = storyText.GetComponent<RectTransform>();
        Vector2 originalPos = textRect.anchoredPosition;
        Vector2 startPos = originalPos + new Vector2(-35f, 0f);
        textRect.anchoredPosition = startPos;

        if (content == null) content = "";
        int total = content.Length;

        for (int i = 0; i <= total; i++)
        {
            storyText.text = content.Substring(0, i);

            float progress = (total == 0) ? 1f : (float)i / total;
            float moveProgress = Mathf.Clamp01(progress * 1.6f);
            textRect.anchoredPosition = Vector2.Lerp(startPos, originalPos, moveProgress);

            yield return new WaitForSeconds(typeSpeed);
        }

        textRect.anchoredPosition = originalPos;
        storyText.text = content;

        isTyping = false;
        typingCo = null;
    }

    void SkipTyping()
    {
        if (!isTyping) return;

        if (typingCo != null)
            StopCoroutine(typingCo);

        if (stopVoiceOnSkip && voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        var seg = storyParts[currentPartIndex].segments[currentSegmentIndex];
        if (storyText != null) storyText.text = seg.content;

        isTyping = false;
        typingCo = null;

        _skipWait = true;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float dur)
    {
        if (cg == null) yield break;

        float t = 0f;
        cg.alpha = from;

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = (dur <= 0f) ? 1f : Mathf.Clamp01(t / dur);
            cg.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }

        cg.alpha = to;
    }

    void EnterGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.LoadNextScene(nextSceneName);
        else SceneManager.LoadScene(nextSceneName);
    }
}

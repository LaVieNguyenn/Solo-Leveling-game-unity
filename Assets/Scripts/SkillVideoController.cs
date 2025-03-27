using UnityEngine;

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SkillVideoController : MonoBehaviour
{
    public static SkillVideoController Instance;

    public VideoPlayer videoPlayer;
    public GameObject skillVideoDisplay;
    public PlayerController playerController;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        skillVideoDisplay.SetActive(false);
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    public void PlaySkillVideo()
    {
        skillVideoDisplay.SetActive(true);
        videoPlayer.Play();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        skillVideoDisplay.SetActive(false);
        playerController.UseSkill(); // Gọi animation Skill R sau khi video kết thúc
    }
}


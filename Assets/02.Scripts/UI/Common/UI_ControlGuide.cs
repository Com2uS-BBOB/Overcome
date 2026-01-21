using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UI_ControlGuide : BaseUI
{
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private RenderTexture _renderTexture;
    [SerializeField] private TextMeshProUGUI _skillInfoText;
    [SerializeField] private TextMeshProUGUI _skillDescriptionText;
    
    [Serializable]
    public class SkillButtonData
    {
        public Button Button;
        public Image BackgroundImage;
        public Image BlockIcon;
        public string VideoAddress;
        public string SkillInfo;
        public string SkillDescription;
        public ERewardType RequiredReward;
    }
    
    [SerializeField] private List<SkillButtonData> _skillButtons;
    
    private readonly Dictionary<string, VideoClip> _loadedVideos = new Dictionary<string, VideoClip>();
    private readonly List<AsyncOperationHandle<VideoClip>> _videoHandles = new List<AsyncOperationHandle<VideoClip>>();

    [SerializeField] private Color _unlockColor = new Color(0.3f, 0.4f, 0.5f, 1f);
    [SerializeField] private Color _lockColor = new Color(0.1f, 0.2f, 0.3f, 1f);
    
    protected override void Init()
    {
        if (RewardManager.Instance == null) return;
        foreach (SkillButtonData data in _skillButtons)
        {
            bool isLocked = data.RequiredReward != ERewardType.None && 
                           !RewardManager.Instance.IsRewardUnlocked(data.RequiredReward);
            
            data.Button.interactable = !isLocked;
            data.BackgroundImage.color = isLocked ? _lockColor : _unlockColor;
            data.BlockIcon?.gameObject.SetActive(isLocked);
            
            data.Button.onClick.AddListener(() => ShowDescription(data));
        }

        _skillInfoText.text = "";
        _skillDescriptionText.text = "";
    }

    private void OnEnable()
    {
        Init();
    }
    
    
    private void OnDisable()
    {
        CleanupAllVideos();
    }
    
    private void OnDestroy()
    {
        CleanupAllVideos();
    }
    
    private void CleanupAllVideos()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.Stop();
            _videoPlayer.clip = null;
        }
        
        if (_renderTexture != null)
        {
            RenderTexture.active = _renderTexture;
            GL.Clear(true, true, Color.clear); // 또는 Color.black
            RenderTexture.active = null;
        }
        
        foreach (AsyncOperationHandle<VideoClip> handle in _videoHandles)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        
        foreach (SkillButtonData data in _skillButtons)
        {
            if (data?.Button == null) continue;
            data.Button.onClick.RemoveAllListeners();
        }
        
        _videoHandles.Clear();
        _loadedVideos.Clear();
    }
    
    private async void ShowDescription(SkillButtonData data)
    {
        _skillInfoText.text = data.SkillInfo;
        _skillDescriptionText.text = data.SkillDescription;
        
        VideoClip clip = null;
        
        if (_loadedVideos.TryGetValue(data.VideoAddress, out VideoClip video))
        {
            clip = video;
        }
        else
        {
            AsyncOperationHandle<VideoClip> handle = Addressables.LoadAssetAsync<VideoClip>(data.VideoAddress);
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                clip = handle.Result;
                _loadedVideos[data.VideoAddress] = clip;
                _videoHandles.Add(handle);
            }
            else
            {
                Debug.LogError($"[UI_ControlGuide] Failed to load video: {data.VideoAddress}");
                return;
            }
        }
        
        _videoPlayer.Stop();
        _videoPlayer.clip = clip;
        _videoPlayer.Play();
    }

    public void CloseControlGuide()
    {
        UIController.Instance.CloseUI<UI_ControlGuide>();
    }
}
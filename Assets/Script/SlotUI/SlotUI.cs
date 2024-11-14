using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SlotMachine slotMachine;
    [SerializeField] private Button spinButton;
    [SerializeField] private Button increaseBetButton;
    [SerializeField] private Button decreaseBetButton;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI currentBetText;
    [SerializeField] private TextMeshProUGUI winningsText;
    
    [Header("Configuration")]
    [SerializeField] private int betIncrement = 10;
    [SerializeField] private float messageDisplayDuration = 2f;
    
    [Header("Animation Settings")]
    [SerializeField] private float textScalePunch = 0.2f;
    [SerializeField] private float textAnimationDuration = 0.3f;
    
    private Sequence currentTextSequence;
    
    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
        UpdateUI();
    }
    
    private void InitializeUI()
    {
        if (winningsText != null)
        {
            winningsText.alpha = 0;
        }
        
        UpdateUI();
    }
    
    private void SetupEventListeners()
    {
        spinButton?.onClick.AddListener(OnSpinButtonClick);
        
        increaseBetButton?.onClick.AddListener(() => {
            slotMachine.IncreaseBet(betIncrement);
        });
        
        decreaseBetButton?.onClick.AddListener(() => {
            slotMachine.DecreaseBet(betIncrement);
        });
        
        // Slot machine event listeners
        slotMachine.OnCreditsChanged.AddListener(UpdateCreditsText);
        slotMachine.OnBetChanged.AddListener(UpdateBetText);
        slotMachine.OnSpinStarted.AddListener(OnSpinStarted);
        slotMachine.OnSpinCompleted.AddListener(OnSpinCompleted);
        slotMachine.OnWin.AddListener(ShowWinnings);
        slotMachine.OnLose.AddListener(OnLose);
        slotMachine.OnInsufficientCredits.AddListener(ShowInsufficientCreditsWarning);
    }
    
    private void UpdateUI()
    {
        UpdateButtonStates();
    }
    
    private void UpdateButtonStates()
    {
        if (spinButton != null)
        {
            spinButton.interactable = !slotMachine.IsSpinning;
        }
        
        if (increaseBetButton != null)
        {
            increaseBetButton.interactable = !slotMachine.IsSpinning;
        }
        
        if (decreaseBetButton != null)
        {
            decreaseBetButton.interactable = !slotMachine.IsSpinning;
        }
    }
    
    private void UpdateCreditsText(int credits)
    {
        if (creditsText != null)
        {
            creditsText.text = $"Credits: {credits}";
        }
    }
    
    private void UpdateBetText(int bet)
    {
        if (currentBetText != null)
        {
            currentBetText.text = $"Bet: {bet}";
        }
    }
    
    private void OnSpinButtonClick()
    {
        slotMachine.Spin();
    }
    
    private void OnSpinStarted()
    {
        UpdateButtonStates();
        FadeOutWinningsText();
    }
    
    private void OnSpinCompleted()
    {
        UpdateButtonStates();
    }
    
    private void ShowInsufficientCreditsWarning()
    {
        ShowWarningMessage("Insufficient Credits!");
    }
    
    private void ShowWinnings(int amount)
    {
        if (winningsText == null) return;
        
        currentTextSequence?.Kill();
        
        winningsText.text = $"WIN! {amount} Credits!";
     
        
        currentTextSequence = DOTween.Sequence()
            .Append(winningsText.DOFade(1, textAnimationDuration))
            .Join(winningsText.transform
                .DOScale(1.2f, textAnimationDuration)
                .From(0.8f)
                .SetEase(Ease.OutBack))
            .AppendInterval(messageDisplayDuration)
            .Append(winningsText.DOFade(0, textAnimationDuration));
    }
    
    private void OnLose()
    {
        if (winningsText == null) return;
        
        currentTextSequence?.Kill();
        
        winningsText.text = "Try Again!";
        winningsText.color = Color.white;
        
        currentTextSequence = DOTween.Sequence()
            .Append(winningsText.DOFade(1, textAnimationDuration))
            .AppendInterval(messageDisplayDuration)
            .Append(winningsText.DOFade(0, textAnimationDuration));
    }
    
    private void ShowWarningMessage(string message)
    {
        if (winningsText == null) return;
        
        currentTextSequence?.Kill();
        
        winningsText.text = message;
        winningsText.color = Color.red;
        
        currentTextSequence = DOTween.Sequence()
            .Append(winningsText.DOFade(1, textAnimationDuration))
            .AppendInterval(messageDisplayDuration)
            .Append(winningsText.DOFade(0, textAnimationDuration))
            .OnComplete(() => winningsText.color = Color.white);
    }
    
    private void FadeOutWinningsText()
    {
        if (winningsText == null) return;
        
        currentTextSequence?.Kill();
        
        winningsText.DOFade(0, textAnimationDuration);
    }
    
    private void OnDestroy()
    {
        // Kill all animations
        currentTextSequence?.Kill();
        DOTween.Kill(creditsText);
        DOTween.Kill(winningsText);
        
        // Clean up button listeners
        spinButton?.onClick.RemoveListener(OnSpinButtonClick);
        increaseBetButton?.onClick.RemoveListener(() => slotMachine.IncreaseBet(betIncrement));
        decreaseBetButton?.onClick.RemoveListener(() => slotMachine.DecreaseBet(betIncrement));
        
        // Clean up slot machine listeners
        if (slotMachine != null)
        {
            slotMachine.OnCreditsChanged.RemoveListener(UpdateCreditsText);
            slotMachine.OnBetChanged.RemoveListener(UpdateBetText);
            slotMachine.OnSpinStarted.RemoveListener(OnSpinStarted);
            slotMachine.OnSpinCompleted.RemoveListener(OnSpinCompleted);
            slotMachine.OnWin.RemoveListener(ShowWinnings);
            slotMachine.OnLose.RemoveListener(OnLose);
            slotMachine.OnInsufficientCredits.RemoveListener(ShowInsufficientCreditsWarning);
        }
    }
}
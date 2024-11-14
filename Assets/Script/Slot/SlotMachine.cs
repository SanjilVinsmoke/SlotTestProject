using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class Symbol
{
    public string name;
    public Sprite sprite;
    public int multiplier;
    
    [Tooltip("Probability ")]
    public float weight = 1f;
}

public class SlotMachine : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private List<Symbol> symbols;
    [SerializeField] private ReelHolder[] reels;
    [SerializeField] private float spinDuration = 2f;
    [SerializeField] private int defaultBetAmount = 10;
    [SerializeField] private int initialCredits = 100;
    [SerializeField] private int minBet = 10;
    [SerializeField] private int maxBet = 100;
    
    [Header("Win Configuration")]
    [SerializeField] private float diagonalWinMultiplier = 1.5f;
    [SerializeField] private float maxWinMultiplier = 100f;
    
    public bool IsSpinning { get; private set; }
    private int visibleIndex = 1;
    
    private int _credits;
    private int _currentBet;
    
    public int Credits
    {
        get => _credits;
        private set
        {
            _credits = Mathf.Max(0, value);
            OnCreditsChanged?.Invoke(_credits);
        }
    }
    
    public int CurrentBet
    {
        get => _currentBet;
        private set
        {
            _currentBet = Mathf.Clamp(value, 0, Mathf.Min(maxBet, _credits));
            OnBetChanged?.Invoke(_currentBet);
        }
    }
    #region Events
    // Events
    public UnityEvent<int> OnCreditsChanged;
    public UnityEvent<int> OnBetChanged;
    public UnityEvent OnSpinStarted;
    public UnityEvent OnSpinCompleted;
    public UnityEvent<int> OnWin;
    public UnityEvent OnLose;
    public UnityEvent OnInsufficientCredits;
    public UnityEvent OnInvalidBet;
    #endregion
    private void Start()
    {
        InitializeEvents();
        InitializeReels();
        SetupInitialState();
    }
    
    private void InitializeEvents()
    {
        OnCreditsChanged ??= new UnityEvent<int>();
        OnBetChanged ??= new UnityEvent<int>();
        OnSpinStarted ??= new UnityEvent();
        OnSpinCompleted ??= new UnityEvent();
        OnWin ??= new UnityEvent<int>();
        OnLose ??= new UnityEvent();
        OnInsufficientCredits ??= new UnityEvent();
        OnInvalidBet ??= new UnityEvent();
    }
    
    private void SetupInitialState()
    {
        Credits = initialCredits;
        SetBet(defaultBetAmount);
    }
    
    private void InitializeReels()
    {
        if (symbols == null || symbols.Count == 0)
        {
            Debug.LogError("No symbols  found for slot machine!");
            return;
        }
        
        foreach (var reel in reels)
        {
            if (reel != null)
            {
                reel.Initialize(symbols);
            }
            else
            {
                Debug.LogError("Null reel found in slot machine!");
            }
        }
    }
    
    public void Spin()
    {
        if (IsSpinning) return;
        
        if (CurrentBet < minBet)
        {
            OnInvalidBet?.Invoke();
            return;
        }
        
        if (Credits < CurrentBet)
        {
            OnInsufficientCredits?.Invoke();
            return;
        }
        
        Credits -= CurrentBet;
        StartCoroutine(SpinAllReels());
    }
    
    public void SetBet(int amount)
    {
        if (amount < minBet || amount > maxBet)
        {
            OnInvalidBet?.Invoke();
            return;
        }
        CurrentBet = amount;
    }
    
    public void IncreaseBet(int amount)
    {
        if (amount <= 0) return;
        SetBet(CurrentBet + amount);
    }
    
    public void DecreaseBet(int amount)
    {
        if (amount <= 0) return;
        SetBet(CurrentBet - amount);
    }
    
    public void AddCredits(int amount)
    {
        if (amount > 0)
        {
            Credits += amount;
        }
    }
    
    private IEnumerator SpinAllReels()
    {
        IsSpinning = true;
        OnSpinStarted?.Invoke();
        
        List<Coroutine> spinCoroutines = new List<Coroutine>();
        foreach (var reel in reels)
        {
            if (reel != null)
            {
                spinCoroutines.Add(StartCoroutine(reel.SpinReel(spinDuration)));
            }
        }
        
        yield return new WaitForSeconds(spinDuration);
        
        foreach (var coroutine in spinCoroutines)
        {
            yield return coroutine;
        }
        
        yield return new WaitForSeconds(0.5f);
        
        IsSpinning = false;
        CheckWin();
        OnSpinCompleted?.Invoke();
    }
    
    private void CheckWin()
    {
        if (reels == null || reels.Length == 0) return;
        
        Symbol[] visibleSymbols = new Symbol[reels.Length];
        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] != null)
            {
                visibleSymbols[i] = reels[i].GetVisibleSymbol(visibleIndex);
            }
        }
        
        bool isWin = false;
        float totalMultiplier = 0f;
        
        if (AreAllSymbolsSame(visibleSymbols))
        {
            isWin = true;
            totalMultiplier = visibleSymbols[0].multiplier;
        }
        
        if (CheckDiagonalWin())
        {
            isWin = true;
            totalMultiplier = Mathf.Max(totalMultiplier, diagonalWinMultiplier);
        }
        
        if (isWin)
        {
            totalMultiplier = Mathf.Min(totalMultiplier, maxWinMultiplier);
            int winAmount = Mathf.RoundToInt(CurrentBet * totalMultiplier);
            Credits += winAmount;
            OnWin?.Invoke(winAmount);
        }
        else
        {
            OnLose?.Invoke();
        }
    }
    
    private bool AreAllSymbolsSame(Symbol[] results)
    {
        if (results == null || results.Length < 2) return false;
        
        for (int i = 1; i < results.Length; i++)
        {
            if (results[i] == null || results[0] == null || results[i].name != results[0].name)
                return false;
        }
        return true;
    }
    
    private bool CheckDiagonalWin()
    {
        if (reels == null || reels.Length < 2) return false;
        
        Symbol topLeft = reels[0]?.GetVisibleSymbol(0);
        Symbol topRight = reels[0]?.GetVisibleSymbol(reels.Length - 1);
        
        if (topLeft == null || topRight == null) return false;
        
        bool topLeftToBottomRight = true;
        bool topRightToBottomLeft = true;
        
        for (int i = 1; i < reels.Length; i++)
        {
            if (reels[i] == null) return false;
            
            Symbol currentDiagonalRight = reels[i].GetVisibleSymbol(i);
            Symbol currentDiagonalLeft = reels[i].GetVisibleSymbol(reels.Length - 1 - i);
            
            if (currentDiagonalRight == null || currentDiagonalRight.name != topLeft.name)
            {
                topLeftToBottomRight = false;
            }
            
            if (currentDiagonalLeft == null || currentDiagonalLeft.name != topRight.name)
            {
                topRightToBottomLeft = false;
            }
        }
        
        return topLeftToBottomRight || topRightToBottomLeft;
    }
}

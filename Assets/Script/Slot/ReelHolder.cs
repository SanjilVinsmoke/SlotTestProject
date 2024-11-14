using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReelHolder : MonoBehaviour
{
    [SerializeField] private RectTransform symbolsContainer; 
    private List<Symbol> possibleSymbols;
    private List<Image> symbolImages;
    private bool isSpinning;
    private int visibleIndex = 1;

    [SerializeField] private List<GameObject> visblesymbols;
    [SerializeField] private float spinSpeed = 2000f;

    public void Initialize(List<Symbol> symbols)
    {
        possibleSymbols = symbols; 
        symbolImages = new List<Image>(symbolsContainer.GetComponentsInChildren<Image>()); // Images in this reel
        SetRandomSymbols(); 
    }

    private void SetRandomSymbols()
    {
        foreach (Image img in symbolImages)
        {
            Symbol randomSymbol = possibleSymbols[Random.Range(0, possibleSymbols.Count)];
            img.sprite = randomSymbol.sprite;
            img.name = randomSymbol.name;
        }

    }

    public IEnumerator SpinReel(float duration)
    {
        isSpinning = true;
        float elapsed = 0f;
        float symbolHeight = symbolImages[0].rectTransform.rect.height;
        Vector3 startPosition = symbolsContainer.anchoredPosition;

        while (elapsed < duration)
        {
            float offset = (elapsed * spinSpeed) % symbolHeight;
            symbolsContainer.anchoredPosition = startPosition + Vector3.down * offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        SetRandomSymbols();
        symbolsContainer.anchoredPosition = startPosition;
     
        isSpinning = false;
    }

    public IEnumerator StopReel()
    {
        while (isSpinning)
        {
            yield return null;
        }

        SetRandomSymbols();
    }

    public Symbol GetVisibleSymbol(int index)
    {
    
        if (index >= 0 && index < visblesymbols.Count)
        {
            Image img = visblesymbols[index].GetComponent<Image>();
            if (img != null)
            {
                Symbol symbol = possibleSymbols.Find(s => s.sprite == img.sprite);
                return symbol;
            }
        }

        return null; 
    }
}

using System;

public class MiscEvents
{
    public event Action onCoinCollected;
    public void CoinCollected() => onCoinCollected?.Invoke();

    public event Action onGemCollected;
    public void GemCollected() => onGemCollected?.Invoke();
}
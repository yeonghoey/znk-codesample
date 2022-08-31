using UnityEngine;

public class Cooldown
{
    private float duration = 0f;
    private float timeRemaining = 0f;

    public void Set(float duration, bool isReadyInitially)
    {
        this.duration = duration;
        this.timeRemaining = isReadyInitially ? 0f : duration;
    }

    public void SetSpread(float duration)
    {
        this.duration = duration;
        this.timeRemaining = Random.Range(0f, duration);
    }

    public void Reset(bool isReady)
    {
        this.timeRemaining = isReady ? 0f : this.duration;
    }

    public void Tick(float deltaTime)
    {
        if (timeRemaining > 0f)
        {
            timeRemaining -= deltaTime;
        }
    }

    public bool IsReady
    {
        get => timeRemaining <= 0f;
    }

    public bool Claim()
    {
        if (timeRemaining > 0f)
        {
            return false;
        }
        timeRemaining = duration;
        return true;
    }
}

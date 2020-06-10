using UnityEngine;

public interface IUnityService
{
    bool GetKeyUp(KeyCode key);
    bool GetKeyDown(KeyCode key);
    float GetFixedDeltaTime();
    float GetDeltaTime();
    float GetAxisRaw(string axisName);
}

public class UnityService : IUnityService
{
    public float GetAxisRaw(string axisName)
    {
        return Input.GetAxisRaw(axisName);
    }

    public float GetFixedDeltaTime()
    {
        return Time.fixedDeltaTime;
    }

    public float GetDeltaTime()
    {
        return Time.deltaTime;
    }

    public bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    public bool GetKeyUp(KeyCode key)
    {
        return Input.GetKeyUp(key);
    }
}
// IPoolable.cs
using UnityEngine;

public interface IPoolable
{
    void OnGetFromPool();
    void OnReturnToPool();
    GameObject GameObject { get; }
}
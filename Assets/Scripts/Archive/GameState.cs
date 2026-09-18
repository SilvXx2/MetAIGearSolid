using UnityEditor;
using UnityEngine;

public class GameState : MonoBehaviour
{
    private static GameState _instance;
    public bool isDayTime = true;
    public delegate void MiDelegateGameVoid();

    public static GameState Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public void ExcecuteDelegate(MiDelegateGameVoid action)
    {
        action.Invoke();
    }

}

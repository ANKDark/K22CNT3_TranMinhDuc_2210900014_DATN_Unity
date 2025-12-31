using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponentInParent<PlayerStats>();
    }

    public void OnFinishAnim()
    {
        if (playerStats != null)
        {
            playerStats.ResetAction();
        }
    }
}
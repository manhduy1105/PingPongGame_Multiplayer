using Fusion;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Networked] public int LeftScore { get; set; }
    [Networked] public int RightScore { get; set; }

    [SerializeField] private TextMeshProUGUI leftText;
    [SerializeField] private TextMeshProUGUI rightText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winText;

    private const int WIN_SCORE = 15;

    public override void Render()
    {
        // update UI cho tất cả client
        leftText.text = LeftScore.ToString();
        rightText.text = RightScore.ToString();
    }

    public void AddScoreLeft()
    {
        if (!Object.HasStateAuthority) return;

        LeftScore++;

        CheckWin();
    }

    public void AddScoreRight()
    {
        if (!Object.HasStateAuthority) return;

        RightScore++;

        CheckWin();
    }

    private void CheckWin()
    {
        if (LeftScore >= WIN_SCORE)
        {
            RPC_ShowWin("LEFT PLAYER WIN");
        }
        else if (RightScore >= WIN_SCORE)
        {
            RPC_ShowWin("RIGHT PLAYER WIN");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowWin(string message)
    {
        winPanel.SetActive(true);
        winText.text = message;
        Time.timeScale = 0f; // pause local
    }
}
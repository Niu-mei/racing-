using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public List<AIController> aiCars = new List<AIController>();

    public void RegisterAI(AIController ai)
    {
        if (!aiCars.Contains(ai))
            aiCars.Add(ai);
    }

    public void UpdateRankings()
    {
        aiCars.Sort((a, b) =>
        {
            if (a == null || b == null)
                return 0;
            return b.GetApproxProgress().CompareTo(a.GetApproxProgress());
        });

        for (int i = 0; i < aiCars.Count; i++)
        {
            aiCars[i].currentRank = i + 1;
        }
    }

    private void Update()
    {
        UpdateRankings();
    }
}

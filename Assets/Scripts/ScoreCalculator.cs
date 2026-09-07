using System.Collections.Generic;

public interface IScoreRule
{
    public virtual int ApplyRule(List<string> heirTokens, List<string> belongingTokens) { return 0; }
}

public readonly struct ScoreRule_ObjectiveCompleted : IScoreRule
{
    public readonly int ApplyRule(List<string> heirTokens, List<string> belongingTokens)
    {
        int bonus = (belongingTokens.Count) * 25;
        return 100 + bonus;
    }
}

public readonly struct ScoreRule_Conjunction : IScoreRule
{
    private readonly List<string> conjunctionTokens;

    public ScoreRule_Conjunction(List<string> conjunctionTokens)
    {
        this.conjunctionTokens = conjunctionTokens;
    }

    public readonly int ApplyRule(List<string> heirTokens, List<string> belongingTokens)
    {
        if (heirTokens.Count < 3) return 0;

        int score = 0;
        int heirIndex = 0;
        while (heirIndex < heirTokens.Count - 2)
        {
            if (conjunctionTokens.Contains(heirTokens[heirIndex + 1]) && heirTokens[heirIndex] != heirTokens[heirIndex + 2])
            {
                score += 75;
            }
            heirIndex += 2;
        }
        return score;
    }
}

public class ScoreCalculator
{
    private readonly List<IScoreRule> rules = new()
    {
        new ScoreRule_ObjectiveCompleted(),
        new ScoreRule_Conjunction(new List<string> { "and", "&" }),
    };

    private readonly List<(List<string> heirTokens, List<string> belongingTokens)> clauses = new();

    public int CalculateTotalScore()
    {
        int totalScore = 0;
        foreach (var (heirTokens, belongingTokens) in clauses)
        {
            totalScore += CalculateClauseScore(heirTokens, belongingTokens);
        }
        return totalScore;
    }

    public void AddClause(List<string> heirTokens, List<string> belongingTokens)
    {
        clauses.Add((heirTokens, belongingTokens));
    }

    public void ClearClauses()
    {
        clauses.Clear();
    }

    private int CalculateClauseScore(List<string> heirTokens, List<string> belongingTokens)
    {
        int clauseScore = 0;
        foreach (var rule in rules)
        {
            clauseScore += rule.ApplyRule(heirTokens, belongingTokens);
        }
        return clauseScore;
    }
}
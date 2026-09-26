using System.Collections.Generic;

public interface IScoreRule
{
    public virtual int ApplyRule(List<string> heirTokens, List<string> belongingTokens) { return 0; }
}

public readonly struct ScoreRule_ObjectiveCompleted : IScoreRule
{
    public readonly int ApplyRule(List<string> heirTokens, List<string> belongingTokens)
    {
        int bonus = belongingTokens.Count * 25;
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
        if (heirTokens.Count < 3 && belongingTokens.Count < 3) return 0;

        int totalScore = 0;
        totalScore += EvaluateConjunctionScore(heirTokens);
        totalScore += EvaluateConjunctionScore(belongingTokens);
        return totalScore;
    }

    private int EvaluateConjunctionScore(List<string> tokens)
    {
        int score = 0;
        int index = 0;
        while (index < tokens.Count - 2)
        {
            if (conjunctionTokens.Contains(tokens[index + 1]) && tokens[index] != tokens[index + 2])
            {
                score += 100;
            }
            index += 2;
        }
        return score;
    }
}

public readonly struct ScoreRule_Punctuation : IScoreRule
{
    private readonly List<string> punctuationTokens;

    public ScoreRule_Punctuation(List<string> punctuationTokens)
    {
        this.punctuationTokens = punctuationTokens;
    }

    public readonly int ApplyRule(List<string> heirTokens, List<string> belongingTokens)
    {
        if (punctuationTokens.Contains(belongingTokens[^1]))
        {
            return 25;
        }
        return 0;
    }
}

public class ScoreCalculator
{
    private readonly List<IScoreRule> rules = new()
    {
        new ScoreRule_ObjectiveCompleted(),
        new ScoreRule_Conjunction(new List<string> { "and", "&" }),
        new ScoreRule_Punctuation(new List<string> { ".", "!", "?", "," }),
    };

    public List<(List<string> heirTokens, List<string> belongingTokens)> Clauses => clauses;

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

    public void AddClauseList(List<(List<string> heirTokens, List<string> belongingTokens)> newClauses)
    {
        clauses.AddRange(newClauses);
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
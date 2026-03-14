namespace psi25_project.Utils;

public static class ScoreCalculator
{
    private const int MAX_SCORE = 5000; 
    private const double BIGGEST_DISTANCE = 5000.0; 
    public static int CalculateGeoScore(double distance)
    {
        double score = MAX_SCORE * Math.Exp(-distance / BIGGEST_DISTANCE);

        if (score < 0) score = 0;
        if (score > MAX_SCORE) score = MAX_SCORE;

        return (int)Math.Round(score);
    }
}

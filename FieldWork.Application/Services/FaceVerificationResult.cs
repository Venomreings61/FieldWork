namespace FieldWork.Application.Services;

public class FaceVerificationResult
{
    public bool Matched { get; set; }
    public double Similarity { get; set; }
    public double Threshold { get; set; }

    // Backward-compatibility aliases if any legacy caller relies on IsMatch / Score
    public bool IsMatch
    {
        get => Matched;
        set => Matched = value;
    }

    public double Score
    {
        get => Similarity;
        set => Similarity = value;
    }
}
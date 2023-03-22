namespace dotty
{
    public class AudioPoints
    {
        public int[][] XValues { get; set; }
        public float[][] YValues { get; set; }

        public AudioPoints(int[][] xPoints, float[][] yPoints) 
        {
            this.XValues = xPoints;
            this.YValues = yPoints;
        }
    }
}
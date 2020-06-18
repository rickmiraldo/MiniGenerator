namespace MiniGenerator.Models
{
    public class ProcessingConfiguration
    {
        public int ResizeFactor { get; }

        public int BorderThickness { get; }

        public ProcessingConfiguration(int resizeFactor, int borderThickness)
        {
            ResizeFactor = resizeFactor;
            BorderThickness = borderThickness;
        }
    }
}

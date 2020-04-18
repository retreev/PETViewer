namespace GUI
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var modelPath = "../test_models/item0_01.pet";
            if (args.Length > 0)
            {
                modelPath = args[0];
            }

            // TODO filepicker/drag'n'drop
            if (modelPath != null)
            {
                using (var window = new Window(800, 600, 60.0, "PETViewer"))
                {
                    window.Run(modelPath);
                }
            }
        }
    }
}

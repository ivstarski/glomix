namespace glomix.console
{
    class Program
    {
        // ReSharper disable once FunctionRecursiveOnAllPaths
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            IBuilder builder = new BuilderImp();
            builder.Menu().Page().Mix()?.BuildAsync();
            Main(null);
        }
    }
}
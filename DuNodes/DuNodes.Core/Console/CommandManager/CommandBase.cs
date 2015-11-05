namespace DuNodes.System.Console.CommandManager
{
    public abstract class CommandBase
    {
        //Method to override
        public abstract void launch(string[] args);

        public abstract void cancelled();


        public abstract void pause();

        public abstract void finished();
        }
}

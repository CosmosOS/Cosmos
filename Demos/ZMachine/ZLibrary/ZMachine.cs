using ZLibrary.Machine;
using ZLibrary.Story;

namespace ZLibrary
{
    public class ZMachine
    {
        public ZStory Story { get; }

        public IZInput Input { get; }
        public IZOutput Output { get; }
        public ZMemory Memory { get; }
        public IZScreen Screen { get; }

        private ZInterpreter mInterpreter { get; set; }

        public ZMachine(byte[] aData)
        {
            Memory = new ZMemory(aData);
            Story = new ZStory(Memory);

            Screen = new ZConsoleScreen(this);
            Input = new ZInput(Story, Screen, Memory);
            Output = new ZOutput(Screen);
        }

        public void Run()
        {
            Memory.Initialize(Story.Header.StartPC);
            mInterpreter = new ZInterpreter(this);

            ZText.Initialize(this);

            mInterpreter.Interpret();
        }

        public void Tick()
        {
            
        }
    }
}

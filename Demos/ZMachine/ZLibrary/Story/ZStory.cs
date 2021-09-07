using System;
using ZLibrary.Constants;
using ZLibrary.Machine;

namespace ZLibrary.Story
{
    public class ZStory
    {
        public ZMemory Memory { get; }
        public ZHeader Header { get; }

        public int FileSize { get; private set; }

        public int StoryScaler { get; private set; }
        public int StorySize { get; private set; }
        public string StoryName { get; private set; }

        public int CodeScaler { get; private set; }

        public ZStory(ZMemory aMemory)
        {
            Memory = aMemory;
            Header = new ZHeader(aMemory);
            Initialize();
        }

        private void Initialize()
        {
            if (Header.Version <= FileVersion.V3)
            {
                StoryScaler = 2;
                CodeScaler = 2;
            }
            else if (Header.Version <= FileVersion.V5)
            {
                StoryScaler = 4;
                CodeScaler = 4;
            }
            else
            {
                throw new ArgumentException("\nFatal: Currently only V1-V5 games are implemented.\n");
            }

            StorySize = Header.FileSize * StoryScaler;
        }
    }
}

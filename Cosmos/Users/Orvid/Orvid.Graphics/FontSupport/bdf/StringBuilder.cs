using System;

namespace Orvid.Graphics.FontSupport.bdf
{
    public class StringBuilder
    {
        /// <summary>
        /// The internal private copy of the string
        /// </summary>
        private char[] _storedString;
        /// <summary>
        /// The number of allocated characters
        /// </summary>
        private int lngAllocated;
        /// <summary>
        /// The number of used characters
        /// </summary>
        private int lngUsed;
        /// <summary>
        /// Number of characters to allocated with an increase
        /// </summary>
        private int lngAllocSize;

        /// <summary>
        /// Get the length of the string
        /// </summary>
        public int Length
        {
            get { return lngUsed; }
        }

        /// <summary>
        /// The default constructor for the class
        /// </summary>
        /// <remarks>Sets the allocation size to 1000</remarks>
        public StringBuilder()
        {
            lngAllocSize = 1000;
            lngAllocated = lngAllocSize;
            _storedString = new char[lngAllocSize];
        }

        /// <summary>
        /// The constructor used to set the initial value of the string
        /// </summary>
        /// <remarks>Sets the allocation size to 1000</remarks>
        public StringBuilder(string InitialValue)
        {
            lngAllocSize = 1000;
            lngAllocated = lngAllocSize;
            _storedString = new char[lngAllocSize];

            Append(InitialValue);
        }

        /// <summary>
        /// Constructor used to define a custom allocation size
        /// </summary>
        /// <param name="AllocationSize">The size of every allocation</param>
        public StringBuilder(int AllocationSize)
        {
            lngAllocSize = AllocationSize;
            lngAllocated = lngAllocSize;
            _storedString = new char[lngAllocSize];
        }

        /// <summary>
        /// The constructor used to set the initial value of the string
        /// </summary>
        /// <param name="InitialValue">The initial value of the string</param>
        /// <param name="AllocationSize">The size of every allocation</param>
        public StringBuilder(string InitialValue, int AllocationSize)
        {
            lngAllocSize = AllocationSize;
            lngAllocated = lngAllocSize;
            _storedString = new char[lngAllocSize];
            Append(InitialValue);
        }

        /// <summary>
        /// Append a string to the internal string
        /// </summary>
        /// <param name="StringToAppend">The string to be appended</param>
        public void Append(string StringToAppend)
        {
            int lngLength;
            int lngToAllocate;

            lngLength = StringToAppend.Length;

            if (lngLength > 0)
            {
                if (lngLength + lngUsed > lngAllocated)
                {
                    throw new Exception();
                    lngToAllocate = lngAllocSize * (1 + (lngUsed + lngLength - lngAllocated) / lngAllocSize);
                    lngAllocated += lngToAllocate;

                    _storedString = ResizeArray(_storedString, lngAllocated);
                }
                int counter = 0;
                foreach (char c in StringToAppend)
                {
                    _storedString[lngUsed + counter] = c;
                    counter++;
                }

                lngUsed += lngLength;
            }

        }

        /// <summary>
        /// Overwrite the string with the specified string
        /// </summary>
        /// <param name="StringToWrite">The new string value of the string</param>
        public void Overwrite(string StringToWrite)
        {
            Clear();
            Append(StringToWrite);
        }

        /// <summary>
        /// Get the string
        /// </summary>
        /// <returns>The string that has been built</returns>
        public override string ToString()
        {
            string returnString = new string(_storedString, 0, lngUsed);
            return returnString;
        }

        /// <summary>
        /// Clear the string from memory
        /// </summary>
        public void Clear()
        {
            _storedString = null;
            lngUsed = 0;
            lngAllocated = lngAllocSize;
            _storedString = new char[lngAllocSize];
        }

        /// <summary>
        /// Resize the internal array
        /// </summary>
        /// <param name="oldArray">The old array to resize</param>
        /// <param name="newSize">The new size of the expanded array</param>
        /// <returns>a New bigger array with all the contents of the old array</returns>
        private char[] ResizeArray(System.Array oldArray, int newSize)
        {
            char[] newArray = new char[newSize];
            Array.Copy(oldArray, newArray, oldArray.Length);

            return newArray;
        }
    }
}

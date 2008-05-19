using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Huffman
{
	/// <summary>
	/// Handles attempt to extract archive that is protected with password,
	/// by using wrong password.
	/// </summary>
	public delegate void WrongPasswordEventHandler();

	/// <summary>
	/// Invoked from all xxxxWithProgress functions whenever another 1 percent
	/// of the function is done.
	/// </summary>
	public delegate void PercentCompletedEventHandler();

	/// <summary>
	/// Implementing the Huffman shrinking algorithm.
	/// This algorithm was ment to be highly fast efficient, it's supports
	/// Data streams with size of up to 2^32 - 1 bytes.
	/// </summary>
	public class HuffmanAlgorithm: IDisposable
	{
		#region Internal classes
		//-------------------------------------------------------------------------------	
		/// <summary>
		/// <c>FrequencyTable</c> build from bytes and their repeatition in the stream.
		/// this is achieved by using  2 arrays with the of same size.
		/// </summary>
		[Serializable]
			internal class FrequencyTable
		{
			/// <summary>
			/// Saves all the varies types of bytes (up to 256 ) found in a stream.
			/// </summary>
			public Byte[] FoundBytes;
			/// <summary>
			/// Saves the amount of times each byte in the stream apears.
			/// </summary>
			public uint[] Frequency;
		}//end of FrequencyTable class
		//-------------------------------------------------------------------------------
		/// <summary>
		/// This is a node that the <c>HuffmanTree</c> made of.
		/// It's used to translate bytes to bits sequence when archiving,
		/// and bits sequence to bytes when extracting.
		/// </summary>
		internal class TreeNode
		{
			#region Members

			public TreeNode
				/// <summary>Pointer to the left son.</summery>
				Lson=null,
				/// <summary>Pointer to the right son.</summery>
				Rson=null,
				///<summery> Pointer to the parent of the node.</summery>
				Parent=null;

			/// <summary>
			/// The Byte value of a leaf, it is relevant only when the node is actualy a leaf.
			/// </summary>
			public Byte ByteValue;

			/// <summary>
			/// This is the frequency value of the node
			/// </summary>
			public ulong Value;

			#endregion
		}//End of TreeNode class
		//-------------------------------------------------------------------------------
		/// <summary>
		/// <c>HuffmanTree</c> is the iplementation of a Huffman algorithm tree.
		/// It's used to translate bytes to bits sequence when archiving,
		/// and bits sequence to bytes when extracting.
		/// </summary>
		internal class HuffmanTree
		{
			#region Members
			/// <summary>
			/// This array hold the value of a byte and it is as long as a frequency table.
			/// </summary>
			public readonly TreeNode[] Leafs;

			/// <summary>The frequency table to build the Huffman tree with.</summary>
			public readonly FrequencyTable FT;

			/// <summary>
			/// This holds nodes without parents;
			/// </summary>
			private ArrayList OrphanNodes= new ArrayList(); 
			/// <summary>
			/// The root node in the tree to be build;
			/// </summary>
			public readonly TreeNode RootNode;

			#endregion

			/// <summary>Build a Huffman tree out of a frequency table.</summary>
			internal HuffmanTree(FrequencyTable FT)
			{
				ushort Length = (ushort)FT.FoundBytes.Length;
				this.FT= FT;
				Leafs = new TreeNode[Length];
				if( Length > 1 )
				{
					for(ushort i=0; i< Length; ++i)
					{
						Leafs[i]=new TreeNode();
						Leafs[i].ByteValue = FT.FoundBytes[i];
						Leafs[i].Value = FT.Frequency[i];
					}
					OrphanNodes.AddRange(Leafs);
					RootNode = BuildTree();
				}
				else
				{//No need to create a tree (only one node below rootnode)
					TreeNode TempNode = new TreeNode();
					TempNode.ByteValue =FT.FoundBytes[0];
					TempNode.Value = FT.Frequency[0];
					RootNode = new TreeNode();
					RootNode.Lson = RootNode.Rson =TempNode;
				}
				OrphanNodes.Clear();
				OrphanNodes=null;
			
			}
			//-------------------------------------------------------------------------------
			/// <summary>
			/// This function build a tree from the frequency table
			/// </summary>
			/// <returns>The root of the tree.</returns>
			private TreeNode BuildTree()
			{  
				TreeNode small, smaller, NewParentNode=null;
				/*stop when the tree is fully build( only one root )*/
				while( OrphanNodes.Count > 1 )
				{
					/*This will return the parent less nodes that thier value togather will
					 *be the smallest one and remove them from the ArrayList*/
					FindSmallestOrphanNodes(out smaller,out  small);
					NewParentNode = new TreeNode();
					NewParentNode.Value = small.Value + smaller.Value;
					NewParentNode.Lson = smaller;
					NewParentNode.Rson = small;
					smaller.Parent = small.Parent = NewParentNode;
					OrphanNodes.Add(NewParentNode);
				}
				//returning the root of the tree (always the last new parent)
				return NewParentNode;
			}
			//-------------------------------------------------------------------------------
			/// <summary>
			/// Finds the smallest and the 2nd smallest value orphan nodes
			/// and removes them them from the arraylist.
			/// </summary>
			/// <param name="Smallest">The smallest node in the <c>OrphanNodes</c> list.</param>
			/// <param name="Small">The 2nd smallest node in the <c>OrphanNodes</c> list.</param>
			private void FindSmallestOrphanNodes(out TreeNode Smallest, out TreeNode Small)
			{
				Smallest = Small = null;
				//Scanning backward
				ulong Tempvalue=18446744073709551614;
				TreeNode TempNode=null;
				int i, j=0;
				int ArrSize = OrphanNodes.Count-1;
				//scanning for the smallest value orphan node
				for(i= ArrSize ; i!=-1 ; --i  )
				{
					TempNode = (TreeNode)OrphanNodes[i];
					if( TempNode.Value < Tempvalue )
					{
						Tempvalue = TempNode.Value;
						Smallest = TempNode;
						j=i;
					}
				}
				OrphanNodes.RemoveAt(j);
				--ArrSize;

				Tempvalue=18446744073709551614;
				//scanning for the second smallest value orphan node
				for(i= ArrSize; i>-1 ; --i  )
				{
					TempNode = (TreeNode)OrphanNodes[i];
					if( TempNode.Value < Tempvalue )
					{
						Tempvalue = TempNode.Value;
						Small = TempNode;
						j=i;
					}
				}
				OrphanNodes.RemoveAt(j);

			}

			//-------------------------------------------------------------------------------
		}//end of HuffmanTree class

		//-------------------------------------------------------------------------------
		/// <summary>
		/// This is a stack of 8 bits (1 byte)
		/// uses to manipulate the bits of a stream(when been extracted or archived).
		/// It's pushing and poping acts more like a queue then a stack.
		/// </summary>
		internal struct BitsStack
		{
			/// <summary>
			/// The unit to write and read from a stream.
			/// </summary>
			public Byte Container;
			private byte Amount;

			/// <summary>
			/// Indicated if the stack unit is full with 8 bits.
			/// </summary>
			/// <returns>true if full, false if not.</returns>
			public bool IsFull()
			{
				return Amount==8;
			}
			/// <summary>
			/// Indicated if the stack unit is empty(0 bits).
			/// </summary>
			/// <returns>true if empty, false if not.</returns>
			public bool IsEmpty()
			{
				return Amount == 0;
			}
			/// <summary>
			/// Get the number of bits, currently located in the stack.
			/// </summary>
			/// <returns>Number of bits located in the stack.</returns>
			public Byte NumOfBits()
			{
				return Amount;
			}
			
			/// <summary>
			/// This function removes all the bits from the stack.
			/// </summary>
			public void Empty(){Amount=Container=0;}
			
			/// <summary>
			/// Push a bit to the left of the stack (Most significant bit).
			/// </summary>
			/// <remarks>The stack must have at least 1 free bit slot.</remarks>
			/// <param name="Flag">The bit to add the stack(true = 1, false = 0)</param>
			/// <exception cref="Exception">
			/// When attempting to push a bit from a full stack.
			/// </exception>
			public void PushFlag(bool Flag)
			{
				if( Amount== 8)throw new Exception("Stack is full");
				Container>>=1;
				if( Flag )Container|=128;
				++Amount;
			}
			
			/// <summary>
			/// Pops a bit from the right of the stack (Least significant bit).
			/// </summary>
			/// <returns></returns>
			/// <remarks>The stack must'nt be empty this function called.</remarks>
			/// <exception cref="Exception">
			/// When attempting to pop a bit from an empty stack.
			/// </exception>
			public bool PopFlag()
			{
				if( Amount == 0)throw new Exception("Stack is empty");
				bool t= (Container & 1)!=0;
				--Amount;
				Container>>=1;
				return t;
			}
			
			/// <summary>
			/// Fill the stack with 8 bits. If the stack is full, the given byte will
			/// override the old bits.
			/// </summary>
			/// <param name="Data">Byte(8 bits) to put in the current stack.</param>
			public void FillStack(Byte Data)
			{
				Container = Data;
				Amount=8;
			}

		}

		//-------------------------------------------------------------------------------		
		/// <summary>
		/// This is the file/stream header that attached to each archived file or stream at the begining.
		/// </summary>
		[Serializable]
			internal class FileHeader
		{

			/// <summary>The version of the archiving code.</summary>
			public readonly byte version;
			
			/// <summary>The frequency table of the archived data.</summary>
			public readonly FrequencyTable FT;
			
			/// <summary>The size of the data before archiving it.</summary>
			public readonly long OriginalSize;
			
			/// <summary>Number of extra bits added to the last  byte of the data.</summary>
			public readonly byte ComplementsBits;

			/// <summary>Security key to the archived stream\file.</summary>
			public readonly ushort Key;

			/// <summary>
			/// Builds a new header that holds info about an archived file\stream.
			/// </summary>
			/// <param name="ver">The version of the archiving program.</param>
			/// <param name="T">The frequency table to rebuild the file from.</param>
			/// <param name="OrgSize">The size of the file\stream before archiving it.</param>
			/// <param name="BitsToFill">
			/// Number of extra bits added to the last byte in the archived file.
			/// </param>
			/// <param name="PasswordKey">Key to gain access to the file\stream later.</param>
			public FileHeader(Byte ver, FrequencyTable T, ref long OrgSize, 
				byte BitsToFill, ushort PasswordKey)
			{
				version=ver; FT=T; OriginalSize=OrgSize; ComplementsBits=BitsToFill;
				Key = PasswordKey;
			}
		}
		#endregion
		//-------------------------------------------------------------------------------
		#region Members
		//-------------------------------------------------------------------------------
		/// <summary>
		/// This is a temporary array to sign  where it's location in the 
		/// <c>BuildFrequencyTable</c> function (the value is the location.
		/// </summary>
		private Byte[] ByteLocation = new Byte[256];
		/// <summary>
		/// This array indicated if the byte with the value that correspond
		/// to the index of the array (0-255) was found or not in the stream.
		/// </summary>
		private bool[] IsByteExist;

		/// <summary>Holds the bytes that where found.</summary>
		private ArrayList BytesList = new ArrayList();

		/// <summary>Holds the amount of repetitions of byte.</summary>
		private ArrayList AmountList = new ArrayList();

		/// <summary>I use this list to write the reverse path to a Byte.</summary>
		private ArrayList BitsList = new ArrayList();
		
		/// <summary>Uses to write and read the Headers to and from a stream.</summary>
		private BinaryFormatter BinFormat = new BinaryFormatter();

		/// <summary>This stack is used to write extracted and shrinked bytes.</summary>
		private BitsStack Stack = new BitsStack();

		/// <summary>
		/// Invoked whenever attempt to extract password protected file\stream, by
		/// using the wrong password. In case this event isn't handaled by the users
		/// an exeption will be thrown(in password error case).
		/// </summary>
		private WrongPasswordEventHandler OnWrongPassword;

		/// <summary>
		/// Invoked from all xxxxWithProgress functions whenever another 1 percent
		/// of the function is done.
		/// </summary>
		private PercentCompletedEventHandler OnPercentCompleted;

		#endregion
		//-------------------------------------------------------------------------------
		#region Public Functions
		//-------------------------------------------------------------------------------
		/// <summary>
		/// Build a frequency table and Huffman tree and shrinking the stream data.
		/// </summary>
		/// <param name="Data">
		/// The data streem to shrink, it will start shrinking from the position of the given
		/// stream as it was given and in the end of the function it's position
		/// won't be at the end of the stream and it won't be closed.
		/// </param>
		/// <param name="Password">
		/// A password to add to the archive, to mark as "password less" assign null instead.
		/// </param>
		/// <returns>The archived stream, positioned at start.</returns>
		/// <remarks>
		/// The given stream must be readable, seekable and it's length
		/// must exceed zero.
		/// </remarks>
		public Stream Shrink(Stream Data, char[] Password)
		{  
			//Tempdirectory
			String TempDir=Environment.GetEnvironmentVariable("temp");

			//Generating the header data from the stream and creating a HuffmanTree
			HuffmanTree HT = new HuffmanTree( BuildFrequencyTable(Data) );
			//Creating temporary file
			FileStream tempFS= new FileStream( TempDir + @"\TempArch.tmp", FileMode.Create);
			//Writing  header
			WriteHeader(tempFS, HT.FT, Data.Length, 11, GetComplementsBits(HT), Password );

			long DataSize= Data.Length;
			TreeNode TempNode=null; 
			Byte Original; //the byte we read from the original stream

			short j; int k;
			for(long i=0;i< DataSize; ++i) 
			{
				Original = (Byte)Data.ReadByte();
				TempNode = HT.Leafs[ ByteLocation[Original] ];
				while( TempNode.Parent!=null ) 
				{
					//If I'm left sone of my parent push 1 else push 0
					BitsList.Add(TempNode.Parent.Lson == TempNode);
					TempNode = TempNode.Parent;//Climb up the tree.
				}
				BitsList.Reverse();
				k = BitsList.Count;
				for(j=0; j<k; ++j ) 
				{
					Stack.PushFlag( (bool)BitsList[j] );
					if(Stack.IsFull())
					{
						tempFS.WriteByte(Stack.Container);
						Stack.Empty();
					}
				}
				BitsList.Clear();
			}
			
			//Writing the last byte if the stack wasn't compleatly full.
			if( !Stack.IsEmpty() ) 
			{
				Byte BitsToComplete = (Byte)(8 - Stack.NumOfBits());
				for(byte Count=0; Count< BitsToComplete; ++Count)//complete to full 8 bits
					Stack.PushFlag(false);
				tempFS.WriteByte(Stack.Container);
				Stack.Empty();
			}
			
			tempFS.Seek( 0, SeekOrigin.Begin );
			return tempFS;
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Build a frequency table and Huffman tree and shrinking the stream data.
		/// This function version, calls the PercentComplete event handler
		/// When anothe 1 percent compleated.
		/// </summary>
		/// <param name="Data">
		/// The data streem to shrink, shrinking starts from the position of the given stream
		/// as it was given and in the end of the function it's position won't be at the end
		/// of the stream and it won't be closed.
		/// </param>
		/// <param name="Password">
		/// A password to add to the archive, to mark as "password less" assign null instead.
		/// </param>
		/// <returns>The archived stream, positioned at start.</returns>
		/// <remarks>
		/// The given stream must be readable, seekable and it's length
		/// must exceed zero.
		/// </remarks>
		public Stream ShrinkWithProgress(Stream Data, char[] Password)
		{  
			//Tempdirectory
			String TempDir=Environment.GetEnvironmentVariable("temp");
			//Generating the header data from the stream and creating a HuffmanTree
			HuffmanTree HT = new HuffmanTree( BuildFrequencyTable(Data) );
			//Creating temporary file
			FileStream tempFS= new FileStream( TempDir + @"\TempArch.tmp", FileMode.Create);
			//Writing  header
			WriteHeader(tempFS, HT.FT, Data.Length, 11, GetComplementsBits(HT), Password );

			long DataSize= Data.Length;
			TreeNode TempNode=null; 
			Byte Original; //the byte we read from the original stream

			short j; int k; float ProgressRatio = 0;
			for(long i=0;i< DataSize; ++i) 
			{
				Original = (Byte)Data.ReadByte();
				TempNode = HT.Leafs[ ByteLocation[Original] ];
				while( TempNode.Parent!=null )
				{
					//If I'm left sone of my parent push 1 else push 0
					BitsList.Add(TempNode.Parent.Lson == TempNode);
					TempNode = TempNode.Parent;//Climb up the tree.
				}
				BitsList.Reverse();
				k = BitsList.Count;
				for(j=0; j<k; ++j )
				{
					Stack.PushFlag( (bool)BitsList[j] );
					if( Stack.IsFull() )
					{
						tempFS.WriteByte(Stack.Container);
						Stack.Empty();
					}
				}
				BitsList.Clear();

				if( ((float)(i))/DataSize - ProgressRatio > 0.01 )
				{
					ProgressRatio+=0.01f;
					if( OnPercentCompleted!=null ) OnPercentCompleted();
				}
			}
			
			//Writing the last byte if the stack wasn't compleatly full.
			if( !Stack.IsEmpty() ) 
			{
				Byte BitsToComplete = (Byte)(8 - Stack.NumOfBits());
				for(byte Count=0; Count< BitsToComplete; ++Count)//complete to full 8 bits
					Stack.PushFlag(false);
				tempFS.WriteByte(Stack.Container);
				Stack.Empty();
			}
			tempFS.Seek( 0, SeekOrigin.Begin );
			return tempFS;
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Build a frequency table and Huffman tree and shrinking the stream data to a new file.
		/// into a file.
		/// </summary>
		/// <param name="Data">
		/// The data streem to shrink, it will start shrinking from the position of the given
		/// stream as it was given and in the end of the function it's position
		/// won't be at the end of the stream and it won't be closed.
		/// </param>
		/// <param name="OutputFile">Path to a file to same the shrinked data in.</param>
		/// <param name="Password">
		/// A passward to add to the archive, to mark as "passward less" assign null instead.
		/// </param>
		/// <returns>The expanded stream, positioned at start.</returns>
		/// <remarks>
		/// The given stream must be readable, seekable and it's length
		/// must exceed zero.
		/// </remarks>
		public void Shrink(Stream Data, string OutputFile, char [] Password)
		{  
			//Generating the header data from the stream and creating a HuffmanTree
			HuffmanTree HT = new HuffmanTree( BuildFrequencyTable(Data) );
			//Creating temporary file
			FileStream tempFS= new FileStream( OutputFile , FileMode.Create);
			//Writing  header
			WriteHeader(tempFS, HT.FT, Data.Length, 11, GetComplementsBits(HT), Password);
			long DataSize= Data.Length;
			TreeNode TempNode=null; 
			Byte Original; //the byte we read from the original stream
			
			short j; int k;
			for(long i=0;i< DataSize; ++i) 
			{
				Original = (Byte)Data.ReadByte();
				TempNode = HT.Leafs[ ByteLocation[Original] ];
				while( TempNode.Parent!=null ) 
				{
					//If I'm left sone of my parent push 1 else push 0
					BitsList.Add(TempNode.Parent.Lson == TempNode);
					TempNode = TempNode.Parent;//Climb up the tree.
				}
				BitsList.Reverse();
				k = BitsList.Count;
				for(j=0; j<k; ++j ) 
				{
					Stack.PushFlag( (bool)BitsList[j] );
					if(Stack.IsFull())
					{
						tempFS.WriteByte(Stack.Container);
						Stack.Empty();
					}
				}
				BitsList.Clear();
			}
			
			//Writing the last byte if the stack wasn't compleatly full.
			if( !Stack.IsEmpty() ) 
			{
				Byte BitsToComplete = (Byte)(8 - Stack.NumOfBits());
				for(byte Count=0; Count< BitsToComplete; ++Count)//complete to full 8 bits
					Stack.PushFlag(false);
				tempFS.WriteByte(Stack.Container);
				Stack.Empty();
			}
			tempFS.Seek( 0, SeekOrigin.Begin );
			tempFS.Close();
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Build a frequency table and Huffman tree and shrinking the stream data to a new file.
		/// into a file.
		/// This function version, calls the PercentComplete event handler
		/// When anothe 1 percent compleated.
		/// </summary>
		/// <param name="Data">
		/// The data streem to shrink, shrinking starts from the position of the given stream
		/// as it was given and in the end of the function it's position won't be at the end
		/// of the stream and it won't be closed.
		/// </param>
		/// <param name="OutputFile">Path to a file to same the shrinked data in.</param>
		/// <param name="Password">
		/// A passward to add to the archive, to mark as "passward less" assign null instead.
		/// </param>
		/// <returns>The expanded stream, positioned at the start.</returns>
		/// <remarks>
		/// The given stream must be readable, seekable and it's length
		/// must exceed zero.
		/// </remarks>
		public void ShrinkWithProgress(Stream Data, string OutputFile, char [] Password)
		{  
			//Generating the header data from the stream and creating a HuffmanTree
			HuffmanTree HT = new HuffmanTree( BuildFrequencyTable(Data) );
			//Creating temporary file
			FileStream tempFS= new FileStream( OutputFile , FileMode.Create);
			//Writing  header 
			WriteHeader(tempFS, HT.FT, Data.Length, 11, GetComplementsBits(HT), Password);

			long DataSize= Data.Length;
			TreeNode TempNode=null; 
			Byte Original; //the byte we read from the original stream
			
			short j; int k; float ProgressRatio = 0;
			for(long i=0;i< DataSize; ++i) 
			{
				Original = (Byte)Data.ReadByte();
				TempNode = HT.Leafs[ ByteLocation[Original] ];
				while( TempNode.Parent!=null )
				{
					//If I'm left sone of my parent push 1 else push 0
					BitsList.Add(TempNode.Parent.Lson == TempNode);
					TempNode = TempNode.Parent;//Climb up the tree.
				}
				BitsList.Reverse();
				k = BitsList.Count;
				for(j=0; j<k; ++j )
				{
					Stack.PushFlag( (bool)BitsList[j] );
					if(Stack.IsFull())
					{
						tempFS.WriteByte(Stack.Container);
						Stack.Empty();
					}
				}
				BitsList.Clear();

				if( ((float)(i))/DataSize - ProgressRatio > 0.01 ) 
				{
					ProgressRatio+=0.01f;
					if( OnPercentCompleted!=null ) OnPercentCompleted();
				}
			}
			
			//Writing the last byte if the stack wasn't compleatly full.
			if( !Stack.IsEmpty() )
			{
				Byte BitsToComplete = (Byte)(8 - Stack.NumOfBits());
				for(byte Count=0; Count< BitsToComplete; ++Count)//complete to full 8 bits
					Stack.PushFlag(false);
				tempFS.WriteByte(Stack.Container);
				Stack.Empty();
			}
			
			tempFS.Seek( 0, SeekOrigin.Begin );
			tempFS.Close();
		}

		//-------------------------------------------------------------------------------

		/// <summary>
		/// Build a frequency table and Huffman tree and extract the archive.
		/// </summary>
		/// <param name="Data">The data streem to shrink</param>
		/// <param name="Password">
		/// A Key to open the archive with, to mark as "passward less" assign null instead.
		/// </param>
		/// <returns>
		/// The data stream to extract, but if wrong password error occur
		/// and the <c>WrongPassword</c> event is handeled, it's returns null.
		/// </returns>
		/// <exception cref="Exception">
		/// On attempt to extract data that wasn't coded with the <c>HuffmanAlgorithm</c> class, 
		/// Or on attempt to extract a password protected stream\file with the wrong password.
		/// (If the <c>WrongPassword</c> event is been handeled by the user the 2nd exception is'nt
		/// relevant).
		/// </exception>
		/// <remarks>
		/// The given stream must be readable, seekable and it's length
		/// must exceed zero. The given stream must be archived stream of the right type.
		/// </remarks>
		public Stream Extract(Stream Data, char[] Password)
		{
			Data.Seek(0, SeekOrigin.Begin);
			//Tempdirectory
			String TempDir=Environment.GetEnvironmentVariable("temp");
			FileHeader Header;
			//Reading the header data from the stream
			
			if( !IsArchivedStream(Data) ) throw new Exception("The given stream is't Huffmans algorithm archive type.");
			Header = (FileHeader)BinFormat.Deserialize(Data);

			//If the stream\file protected and password doesn't fit
			if(Header.Key !=0 &&  Header.Key != PasswordGen( Password )) 
			{
				//Invoke if user handles it
				if(OnWrongPassword!=null)
				{
					new Thread( new ThreadStart(OnWrongPassword)).Start();
					return null;
				}
				else throw new Exception("Wrong password error, on attempt to extract protected data.");
			}

			//Gernerating Huffman tree out of the frequency table in the header
			HuffmanTree HT = new HuffmanTree( Header.FT );
			//Creating temporary file
			FileStream tempFS= new FileStream( TempDir + @"\TempArch.tmp", FileMode.Create);
			BitsStack Stack = new BitsStack();
			long DataSize= Data.Length - Data.Position;
			if( Header.ComplementsBits==0 )DataSize+=1;
			TreeNode TempNode=null; 

			while( true )
			{
				TempNode = HT.RootNode;

				//As long it's not a leaf, go down the tree
				while(TempNode.Lson!=null && TempNode.Rson!=null)
				{
					//If the stack is empty refill it.
					if( Stack.IsEmpty() )
					{
						Stack.FillStack( (Byte)Data.ReadByte() );
						if( (--DataSize) == 0 ) 
						{
							goto AlmostDone;
						}
					}
					//Going left or right according to the bit
					TempNode = Stack.PopFlag() ?  TempNode.Lson : TempNode.Rson;
				}
				//By now reached for a leaf and writes it's data.
				tempFS.WriteByte( TempNode.ByteValue );
			}//end of while

			//To this lable u can jump only from the while loop (only one byte left).
			AlmostDone:

				short BitsLeft = (Byte)( Stack.NumOfBits() - Header.ComplementsBits);
			
			//Writing the rest of the last byte.
			if(BitsLeft != 8 )
			{
				bool Test = TempNode.Lson==null && TempNode.Rson==null;
				while( BitsLeft > 0)
				{
					//If at itteration, TempNode not done going down the huffman tree.
					if( Test ) TempNode = HT.RootNode;
					while(TempNode.Lson!=null && TempNode.Rson!=null)
					{
						//Going left or right according to the bit
						TempNode = Stack.PopFlag() ? TempNode.Lson : TempNode.Rson;
						--BitsLeft;
					}
					//By now reached for a leaf and writes it's data.
					tempFS.WriteByte( TempNode.ByteValue );
					Test=true;
				}
			}
			tempFS.Seek( 0, SeekOrigin.Begin );
			return tempFS;
		}

		//-------------------------------------------------------------------------------

		/// <summary>
		/// Build a frequency table and Huffman tree and extract the archive.
		/// This function version, calls the PercentComplete event handler
		/// When anothe 1 percent compleated.
		/// </summary>
		/// <param name="Data">The data streem to shrink</param>
		/// <param name="Password">
		/// A Key to open the archive with, to mark as "passward less" assign null instead.
		/// </param>
		/// <returns>
		/// The data stream to extract, but if wrong password error occur
		/// and the <c>WrongPassword</c> event is handeled, it's returns null.
		/// </returns>
		/// <exception cref="Exception">
		/// On attempt to extract data that wasn't coded with the <c>HuffmanAlgorithm</c> class, 
		/// Or on attempt to extract a password protected stream\file with the wrong password.
		/// (If the <c>WrongPassword</c> event is been handeled by the user the 2nd exception is'nt
		/// relevant).
		/// </exception>
		/// <remarks>
		/// The given stream must be readable, seekable and it's length
		/// must exceed zero. The given stream must be archived stream of the right type.
		/// </remarks>
		public Stream ExtractWithProgress(Stream Data, char[] Password)
		{
			Data.Seek(0, SeekOrigin.Begin);
			//Tempdirectory
			String TempDir=Environment.GetEnvironmentVariable("temp");
			FileHeader Header;
			//Reading the header data from the stream
			
			if( !IsArchivedStream(Data) ) throw new Exception("The given stream is't Huffmans algorithm archive type.");
			Header = (FileHeader)BinFormat.Deserialize(Data);

			//If the stream\file protected and password doesn't fit
			if(Header.Key !=0 &&  Header.Key != PasswordGen( Password )) 
			{
				//Invoke if user handles it
				if(OnWrongPassword!=null)
				{
					new Thread( new ThreadStart(OnWrongPassword)).Start();
					return null;
				}
				else throw new Exception("Wrong password error, on attempt to extract protected data.");
			}

			//Gernerating Huffman tree out of the frequency table in the header
			HuffmanTree HT = new HuffmanTree( Header.FT );
			//Creating temporary file
			FileStream tempFS= new FileStream( TempDir + @"\TempArch.tmp", FileMode.Create);
			BitsStack Stack = new BitsStack();
			long DataSize= Data.Length - Data.Position;
			if( Header.ComplementsBits==0 )DataSize+=1;
			TreeNode TempNode=null; 
			long DataSize2 = DataSize; float ProgressRatio=0;//Needed to calculate progress.

			while( true )
			{
				TempNode = HT.RootNode;

				//As long it's not a leaf, go down the tree
				while(TempNode.Lson!=null && TempNode.Rson!=null)
				{
					//If the stack is empty refill it.
					if( Stack.IsEmpty() )
					{
						Stack.FillStack( (Byte)Data.ReadByte() );
						if( (--DataSize) == 0 ) 
						{
							goto AlmostDone;
						}
					}
					//Going left or right according to the bit
					TempNode = Stack.PopFlag() ?  TempNode.Lson : TempNode.Rson;
				}
				//By now reached for a leaf and writes it's data.
				tempFS.WriteByte( TempNode.ByteValue );

				if( ((float)(DataSize2-DataSize))/DataSize2 - ProgressRatio > 0.01 )
				{
					ProgressRatio+=0.01f;
					if( OnPercentCompleted!=null ) OnPercentCompleted();
				}
			}//end of while

			//To this lable u can jump only from the while loop (only one byte left).
			AlmostDone:

				short BitsLeft = (Byte)( Stack.NumOfBits() - Header.ComplementsBits);
			
			//Writing the rest of the last byte.
			if(BitsLeft != 8 )
			{
				bool Test = TempNode.Lson==null && TempNode.Rson==null;
				while( BitsLeft > 0)
				{
					//If at itteration, TempNode not done going down the huffman tree.
					if( Test ) TempNode = HT.RootNode;
					while(TempNode.Lson!=null && TempNode.Rson!=null)
					{
						//Going left or right according to the bit
						TempNode = Stack.PopFlag() ? TempNode.Lson : TempNode.Rson;
						--BitsLeft;
					}
					//By now reached for a leaf and writes it's data.
					tempFS.WriteByte( TempNode.ByteValue );
					Test=true;
				}
			}
			
			tempFS.Seek( 0, SeekOrigin.Begin );
			return tempFS;
		}

		//-------------------------------------------------------------------------------

		/// <summary>
		/// Build a frequency table and Huffman tree and extract the archive to a new file.
		/// </summary>
		/// <param name="Data">The data streem to shrink</param>
		/// <param name="OutputFile">Path to to save the extracted data.</param>
		/// <param name="Password">
		/// A Key to open the archive with, to mark as "passward less" assign null instead.
		/// </param>
		/// <returns>
		/// flag that indicates if extraction went well or not: true = successful,
		/// false = wrong password error occured (the WrongPassword event will take place). 
		/// </returns>
		/// <exception cref="Exception">
		/// On attempt to extract data that wasn't coded with the <c>HuffmanAlgorithm</c> class, 
		/// Or on attempt to extract a password protected stream\file with the wrong password.
		/// (If the <c>WrongPassword</c> event is been handeled by the user the 2nd exception is'nt
		/// relevant).
		/// </exception>
		/// <remarks>
		/// The given stream must be readable, seekable and it's length
		/// must exceed zero. The given stream must be archived stream of the right type.
		/// </remarks>
		public bool Extract(Stream Data, string OutputFile, char[] Password)
		{
			Data.Seek(0, SeekOrigin.Begin);
			FileHeader Header;

			//Reading the header data from the stream
			if( !IsArchivedStream(Data) ) throw new Exception("The given stream is't my Huffman algorithm type.");
			Header = (FileHeader)BinFormat.Deserialize(Data);

			//If the stream\file protected and password doesn't fit
			if(Header.Key !=0 &&  Header.Key != PasswordGen( Password )) 
			{
				//Invoke if user handles it
				if(OnWrongPassword!=null)
				{
					new Thread( new ThreadStart(OnWrongPassword)).Start();
					return false;
				}
				else throw new Exception("Wrong password error, on attempt to extract protected data.");
			}
			
			//Gernerating Huffman tree out of the frequency table in the header
			HuffmanTree HT = new HuffmanTree( Header.FT );
			//Creating temporary file
			FileStream tempFS= new FileStream(OutputFile, FileMode.Create);
			BitsStack Stack = new BitsStack();
			long DataSize= Data.Length - Data.Position;
			if( Header.ComplementsBits==0 )DataSize+=1;
			TreeNode TempNode=null; 

			while( true )
			{
				TempNode = HT.RootNode;

				//As long it's not a leaf, go down the tree
				while(TempNode.Lson!=null && TempNode.Rson!=null)
				{
					//If the stack is empty refill it.
					if( Stack.IsEmpty() )
					{
						Stack.FillStack( (Byte)Data.ReadByte() );
						if( (--DataSize) == 0 )
						{
							goto AlmostDone;
						}
					}
					//Going left or right according to the bit
					TempNode = Stack.PopFlag() ?  TempNode.Lson : TempNode.Rson;
				}
				//By now reached for a leaf and writes it's data.
				tempFS.WriteByte( TempNode.ByteValue );
			}//end of while

			//To this lable u can jump only from the while loop (only one byte left).
			AlmostDone:

				short BitsLeft = (Byte)( Stack.NumOfBits() - Header.ComplementsBits);
			
			//Writing the rest of the last byte.
			if(BitsLeft != 8 )
			{
				bool Test = TempNode.Lson==null && TempNode.Rson==null;
				while( BitsLeft > 0) 
				{
					//If at itteration, TempNode not done going down the huffman tree.
					if( Test ) TempNode = HT.RootNode;
					while(TempNode.Lson!=null && TempNode.Rson!=null)
					{
						//Going left or right according to the bit
						TempNode = Stack.PopFlag() ? TempNode.Lson : TempNode.Rson;
						--BitsLeft;
					}
					//By now reached for a leaf and writes it's data.
					tempFS.WriteByte( TempNode.ByteValue );
					Test=true;
				}
			}
			
			tempFS.Close();
			return true;
		}

		//-------------------------------------------------------------------------------

		/// <summary>
		/// Build a frequency table and Huffman tree and extract the archive to a new file.
		/// This function version, calls the PercentComplete event handler
		/// When anothe 1 percent compleated.
		/// </summary>
		/// <param name="Data">The data streem to shrink</param>
		/// <param name="OutputFile">Path to to save the extracted data.</param>
		/// <param name="Password">
		/// A Key to open the archive with, to mark as "passward less" assign null instead.
		/// </param>
		/// <returns>
		/// flag that indicates if extraction went well or not: true = successful,
		/// false = wrong password error occured (the WrongPassword event will take place). 
		/// </returns>
		/// <exception cref="Exception">
		/// On attempt to extract data that wasn't coded with the <c>HuffmanAlgorithm</c> class, 
		/// Or on attempt to extract a password protected stream\file with the wrong password.
		/// (If the <c>WrongPassword</c> event is been handeled by the user the 2nd exception is'nt
		/// relevant).
		/// </exception>
		/// <remarks>
		/// The given stream must be readable, seekable and it's length
		/// must exceed zero. The given stream must be archived stream of the right type.
		/// </remarks>
		public bool ExtractWithProgress(Stream Data, string OutputFile, char[] Password)
		{
			Data.Seek(0, SeekOrigin.Begin);
			FileHeader Header;

			//Reading the header data from the stream
			if( !IsArchivedStream(Data) ) throw new Exception("The given stream is't my Huffman algorithm type.");
			Header = (FileHeader)BinFormat.Deserialize(Data);

			//If the stream\file protected and password doesn't fit
			if(Header.Key !=0 &&  Header.Key != PasswordGen( Password )) 
			{
				//Invoke if user handles it
				if(OnWrongPassword!=null)
				{
					new Thread( new ThreadStart(OnWrongPassword)).Start();
					return false;
				}
				else throw new Exception("Wrong password error, on attempt to extract protected data.");
			}
			
			//Gernerating Huffman tree out of the frequency table in the header
			HuffmanTree HT = new HuffmanTree( Header.FT );
			//Creating temporary file
			FileStream tempFS= new FileStream(OutputFile, FileMode.Create);
			BitsStack Stack = new BitsStack();
			long DataSize= Data.Length - Data.Position;

			if( Header.ComplementsBits==0 )DataSize+=1;
			TreeNode TempNode=null; 
			long DataSize2 = DataSize; float ProgressRatio=0;//Needed to calculate progress.

			while( true )
			{
				TempNode = HT.RootNode;
				//As long it's not a leaf, go down the tree
				while(TempNode.Lson!=null && TempNode.Rson!=null)
				{
					//If the stack is empty refill it.
					if( Stack.IsEmpty() )
					{
						Stack.FillStack( (Byte)Data.ReadByte() );
						if( (--DataSize) == 0 )
							goto AlmostDone;
					}
					//Going left or right according to the bit
					TempNode = Stack.PopFlag() ?  TempNode.Lson : TempNode.Rson;
				}
				//By now reached for a leaf and writes it's data.
				tempFS.WriteByte( TempNode.ByteValue );

				if( ((float)(DataSize2-DataSize))/DataSize2 - ProgressRatio > 0.01 )
				{
					ProgressRatio+=0.01f;
					if( OnPercentCompleted!=null ) OnPercentCompleted();
				}

			}//end of while

			//To this lable u can jump only from the while loop (only one byte left).
			AlmostDone:

				short BitsLeft = (Byte)( Stack.NumOfBits() - Header.ComplementsBits);
			
			//Writing the rest of the last byte.
			if(BitsLeft != 8 )
			{
				bool Test = TempNode.Lson==null && TempNode.Rson==null;
				while( BitsLeft > 0) 
				{
					//If at itteration, TempNode not done going down the huffman tree.
					if( Test ) TempNode = HT.RootNode;
					while(TempNode.Lson!=null && TempNode.Rson!=null)
					{
						//Going left or right according to the bit
						TempNode = Stack.PopFlag() ? TempNode.Lson : TempNode.Rson;
						--BitsLeft;
					}
					//By now reached for a leaf and writes it's data.
					tempFS.WriteByte( TempNode.ByteValue );
					Test=true;
				}
			}
			
			tempFS.Close();
			return true;
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Checks if a data stream is archived.
		/// </summary>
		/// <param name="Data">The stream to test.</param>
		/// <returns>true if the stream is archive, false if not.</returns>
		public bool IsArchivedStream(Stream Data)
		{	
			Data.Seek(0, SeekOrigin.Begin);
			bool test = true;
			try
			{
				FileHeader Header = (FileHeader)BinFormat.Deserialize(Data);
				Header=null;
			}
			catch(Exception) 
			{
				//if header wasn't found
				test = false;
			}
			finally 
			{
				Data.Seek(0, SeekOrigin.Begin);
			}
			return test;
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Checks if a given archived data stream is password protected.
		/// </summary>
		/// <param name="Data">Archived stream to test.</param>
		/// <returns>true if the stream is password protected, false if not.</returns>
		/// <exception cref="Exception">
		/// When the given stream isn't correct Huffman archived stream or has been corrupted.
		/// </exception>
		public bool IsPasswardProtectedStream(Stream Data)
		{	
			Data.Seek(0, SeekOrigin.Begin);
			bool test = true;
			try
			{
				FileHeader Header = (FileHeader)BinFormat.Deserialize(Data);
				test = (Header.Key != 0);
				Header=null;
			}
			catch(Exception)
			{
				//if header wasn't found
				throw new Exception("Error, the given stream isn't Huffman archived or corrupted.");
			}
			finally
			{
				Data.Seek(0, SeekOrigin.Begin);
			}
			return test;
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// This function calculates the the archiving ratio of a given archived stream. 
		/// </summary>
		/// <param name="Data">Archived stream to calculate archiving ratio to.</param>
		/// <returns>The archiving ratio.</returns>
		/// <exception cref="Exception">
		/// When the given stream isn't correct Huffman archived stream or has been corrupted.
		/// </exception>
		public float GetArchivingRatio(Stream Data)
		{	
			Data.Seek(0, SeekOrigin.Begin);
			float Result;
			try
			{
				FileHeader Header = (FileHeader)BinFormat.Deserialize(Data);
				Result = (100f*Data.Length)/Header.OriginalSize;
				Header=null;
			}
			catch(Exception)
			{
				//if header wasn't found
				throw new Exception("Error, the given stream isn't Huffman archived or corrupted.");
			}
			finally
			{
				Data.Seek(0, SeekOrigin.Begin);
			}
			return Result;
		}

		#endregion
		//-------------------------------------------------------------------------------
		#region Public events
		/// <summary>
		/// This is Asynchronic event and accures when the <c>Extract</c> function returns
		/// on wrong password error.
		/// Invoked whenever attempt to extract password protected file\stream, by
		/// using the wrong password(Fatal error). In case this event isn't handaled by the users
		/// an exeption will be thrown(in password error case).
		/// </summary>
		[Description("Invoked whenever attempt to extract password protected file\\stream " +
			 "by using the wrong password. In case this event isn't handaled by the users "+
			 "an exeption will be thrown(in password error case).")
		]
		[Category("Behavior")]
		public event WrongPasswordEventHandler WrongPassword
		{
			add{OnWrongPassword+=value;}
			remove{OnWrongPassword-=value;}
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// This is Asynchronic event and invoked only from xxxxWithProgress functions.
		/// Invoked whenever another 1 percent of the function is done.
		/// </summary>
		[Description("This is Asynchronic event and invoked only from xxxxWithProgress "+
			 "functions. Invoked whenever another 1 percent of the function is done.")
		]
		[Category("Action")]
		public event PercentCompletedEventHandler PercentCompleted
		{
			add{OnPercentCompleted+=value;}
			remove{OnPercentCompleted-=value;}
		}

		#endregion
		//-------------------------------------------------------------------------------
		#region Private Functions
		//-------------------------------------------------------------------------------
		/// <summary>
		/// Scanning for repeated bytes and according to them build frequency table.
		/// </summary>
		/// <param name="DataSource">The stream to build <c>FrequencyTable</c> for.</param>
		/// <returns>The generated frequency table.</returns>
		/// <remarks>
		/// DataSource must be readable and seekable stream.
		/// Don't try to extract somthing smaller then 415 bytes(it's not worth it)
		/// </remarks>
		private FrequencyTable BuildFrequencyTable(Stream DataSource)
		{
			long OriginalPosition = DataSource.Position;
			FrequencyTable FT = new FrequencyTable();
			IsByteExist = new bool[256]; //false by default

			Byte bTemp;
			//Counting bytes and saving them
			for(long i=0; i<DataSource.Length; ++i ) 
			{
				bTemp = (Byte)DataSource.ReadByte();
				if( IsByteExist[ bTemp ] ) //If the byte was found before increase the repeatition
					AmountList[ ByteLocation[bTemp] ]= (uint)AmountList[ ByteLocation[bTemp] ] + 1;
				else/*If new byte*/
				{
					IsByteExist[ bTemp ]=true; //Mark as found
					ByteLocation[ bTemp ] = (Byte)BytesList.Count ; //Save the new location of the byte in the bouth ArrayLists
					AmountList.Add(1u); //Marking that one was found
					BytesList.Add(bTemp);
				}
			}
			int ArraySize=BytesList.Count;
			//Moving the data from the arraylists to std arrays
			FT.FoundBytes = new byte[ArraySize];
			FT.Frequency = new uint[ArraySize];
			short ArraysSize = (short)ArraySize;
			//Copy the list to arrays;
			for(short i=0; i<ArraysSize; ++i ) 
			{
				FT.FoundBytes[i] = (Byte)BytesList[i];
				FT.Frequency[i] = (uint)AmountList[i];
			}
			//sort the arrays according to the Frequency
			SortArrays( FT.Frequency, FT.FoundBytes, ArraysSize );

			//Cleaning resources
			IsByteExist = null;
			BytesList.Clear();
			AmountList.Clear();
			DataSource.Seek(OriginalPosition, SeekOrigin.Begin);
			return FT;
		}
		//-------------------------------------------------------------------------------
		/// <summary>
		/// This function takes a password cstring and converts it to a ushort number
		/// that's fit the header of a shrinked file.
		/// </summary>
		/// <param name="Password">
		/// Password to a shrinked file (8 chars tops), is it's null, no password will
		/// take place in the file (zero value).
		/// </param>
		/// <returns>A numeric representation of the given password.</returns>
		/// <exception cref="Exception">
		/// When a given password is longer then 8 characters.
		/// </exception>
		private ushort PasswordGen(char[] Password)
		{
			if(Password==null )return 0;

			if(Password.Length > 8) 
				throw new Exception("Password's is 8 chars length tops, you've entered " +
					Password.Length + " bytes.");
			Byte Size = (byte)Password.Length;
			ushort Result=0;
			for(Byte i=0; i<Size; ++i)
				Result += (ushort)((Password[i] + 1)*i);

			return Result;			
		}
		//-------------------------------------------------------------------------------
		/// <summary>
		/// Bouble sort <c>FrequencyTable</c>( according frequency level )
		/// and making the same changes on the corresponding array.
		/// </summary>
		/// <param name="SortTarget">The array to sort by.</param>
		/// <param name="TweenArray">
		/// The array that will take the same swaping as the target array.
		/// </param>
		private void SortArrays(uint[] SortTarget, Byte[] TweenArray, short size)
		{
			--size;
			bool TestSwitch=false;
			Byte BTemp;
			uint uiTemp;
			short i,j;
			for(i=0; i < size ; ++i )
			{
				for(j=0; j < size ; ++j ) 
				{
					if( SortTarget[j] < SortTarget[j+1] ) 
					{
						TestSwitch = true;//Making switch action
						uiTemp = SortTarget[j];
						SortTarget[j] = SortTarget[j+1];
						SortTarget[j+1]=uiTemp;
						//Doing same to corresponding array
						BTemp = TweenArray[j];
						TweenArray[j] = TweenArray[j+1];
						TweenArray[j+1]=BTemp;
					}
				}//end of for
				if(!TestSwitch)break;//if no switch action in this round, no need for more.
				TestSwitch = false;
			}//end of for
			for(i=0;i<SortTarget.Length; ++i )
				ByteLocation[ TweenArray[i] ]=(Byte)i;

		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Write a header to the stream. This header is vital when extracting the data.
		/// </summary>
		/// <param name="St">The output stream.</param>
		/// <param name="FT">The frequency table of the data.</param>
		/// <param name="OriginalSize">The original of the data before shrinking.</param>
		/// <param name="version">The version of the shrinking code.</param>
		/// <param name="ComplementsBits">Number of extra bits added to the last byte of the data.</param>
		/// <param name="Password">A key to gain access to the archived file\stream.</param>
		private void WriteHeader(Stream St,FrequencyTable FT , long OriginalSize,
			Byte version, Byte ComplementsBits, char[] Password)
		{
			FileHeader Header = new FileHeader(version, FT, ref OriginalSize, 
				ComplementsBits, PasswordGen(Password ));
			BinFormat.Serialize(St, Header);
		}
		//-------------------------------------------------------------------------------
		/// <summary>
		/// Calculates the amount of complements bits, needed for the last byte writing.
		/// </summary>
		/// <param name="HT">The huffman tree of the stream to be archived.</param>
		/// <returns>Amount of complements bits</returns>
		private Byte GetComplementsBits(HuffmanTree HT)
		{
			//Getting the deapth of each leaf in the huffman tree
			short i = (short)HT.Leafs.Length;
			ushort[] NodesDeapth=new ushort[i];
			long SizeInOfBits=0;
			while( --i != -1 )
			{
				TreeNode TN = HT.Leafs[i];
				while( TN.Parent!=null ) 
				{
					TN = TN.Parent;
					++NodesDeapth[i];
				}
				SizeInOfBits+=NodesDeapth[i]*HT.FT.Frequency[i];
			}
			return (byte)( 8 - SizeInOfBits%8 );
		}

		#endregion
		//-------------------------------------------------------------------------------
		#region IDisposable Members

		public void Dispose()
		{
			BytesList = null;
			IsByteExist=null;
			AmountList = null;
			BinFormat = null;
			BitsList=null;
			ByteLocation = null;
			OnWrongPassword = null;
			OnPercentCompleted=null;
		}

		#endregion
		//-----------------------------------------------------------------------------
	}
}

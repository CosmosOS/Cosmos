# Manifest Resource Streams
Manifest Resource Streams allow you to include data from the files as byte arrays in your code. An example of its use is in the [ZMachine Demo](https://github.com/CosmosOS/Cosmos/blob/5973a3fae95c989dc13505184aff9a15aae9f65f/Demos/ZMachine/ZKernel/Kernel.cs#L19) 

## Drawbacks
Due to short comings in NASM there is a maximum size to the files which can be included this way.

## How to use
1. Set the file you want to use as embedded resource using the File Properties window in VS.
![image](https://user-images.githubusercontent.com/8559822/132468001-256b92d1-0b29-4db3-9ef5-3383bfdef023.png)
2. In the code reference the file using the following format:
```
[ManifestResourceStream(ResourceName = “{project_name}.{filename_with_extension}”)] 
static byte[] file;
```
The field _must_ be static. 

3. To access the data simply read from the byte array defined. 

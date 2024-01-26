FROM ubuntu:latest

WORKDIR /Cosmos

RUN apt-get update
RUN apt-get install -y dotnet6 git make xorriso sudo yasm binutils

RUN git config --global pack.window 1

# Cloning Cosmos Repos
RUN git clone https://github.com/CosmosOS/Cosmos.git
RUN git clone https://github.com/CosmosOS/XSharp.git
RUN git clone https://github.com/CosmosOS/IL2CPU.git
RUN git clone https://github.com/CosmosOS/Common.git

# Run make
WORKDIR /Cosmos
RUN make -C Cosmos

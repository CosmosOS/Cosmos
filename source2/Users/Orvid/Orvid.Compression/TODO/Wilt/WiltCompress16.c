#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <stdint.h>
#include <stdbool.h>




typedef struct RangeEncoder
{
	uint32_t range;
	uint64_t low;
	uint8_t cache;
	uint32_t cachesize;

	FILE *fh;
} RangeEncoder;

static void InitRangeEncoder(RangeEncoder *self,FILE *fh)
{
	self->range=0xffffffff;
	self->low=0;
	self->cache=0xff; // TODO: Is this right? Original has cache=0 and cachesize=1,
	self->cachesize=0; // and output a useless 0 byte at the start.

	self->fh=fh;
}

static void ShiftOutputFromRangeEncoder(RangeEncoder *self)
{
	if((self->low&0xffffffff)<0xff000000||(self->low>>32)!=0)
	{
		uint8_t temp=self->cache;
		for(int i=0;i<self->cachesize;i++)
		{
			fputc((temp+(self->low>>32))&0xff,self->fh);
			temp=0xff;
		}
		self->cachesize=0;
		self->cache=(self->low>>24)&0xff;
	}
	self->cachesize++;
	self->low=(self->low<<8)&0xffffffff;
}

static void WriteBitAndUpdateWeight(RangeEncoder *self,int bit,uint16_t *weight,int shift)
{
	uint32_t threshold=(self->range>>12)*(*weight);

	if(bit==0)
	{
		self->range=threshold;
		*weight+=(0x1000-*weight)>>shift;
	}
	else
	{
		self->range-=threshold;
		self->low+=threshold;
		*weight-=*weight>>shift;
	}

	while(self->range<0x1000000)
	{
		self->range<<=8;
		ShiftOutputFromRangeEncoder(self);
	}
}

static void WriteUniversalCode(RangeEncoder *self,uint32_t value,uint16_t *weights1,int shift1,uint16_t *weights2,int shift2)
{
	int maxbit=31;
	while(maxbit>=0 && (value>>maxbit&1)==0) maxbit--;

	for(int i=0;i<=maxbit;i++) WriteBitAndUpdateWeight(self,1,&weights1[i],shift1);
	WriteBitAndUpdateWeight(self,0,&weights1[maxbit+1],shift1);

	for(int i=maxbit-1;i>=0;i--) WriteBitAndUpdateWeight(self,(value>>i)&1,&weights2[i],shift2);
}

static void FinishRangeEncoder(RangeEncoder *self)
{
	for(int i=0;i<5;i++)
	{
		ShiftOutputFromRangeEncoder(self);
	}
}




typedef struct DictionaryEntry
{
	uint32_t dataoffset,nextoffset;
} DictionaryEntry;

typedef struct DictionaryLookup
{
	uint8_t *buf;
	uint32_t size;
	DictionaryEntry *entries;
	uint32_t offsets[65536];
} DictionaryLookup;

static inline uint16_t GetUInt16LE(uint8_t *ptr)
{
	return ptr[0]+(ptr[1]<<8);
}

static void InitDictionaryLookup(DictionaryLookup *self,uint8_t *buf,uint32_t size)
{
	self->buf=buf;
	self->size=size;
	self->entries=malloc((size/2)*sizeof(DictionaryEntry));
	memset(self->offsets,0xff,sizeof(self->offsets));

	for(int i=size/2-2;i>=0;i--)
	{
		uint16_t val=GetUInt16LE(&buf[i*2]);

		DictionaryEntry *entry=&self->entries[i];
		entry->dataoffset=i*2;
		entry->nextoffset=self->offsets[val];
		self->offsets[val]=i;
	}
}

static bool FindDictionaryMatch(DictionaryLookup *self,int start,int *length,int *offs)
{
	int maxlength=0,maxpos=-1;

	uint16_t first=GetUInt16LE(&self->buf[start]);
	uint32_t entryoffset=self->offsets[first];

	while(entryoffset!=0xffffffff && self->entries[entryoffset].dataoffset<start)
	{
		int pos=self->entries[entryoffset].dataoffset;
		int matchlen=2;
		while(pos+matchlen+2<=self->size && start+matchlen+2<=self->size
		&& self->buf[pos+matchlen]==self->buf[start+matchlen]
		&& self->buf[pos+matchlen+1]==self->buf[start+matchlen+1]) matchlen+=2;

		if(matchlen>=maxlength) // Use >= to capture the *last* hit for multiples.
		{
			maxlength=matchlen;
			maxpos=pos;
		}

		entryoffset=self->entries[entryoffset].nextoffset;
	}

	if(maxlength>=4)
	{
		*length=maxlength;
		*offs=maxpos;
		return true;
	}
	else return false;
}




void CompressData(FILE *fh,uint8_t *buf,uint32_t size,
int typeshift,int literalshift,int lengthshift1,int lengthshift2,int offsetshift1,int offsetshift2)
{
	RangeEncoder comp;
	InitRangeEncoder(&comp,fh);

	DictionaryLookup dict;
	InitDictionaryLookup(&dict,buf,size);

	uint16_t typeweight=0x800;

	uint16_t lengthweights1[32],lengthweights2[32];
	uint16_t offsetweights1[32],offsetweights2[32];
	for(int i=0;i<32;i++)
	lengthweights1[i]=lengthweights2[i]=offsetweights1[i]=offsetweights2[i]=0x800;

	uint16_t literalbitweights[16][16];
	for(int i=0;i<16;i++)
	for(int j=0;j<16;j++)
	literalbitweights[i][j]=0x800;

	int pos=0;
	while(pos<size)
	{
		int length,offs;
		if(FindDictionaryMatch(&dict,pos,&length,&offs))
		{
			WriteBitAndUpdateWeight(&comp,1,&typeweight,typeshift);

			WriteUniversalCode(&comp,length/2-2,lengthweights1,lengthshift1,lengthweights2,lengthshift2);
			WriteUniversalCode(&comp,(pos-offs)/2-1,offsetweights1,offsetshift1,offsetweights2,offsetshift2);

			pos+=length;
		}
		else
		{
			WriteBitAndUpdateWeight(&comp,0,&typeweight,typeshift);

			uint16_t val=GetUInt16LE(&buf[pos]);

			for(int i=15;i>=0;i--)
			{
				WriteBitAndUpdateWeight(&comp,(val>>i)&1,&literalbitweights[i][(val>>(i+1))&15],literalshift);
			}

			pos+=2;
		}
	}

	FinishRangeEncoder(&comp);
}




uint8_t *AllocAndReadFile(FILE *fh,uint32_t *size)
{
	const int blocksize=4096;
	uint8_t *buf=malloc(blocksize);
	uint32_t total=0;
	for(;;)
	{
		uint32_t actual=fread(buf+total,1,blocksize,fh);
		total+=actual;
		if(actual<blocksize) break;
		buf=realloc(buf,total+blocksize);
	}

	*size=total;
	return buf;
}

int main(int argc,char **argv)
{
	if(argc!=1&&argc!=7)
	{
		fprintf(stderr,"Usage: %s <inputfile >outputfile [typeshift literalshift lengthshift1 lengthshift2 offsetshift1 offsetshift2]\n",argv[0]);
		exit(1);
	}

	uint32_t size;
	uint8_t *file=AllocAndReadFile(stdin,&size);

	int typeshift=4,literalshift=2,lengthshift1=4,lengthshift2=4,offsetshift1=4,offsetshift2=4;
	if(argc==7)
	{
		typeshift=atoi(argv[1]);
		literalshift=atoi(argv[2]);
		lengthshift1=atoi(argv[3]);
		lengthshift2=atoi(argv[4]);
		offsetshift1=atoi(argv[5]);
		offsetshift2=atoi(argv[6]);
	}

	fputc(size&0xff,stdout);
	fputc((size>>8)&0xff,stdout);
	fputc((size>>16)&0xff,stdout);
	fputc((size>>24)&0xff,stdout);

	fputc((offsetshift1<<4)|offsetshift2,stdout);
	fputc((lengthshift1<<4)|lengthshift2,stdout);
	fputc((typeshift<<4)|literalshift,stdout);

	CompressData(stdout,file,size,typeshift,literalshift,lengthshift1,lengthshift2,offsetshift1,offsetshift2);
}

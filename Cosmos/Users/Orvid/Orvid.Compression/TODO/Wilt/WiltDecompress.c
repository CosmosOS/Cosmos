#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>




typedef struct RangeDecoder
{
	uint32_t range,code;
	FILE *fh;
} RangeDecoder;

static void InitRangeDecoder(RangeDecoder *self,FILE *fh)
{
	self->range=0xffffffff;
	self->code=0;
	self->fh=fh;

	for(int i=0;i<4;i++) self->code=(self->code<<8)|fgetc(fh);
}

static void NormalizeRangeDecoder(RangeDecoder *self)
{
	while(self->range<0x1000000)
	{
		self->code=(self->code<<8)|fgetc(self->fh);
		self->range<<=8;
	}
}

static int ReadBitAndUpdateWeight(RangeDecoder *self,uint16_t *weight,int shift)
{
	NormalizeRangeDecoder(self);

	uint32_t threshold=(self->range>>12)*(*weight);

	if(self->code<threshold)
	{
		self->range=threshold;
		*weight+=(0x1000-*weight)>>shift;
		return 0;
	}
	else
	{
		self->range-=threshold;
		self->code-=threshold;
		*weight-=*weight>>shift;
		return 1;
	}
}

static uint32_t ReadUniversalCode(RangeDecoder *self,uint16_t *weights1,int shift1,uint16_t *weights2,int shift2)
{
	int numbits=0;

	while(ReadBitAndUpdateWeight(self,&weights1[numbits],shift1)==1) numbits++;
	if(!numbits) return 0;

	uint32_t val=1;

	for(int i=0;i<numbits-1;i++)
	val=(val<<1)|ReadBitAndUpdateWeight(self,&weights2[numbits-1-i],shift2);

	return val;
}




static void CopyMemory(uint8_t *dest,uint8_t *src,int length)
{
	for(int i=0;i<length;i++) *dest++=*src++;
}

void DecompressData(FILE *fh,uint8_t *buf,uint32_t size,
int typeshift,int literalshift,int lengthshift1,int lengthshift2,int offsetshift1,int offsetshift2)
{
	RangeDecoder dec;
	InitRangeDecoder(&dec,fh);

	uint16_t typeweight=0x800;

	uint16_t lengthweights1[32],lengthweights2[32];
	uint16_t offsetweights1[32],offsetweights2[32];
	for(int i=0;i<32;i++)
	lengthweights1[i]=lengthweights2[i]=offsetweights1[i]=offsetweights2[i]=0x800;

	uint16_t literalbitweights[256];
	for(int i=0;i<256;i++)
	literalbitweights[i]=0x800;

	int pos=0;
	while(pos<size)
	{
		int length,offs;

		if(ReadBitAndUpdateWeight(&dec,&typeweight,typeshift)==1)
		{
			int length=(ReadUniversalCode(&dec,lengthweights1,lengthshift1,lengthweights2,lengthshift2)+3);
			int offs=(ReadUniversalCode(&dec,offsetweights1,offsetshift1,offsetweights2,offsetshift2)+1);

			CopyMemory(&buf[pos],&buf[pos-offs],length);
			pos+=length;
		}
		else
		{
			int val=1;
			for(int i=0;i<8;i++)
			{
				int bit=ReadBitAndUpdateWeight(&dec,&literalbitweights[val],literalshift);
				val=(val<<1)|bit;
			}
			buf[pos++]=val;
		}
	}
}




int main(int argc,char **argv)
{
	uint32_t size;
	size=fgetc(stdin);
	size|=fgetc(stdin)<<8;
	size|=fgetc(stdin)<<16;
	size|=fgetc(stdin)<<24;

	uint32_t shifts;
	shifts=fgetc(stdin);
	shifts|=fgetc(stdin)<<8;
	shifts|=fgetc(stdin)<<16;

	uint8_t *buf=malloc(size);

	DecompressData(stdin,buf,size,shifts>>20,(shifts>>16)&0xf,(shifts>>12)&0xf,(shifts>>8)&0xf,(shifts>>4)&0xf,shifts&0xf);

	fwrite(buf,size,1,stdout);
}

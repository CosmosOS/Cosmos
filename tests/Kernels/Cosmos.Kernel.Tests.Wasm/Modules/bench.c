// Workloads every VM runs. Each export returns an i32 checksum, so a run is
// checked against the native build of the same source. Run build.sh after a
// change to regenerate WasmModules.cs.

typedef unsigned int u32;

#define EXPORT(name) __attribute__((export_name(#name)))

EXPORT(fib)
int fib(int n)
{
    return n < 2 ? n : fib(n - 1) + fib(n - 2);
}

EXPORT(lcg)
u32 lcg(int n)
{
    u32 x = 1;
    u32 acc = 0;
    for (int i = 0; i < n; i++)
    {
        x = x * 1103515245u + 12345u;
        acc ^= x >> 16;
        acc = (acc << 1) | (acc >> 31);
    }
    return acc;
}

static unsigned char s_sieve[1 << 20];

EXPORT(sieve)
int sieve(int n)
{
    for (int i = 0; i <= n; i++)
    {
        s_sieve[i] = 1;
    }
    s_sieve[0] = 0;
    s_sieve[1] = 0;
    for (int i = 2; i * i <= n; i++)
    {
        if (s_sieve[i])
        {
            for (int j = i * i; j <= n; j += i)
            {
                s_sieve[j] = 0;
            }
        }
    }
    int count = 0;
    for (int i = 0; i <= n; i++)
    {
        count += s_sieve[i];
    }
    return count;
}

#define MAT_MAX 64
static double s_a[MAT_MAX * MAT_MAX];
static double s_b[MAT_MAX * MAT_MAX];
static double s_c[MAT_MAX * MAT_MAX];

EXPORT(matmul)
int matmul(int n)
{
    for (int i = 0; i < n * n; i++)
    {
        s_a[i] = (double)(i % 7) * 0.5;
        s_b[i] = (double)(i % 5) * 0.25;
    }
    for (int i = 0; i < n; i++)
    {
        for (int j = 0; j < n; j++)
        {
            double sum = 0.0;
            for (int k = 0; k < n; k++)
            {
                sum += s_a[i * n + k] * s_b[k * n + j];
            }
            s_c[i * n + j] = sum;
        }
    }
    double total = 0.0;
    for (int i = 0; i < n * n; i++)
    {
        total += s_c[i];
    }
    // Every product is a multiple of 1/8, so the scaled sum is exact.
    return (int)(total * 8.0);
}

EXPORT(mandel)
int mandel(int width, int height, int maxIter)
{
    int total = 0;
    for (int py = 0; py < height; py++)
    {
        double y0 = (double)py / height * 2.0 - 1.0;
        for (int px = 0; px < width; px++)
        {
            double x0 = (double)px / width * 3.0 - 2.0;
            double x = 0.0;
            double y = 0.0;
            int iter = 0;
            while (x * x + y * y <= 4.0 && iter < maxIter)
            {
                double xt = x * x - y * y + x0;
                y = 2.0 * x * y + y0;
                x = xt;
                iter++;
            }
            total += iter;
        }
    }
    return total;
}

static u32 s_crcTable[256];
static unsigned char s_crcData[1 << 16];

EXPORT(crc32)
u32 crc32(int length, int rounds)
{
    for (u32 i = 0; i < 256; i++)
    {
        u32 c = i;
        for (int k = 0; k < 8; k++)
        {
            c = (c & 1) ? 0xEDB88320u ^ (c >> 1) : c >> 1;
        }
        s_crcTable[i] = c;
    }
    for (int i = 0; i < length; i++)
    {
        s_crcData[i] = (unsigned char)(i * 31 + 7);
    }
    u32 crc = 0;
    for (int r = 0; r < rounds; r++)
    {
        crc = ~crc;
        for (int i = 0; i < length; i++)
        {
            crc = s_crcTable[(crc ^ s_crcData[i]) & 0xFF] ^ (crc >> 8);
        }
        crc = ~crc;
    }
    return crc;
}

#if defined(__wasm__)
EXPORT(grow)
int grow(int pages)
{
    int before = __builtin_wasm_memory_size(0);
    if (__builtin_wasm_memory_grow(0, pages) != before)
    {
        return -1;
    }
    volatile unsigned char *last = (unsigned char *)((before + pages) * 65536 - 1);
    *last = 0x5A;
    return *last == 0x5A ? 1 : -2;
}
#endif

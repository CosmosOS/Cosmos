// ACPI wrapper for Cosmos OS (Multi-Architecture)
// Finds the tables through uACPI's table API (early table access from kmain)
// and keeps what the kernel reads from them:
// x86: parses MADT for Local APIC / IO APIC / ISOs
// ARM64: parses MADT for GICD / GICR / GICC entries
// Both: MCFG (PCI ECAM), ARM64 also IORT.
// x86 power-off and reset go through uACPI's AML interpreter.

#include <stdint.h>
#include <stddef.h>

#include <uacpi/uacpi.h>
#include <uacpi/tables.h>
#include <uacpi/acpi.h>
#include <uacpi/sleep.h>

#define NULL_PTR ((void*)0)

// Serial output (implemented in C#)
extern void __cosmos_serial_write(const char* message);
extern void __cosmos_serial_write_hex_u32(uint32_t value);
extern void __cosmos_serial_write_hex_u64(uint64_t value);
extern void __cosmos_serial_write_dec_u32(uint32_t value);
extern void __cosmos_serial_write_dec_u64(uint64_t value);

// uacpi_host.c
extern void cosmos_uacpi_set_boot_info(void* rsdp_address, uint64_t hhdm_offset);

// ============================================================================
// ARM64 GIC structures (MADT subtable types 0x0B-0x0F)
// ============================================================================

#ifdef __aarch64__

#define MADT_TYPE_GICC  0x0B
#define MADT_TYPE_GICD  0x0C
#define MADT_TYPE_MSI   0x0D
#define MADT_TYPE_GICR  0x0E
#define MADT_TYPE_ITS   0x0F

typedef struct {
    uint8_t  found;
    uint8_t  version;       // GIC version: 2 or 3
    uint8_t  its_found;     // 1 if at least one GIC ITS subtable seen
    uint8_t  _pad[5];
    uint64_t dist_base;     // GICD physical base
    uint64_t redist_base;   // GICR physical base (GICv3)
    uint64_t redist_length; // GICR region length
    uint64_t cpu_if_base;   // GICC physical base (GICv2)
    uint64_t its_base;      // GIC ITS physical base (GICv3 + ITS only); 0 if absent
    uint32_t its_id;        // ITS Translation ID (referenced by IORT)
    uint32_t _pad2;
} acpi_gic_info_t;

static acpi_gic_info_t g_gic_info;

#endif // __aarch64__

// ============================================================================
// x86 MADT structures (MADT subtable types 0-2)
// ============================================================================

#ifdef ARCH_X64

#define MAX_CPUS 256
#define MAX_IOAPICS 16
#define MAX_ISO_ENTRIES 32

typedef struct {
    uint8_t processor_id;
    uint8_t apic_id;
    uint32_t flags;
} acpi_cpu_t;

typedef struct {
    uint8_t id;
    uint32_t address;
    uint32_t gsi_base;
} acpi_ioapic_t;

typedef struct {
    uint8_t source;
    uint32_t gsi;
    uint16_t flags;
} acpi_iso_t;

typedef struct {
    uint32_t local_apic_address;
    uint32_t flags;
    uint32_t cpu_count;
    acpi_cpu_t cpus[MAX_CPUS];
    uint32_t ioapic_count;
    acpi_ioapic_t ioapics[MAX_IOAPICS];
    uint32_t iso_count;
    acpi_iso_t isos[MAX_ISO_ENTRIES];
} acpi_madt_info_t;

static acpi_madt_info_t g_madt_info;

#endif // ARCH_X64

// ============================================================================
// PCI MCFG structure (shared across architectures)
// ============================================================================

typedef struct {
    uint8_t  found;
    uint8_t  start_bus;
    uint8_t  end_bus;
    uint8_t  _pad1;
    uint16_t segment;
    uint16_t _pad2;
    uint64_t base_address;  // ECAM physical base
} acpi_mcfg_info_t;

static acpi_mcfg_info_t g_mcfg_info;

// ============================================================================
// Global state
// ============================================================================

static uint8_t g_initialized = 0;

// uACPI keeps its table list here until uacpi_initialize() moves it to the
// heap: kmain looks the tables up before the heap exists. About 56 bytes per
// table, on top of uACPI's 16 static slots.
static uint64_t g_early_table_buffer[4096 / sizeof(uint64_t)];

static void write_status(const char* prefix, uacpi_status status) {
    __cosmos_serial_write(prefix);
    __cosmos_serial_write(uacpi_status_to_string(status));
    __cosmos_serial_write("\n");
}

// ============================================================================
// MADT parsing - shared entry point
// ============================================================================

static void parse_madt(struct acpi_sdt_hdr* madt_header) {
    uint8_t* madt = (uint8_t*)madt_header;
    uint32_t length = madt_header->length;

    // MADT: header(36) + local_controller_addr(4) + flags(4) + entries
    uint32_t offset = sizeof(struct acpi_sdt_hdr) + 8;

#ifdef ARCH_X64
    // x86: Extract Local APIC address from MADT header
    g_madt_info.local_apic_address = *(uint32_t*)(madt + sizeof(struct acpi_sdt_hdr));
    __cosmos_serial_write("[ACPI] Local APIC at: 0x");
    __cosmos_serial_write_hex_u32(g_madt_info.local_apic_address);
    __cosmos_serial_write("\n");
#endif

#ifdef __aarch64__
    uint8_t found_gicd = 0;
    uint8_t found_gicr = 0;
#endif

    while (offset + 2 <= length) {
        uint8_t type = madt[offset];
        uint8_t entry_len = madt[offset + 1];
        if (entry_len < 2) break;

#ifdef ARCH_X64
        switch (type) {
            case 0: { // Processor Local APIC
                if (g_madt_info.cpu_count < MAX_CPUS) {
                    uint8_t acpi_id = madt[offset + 2];
                    uint8_t apic_id = madt[offset + 3];
                    uint32_t flags = *(uint32_t*)(madt + offset + 4);
                    if (flags & 1) {
                        g_madt_info.cpus[g_madt_info.cpu_count].processor_id = acpi_id;
                        g_madt_info.cpus[g_madt_info.cpu_count].apic_id = apic_id;
                        g_madt_info.cpus[g_madt_info.cpu_count].flags = flags;
                        g_madt_info.cpu_count++;
                        __cosmos_serial_write("[ACPI] CPU (ID=");
                        __cosmos_serial_write_dec_u32(acpi_id);
                        __cosmos_serial_write(" APIC=");
                        __cosmos_serial_write_dec_u32(apic_id);
                        __cosmos_serial_write(")\n");
                    }
                }
                break;
            }
            case 1: { // I/O APIC
                if (g_madt_info.ioapic_count < MAX_IOAPICS) {
                    g_madt_info.ioapics[g_madt_info.ioapic_count].id = madt[offset + 2];
                    g_madt_info.ioapics[g_madt_info.ioapic_count].address = *(uint32_t*)(madt + offset + 4);
                    g_madt_info.ioapics[g_madt_info.ioapic_count].gsi_base = *(uint32_t*)(madt + offset + 8);
                    __cosmos_serial_write("[ACPI] I/O APIC (ID=");
                    __cosmos_serial_write_dec_u32(madt[offset + 2]);
                    __cosmos_serial_write(" at 0x");
                    __cosmos_serial_write_hex_u32(*(uint32_t*)(madt + offset + 4));
                    __cosmos_serial_write(")\n");
                    g_madt_info.ioapic_count++;
                }
                break;
            }
            case 2: { // Interrupt Source Override
                if (g_madt_info.iso_count < MAX_ISO_ENTRIES) {
                    g_madt_info.isos[g_madt_info.iso_count].source = madt[offset + 3];
                    g_madt_info.isos[g_madt_info.iso_count].gsi = *(uint32_t*)(madt + offset + 4);
                    g_madt_info.isos[g_madt_info.iso_count].flags = *(uint16_t*)(madt + offset + 8);
                    g_madt_info.iso_count++;
                }
                break;
            }
        }
#endif // ARCH_X64

#ifdef __aarch64__
        switch (type) {
            case MADT_TYPE_GICD: {
                // GICD: offset+8=base(8), offset+20=version(1)
                if (entry_len >= 24) {
                    uint64_t base = *(uint64_t*)(madt + offset + 8);
                    uint8_t ver = madt[offset + 20];
                    __cosmos_serial_write("[ACPI-GIC] GICD: base=0x");
                    __cosmos_serial_write_hex_u64(base);
                    __cosmos_serial_write(" ver=");
                    char buf[2] = { '0' + ver, 0 };
                    __cosmos_serial_write(buf);
                    __cosmos_serial_write("\n");
                    g_gic_info.dist_base = base;
                    if (ver >= 3) g_gic_info.version = 3;
                    else if (ver >= 1) g_gic_info.version = ver;
                    found_gicd = 1;
                }
                break;
            }
            case MADT_TYPE_GICR: {
                // GICR: offset+4=base(8), offset+12=length(4)
                if (entry_len >= 16 && !found_gicr) {
                    uint64_t base = *(uint64_t*)(madt + offset + 4);
                    uint32_t len = *(uint32_t*)(madt + offset + 12);
                    __cosmos_serial_write("[ACPI-GIC] GICR: base=0x");
                    __cosmos_serial_write_hex_u64(base);
                    __cosmos_serial_write(" len=0x");
                    __cosmos_serial_write_hex_u64(len);
                    __cosmos_serial_write("\n");
                    g_gic_info.redist_base = base;
                    g_gic_info.redist_length = len;
                    if (g_gic_info.version < 3) g_gic_info.version = 3;
                    found_gicr = 1;
                }
                break;
            }
            case MADT_TYPE_GICC: {
                // GICC: offset+32=Physical Base(8)
                if (entry_len >= 40 && g_gic_info.cpu_if_base == 0) {
                    uint64_t base = *(uint64_t*)(madt + offset + 32);
                    if (base != 0) {
                        g_gic_info.cpu_if_base = base;
                        __cosmos_serial_write("[ACPI-GIC] GICC: base=0x");
                        __cosmos_serial_write_hex_u64(base);
                        __cosmos_serial_write("\n");
                    }
                }
                break;
            }
            case MADT_TYPE_ITS: {
                // GIC ITS subtable (ACPI 6.5 §5.2.12.18):
                // type(1) length(1) reserved(2) translation_id(4)
                // physical_base(8) reserved(4)
                if (entry_len >= 20 && !g_gic_info.its_found) {
                    uint32_t id = *(uint32_t*)(madt + offset + 4);
                    uint64_t base = *(uint64_t*)(madt + offset + 8);
                    g_gic_info.its_base = base;
                    g_gic_info.its_id = id;
                    g_gic_info.its_found = 1;
                    __cosmos_serial_write("[ACPI-GIC] ITS: id=");
                    __cosmos_serial_write_dec_u32(id);
                    __cosmos_serial_write(" base=0x");
                    __cosmos_serial_write_hex_u64(base);
                    __cosmos_serial_write("\n");
                }
                break;
            }
        }
#endif // __aarch64__

        offset += entry_len;
    }

#ifdef __aarch64__
    if (found_gicd) {
        g_gic_info.found = 1;
        if (g_gic_info.version == 0) g_gic_info.version = 2;
        __cosmos_serial_write("[ACPI-GIC] Result: GICv");
        char buf[2] = { '0' + g_gic_info.version, 0 };
        __cosmos_serial_write(buf);
        __cosmos_serial_write(" GICD=0x");
        __cosmos_serial_write_hex_u64(g_gic_info.dist_base);
        if (found_gicr) {
            __cosmos_serial_write(" GICR=0x");
            __cosmos_serial_write_hex_u64(g_gic_info.redist_base);
        }
        __cosmos_serial_write("\n");
    } else {
        __cosmos_serial_write("[ACPI-GIC] No GICD found in MADT\n");
    }
#endif

    __cosmos_serial_write("[ACPI] MADT parsing complete\n");
}

// ============================================================================
// MCFG parsing (PCI ECAM base address discovery)
// ============================================================================

static void parse_mcfg(struct acpi_sdt_hdr* mcfg_header) {
    uint8_t* mcfg = (uint8_t*)mcfg_header;
    uint32_t length = mcfg_header->length;

    // MCFG: header(36) + reserved(8) + entries(16 each)
    uint32_t offset = sizeof(struct acpi_sdt_hdr) + 8;

    if (offset + 16 > length) {
        __cosmos_serial_write("[ACPI-MCFG] No entries in MCFG table\n");
        return;
    }

    // Parse first entry (segment 0)
    g_mcfg_info.base_address = *(uint64_t*)(mcfg + offset);
    g_mcfg_info.segment = *(uint16_t*)(mcfg + offset + 8);
    g_mcfg_info.start_bus = mcfg[offset + 10];
    g_mcfg_info.end_bus = mcfg[offset + 11];
    g_mcfg_info.found = 1;

    __cosmos_serial_write("[ACPI-MCFG] ECAM base=0x");
    __cosmos_serial_write_hex_u64(g_mcfg_info.base_address);
    __cosmos_serial_write(" segment=");
    __cosmos_serial_write_dec_u32(g_mcfg_info.segment);
    __cosmos_serial_write(" bus=");
    __cosmos_serial_write_dec_u32(g_mcfg_info.start_bus);
    __cosmos_serial_write("-");
    __cosmos_serial_write_dec_u32(g_mcfg_info.end_bus);
    __cosmos_serial_write("\n");
}

// ============================================================================
// IORT parsing (ARM64 only) — translates PCI requester IDs to ITS DeviceIDs
// per ARM DEN 0049 "IO Remapping Table" specification.
// ============================================================================

#ifdef __aarch64__

#define IORT_NODE_ITS_GROUP    0x00
#define IORT_NODE_ROOT_COMPLEX 0x02
#define IORT_NODE_SMMU_V1V2    0x03
#define IORT_NODE_SMMU_V3      0x04

// Cached pointer to the IORT, virtual address. NULL if no IORT was found.
static uint8_t* g_iort_base = NULL_PTR;
static uint32_t g_iort_length = 0;

// Read 32-bit little-endian field at unaligned offset.
static inline uint32_t iort_rd32(const uint8_t* p, uint32_t off) {
    return (uint32_t)p[off] | ((uint32_t)p[off+1] << 8)
         | ((uint32_t)p[off+2] << 16) | ((uint32_t)p[off+3] << 24);
}
static inline uint16_t iort_rd16(const uint8_t* p, uint32_t off) {
    return (uint16_t)p[off] | ((uint16_t)p[off+1] << 8);
}

// Find the node at file-relative byte offset `node_off`. Returns NULL if it
// would extend beyond the table.
static const uint8_t* iort_node_at(uint32_t node_off) {
    if (g_iort_base == NULL_PTR) return NULL_PTR;
    if (node_off + 16 > g_iort_length) return NULL_PTR;
    uint16_t len = iort_rd16(g_iort_base, node_off + 1);
    if (len < 16 || node_off + len > g_iort_length) return NULL_PTR;
    return g_iort_base + node_off;
}

static void parse_iort(struct acpi_sdt_hdr* iort_header) {
    g_iort_base = (uint8_t*)iort_header;
    g_iort_length = iort_header->length;

    uint32_t node_count = iort_rd32(g_iort_base, sizeof(struct acpi_sdt_hdr));
    uint32_t node_array_off = iort_rd32(g_iort_base, sizeof(struct acpi_sdt_hdr) + 4);

    __cosmos_serial_write("[ACPI-IORT] nodes=");
    __cosmos_serial_write_dec_u32(node_count);
    __cosmos_serial_write(" base=0x");
    __cosmos_serial_write_hex_u64((uint64_t)(uintptr_t)g_iort_base);
    __cosmos_serial_write("\n");

    // Sanity: walk once for diagnostics.
    uint32_t off = node_array_off;
    for (uint32_t i = 0; i < node_count && off + 16 <= g_iort_length; i++) {
        uint8_t  type = g_iort_base[off];
        uint16_t len  = iort_rd16(g_iort_base, off + 1);
        if (len < 16) break;
        if (type == IORT_NODE_ROOT_COMPLEX && len >= 32) {
            uint32_t segment = iort_rd32(g_iort_base, off + 28);
            uint32_t mappings = iort_rd32(g_iort_base, off + 8);
            __cosmos_serial_write("[ACPI-IORT] Root Complex seg=");
            __cosmos_serial_write_dec_u32(segment);
            __cosmos_serial_write(" mappings=");
            __cosmos_serial_write_dec_u32(mappings);
            __cosmos_serial_write("\n");
        } else if (type == IORT_NODE_ITS_GROUP) {
            uint32_t its_count = (len >= 20) ? iort_rd32(g_iort_base, off + 16) : 0;
            __cosmos_serial_write("[ACPI-IORT] ITS Group count=");
            __cosmos_serial_write_dec_u32(its_count);
            __cosmos_serial_write("\n");
        }
        off += len;
    }
}

// Resolve a (segment, bdf) requester ID to an ITS DeviceID by walking IORT
// Root Complex → ID mappings → ITS Group. Returns 0 on success, nonzero
// otherwise. SMMU pass-through nodes are followed once (their input is the
// requester ID, output continues to ITS).
int acpi_iort_resolve_device_id(uint32_t segment, uint32_t bdf, uint32_t* out_device_id) {
    if (out_device_id == NULL_PTR) return 1;
    *out_device_id = bdf;  // identity fallback even on early return

    if (g_iort_base == NULL_PTR) return 1;

    uint32_t node_count = iort_rd32(g_iort_base, sizeof(struct acpi_sdt_hdr));
    uint32_t node_array_off = iort_rd32(g_iort_base, sizeof(struct acpi_sdt_hdr) + 4);

    // Find Root Complex matching segment.
    uint32_t off = node_array_off;
    const uint8_t* rc = NULL_PTR;
    for (uint32_t i = 0; i < node_count; i++) {
        const uint8_t* n = iort_node_at(off);
        if (n == NULL_PTR) return 1;
        uint16_t len = iort_rd16(n, 1);
        if (n[0] == IORT_NODE_ROOT_COMPLEX && len >= 32) {
            uint32_t seg = iort_rd32(n, 28);
            if (seg == segment) { rc = n; break; }
        }
        off += len;
    }
    if (rc == NULL_PTR) return 1;

    uint32_t cur_id = bdf;
    const uint8_t* node = rc;

    // Walk up to two hops: RC → (optional) SMMU → ITS.
    for (int hop = 0; hop < 3; hop++) {
        uint8_t  ntype = node[0];
        if (ntype == IORT_NODE_ITS_GROUP) {
            *out_device_id = cur_id;
            return 0;
        }

        uint32_t num_map = iort_rd32(node, 8);
        uint32_t map_off = iort_rd32(node, 12);
        if (num_map == 0) return 1;

        uint32_t node_off_in_table = (uint32_t)(node - g_iort_base);
        const uint8_t* map = node + map_off;
        // ID mapping: input_base(4) id_count(4) output_base(4) output_ref(4) flags(4) — 20 bytes
        const uint8_t* hit = NULL_PTR;
        for (uint32_t i = 0; i < num_map; i++) {
            const uint8_t* m = map + i * 20;
            // Bound check: map entry must fit within the IORT.
            uint32_t m_off = node_off_in_table + map_off + i * 20;
            if (m_off + 20 > g_iort_length) return 1;
            uint32_t in_base = iort_rd32(m, 0);
            uint32_t id_cnt  = iort_rd32(m, 4);
            uint32_t flags   = iort_rd32(m, 16);
            // Single-mapping entries (flags bit 0) are only architecturally
            // valid on Named Component / PMCG nodes (ARM DEN 0049), neither
            // of which appears on this RC → SMMU → ITS walk. On an RC hop a
            // single-mapping entry would collapse EVERY RID into one
            // DeviceID (two devices sharing a DeviceID: the second MAPD
            // re-issues a fresh ITT and destroys the first device's event
            // mappings); on SMMU nodes it describes the SMMU's own MSI
            // DeviceID. Linux's iort_id_map rejects the flag on both as a
            // firmware bug — skip such entries entirely, and never
            // range-match them (id_count is 0, so the inclusive check
            // below could fire on in_base).
            if (flags & 1u) {
                continue;
            }
            // "Number of IDs" holds the range size MINUS ONE (ARM DEN 0049),
            // so the match is inclusive. An exclusive `< in_base + id_cnt`
            // check drops the last ID of every range and never matches
            // single-ID ranges (id_count == 0) — firmware with a
            // non-identity mapping would then silently fall back to
            // DeviceID = BDF and the ITS would discard every MSI.
            if (cur_id >= in_base && cur_id - in_base <= id_cnt) {
                hit = m;
                break;
            }
        }
        if (hit == NULL_PTR) return 1;

        uint32_t in_base    = iort_rd32(hit, 0);
        uint32_t out_base   = iort_rd32(hit, 8);
        uint32_t out_ref    = iort_rd32(hit, 12);
        // hit is always a range mapping (single-mapping entries are
        // skipped above), so the output ID is base plus offset.
        cur_id = out_base + (cur_id - in_base);

        const uint8_t* next = iort_node_at(out_ref);
        if (next == NULL_PTR) return 1;
        node = next;
    }
    return 1;
}

#else // !__aarch64__

int acpi_iort_resolve_device_id(uint32_t segment, uint32_t bdf, uint32_t* out_device_id) {
    if (out_device_id != NULL_PTR) *out_device_id = bdf;
    return 1;
}

#endif // __aarch64__

// ============================================================================
// XSDT/RSDT table walking (shared)
// ============================================================================

void acpi_early_init(void* rsdp_address, uint64_t hhdm_offset) {
    __cosmos_serial_write("[ACPI] acpi_early_init()\n");

    if (rsdp_address == NULL_PTR) {
        __cosmos_serial_write("[ACPI] ERROR: RSDP is NULL\n");
        return;
    }

    // Clear state
#ifdef ARCH_X64
    for (int i = 0; i < (int)sizeof(g_madt_info); i++)
        ((uint8_t*)&g_madt_info)[i] = 0;
#endif
#ifdef __aarch64__
    for (int i = 0; i < (int)sizeof(g_gic_info); i++)
        ((uint8_t*)&g_gic_info)[i] = 0;
#endif
    for (int i = 0; i < (int)sizeof(g_mcfg_info); i++)
        ((uint8_t*)&g_mcfg_info)[i] = 0;

    struct acpi_rsdp* rsdp = (struct acpi_rsdp*)rsdp_address;

    // Validate RSDP signature
    if (rsdp->signature[0] != 'R' || rsdp->signature[1] != 'S' ||
        rsdp->signature[2] != 'D' || rsdp->signature[3] != ' ' ||
        rsdp->signature[4] != 'P' || rsdp->signature[5] != 'T' ||
        rsdp->signature[6] != 'R' || rsdp->signature[7] != ' ') {
        __cosmos_serial_write("[ACPI] Invalid RSDP signature\n");
        return;
    }

    cosmos_uacpi_set_boot_info(rsdp_address, hhdm_offset);

    uacpi_status ret = uacpi_setup_early_table_access(
        g_early_table_buffer, sizeof(g_early_table_buffer));
    if (uacpi_unlikely_error(ret)) {
        write_status("[ACPI] ERROR: uACPI early table access failed: ", ret);
        return;
    }

    uacpi_table madt;
    if (uacpi_table_find_by_signature(ACPI_MADT_SIGNATURE, &madt) == UACPI_STATUS_OK) {
        __cosmos_serial_write("[ACPI] MADT found, parsing...\n");
        parse_madt(madt.hdr);
        uacpi_table_unref(&madt);
    } else {
        __cosmos_serial_write("[ACPI] WARNING: MADT not found\n");
    }

    uacpi_table mcfg;
    if (uacpi_table_find_by_signature(ACPI_MCFG_SIGNATURE, &mcfg) == UACPI_STATUS_OK) {
        __cosmos_serial_write("[ACPI] MCFG found, parsing...\n");
        parse_mcfg(mcfg.hdr);
        uacpi_table_unref(&mcfg);
    }

#ifdef __aarch64__
    // The reference is kept: acpi_iort_resolve_device_id() walks the table
    // for as long as the kernel runs.
    uacpi_table iort;
    if (uacpi_table_find_by_signature("IORT", &iort) == UACPI_STATUS_OK) {
        __cosmos_serial_write("[ACPI] IORT found, parsing...\n");
        parse_iort(iort.hdr);
    } else {
        __cosmos_serial_write("[ACPI] IORT not present (DeviceID = BDF)\n");
    }
#endif

    g_initialized = 1;
    __cosmos_serial_write("[ACPI] Init complete\n");
}

// ============================================================================
// Public API for C# interop
// ============================================================================

#ifdef ARCH_X64
const acpi_madt_info_t* acpi_get_madt_info(void) {
    return g_initialized ? &g_madt_info : NULL_PTR;
}
#endif

#ifdef __aarch64__
const acpi_gic_info_t* acpi_get_gic_info(void) {
    return g_initialized ? &g_gic_info : NULL_PTR;
}
#endif

const acpi_mcfg_info_t* acpi_get_mcfg_info(void) {
    return g_initialized ? &g_mcfg_info : NULL_PTR;
}

// For uacpi_host.c: the ECAM base (the address of bus 0) of the window the
// MCFG gives for 'segment', if it covers 'bus'.
int cosmos_acpi_ecam_lookup(uint16_t segment, uint8_t bus, uint64_t* out_base) {
    if (!g_mcfg_info.found || g_mcfg_info.segment != segment ||
        bus < g_mcfg_info.start_bus || bus > g_mcfg_info.end_bus) {
        return 0;
    }
    *out_base = g_mcfg_info.base_address;
    return 1;
}

// ============================================================================
// Power management: ACPI S5 and the FADT reset register through uACPI (x86)
// ============================================================================

#ifdef ARCH_X64

// The interpreter is brought up by the first power call rather than at boot:
// nothing else in the kernel evaluates AML yet, and this keeps the firmware's
// _INI methods off every boot. uacpi_initialize() also switches the chipset
// to ACPI mode (SMI_CMD), which firmware that boots in legacy mode needs
// before it honours a sleep request.
static uacpi_status acpi_bring_up(uacpi_init_level target) {
    uacpi_status ret;

    if (uacpi_get_current_init_level() < UACPI_INIT_LEVEL_SUBSYSTEM_INITIALIZED) {
        __cosmos_serial_write("[ACPI-PM] Initializing uACPI...\n");
        ret = uacpi_initialize(0);
        if (uacpi_unlikely_error(ret)) {
            write_status("[ACPI-PM] uacpi_initialize failed: ", ret);
            return ret;
        }
    }

    if (target < UACPI_INIT_LEVEL_NAMESPACE_LOADED) {
        return UACPI_STATUS_OK;
    }

    if (uacpi_get_current_init_level() < UACPI_INIT_LEVEL_NAMESPACE_LOADED) {
        __cosmos_serial_write("[ACPI-PM] Loading the AML namespace...\n");
        ret = uacpi_namespace_load();
        if (uacpi_unlikely_error(ret)) {
            write_status("[ACPI-PM] uacpi_namespace_load failed: ", ret);
            return ret;
        }
    }

    if (uacpi_get_current_init_level() < UACPI_INIT_LEVEL_NAMESPACE_INITIALIZED) {
        ret = uacpi_namespace_initialize();
        if (uacpi_unlikely_error(ret)) {
            write_status("[ACPI-PM] uacpi_namespace_initialize failed: ", ret);
            return ret;
        }
    }

    return UACPI_STATUS_OK;
}

// Brings the interpreter up without using it: 0 once the namespace is loaded
// and initialized, 1 if that failed, 2 where there is no interpreter (ARM64).
int cosmos_acpi_load_namespace(void) {
    return uacpi_unlikely_error(acpi_bring_up(UACPI_INIT_LEVEL_NAMESPACE_INITIALIZED)) ? 1 : 0;
}

int cosmos_acpi_shutdown(void) {
    uacpi_status ret = acpi_bring_up(UACPI_INIT_LEVEL_NAMESPACE_INITIALIZED);
    if (uacpi_unlikely_error(ret)) {
        return 1;
    }

    // Reads \_S5 and runs _PTS.
    ret = uacpi_prepare_for_sleep_state(UACPI_SLEEP_STATE_S5);
    if (uacpi_unlikely_error(ret)) {
        write_status("[ACPI-PM] Preparing for S5 failed: ", ret);
        return 1;
    }

    __cosmos_serial_write("[ACPI-PM] Entering S5\n");
    ret = uacpi_enter_sleep_state(UACPI_SLEEP_STATE_S5);
    write_status("[ACPI-PM] Still running after the S5 request: ", ret);
    return 1;
}

int cosmos_acpi_reset(void) {
    // The reset register is in the FADT: no namespace needed.
    uacpi_status ret = acpi_bring_up(UACPI_INIT_LEVEL_SUBSYSTEM_INITIALIZED);
    if (uacpi_unlikely_error(ret)) {
        return 1;
    }

    __cosmos_serial_write("[ACPI-PM] Resetting through the FADT reset register\n");
    ret = uacpi_reboot();
    write_status("[ACPI-PM] Still running after the FADT reset: ", ret);
    return 1;
}

#else

// ARM64 powers off and resets through PSCI.
int cosmos_acpi_load_namespace(void) { return 2; }
int cosmos_acpi_shutdown(void) { return 1; }
int cosmos_acpi_reset(void) { return 1; }

#endif


; Assembly Editor core
; Copyright (c) 2001-2007, Tomasz Grysztar.
; All rights reserved.

init_editor_memory:
        mov     ecx,BLOCK_LENGTH
        call    get_memory
        or      eax,eax
        jz      memory_error
        mov     [editor_memory],eax
        mov     dword [eax],0
        mov     dword [eax+4],0
        mov     dword [eax+8],ebx
        lea     ebx,[eax+SEGMENT_LENGTH]
        mov     [unallocated_segments],ebx
        mov     [memory_search_segment],ebx
        add     eax,BLOCK_LENGTH
        mov     [unallocated_segments_end],eax
        mov     [memory_search_block],eax
        mov     [released_segments],0
        call    allocate_segment
        mov     [first_line],eax
        mov     [lines_count],1
        mov     [peak_line_length],0
        mov     [caret_line],eax
        mov     [caret_line_number],1
        mov     [window_line],eax
        mov     [window_line_number],1
        mov     edi,eax
        xor     eax,eax
        mov     ecx,SEGMENT_HEADER_LENGTH shr 2
        rep     stosd
        mov     eax,20202020h
        mov     ecx,SEGMENT_DATA_LENGTH shr 2
        rep     stosd
        call    allocate_segment
        jc      memory_shortage
        mov     [lengths_table],eax
        mov     edi,eax
        xor     eax,eax
        mov     ecx,SEGMENT_LENGTH shr 2
        rep     stosd
        mov     [caret_position],0
        mov     [window_position],0
        mov     [selection_line],0
        mov     [undo_data],0
        mov     [search_data],0
        mov     [editor_mode],0
        clc
        retn
    memory_error:
        stc
        retn

reset_editor_memory:
        mov     esi,[editor_memory]
        lea     eax,[esi+SEGMENT_LENGTH]
        mov     [unallocated_segments],eax
        mov     [memory_search_segment],eax
        lea     eax,[esi+BLOCK_LENGTH]
        mov     [unallocated_segments_end],eax
        mov     [memory_search_block],eax
        mov     [released_segments],0
        mov     ebx,[esi]
    release_blocks:
        or      ebx,ebx
        jz      release_done
        push    dword [ebx]
        mov     ebx,[ebx+8]
        call    release_memory
        pop     ebx
        jmp     release_blocks
    release_done:
        mov     ebx,[editor_memory]
        xor     eax,eax
        mov     [ebx],eax
        mov     [undo_data],eax
        mov     [search_data],eax
        call    allocate_segment
        jc      memory_shortage
        mov     [first_line],eax
        mov     [window_line],eax
        mov     [caret_line],eax
        mov     edi,eax
        xor     eax,eax
        mov     ecx,SEGMENT_HEADER_LENGTH shr 2
        rep     stosd
        mov     eax,20202020h
        mov     ecx,SEGMENT_DATA_LENGTH shr 2
        rep     stosd
        xor     eax,eax
        mov     [selection_line],eax
        mov     [peak_line_length],eax
        mov     [window_position],eax
        inc     eax
        mov     [window_line_number],eax
        mov     [caret_line_number],eax
        mov     [lines_count],eax
        call    allocate_segment
        jc      memory_shortage
        mov     [lengths_table],eax
        mov     edi,eax
        xor     eax,eax
        mov     ecx,SEGMENT_LENGTH shr 2
        rep     stosd
        retn

release_editor_memory:
        mov     esi,[editor_memory]
    release:
        push    dword [esi]
        mov     ebx,[esi+8]
        call    release_memory
        pop     esi
        or      esi,esi
        jnz     release
        mov     [editor_memory],0
        retn

allocate_segment:
        mov     eax,[unallocated_segments]
        cmp     eax,[unallocated_segments_end]
        je      simple_allocation_failed
        add     [unallocated_segments],SEGMENT_LENGTH
        clc
        retn
    simple_allocation_failed:
        push    ebx esi
        mov     ebx,[memory_search_block]
        mov     esi,[memory_search_segment]
        cmp     [released_segments],16
        jb      add_new_block
    find_free_segment:
        cmp     esi,ebx
        je      find_in_next_block
        cmp     dword [esi],-1
        je      reuse_segment
        add     esi,SEGMENT_LENGTH
        cmp     esi,[memory_search_segment]
        jne     find_free_segment
    add_new_block:
        sub     ebx,BLOCK_LENGTH
    find_last_memory_block:
        cmp     dword [ebx],0
        je      allocate_more_memory
        mov     ebx,[ebx]
        jmp     find_last_memory_block
    allocate_more_memory:
        mov     ecx,BLOCK_LENGTH
        push    ebx
        call    get_memory
        pop     esi
        or      eax,eax
        jz      allocation_failed
        mov     [esi],eax
        mov     [eax],dword 0
        mov     [eax+4],esi
        mov     [eax+8],ebx
        lea     ebx,[eax+BLOCK_LENGTH]
        mov     [unallocated_segments_end],ebx
        add     eax,SEGMENT_LENGTH
        lea     ebx,[eax+SEGMENT_LENGTH]
        mov     [unallocated_segments],ebx
        mov     [released_segments],0
        pop     esi ebx
        clc
        retn
    allocation_failed:
        pop     esi ebx
        stc
        retn
    reuse_segment:
        mov     eax,esi
        mov     [memory_search_block],ebx
        add     esi,SEGMENT_LENGTH
        mov     [memory_search_segment],esi
        dec     [released_segments]
        pop     esi ebx
        clc
        retn
    find_in_next_block:
        sub     ebx,BLOCK_LENGTH
        mov     esi,[ebx]
        lea     ebx,[esi+BLOCK_LENGTH]
        or      esi,esi
        jnz     find_free_segment
        mov     ebx,[editor_memory]
        mov     esi,ebx
        add     ebx,BLOCK_LENGTH
        jmp     find_free_segment

memory_shortage:
        call    undo_changes
        jmp     not_enough_memory

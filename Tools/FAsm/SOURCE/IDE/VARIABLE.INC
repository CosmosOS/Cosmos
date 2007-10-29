
; Assembly Editor core
; Copyright (c) 2001-2007, Tomasz Grysztar.
; All rights reserved.

editor_memory dd ?

label editor_status

first_line dd ?
lines_count dd ?
peak_line_length dd ?
maximum_position dd ?
window_line dd ?
window_position dd ?
window_line_number dd ?
caret_line dd ?
caret_position dd ?
caret_line_number dd ?
selection_line dd ?
selection_position dd ?
selection_line_number dd ?
editor_mode dd ?

editor_status_size = $ - editor_status

window_width dd ?
window_height dd ?
unallocated_segments dd ?
unallocated_segments_end dd ?
released_segments dd ?
memory_search_block dd ?
memory_search_segment dd ?
lengths_table dd ?
undo_data dd ?
search_data dd ?
search_flags dd ?
search_handle dd ?



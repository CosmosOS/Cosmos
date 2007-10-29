
; flat assembler core variables
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

; Variables which have to be set up by interface:

memory_start dd ?
memory_end dd ?

additional_memory dd ?
additional_memory_end dd ?

stack_limit dd ?

input_file dd ?
output_file dd ?

passes_limit dw ?

; Internal core variables:

current_pass dw ?

include_paths dd ?
free_additional_memory dd ?
source_start dd ?
code_start dd ?
code_size dd ?
real_code_size dd ?
written_size dd ?
headers_size dd ?

current_line dd ?
macro_line dd ?
macro_block dd ?
macro_block_line dd ?
macro_block_line_number dd ?
macro_symbols dd ?
struc_name dd ?
struc_label dd ?
instant_macro_start dd ?
parameters_end dd ?
locals_counter rb 8
current_locals_prefix dd ?
anonymous_reverse dd ?
anonymous_forward dd ?
labels_list dd ?
label_hash dd ?
label_leaf dd ?
hash_tree dd ?
org_origin dq ?
org_registers dd ?
org_start dd ?
org_symbol dd ?
undefined_data_start dd ?
undefined_data_end dd ?
counter dd ?
counter_limit dd ?
error_line dd ?
error dd ?
display_buffer dd ?
structures_buffer dd ?
number_start dd ?
current_offset dd ?
value dq ?
fp_value rd 8
adjustment dq ?
symbol_identifier dd ?
address_symbol dd ?
address_high dd ?
format_flags dd ?
resolver_flags dd ?
symbols_stream dd ?
number_of_relocations dd ?
number_of_sections dd ?
stub_size dd ?
stub_file dd ?
current_section dd ?
machine dw ?
subsystem dw ?
subsystem_version dd ?
image_base dd ?
image_base_high dd ?
resource_data dd ?
resource_size dd ?
parenthesis_stack dd ?
blocks_stack dd ?
parsed_lines dd ?
logical_value_parentheses dd ?
file_extension dd ?

next_pass_needed db ?
macro_status db ?
output_format db ?
code_type db ?
labels_type db ?
prefixed_instruction db ?
virtual_data db ?
fp_sign db ?
fp_format db ?
value_size db ?
size_override db ?
address_size db ?
operand_size db ?
size_declared db ?
value_undefined db ?
value_type db ?
compare_type db ?
base_code db ?
extended_code db ?
supplemental_code db ?
postbyte_register db ?
segment_register db ?
mmx_size db ?
jump_type db ?
push_size db ?
operand_prefix db ?
rex_prefix db ?
immediate_size db ?
logical_value_wrapping db ?

characters rb 100h
converted rb 100h

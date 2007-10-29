
; flat assembler core
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

out_of_memory:
	call	fatal_error
	db	'out of memory',0
stack_overflow:
	call	fatal_error
	db	'out of stack space',0
main_file_not_found:
	call	fatal_error
	db	'source file not found',0
unexpected_end_of_file:
	call	fatal_error
	db	'unexpected end of file',0
code_cannot_be_generated:
	call	fatal_error
	db	'code cannot be generated',0
format_limitations_exceeded:
	call	fatal_error
	db	'format limitations exceeded',0
write_failed:
	call	fatal_error
	db	'write failed',0
file_not_found:
	call	assembler_error
	db	'file not found',0
error_reading_file:
	call	assembler_error
	db	'error reading file',0
invalid_file_format:
	call	assembler_error
	db	'invalid file format',0
invalid_macro_arguments:
	call	assembler_error
	db	'invalid macro arguments',0
incomplete_macro:
	call	assembler_error
	db	'incomplete macro',0
unexpected_characters:
	call	assembler_error
	db	'unexpected characters',0
invalid_argument:
	call	assembler_error
	db	'invalid argument',0
illegal_instruction:
	call	assembler_error
	db	'illegal instruction',0
invalid_operand:
	call	assembler_error
	db	'invalid operand',0
invalid_operand_size:
	call	assembler_error
	db	'invalid size of operand',0
operand_size_not_specified:
	call	assembler_error
	db	'operand size not specified',0
operand_sizes_do_not_match:
	call	assembler_error
	db	'operand sizes do not match',0
invalid_address_size:
	call	assembler_error
	db	'invalid size of address value',0
address_sizes_do_not_agree:
	call	assembler_error
	db	'address sizes do not agree',0
prefix_conflict:
	call	assembler_error
	db	'disallowed combination of registers',0
long_immediate_not_encodable:
	call	assembler_error
	db	'not encodable with long immediate',0
relative_jump_out_of_range:
	call	assembler_error
	db	'relative jump out of range',0
invalid_expression:
	call	assembler_error
	db	'invalid expression',0
invalid_address:
	call	assembler_error
	db	'invalid address',0
invalid_value:
	call	assembler_error
	db	'invalid value',0
value_out_of_range:
	call	assembler_error
	db	'value out of range',0
undefined_symbol:
	call	assembler_error
	db	'undefined symbol',0
invalid_use_of_symbol:
	call	assembler_error
	db	'invalid use of symbol',0
name_too_long:
	call	assembler_error
	db	'name too long',0
invalid_name:
	call	assembler_error
	db	'invalid name',0
reserved_word_used_as_symbol:
	call	assembler_error
	db	'reserved word used as symbol',0
symbol_already_defined:
	call	assembler_error
	db	'symbol already defined',0
missing_end_quote:
	call	assembler_error
	db	'missing end quote',0
missing_end_directive:
	call	assembler_error
	db	'missing end directive',0
unexpected_instruction:
	call	assembler_error
	db	'unexpected instruction',0
extra_characters_on_line:
	call	assembler_error
	db	'extra characters on line',0
section_not_aligned_enough:
	call	assembler_error
	db	'section is not aligned enough',0
setting_already_specified:
	call	assembler_error
	db	'setting already specified',0
data_already_defined:
	call	assembler_error
	db	'data already defined',0
too_many_repeats:
	call	assembler_error
	db	'too many repeats',0

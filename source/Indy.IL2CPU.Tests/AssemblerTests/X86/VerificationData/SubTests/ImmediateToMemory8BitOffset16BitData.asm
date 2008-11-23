use32
org 0x200000
			sub word [EAX + 203], 0x41
			sub word [EBX + 203], 0x41
			sub word [ECX + 203], 0x41
			sub word [EDX + 203], 0x41
			sub word [ESI + 203], 0x41
			sub word [EDI + 203], 0x41
			sub word [EBP + 203], 0x41
			sub word [ESP + 203], 0x41

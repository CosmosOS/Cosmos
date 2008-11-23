use32
org 0x200000
			sub byte [EAX + 203], 0x41
			sub byte [EBX + 203], 0x41
			sub byte [ECX + 203], 0x41
			sub byte [EDX + 203], 0x41
			sub byte [ESI + 203], 0x41
			sub byte [EDI + 203], 0x41
			sub byte [EBP + 203], 0x41
			sub byte [ESP + 203], 0x41

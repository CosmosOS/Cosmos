use32
org 0x200000
			add byte [EAX + 203], 0x41
			add byte [EBX + 203], 0x41
			add byte [ECX + 203], 0x41
			add byte [EDX + 203], 0x41
			add byte [ESI + 203], 0x41
			add byte [EDI + 203], 0x41
			add byte [EBP + 203], 0x41
			add byte [ESP + 203], 0x41

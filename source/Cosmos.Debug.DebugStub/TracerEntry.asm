; Generated at 20-7-2015 14:42:43





DebugStub_TracerEntry:

cli


Pushad
Mov [DebugStub_PushAllPtr], ESP
Mov [DebugStub_CallerEBP], EBP

Mov EBP, ESP
Add EBP, 32
Mov EAX, [EBP + 0]

Add EBP, 12
Mov [DebugStub_CallerESP], EBP


Mov EBX, EAX
MOV EAX, DR6
And EAX, 0x4000
Cmp EAX, 0x4000
JE DebugStub_TracerEntry_Block1_End
Dec EBX
DebugStub_TracerEntry_Block1_End:
Mov EAX, EBX

Mov [DebugStub_CallerEIP], EAX

Call DebugStub_Executing

Popad

sti

DebugStub_TracerEntry_Exit:
IRet


DebugStub_Interrupt_0:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_0_StartLoop:
hlt
Jmp DebugStub_Interrupt_0_StartLoop
DebugStub_Interrupt_0_Exit:
IRet

DebugStub_Interrupt_2:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_2_StartLoop:
hlt
Jmp DebugStub_Interrupt_2_StartLoop
DebugStub_Interrupt_2_Exit:
IRet

DebugStub_Interrupt_4:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_4_StartLoop:
hlt
Jmp DebugStub_Interrupt_4_StartLoop
DebugStub_Interrupt_4_Exit:
IRet

DebugStub_Interrupt_5:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_5_StartLoop:
hlt
Jmp DebugStub_Interrupt_5_StartLoop
DebugStub_Interrupt_5_Exit:
IRet

DebugStub_Interrupt_6:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_6_StartLoop:
hlt
Jmp DebugStub_Interrupt_6_StartLoop
DebugStub_Interrupt_6_Exit:
IRet

DebugStub_Interrupt_7:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_7_StartLoop:
hlt
Jmp DebugStub_Interrupt_7_StartLoop
DebugStub_Interrupt_7_Exit:
IRet

DebugStub_Interrupt_8:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_8_StartLoop:
hlt
Jmp DebugStub_Interrupt_8_StartLoop
DebugStub_Interrupt_8_Exit:
IRet

DebugStub_Interrupt_9:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_9_StartLoop:
hlt
Jmp DebugStub_Interrupt_9_StartLoop
DebugStub_Interrupt_9_Exit:
IRet

DebugStub_Interrupt_10:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_10_StartLoop:
hlt
Jmp DebugStub_Interrupt_10_StartLoop
DebugStub_Interrupt_10_Exit:
IRet

DebugStub_Interrupt_11:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_11_StartLoop:
hlt
Jmp DebugStub_Interrupt_11_StartLoop
DebugStub_Interrupt_11_Exit:
IRet

DebugStub_Interrupt_12:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_12_StartLoop:
hlt
Jmp DebugStub_Interrupt_12_StartLoop
DebugStub_Interrupt_12_Exit:
IRet

DebugStub_Interrupt_13:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_13_StartLoop:
hlt
Jmp DebugStub_Interrupt_13_StartLoop
DebugStub_Interrupt_13_Exit:
IRet

DebugStub_Interrupt_14:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_14_StartLoop:
hlt
Jmp DebugStub_Interrupt_14_StartLoop
DebugStub_Interrupt_14_Exit:
IRet

DebugStub_Interrupt_15:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_15_StartLoop:
hlt
Jmp DebugStub_Interrupt_15_StartLoop
DebugStub_Interrupt_15_Exit:
IRet

DebugStub_Interrupt_16:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_16_StartLoop:
hlt
Jmp DebugStub_Interrupt_16_StartLoop
DebugStub_Interrupt_16_Exit:
IRet

DebugStub_Interrupt_17:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_17_StartLoop:
hlt
Jmp DebugStub_Interrupt_17_StartLoop
DebugStub_Interrupt_17_Exit:
IRet

DebugStub_Interrupt_18:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_18_StartLoop:
hlt
Jmp DebugStub_Interrupt_18_StartLoop
DebugStub_Interrupt_18_Exit:
IRet

DebugStub_Interrupt_19:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_19_StartLoop:
hlt
Jmp DebugStub_Interrupt_19_StartLoop
DebugStub_Interrupt_19_Exit:
IRet

DebugStub_Interrupt_20:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_20_StartLoop:
hlt
Jmp DebugStub_Interrupt_20_StartLoop
DebugStub_Interrupt_20_Exit:
IRet

DebugStub_Interrupt_21:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_21_StartLoop:
hlt
Jmp DebugStub_Interrupt_21_StartLoop
DebugStub_Interrupt_21_Exit:
IRet

DebugStub_Interrupt_22:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_22_StartLoop:
hlt
Jmp DebugStub_Interrupt_22_StartLoop
DebugStub_Interrupt_22_Exit:
IRet

DebugStub_Interrupt_23:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_23_StartLoop:
hlt
Jmp DebugStub_Interrupt_23_StartLoop
DebugStub_Interrupt_23_Exit:
IRet

DebugStub_Interrupt_24:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_24_StartLoop:
hlt
Jmp DebugStub_Interrupt_24_StartLoop
DebugStub_Interrupt_24_Exit:
IRet

DebugStub_Interrupt_25:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_25_StartLoop:
hlt
Jmp DebugStub_Interrupt_25_StartLoop
DebugStub_Interrupt_25_Exit:
IRet

DebugStub_Interrupt_26:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_26_StartLoop:
hlt
Jmp DebugStub_Interrupt_26_StartLoop
DebugStub_Interrupt_26_Exit:
IRet

DebugStub_Interrupt_27:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_27_StartLoop:
hlt
Jmp DebugStub_Interrupt_27_StartLoop
DebugStub_Interrupt_27_Exit:
IRet

DebugStub_Interrupt_28:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_28_StartLoop:
hlt
Jmp DebugStub_Interrupt_28_StartLoop
DebugStub_Interrupt_28_Exit:
IRet

DebugStub_Interrupt_29:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_29_StartLoop:
hlt
Jmp DebugStub_Interrupt_29_StartLoop
DebugStub_Interrupt_29_Exit:
IRet

DebugStub_Interrupt_30:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_30_StartLoop:
hlt
Jmp DebugStub_Interrupt_30_StartLoop
DebugStub_Interrupt_30_Exit:
IRet

DebugStub_Interrupt_31:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_31_StartLoop:
hlt
Jmp DebugStub_Interrupt_31_StartLoop
DebugStub_Interrupt_31_Exit:
IRet

DebugStub_Interrupt_32:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_32_StartLoop:
hlt
Jmp DebugStub_Interrupt_32_StartLoop
DebugStub_Interrupt_32_Exit:
IRet

DebugStub_Interrupt_33:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_33_StartLoop:
hlt
Jmp DebugStub_Interrupt_33_StartLoop
DebugStub_Interrupt_33_Exit:
IRet

DebugStub_Interrupt_34:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_34_StartLoop:
hlt
Jmp DebugStub_Interrupt_34_StartLoop
DebugStub_Interrupt_34_Exit:
IRet

DebugStub_Interrupt_35:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_35_StartLoop:
hlt
Jmp DebugStub_Interrupt_35_StartLoop
DebugStub_Interrupt_35_Exit:
IRet

DebugStub_Interrupt_36:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_36_StartLoop:
hlt
Jmp DebugStub_Interrupt_36_StartLoop
DebugStub_Interrupt_36_Exit:
IRet

DebugStub_Interrupt_37:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_37_StartLoop:
hlt
Jmp DebugStub_Interrupt_37_StartLoop
DebugStub_Interrupt_37_Exit:
IRet

DebugStub_Interrupt_38:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_38_StartLoop:
hlt
Jmp DebugStub_Interrupt_38_StartLoop
DebugStub_Interrupt_38_Exit:
IRet

DebugStub_Interrupt_39:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_39_StartLoop:
hlt
Jmp DebugStub_Interrupt_39_StartLoop
DebugStub_Interrupt_39_Exit:
IRet

DebugStub_Interrupt_40:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_40_StartLoop:
hlt
Jmp DebugStub_Interrupt_40_StartLoop
DebugStub_Interrupt_40_Exit:
IRet

DebugStub_Interrupt_41:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_41_StartLoop:
hlt
Jmp DebugStub_Interrupt_41_StartLoop
DebugStub_Interrupt_41_Exit:
IRet

DebugStub_Interrupt_42:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_42_StartLoop:
hlt
Jmp DebugStub_Interrupt_42_StartLoop
DebugStub_Interrupt_42_Exit:
IRet

DebugStub_Interrupt_43:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_43_StartLoop:
hlt
Jmp DebugStub_Interrupt_43_StartLoop
DebugStub_Interrupt_43_Exit:
IRet

DebugStub_Interrupt_44:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_44_StartLoop:
hlt
Jmp DebugStub_Interrupt_44_StartLoop
DebugStub_Interrupt_44_Exit:
IRet

DebugStub_Interrupt_45:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_45_StartLoop:
hlt
Jmp DebugStub_Interrupt_45_StartLoop
DebugStub_Interrupt_45_Exit:
IRet

DebugStub_Interrupt_46:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_46_StartLoop:
hlt
Jmp DebugStub_Interrupt_46_StartLoop
DebugStub_Interrupt_46_Exit:
IRet

DebugStub_Interrupt_47:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_47_StartLoop:
hlt
Jmp DebugStub_Interrupt_47_StartLoop
DebugStub_Interrupt_47_Exit:
IRet

DebugStub_Interrupt_48:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_48_StartLoop:
hlt
Jmp DebugStub_Interrupt_48_StartLoop
DebugStub_Interrupt_48_Exit:
IRet

DebugStub_Interrupt_49:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_49_StartLoop:
hlt
Jmp DebugStub_Interrupt_49_StartLoop
DebugStub_Interrupt_49_Exit:
IRet

DebugStub_Interrupt_50:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_50_StartLoop:
hlt
Jmp DebugStub_Interrupt_50_StartLoop
DebugStub_Interrupt_50_Exit:
IRet

DebugStub_Interrupt_51:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_51_StartLoop:
hlt
Jmp DebugStub_Interrupt_51_StartLoop
DebugStub_Interrupt_51_Exit:
IRet

DebugStub_Interrupt_52:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_52_StartLoop:
hlt
Jmp DebugStub_Interrupt_52_StartLoop
DebugStub_Interrupt_52_Exit:
IRet

DebugStub_Interrupt_53:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_53_StartLoop:
hlt
Jmp DebugStub_Interrupt_53_StartLoop
DebugStub_Interrupt_53_Exit:
IRet

DebugStub_Interrupt_54:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_54_StartLoop:
hlt
Jmp DebugStub_Interrupt_54_StartLoop
DebugStub_Interrupt_54_Exit:
IRet

DebugStub_Interrupt_55:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_55_StartLoop:
hlt
Jmp DebugStub_Interrupt_55_StartLoop
DebugStub_Interrupt_55_Exit:
IRet

DebugStub_Interrupt_56:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_56_StartLoop:
hlt
Jmp DebugStub_Interrupt_56_StartLoop
DebugStub_Interrupt_56_Exit:
IRet

DebugStub_Interrupt_57:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_57_StartLoop:
hlt
Jmp DebugStub_Interrupt_57_StartLoop
DebugStub_Interrupt_57_Exit:
IRet

DebugStub_Interrupt_58:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_58_StartLoop:
hlt
Jmp DebugStub_Interrupt_58_StartLoop
DebugStub_Interrupt_58_Exit:
IRet

DebugStub_Interrupt_59:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_59_StartLoop:
hlt
Jmp DebugStub_Interrupt_59_StartLoop
DebugStub_Interrupt_59_Exit:
IRet

DebugStub_Interrupt_60:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_60_StartLoop:
hlt
Jmp DebugStub_Interrupt_60_StartLoop
DebugStub_Interrupt_60_Exit:
IRet

DebugStub_Interrupt_61:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_61_StartLoop:
hlt
Jmp DebugStub_Interrupt_61_StartLoop
DebugStub_Interrupt_61_Exit:
IRet

DebugStub_Interrupt_62:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_62_StartLoop:
hlt
Jmp DebugStub_Interrupt_62_StartLoop
DebugStub_Interrupt_62_Exit:
IRet

DebugStub_Interrupt_63:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_63_StartLoop:
hlt
Jmp DebugStub_Interrupt_63_StartLoop
DebugStub_Interrupt_63_Exit:
IRet

DebugStub_Interrupt_64:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_64_StartLoop:
hlt
Jmp DebugStub_Interrupt_64_StartLoop
DebugStub_Interrupt_64_Exit:
IRet

DebugStub_Interrupt_65:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_65_StartLoop:
hlt
Jmp DebugStub_Interrupt_65_StartLoop
DebugStub_Interrupt_65_Exit:
IRet

DebugStub_Interrupt_66:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_66_StartLoop:
hlt
Jmp DebugStub_Interrupt_66_StartLoop
DebugStub_Interrupt_66_Exit:
IRet

DebugStub_Interrupt_67:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_67_StartLoop:
hlt
Jmp DebugStub_Interrupt_67_StartLoop
DebugStub_Interrupt_67_Exit:
IRet

DebugStub_Interrupt_68:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_68_StartLoop:
hlt
Jmp DebugStub_Interrupt_68_StartLoop
DebugStub_Interrupt_68_Exit:
IRet

DebugStub_Interrupt_69:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_69_StartLoop:
hlt
Jmp DebugStub_Interrupt_69_StartLoop
DebugStub_Interrupt_69_Exit:
IRet

DebugStub_Interrupt_70:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_70_StartLoop:
hlt
Jmp DebugStub_Interrupt_70_StartLoop
DebugStub_Interrupt_70_Exit:
IRet

DebugStub_Interrupt_71:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_71_StartLoop:
hlt
Jmp DebugStub_Interrupt_71_StartLoop
DebugStub_Interrupt_71_Exit:
IRet

DebugStub_Interrupt_72:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_72_StartLoop:
hlt
Jmp DebugStub_Interrupt_72_StartLoop
DebugStub_Interrupt_72_Exit:
IRet

DebugStub_Interrupt_73:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_73_StartLoop:
hlt
Jmp DebugStub_Interrupt_73_StartLoop
DebugStub_Interrupt_73_Exit:
IRet

DebugStub_Interrupt_74:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_74_StartLoop:
hlt
Jmp DebugStub_Interrupt_74_StartLoop
DebugStub_Interrupt_74_Exit:
IRet

DebugStub_Interrupt_75:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_75_StartLoop:
hlt
Jmp DebugStub_Interrupt_75_StartLoop
DebugStub_Interrupt_75_Exit:
IRet

DebugStub_Interrupt_76:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_76_StartLoop:
hlt
Jmp DebugStub_Interrupt_76_StartLoop
DebugStub_Interrupt_76_Exit:
IRet

DebugStub_Interrupt_77:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_77_StartLoop:
hlt
Jmp DebugStub_Interrupt_77_StartLoop
DebugStub_Interrupt_77_Exit:
IRet

DebugStub_Interrupt_78:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_78_StartLoop:
hlt
Jmp DebugStub_Interrupt_78_StartLoop
DebugStub_Interrupt_78_Exit:
IRet

DebugStub_Interrupt_79:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_79_StartLoop:
hlt
Jmp DebugStub_Interrupt_79_StartLoop
DebugStub_Interrupt_79_Exit:
IRet

DebugStub_Interrupt_80:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_80_StartLoop:
hlt
Jmp DebugStub_Interrupt_80_StartLoop
DebugStub_Interrupt_80_Exit:
IRet

DebugStub_Interrupt_81:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_81_StartLoop:
hlt
Jmp DebugStub_Interrupt_81_StartLoop
DebugStub_Interrupt_81_Exit:
IRet

DebugStub_Interrupt_82:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_82_StartLoop:
hlt
Jmp DebugStub_Interrupt_82_StartLoop
DebugStub_Interrupt_82_Exit:
IRet

DebugStub_Interrupt_83:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_83_StartLoop:
hlt
Jmp DebugStub_Interrupt_83_StartLoop
DebugStub_Interrupt_83_Exit:
IRet

DebugStub_Interrupt_84:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_84_StartLoop:
hlt
Jmp DebugStub_Interrupt_84_StartLoop
DebugStub_Interrupt_84_Exit:
IRet

DebugStub_Interrupt_85:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_85_StartLoop:
hlt
Jmp DebugStub_Interrupt_85_StartLoop
DebugStub_Interrupt_85_Exit:
IRet

DebugStub_Interrupt_86:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_86_StartLoop:
hlt
Jmp DebugStub_Interrupt_86_StartLoop
DebugStub_Interrupt_86_Exit:
IRet

DebugStub_Interrupt_87:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_87_StartLoop:
hlt
Jmp DebugStub_Interrupt_87_StartLoop
DebugStub_Interrupt_87_Exit:
IRet

DebugStub_Interrupt_88:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_88_StartLoop:
hlt
Jmp DebugStub_Interrupt_88_StartLoop
DebugStub_Interrupt_88_Exit:
IRet

DebugStub_Interrupt_89:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_89_StartLoop:
hlt
Jmp DebugStub_Interrupt_89_StartLoop
DebugStub_Interrupt_89_Exit:
IRet

DebugStub_Interrupt_90:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_90_StartLoop:
hlt
Jmp DebugStub_Interrupt_90_StartLoop
DebugStub_Interrupt_90_Exit:
IRet

DebugStub_Interrupt_91:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_91_StartLoop:
hlt
Jmp DebugStub_Interrupt_91_StartLoop
DebugStub_Interrupt_91_Exit:
IRet

DebugStub_Interrupt_92:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_92_StartLoop:
hlt
Jmp DebugStub_Interrupt_92_StartLoop
DebugStub_Interrupt_92_Exit:
IRet

DebugStub_Interrupt_93:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_93_StartLoop:
hlt
Jmp DebugStub_Interrupt_93_StartLoop
DebugStub_Interrupt_93_Exit:
IRet

DebugStub_Interrupt_94:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_94_StartLoop:
hlt
Jmp DebugStub_Interrupt_94_StartLoop
DebugStub_Interrupt_94_Exit:
IRet

DebugStub_Interrupt_95:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_95_StartLoop:
hlt
Jmp DebugStub_Interrupt_95_StartLoop
DebugStub_Interrupt_95_Exit:
IRet

DebugStub_Interrupt_96:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_96_StartLoop:
hlt
Jmp DebugStub_Interrupt_96_StartLoop
DebugStub_Interrupt_96_Exit:
IRet

DebugStub_Interrupt_97:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_97_StartLoop:
hlt
Jmp DebugStub_Interrupt_97_StartLoop
DebugStub_Interrupt_97_Exit:
IRet

DebugStub_Interrupt_98:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_98_StartLoop:
hlt
Jmp DebugStub_Interrupt_98_StartLoop
DebugStub_Interrupt_98_Exit:
IRet

DebugStub_Interrupt_99:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_99_StartLoop:
hlt
Jmp DebugStub_Interrupt_99_StartLoop
DebugStub_Interrupt_99_Exit:
IRet

DebugStub_Interrupt_100:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_100_StartLoop:
hlt
Jmp DebugStub_Interrupt_100_StartLoop
DebugStub_Interrupt_100_Exit:
IRet

DebugStub_Interrupt_101:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_101_StartLoop:
hlt
Jmp DebugStub_Interrupt_101_StartLoop
DebugStub_Interrupt_101_Exit:
IRet

DebugStub_Interrupt_102:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_102_StartLoop:
hlt
Jmp DebugStub_Interrupt_102_StartLoop
DebugStub_Interrupt_102_Exit:
IRet

DebugStub_Interrupt_103:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_103_StartLoop:
hlt
Jmp DebugStub_Interrupt_103_StartLoop
DebugStub_Interrupt_103_Exit:
IRet

DebugStub_Interrupt_104:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_104_StartLoop:
hlt
Jmp DebugStub_Interrupt_104_StartLoop
DebugStub_Interrupt_104_Exit:
IRet

DebugStub_Interrupt_105:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_105_StartLoop:
hlt
Jmp DebugStub_Interrupt_105_StartLoop
DebugStub_Interrupt_105_Exit:
IRet

DebugStub_Interrupt_106:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_106_StartLoop:
hlt
Jmp DebugStub_Interrupt_106_StartLoop
DebugStub_Interrupt_106_Exit:
IRet

DebugStub_Interrupt_107:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_107_StartLoop:
hlt
Jmp DebugStub_Interrupt_107_StartLoop
DebugStub_Interrupt_107_Exit:
IRet

DebugStub_Interrupt_108:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_108_StartLoop:
hlt
Jmp DebugStub_Interrupt_108_StartLoop
DebugStub_Interrupt_108_Exit:
IRet

DebugStub_Interrupt_109:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_109_StartLoop:
hlt
Jmp DebugStub_Interrupt_109_StartLoop
DebugStub_Interrupt_109_Exit:
IRet

DebugStub_Interrupt_110:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_110_StartLoop:
hlt
Jmp DebugStub_Interrupt_110_StartLoop
DebugStub_Interrupt_110_Exit:
IRet

DebugStub_Interrupt_111:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_111_StartLoop:
hlt
Jmp DebugStub_Interrupt_111_StartLoop
DebugStub_Interrupt_111_Exit:
IRet

DebugStub_Interrupt_112:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_112_StartLoop:
hlt
Jmp DebugStub_Interrupt_112_StartLoop
DebugStub_Interrupt_112_Exit:
IRet

DebugStub_Interrupt_113:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_113_StartLoop:
hlt
Jmp DebugStub_Interrupt_113_StartLoop
DebugStub_Interrupt_113_Exit:
IRet

DebugStub_Interrupt_114:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_114_StartLoop:
hlt
Jmp DebugStub_Interrupt_114_StartLoop
DebugStub_Interrupt_114_Exit:
IRet

DebugStub_Interrupt_115:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_115_StartLoop:
hlt
Jmp DebugStub_Interrupt_115_StartLoop
DebugStub_Interrupt_115_Exit:
IRet

DebugStub_Interrupt_116:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_116_StartLoop:
hlt
Jmp DebugStub_Interrupt_116_StartLoop
DebugStub_Interrupt_116_Exit:
IRet

DebugStub_Interrupt_117:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_117_StartLoop:
hlt
Jmp DebugStub_Interrupt_117_StartLoop
DebugStub_Interrupt_117_Exit:
IRet

DebugStub_Interrupt_118:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_118_StartLoop:
hlt
Jmp DebugStub_Interrupt_118_StartLoop
DebugStub_Interrupt_118_Exit:
IRet

DebugStub_Interrupt_119:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_119_StartLoop:
hlt
Jmp DebugStub_Interrupt_119_StartLoop
DebugStub_Interrupt_119_Exit:
IRet

DebugStub_Interrupt_120:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_120_StartLoop:
hlt
Jmp DebugStub_Interrupt_120_StartLoop
DebugStub_Interrupt_120_Exit:
IRet

DebugStub_Interrupt_121:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_121_StartLoop:
hlt
Jmp DebugStub_Interrupt_121_StartLoop
DebugStub_Interrupt_121_Exit:
IRet

DebugStub_Interrupt_122:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_122_StartLoop:
hlt
Jmp DebugStub_Interrupt_122_StartLoop
DebugStub_Interrupt_122_Exit:
IRet

DebugStub_Interrupt_123:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_123_StartLoop:
hlt
Jmp DebugStub_Interrupt_123_StartLoop
DebugStub_Interrupt_123_Exit:
IRet

DebugStub_Interrupt_124:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_124_StartLoop:
hlt
Jmp DebugStub_Interrupt_124_StartLoop
DebugStub_Interrupt_124_Exit:
IRet

DebugStub_Interrupt_125:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_125_StartLoop:
hlt
Jmp DebugStub_Interrupt_125_StartLoop
DebugStub_Interrupt_125_Exit:
IRet

DebugStub_Interrupt_126:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_126_StartLoop:
hlt
Jmp DebugStub_Interrupt_126_StartLoop
DebugStub_Interrupt_126_Exit:
IRet

DebugStub_Interrupt_127:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_127_StartLoop:
hlt
Jmp DebugStub_Interrupt_127_StartLoop
DebugStub_Interrupt_127_Exit:
IRet

DebugStub_Interrupt_128:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_128_StartLoop:
hlt
Jmp DebugStub_Interrupt_128_StartLoop
DebugStub_Interrupt_128_Exit:
IRet

DebugStub_Interrupt_129:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_129_StartLoop:
hlt
Jmp DebugStub_Interrupt_129_StartLoop
DebugStub_Interrupt_129_Exit:
IRet

DebugStub_Interrupt_130:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_130_StartLoop:
hlt
Jmp DebugStub_Interrupt_130_StartLoop
DebugStub_Interrupt_130_Exit:
IRet

DebugStub_Interrupt_131:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_131_StartLoop:
hlt
Jmp DebugStub_Interrupt_131_StartLoop
DebugStub_Interrupt_131_Exit:
IRet

DebugStub_Interrupt_132:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_132_StartLoop:
hlt
Jmp DebugStub_Interrupt_132_StartLoop
DebugStub_Interrupt_132_Exit:
IRet

DebugStub_Interrupt_133:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_133_StartLoop:
hlt
Jmp DebugStub_Interrupt_133_StartLoop
DebugStub_Interrupt_133_Exit:
IRet

DebugStub_Interrupt_134:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_134_StartLoop:
hlt
Jmp DebugStub_Interrupt_134_StartLoop
DebugStub_Interrupt_134_Exit:
IRet

DebugStub_Interrupt_135:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_135_StartLoop:
hlt
Jmp DebugStub_Interrupt_135_StartLoop
DebugStub_Interrupt_135_Exit:
IRet

DebugStub_Interrupt_136:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_136_StartLoop:
hlt
Jmp DebugStub_Interrupt_136_StartLoop
DebugStub_Interrupt_136_Exit:
IRet

DebugStub_Interrupt_137:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_137_StartLoop:
hlt
Jmp DebugStub_Interrupt_137_StartLoop
DebugStub_Interrupt_137_Exit:
IRet

DebugStub_Interrupt_138:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_138_StartLoop:
hlt
Jmp DebugStub_Interrupt_138_StartLoop
DebugStub_Interrupt_138_Exit:
IRet

DebugStub_Interrupt_139:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_139_StartLoop:
hlt
Jmp DebugStub_Interrupt_139_StartLoop
DebugStub_Interrupt_139_Exit:
IRet

DebugStub_Interrupt_140:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_140_StartLoop:
hlt
Jmp DebugStub_Interrupt_140_StartLoop
DebugStub_Interrupt_140_Exit:
IRet

DebugStub_Interrupt_141:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_141_StartLoop:
hlt
Jmp DebugStub_Interrupt_141_StartLoop
DebugStub_Interrupt_141_Exit:
IRet

DebugStub_Interrupt_142:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_142_StartLoop:
hlt
Jmp DebugStub_Interrupt_142_StartLoop
DebugStub_Interrupt_142_Exit:
IRet

DebugStub_Interrupt_143:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_143_StartLoop:
hlt
Jmp DebugStub_Interrupt_143_StartLoop
DebugStub_Interrupt_143_Exit:
IRet

DebugStub_Interrupt_144:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_144_StartLoop:
hlt
Jmp DebugStub_Interrupt_144_StartLoop
DebugStub_Interrupt_144_Exit:
IRet

DebugStub_Interrupt_145:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_145_StartLoop:
hlt
Jmp DebugStub_Interrupt_145_StartLoop
DebugStub_Interrupt_145_Exit:
IRet

DebugStub_Interrupt_146:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_146_StartLoop:
hlt
Jmp DebugStub_Interrupt_146_StartLoop
DebugStub_Interrupt_146_Exit:
IRet

DebugStub_Interrupt_147:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_147_StartLoop:
hlt
Jmp DebugStub_Interrupt_147_StartLoop
DebugStub_Interrupt_147_Exit:
IRet

DebugStub_Interrupt_148:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_148_StartLoop:
hlt
Jmp DebugStub_Interrupt_148_StartLoop
DebugStub_Interrupt_148_Exit:
IRet

DebugStub_Interrupt_149:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_149_StartLoop:
hlt
Jmp DebugStub_Interrupt_149_StartLoop
DebugStub_Interrupt_149_Exit:
IRet

DebugStub_Interrupt_150:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_150_StartLoop:
hlt
Jmp DebugStub_Interrupt_150_StartLoop
DebugStub_Interrupt_150_Exit:
IRet

DebugStub_Interrupt_151:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_151_StartLoop:
hlt
Jmp DebugStub_Interrupt_151_StartLoop
DebugStub_Interrupt_151_Exit:
IRet

DebugStub_Interrupt_152:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_152_StartLoop:
hlt
Jmp DebugStub_Interrupt_152_StartLoop
DebugStub_Interrupt_152_Exit:
IRet

DebugStub_Interrupt_153:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_153_StartLoop:
hlt
Jmp DebugStub_Interrupt_153_StartLoop
DebugStub_Interrupt_153_Exit:
IRet

DebugStub_Interrupt_154:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_154_StartLoop:
hlt
Jmp DebugStub_Interrupt_154_StartLoop
DebugStub_Interrupt_154_Exit:
IRet

DebugStub_Interrupt_155:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_155_StartLoop:
hlt
Jmp DebugStub_Interrupt_155_StartLoop
DebugStub_Interrupt_155_Exit:
IRet

DebugStub_Interrupt_156:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_156_StartLoop:
hlt
Jmp DebugStub_Interrupt_156_StartLoop
DebugStub_Interrupt_156_Exit:
IRet

DebugStub_Interrupt_157:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_157_StartLoop:
hlt
Jmp DebugStub_Interrupt_157_StartLoop
DebugStub_Interrupt_157_Exit:
IRet

DebugStub_Interrupt_158:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_158_StartLoop:
hlt
Jmp DebugStub_Interrupt_158_StartLoop
DebugStub_Interrupt_158_Exit:
IRet

DebugStub_Interrupt_159:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_159_StartLoop:
hlt
Jmp DebugStub_Interrupt_159_StartLoop
DebugStub_Interrupt_159_Exit:
IRet

DebugStub_Interrupt_160:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_160_StartLoop:
hlt
Jmp DebugStub_Interrupt_160_StartLoop
DebugStub_Interrupt_160_Exit:
IRet

DebugStub_Interrupt_161:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_161_StartLoop:
hlt
Jmp DebugStub_Interrupt_161_StartLoop
DebugStub_Interrupt_161_Exit:
IRet

DebugStub_Interrupt_162:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_162_StartLoop:
hlt
Jmp DebugStub_Interrupt_162_StartLoop
DebugStub_Interrupt_162_Exit:
IRet

DebugStub_Interrupt_163:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_163_StartLoop:
hlt
Jmp DebugStub_Interrupt_163_StartLoop
DebugStub_Interrupt_163_Exit:
IRet

DebugStub_Interrupt_164:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_164_StartLoop:
hlt
Jmp DebugStub_Interrupt_164_StartLoop
DebugStub_Interrupt_164_Exit:
IRet

DebugStub_Interrupt_165:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_165_StartLoop:
hlt
Jmp DebugStub_Interrupt_165_StartLoop
DebugStub_Interrupt_165_Exit:
IRet

DebugStub_Interrupt_166:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_166_StartLoop:
hlt
Jmp DebugStub_Interrupt_166_StartLoop
DebugStub_Interrupt_166_Exit:
IRet

DebugStub_Interrupt_167:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_167_StartLoop:
hlt
Jmp DebugStub_Interrupt_167_StartLoop
DebugStub_Interrupt_167_Exit:
IRet

DebugStub_Interrupt_168:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_168_StartLoop:
hlt
Jmp DebugStub_Interrupt_168_StartLoop
DebugStub_Interrupt_168_Exit:
IRet

DebugStub_Interrupt_169:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_169_StartLoop:
hlt
Jmp DebugStub_Interrupt_169_StartLoop
DebugStub_Interrupt_169_Exit:
IRet

DebugStub_Interrupt_170:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_170_StartLoop:
hlt
Jmp DebugStub_Interrupt_170_StartLoop
DebugStub_Interrupt_170_Exit:
IRet

DebugStub_Interrupt_171:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_171_StartLoop:
hlt
Jmp DebugStub_Interrupt_171_StartLoop
DebugStub_Interrupt_171_Exit:
IRet

DebugStub_Interrupt_172:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_172_StartLoop:
hlt
Jmp DebugStub_Interrupt_172_StartLoop
DebugStub_Interrupt_172_Exit:
IRet

DebugStub_Interrupt_173:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_173_StartLoop:
hlt
Jmp DebugStub_Interrupt_173_StartLoop
DebugStub_Interrupt_173_Exit:
IRet

DebugStub_Interrupt_174:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_174_StartLoop:
hlt
Jmp DebugStub_Interrupt_174_StartLoop
DebugStub_Interrupt_174_Exit:
IRet

DebugStub_Interrupt_175:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_175_StartLoop:
hlt
Jmp DebugStub_Interrupt_175_StartLoop
DebugStub_Interrupt_175_Exit:
IRet

DebugStub_Interrupt_176:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_176_StartLoop:
hlt
Jmp DebugStub_Interrupt_176_StartLoop
DebugStub_Interrupt_176_Exit:
IRet

DebugStub_Interrupt_177:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_177_StartLoop:
hlt
Jmp DebugStub_Interrupt_177_StartLoop
DebugStub_Interrupt_177_Exit:
IRet

DebugStub_Interrupt_178:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_178_StartLoop:
hlt
Jmp DebugStub_Interrupt_178_StartLoop
DebugStub_Interrupt_178_Exit:
IRet

DebugStub_Interrupt_179:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_179_StartLoop:
hlt
Jmp DebugStub_Interrupt_179_StartLoop
DebugStub_Interrupt_179_Exit:
IRet

DebugStub_Interrupt_180:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_180_StartLoop:
hlt
Jmp DebugStub_Interrupt_180_StartLoop
DebugStub_Interrupt_180_Exit:
IRet

DebugStub_Interrupt_181:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_181_StartLoop:
hlt
Jmp DebugStub_Interrupt_181_StartLoop
DebugStub_Interrupt_181_Exit:
IRet

DebugStub_Interrupt_182:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_182_StartLoop:
hlt
Jmp DebugStub_Interrupt_182_StartLoop
DebugStub_Interrupt_182_Exit:
IRet

DebugStub_Interrupt_183:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_183_StartLoop:
hlt
Jmp DebugStub_Interrupt_183_StartLoop
DebugStub_Interrupt_183_Exit:
IRet

DebugStub_Interrupt_184:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_184_StartLoop:
hlt
Jmp DebugStub_Interrupt_184_StartLoop
DebugStub_Interrupt_184_Exit:
IRet

DebugStub_Interrupt_185:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_185_StartLoop:
hlt
Jmp DebugStub_Interrupt_185_StartLoop
DebugStub_Interrupt_185_Exit:
IRet

DebugStub_Interrupt_186:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_186_StartLoop:
hlt
Jmp DebugStub_Interrupt_186_StartLoop
DebugStub_Interrupt_186_Exit:
IRet

DebugStub_Interrupt_187:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_187_StartLoop:
hlt
Jmp DebugStub_Interrupt_187_StartLoop
DebugStub_Interrupt_187_Exit:
IRet

DebugStub_Interrupt_188:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_188_StartLoop:
hlt
Jmp DebugStub_Interrupt_188_StartLoop
DebugStub_Interrupt_188_Exit:
IRet

DebugStub_Interrupt_189:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_189_StartLoop:
hlt
Jmp DebugStub_Interrupt_189_StartLoop
DebugStub_Interrupt_189_Exit:
IRet

DebugStub_Interrupt_190:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_190_StartLoop:
hlt
Jmp DebugStub_Interrupt_190_StartLoop
DebugStub_Interrupt_190_Exit:
IRet

DebugStub_Interrupt_191:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_191_StartLoop:
hlt
Jmp DebugStub_Interrupt_191_StartLoop
DebugStub_Interrupt_191_Exit:
IRet

DebugStub_Interrupt_192:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_192_StartLoop:
hlt
Jmp DebugStub_Interrupt_192_StartLoop
DebugStub_Interrupt_192_Exit:
IRet

DebugStub_Interrupt_193:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_193_StartLoop:
hlt
Jmp DebugStub_Interrupt_193_StartLoop
DebugStub_Interrupt_193_Exit:
IRet

DebugStub_Interrupt_194:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_194_StartLoop:
hlt
Jmp DebugStub_Interrupt_194_StartLoop
DebugStub_Interrupt_194_Exit:
IRet

DebugStub_Interrupt_195:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_195_StartLoop:
hlt
Jmp DebugStub_Interrupt_195_StartLoop
DebugStub_Interrupt_195_Exit:
IRet

DebugStub_Interrupt_196:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_196_StartLoop:
hlt
Jmp DebugStub_Interrupt_196_StartLoop
DebugStub_Interrupt_196_Exit:
IRet

DebugStub_Interrupt_197:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_197_StartLoop:
hlt
Jmp DebugStub_Interrupt_197_StartLoop
DebugStub_Interrupt_197_Exit:
IRet

DebugStub_Interrupt_198:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_198_StartLoop:
hlt
Jmp DebugStub_Interrupt_198_StartLoop
DebugStub_Interrupt_198_Exit:
IRet

DebugStub_Interrupt_199:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_199_StartLoop:
hlt
Jmp DebugStub_Interrupt_199_StartLoop
DebugStub_Interrupt_199_Exit:
IRet

DebugStub_Interrupt_200:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_200_StartLoop:
hlt
Jmp DebugStub_Interrupt_200_StartLoop
DebugStub_Interrupt_200_Exit:
IRet

DebugStub_Interrupt_201:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_201_StartLoop:
hlt
Jmp DebugStub_Interrupt_201_StartLoop
DebugStub_Interrupt_201_Exit:
IRet

DebugStub_Interrupt_202:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_202_StartLoop:
hlt
Jmp DebugStub_Interrupt_202_StartLoop
DebugStub_Interrupt_202_Exit:
IRet

DebugStub_Interrupt_203:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_203_StartLoop:
hlt
Jmp DebugStub_Interrupt_203_StartLoop
DebugStub_Interrupt_203_Exit:
IRet

DebugStub_Interrupt_204:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_204_StartLoop:
hlt
Jmp DebugStub_Interrupt_204_StartLoop
DebugStub_Interrupt_204_Exit:
IRet

DebugStub_Interrupt_205:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_205_StartLoop:
hlt
Jmp DebugStub_Interrupt_205_StartLoop
DebugStub_Interrupt_205_Exit:
IRet

DebugStub_Interrupt_206:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_206_StartLoop:
hlt
Jmp DebugStub_Interrupt_206_StartLoop
DebugStub_Interrupt_206_Exit:
IRet

DebugStub_Interrupt_207:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_207_StartLoop:
hlt
Jmp DebugStub_Interrupt_207_StartLoop
DebugStub_Interrupt_207_Exit:
IRet

DebugStub_Interrupt_208:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_208_StartLoop:
hlt
Jmp DebugStub_Interrupt_208_StartLoop
DebugStub_Interrupt_208_Exit:
IRet

DebugStub_Interrupt_209:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_209_StartLoop:
hlt
Jmp DebugStub_Interrupt_209_StartLoop
DebugStub_Interrupt_209_Exit:
IRet

DebugStub_Interrupt_210:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_210_StartLoop:
hlt
Jmp DebugStub_Interrupt_210_StartLoop
DebugStub_Interrupt_210_Exit:
IRet

DebugStub_Interrupt_211:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_211_StartLoop:
hlt
Jmp DebugStub_Interrupt_211_StartLoop
DebugStub_Interrupt_211_Exit:
IRet

DebugStub_Interrupt_212:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_212_StartLoop:
hlt
Jmp DebugStub_Interrupt_212_StartLoop
DebugStub_Interrupt_212_Exit:
IRet

DebugStub_Interrupt_213:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_213_StartLoop:
hlt
Jmp DebugStub_Interrupt_213_StartLoop
DebugStub_Interrupt_213_Exit:
IRet

DebugStub_Interrupt_214:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_214_StartLoop:
hlt
Jmp DebugStub_Interrupt_214_StartLoop
DebugStub_Interrupt_214_Exit:
IRet

DebugStub_Interrupt_215:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_215_StartLoop:
hlt
Jmp DebugStub_Interrupt_215_StartLoop
DebugStub_Interrupt_215_Exit:
IRet

DebugStub_Interrupt_216:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_216_StartLoop:
hlt
Jmp DebugStub_Interrupt_216_StartLoop
DebugStub_Interrupt_216_Exit:
IRet

DebugStub_Interrupt_217:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_217_StartLoop:
hlt
Jmp DebugStub_Interrupt_217_StartLoop
DebugStub_Interrupt_217_Exit:
IRet

DebugStub_Interrupt_218:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_218_StartLoop:
hlt
Jmp DebugStub_Interrupt_218_StartLoop
DebugStub_Interrupt_218_Exit:
IRet

DebugStub_Interrupt_219:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_219_StartLoop:
hlt
Jmp DebugStub_Interrupt_219_StartLoop
DebugStub_Interrupt_219_Exit:
IRet

DebugStub_Interrupt_220:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_220_StartLoop:
hlt
Jmp DebugStub_Interrupt_220_StartLoop
DebugStub_Interrupt_220_Exit:
IRet

DebugStub_Interrupt_221:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_221_StartLoop:
hlt
Jmp DebugStub_Interrupt_221_StartLoop
DebugStub_Interrupt_221_Exit:
IRet

DebugStub_Interrupt_222:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_222_StartLoop:
hlt
Jmp DebugStub_Interrupt_222_StartLoop
DebugStub_Interrupt_222_Exit:
IRet

DebugStub_Interrupt_223:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_223_StartLoop:
hlt
Jmp DebugStub_Interrupt_223_StartLoop
DebugStub_Interrupt_223_Exit:
IRet

DebugStub_Interrupt_224:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_224_StartLoop:
hlt
Jmp DebugStub_Interrupt_224_StartLoop
DebugStub_Interrupt_224_Exit:
IRet

DebugStub_Interrupt_225:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_225_StartLoop:
hlt
Jmp DebugStub_Interrupt_225_StartLoop
DebugStub_Interrupt_225_Exit:
IRet

DebugStub_Interrupt_226:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_226_StartLoop:
hlt
Jmp DebugStub_Interrupt_226_StartLoop
DebugStub_Interrupt_226_Exit:
IRet

DebugStub_Interrupt_227:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_227_StartLoop:
hlt
Jmp DebugStub_Interrupt_227_StartLoop
DebugStub_Interrupt_227_Exit:
IRet

DebugStub_Interrupt_228:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_228_StartLoop:
hlt
Jmp DebugStub_Interrupt_228_StartLoop
DebugStub_Interrupt_228_Exit:
IRet

DebugStub_Interrupt_229:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_229_StartLoop:
hlt
Jmp DebugStub_Interrupt_229_StartLoop
DebugStub_Interrupt_229_Exit:
IRet

DebugStub_Interrupt_230:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_230_StartLoop:
hlt
Jmp DebugStub_Interrupt_230_StartLoop
DebugStub_Interrupt_230_Exit:
IRet

DebugStub_Interrupt_231:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_231_StartLoop:
hlt
Jmp DebugStub_Interrupt_231_StartLoop
DebugStub_Interrupt_231_Exit:
IRet

DebugStub_Interrupt_232:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_232_StartLoop:
hlt
Jmp DebugStub_Interrupt_232_StartLoop
DebugStub_Interrupt_232_Exit:
IRet

DebugStub_Interrupt_233:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_233_StartLoop:
hlt
Jmp DebugStub_Interrupt_233_StartLoop
DebugStub_Interrupt_233_Exit:
IRet

DebugStub_Interrupt_234:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_234_StartLoop:
hlt
Jmp DebugStub_Interrupt_234_StartLoop
DebugStub_Interrupt_234_Exit:
IRet

DebugStub_Interrupt_235:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_235_StartLoop:
hlt
Jmp DebugStub_Interrupt_235_StartLoop
DebugStub_Interrupt_235_Exit:
IRet

DebugStub_Interrupt_236:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_236_StartLoop:
hlt
Jmp DebugStub_Interrupt_236_StartLoop
DebugStub_Interrupt_236_Exit:
IRet

DebugStub_Interrupt_237:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_237_StartLoop:
hlt
Jmp DebugStub_Interrupt_237_StartLoop
DebugStub_Interrupt_237_Exit:
IRet

DebugStub_Interrupt_238:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_238_StartLoop:
hlt
Jmp DebugStub_Interrupt_238_StartLoop
DebugStub_Interrupt_238_Exit:
IRet

DebugStub_Interrupt_239:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_239_StartLoop:
hlt
Jmp DebugStub_Interrupt_239_StartLoop
DebugStub_Interrupt_239_Exit:
IRet

DebugStub_Interrupt_240:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_240_StartLoop:
hlt
Jmp DebugStub_Interrupt_240_StartLoop
DebugStub_Interrupt_240_Exit:
IRet

DebugStub_Interrupt_241:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_241_StartLoop:
hlt
Jmp DebugStub_Interrupt_241_StartLoop
DebugStub_Interrupt_241_Exit:
IRet

DebugStub_Interrupt_242:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_242_StartLoop:
hlt
Jmp DebugStub_Interrupt_242_StartLoop
DebugStub_Interrupt_242_Exit:
IRet

DebugStub_Interrupt_243:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_243_StartLoop:
hlt
Jmp DebugStub_Interrupt_243_StartLoop
DebugStub_Interrupt_243_Exit:
IRet

DebugStub_Interrupt_244:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_244_StartLoop:
hlt
Jmp DebugStub_Interrupt_244_StartLoop
DebugStub_Interrupt_244_Exit:
IRet

DebugStub_Interrupt_245:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_245_StartLoop:
hlt
Jmp DebugStub_Interrupt_245_StartLoop
DebugStub_Interrupt_245_Exit:
IRet

DebugStub_Interrupt_246:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 54

Call DebugStub_ComWriteAL
DebugStub_Interrupt_246_StartLoop:
hlt
Jmp DebugStub_Interrupt_246_StartLoop
DebugStub_Interrupt_246_Exit:
IRet

DebugStub_Interrupt_247:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 55

Call DebugStub_ComWriteAL
DebugStub_Interrupt_247_StartLoop:
hlt
Jmp DebugStub_Interrupt_247_StartLoop
DebugStub_Interrupt_247_Exit:
IRet

DebugStub_Interrupt_248:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 56

Call DebugStub_ComWriteAL
DebugStub_Interrupt_248_StartLoop:
hlt
Jmp DebugStub_Interrupt_248_StartLoop
DebugStub_Interrupt_248_Exit:
IRet

DebugStub_Interrupt_249:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
Mov eax, 57

Call DebugStub_ComWriteAL
DebugStub_Interrupt_249_StartLoop:
hlt
Jmp DebugStub_Interrupt_249_StartLoop
DebugStub_Interrupt_249_Exit:
IRet

DebugStub_Interrupt_250:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 48

Call DebugStub_ComWriteAL
DebugStub_Interrupt_250_StartLoop:
hlt
Jmp DebugStub_Interrupt_250_StartLoop
DebugStub_Interrupt_250_Exit:
IRet

DebugStub_Interrupt_251:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 49

Call DebugStub_ComWriteAL
DebugStub_Interrupt_251_StartLoop:
hlt
Jmp DebugStub_Interrupt_251_StartLoop
DebugStub_Interrupt_251_Exit:
IRet

DebugStub_Interrupt_252:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
DebugStub_Interrupt_252_StartLoop:
hlt
Jmp DebugStub_Interrupt_252_StartLoop
DebugStub_Interrupt_252_Exit:
IRet

DebugStub_Interrupt_253:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 51

Call DebugStub_ComWriteAL
DebugStub_Interrupt_253_StartLoop:
hlt
Jmp DebugStub_Interrupt_253_StartLoop
DebugStub_Interrupt_253_Exit:
IRet

DebugStub_Interrupt_254:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 52

Call DebugStub_ComWriteAL
DebugStub_Interrupt_254_StartLoop:
hlt
Jmp DebugStub_Interrupt_254_StartLoop
DebugStub_Interrupt_254_Exit:
IRet

DebugStub_Interrupt_255:
Pushad
Mov eax, 73

Call DebugStub_ComWriteAL
Mov eax, 110

Call DebugStub_ComWriteAL
Mov eax, 116

Call DebugStub_ComWriteAL
Mov eax, 50

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
Mov eax, 53

Call DebugStub_ComWriteAL
DebugStub_Interrupt_255_StartLoop:
hlt
Jmp DebugStub_Interrupt_255_StartLoop
DebugStub_Interrupt_255_Exit:
IRet



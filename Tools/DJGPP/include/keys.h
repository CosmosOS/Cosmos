/* Copyright (C) 1996 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_keys_h_
#define __dj_include_keys_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

/* Values as returned from getkey() and getxkey() */

#define K_Control_A            0x001
#define K_Control_B            0x002
#define K_Control_C            0x003
#define K_Control_D            0x004
#define K_Control_E            0x005
#define K_Control_F            0x006
#define K_Control_G            0x007
#define K_BackSpace            0x008
#define K_Control_H            0x008
#define K_Tab                  0x009
#define K_Control_I            0x009
#define K_LineFeed             0x00a
#define K_Control_J            0x00a
#define K_Control_K            0x00b
#define K_Control_L            0x00c
#define K_Return               0x00d
#define K_Control_M            0x00d
#define K_Control_N            0x00e
#define K_Control_O            0x00f
#define K_Control_P            0x010
#define K_Control_Q            0x011
#define K_Control_R            0x012
#define K_Control_S            0x013
#define K_Control_T            0x014
#define K_Control_U            0x015
#define K_Control_V            0x016
#define K_Control_W            0x017
#define K_Control_X            0x018
#define K_Control_Y            0x019
#define K_Control_Z            0x01a
#define K_Control_LBracket     0x01b
#define K_Escape               0x01b
#define K_Control_BackSlash    0x01c
#define K_Control_RBracket     0x01d
#define K_Control_Caret        0x01e
#define K_Control_Underscore   0x01f
#define K_Space                0x020
#define K_ExclamationPoint     0x021
#define K_DoubleQuote          0x022
#define K_Hash                 0x023
#define K_Dollar               0x024
#define K_Percent              0x025
#define K_Ampersand            0x026
#define K_Quote                0x027
#define K_LParen               0x028
#define K_RParen               0x029
#define K_Star                 0x02a
#define K_Plus                 0x02b
#define K_Comma                0x02c
#define K_Dash                 0x02d
#define K_Period               0x02e
#define K_Slash                0x02f
#define K_Colon                0x03a
#define K_SemiColon            0x03b
#define K_LAngle               0x03c
#define K_Equals               0x03d
#define K_RAngle               0x03e
#define K_QuestionMark         0x03f
#define K_At                   0x040
#define K_LBracket             0x05b
#define K_BackSlash            0x05c
#define K_RBracket             0x05d
#define K_Caret                0x05e
#define K_UnderScore           0x05f
#define K_BackQuote            0x060
#define K_LBrace               0x07b
#define K_Pipe                 0x07c
#define K_RBrace               0x07d
#define K_Tilde                0x07e
#define K_Control_Backspace    0x07f

#define K_Alt_Escape           0x101
#define K_Control_At           0x103
#define K_Alt_Backspace        0x10e
#define K_BackTab              0x10f
#define K_Alt_Q                0x110
#define K_Alt_W                0x111
#define K_Alt_E                0x112
#define K_Alt_R                0x113
#define K_Alt_T                0x114
#define K_Alt_Y                0x115
#define K_Alt_U                0x116
#define K_Alt_I                0x117
#define K_Alt_O                0x118
#define K_Alt_P                0x119
#define K_Alt_LBracket         0x11a
#define K_Alt_RBracket         0x11b
#define K_Alt_Return           0x11c
#define K_Alt_A                0x11e
#define K_Alt_S                0x11f
#define K_Alt_D                0x120
#define K_Alt_F                0x121
#define K_Alt_G                0x122
#define K_Alt_H                0x123
#define K_Alt_J                0x124
#define K_Alt_K                0x125
#define K_Alt_L                0x126
#define K_Alt_Semicolon        0x127
#define K_Alt_Quote            0x128
#define K_Alt_Backquote        0x129
#define K_Alt_Backslash        0x12b
#define K_Alt_Z                0x12c
#define K_Alt_X                0x12d
#define K_Alt_C                0x12e
#define K_Alt_V                0x12f
#define K_Alt_B                0x130
#define K_Alt_N                0x131
#define K_Alt_M                0x132
#define K_Alt_Comma            0x133
#define K_Alt_Period           0x134
#define K_Alt_Slash            0x135
#define K_Alt_KPStar           0x137
#define K_F1                   0x13b
#define K_F2                   0x13c
#define K_F3                   0x13d
#define K_F4                   0x13e
#define K_F5                   0x13f
#define K_F6                   0x140
#define K_F7                   0x141
#define K_F8                   0x142
#define K_F9                   0x143
#define K_F10                  0x144
#define K_Home                 0x147
#define K_Up                   0x148
#define K_PageUp               0x149
#define K_Alt_KPMinus          0x14a
#define K_Left                 0x14b
#define K_Center               0x14c
#define K_Right                0x14d
#define K_Alt_KPPlus           0x14e
#define K_End                  0x14f
#define K_Down                 0x150
#define K_PageDown             0x151
#define K_Insert               0x152
#define K_Delete               0x153
#define K_Shift_F1             0x154
#define K_Shift_F2             0x155
#define K_Shift_F3             0x156
#define K_Shift_F4             0x157
#define K_Shift_F5             0x158
#define K_Shift_F6             0x159
#define K_Shift_F7             0x15a
#define K_Shift_F8             0x15b
#define K_Shift_F9             0x15c
#define K_Shift_F10            0x15d
#define K_Control_F1           0x15e
#define K_Control_F2           0x15f
#define K_Control_F3           0x160
#define K_Control_F4           0x161
#define K_Control_F5           0x162
#define K_Control_F6           0x163
#define K_Control_F7           0x164
#define K_Control_F8           0x165
#define K_Control_F9           0x166
#define K_Control_F10          0x167
#define K_Alt_F1               0x168
#define K_Alt_F2               0x169
#define K_Alt_F3               0x16a
#define K_Alt_F4               0x16b
#define K_Alt_F5               0x16c
#define K_Alt_F6               0x16d
#define K_Alt_F7               0x16e
#define K_Alt_F8               0x16f
#define K_Alt_F9               0x170
#define K_Alt_F10              0x171
#define K_Control_Print	       0x172
#define K_Control_Left         0x173
#define K_Control_Right        0x174
#define K_Control_End          0x175
#define K_Control_PageDown     0x176
#define K_Control_Home         0x177
#define K_Alt_1                0x178
#define K_Alt_2                0x179
#define K_Alt_3                0x17a
#define K_Alt_4                0x17b
#define K_Alt_5                0x17c
#define K_Alt_6                0x17d
#define K_Alt_7                0x17e
#define K_Alt_8                0x17f
#define K_Alt_9                0x180
#define K_Alt_0                0x181
#define K_Alt_Dash             0x182
#define K_Alt_Equals           0x183
#define K_Control_PageUp       0x184
#define K_F11                  0x185
#define K_F12                  0x186
#define K_Shift_F11            0x187
#define K_Shift_F12            0x188
#define K_Control_F11          0x189
#define K_Control_F12          0x18a
#define K_Alt_F11              0x18b
#define K_Alt_F12              0x18c
#define K_Control_Up           0x18d
#define K_Control_KPDash       0x18e
#define K_Control_Center       0x18f
#define K_Control_KPPlus       0x190
#define K_Control_Down         0x191
#define K_Control_Insert       0x192
#define K_Control_Delete       0x193
#define K_Control_KPSlash      0x195
#define K_Control_KPStar       0x196
#define K_Alt_EHome            0x197
#define K_Alt_EUp              0x198
#define K_Alt_EPageUp          0x199
#define K_Alt_ELeft            0x19b
#define K_Alt_ERight           0x19d
#define K_Alt_EEnd             0x19f
#define K_Alt_EDown            0x1a0
#define K_Alt_EPageDown        0x1a1
#define K_Alt_EInsert          0x1a2
#define K_Alt_EDelete          0x1a3
#define K_Alt_KPSlash          0x1a4
#define K_Alt_Tab              0x1a5
#define K_Alt_Enter            0x1a6

#define K_EHome                0x247
#define K_EUp                  0x248
#define K_EPageUp              0x249
#define K_ELeft                0x24b
#define K_ERight               0x24d
#define K_EEnd                 0x24f
#define K_EDown                0x250
#define K_EPageDown            0x251
#define K_EInsert              0x252
#define K_EDelete              0x253
#define K_Control_ELeft        0x273
#define K_Control_ERight       0x274
#define K_Control_EEnd         0x275
#define K_Control_EPageDown    0x276
#define K_Control_EHome        0x277
#define K_Control_EPageUp      0x284
#define K_Control_EUp          0x28d
#define K_Control_EDown        0x291
#define K_Control_EInsert      0x292
#define K_Control_EDelete      0x293

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_keys_h_ */

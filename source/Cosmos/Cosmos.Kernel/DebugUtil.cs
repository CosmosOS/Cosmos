
namespace Cosmos.Kernel {
	public static class DebugUtil {
		public static void Initialize() {
			Hardware.DebugUtil.Initialize();
		}

		private static void StartLogging() {
			Hardware.DebugUtil.StartLogging();
		}

		private static void EndLogging() {
			Hardware.DebugUtil.EndLogging();
		}

		public static void SendNumber(string aModule, string aDescription, uint aNumber, byte aBits) {
			StartLogging();
			Serial.Write(0, "<Number Module=\"");
			Serial.Write(0, aModule);
			Serial.Write(0, "\" Description=\"");
			Serial.Write(0, aDescription);
			Serial.Write(0, "\" Number=\"");
			Hardware.DebugUtil.WriteNumber(aNumber, aBits);
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		public static void SendDoubleNumber(string aModule, string aDescription, uint aNumber, byte aBits, uint aNumber2, byte aBits2) {
			StartLogging();
			Serial.Write(0, "<Number Module=\"");
			Serial.Write(0, aModule);
			Serial.Write(0, "\" Description=\"");
			Serial.Write(0, aDescription);
			Serial.Write(0, "\" Number1=\"");
			Hardware.DebugUtil.WriteNumber(aNumber, aBits);
			Serial.Write(0, "\" Number2=\"");
			Hardware.DebugUtil.WriteNumber(aNumber2, aBits2);
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		public static void SendMessage(string aModule, string aData) {
			StartLogging();
			Serial.Write(0, "<Message Type=\"Info\" Module=\"");
			Serial.Write(0, aModule);
			Serial.Write(0, "\" String=\"");
			Serial.Write(0, aData);
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		public static void SendKeyboardEvent(uint aScanCode, bool aReleased) {
			StartLogging();
			Serial.Write(0, "<KeyboardEvent ScanCode=\"");
			Hardware.DebugUtil.WriteNumber(aScanCode, 32);
			Serial.Write(0, "\" Released=\"");
			if (aReleased) {
				Serial.Write(0, "true");
			} else {
				Serial.Write(0, "false");
			}
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		public static void SendError(string aModule, string aData) {
			StartLogging();
			Serial.Write(0, "<Message Type=\"Error\" Module=\"");
			Serial.Write(0, aModule);
			Serial.Write(0, "\" String=\"");
			Serial.Write(0, aData);
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		public static void SendWarning(string aModule, string aData) {
			StartLogging();
			Serial.Write(0, "<Message Type=\"Warning\" Module=\"");
			Serial.Write(0, aModule);
			Serial.Write(0, "\" String=\"");
			Serial.Write(0, aData);
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		public static void SendMM_Alloc(uint aStartAddr, uint aLength) {
			StartLogging();
			Serial.Write(0, "<MM_Alloc StartAddr=\"");
			Hardware.DebugUtil.WriteNumber(aStartAddr, 32);
			Serial.Write(0, "\" Length=\"");
			Hardware.DebugUtil.WriteNumber(aLength, 32);
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		internal static unsafe void SendExt2_GroupDescriptor(string aDescription, int aBlock, int aIndex, uint aAddresss, FileSystem.Ext2.GroupDescriptor* aDescriptor) {
			StartLogging();
			Serial.Write(0, "<Ext2_GroupDescriptor Description=\"");
			Serial.Write(0, aDescription);
			Serial.Write(0, "\" Block=\"");
			Hardware.DebugUtil.WriteNumber((uint)aBlock, 32);
			Serial.Write(0, "\" Index=\"");
			Hardware.DebugUtil.WriteNumber((uint)aIndex, 32);
			Serial.Write(0, "\" Address=\"");
			Hardware.DebugUtil.WriteNumber(aAddresss, 32);
			Serial.Write(0, "\" BlockBitmap=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->BlockBitmap, 32);
			Serial.Write(0, "\" INodeBitmap=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->INodeBitmap, 32);
			Serial.Write(0, "\" INodeTable=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->INodeTable, 32);
			Serial.Write(0, "\" FreeBlocksCount=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->FreeBlocksCount, 32);
			Serial.Write(0, "\" FreeINodesCount=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->FreeINodesCount, 32);
			Serial.Write(0, "\" UsedDirsCount=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->UsedDirsCount, 32);
			Serial.Write(0, "\" Pad=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->Pad, 32);
			Serial.WriteLine(0, "\"/>");
			EndLogging();
		}

		internal static unsafe void SendExt2_SuperBlock(string aDescription, FileSystem.Ext2.SuperBlock* aSuperBlock) {
			StartLogging();
			Serial.Write(0, "<Ext2_SuperBlock1 Description=\"");
			Serial.Write(0, aDescription);
			Serial.Write(0, "\" INodesCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->INodesCount, 32);
			Serial.Write(0, "\" BlockCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->BlockCount, 32);
			Serial.Write(0, "\" RBlocksCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->RBlocksCount, 32);
			Serial.Write(0, "\" FreeBlocksCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->FreeBlocksCount, 32);
			Serial.Write(0, "\" FreeINodesCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->FreeINodesCount, 32);
			Serial.Write(0, "\" FirstDataBlock=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->FirstDataBlock, 32);
			Serial.Write(0, "\" LogBlockSize=\"");
			Hardware.DebugUtil.WriteNumber((uint)aSuperBlock->LogBlockSize, 32);
			Serial.Write(0, "\" LogFragSize=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->LogFragSize, 32);
			Serial.Write(0, "\" BlocksPerGroup=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->BlocksPerGroup, 32);
			Serial.Write(0, "\" FragsPerGroup=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->FragsPerGroup, 32);
			Serial.Write(0, "\" INodesPerGroup=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->INodesPerGroup, 32);
			Serial.Write(0, "\" MTime=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->MTime, 32);
			Serial.Write(0, "\" WTime=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->WTime, 32);
			Serial.Write(0, "\" MntCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->MntCount, 16);
			Serial.Write(0, "\" MaxMntCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->MaxMntCount, 16);
			Serial.Write(0, "\" Magic=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Magic, 16);
			Serial.Write(0, "\" State=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->State, 16);
			Serial.Write(0, "\" Errors=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Errors, 16);
			Serial.Write(0, "\" MinorRevLevel=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->MinorRevLevel, 16);
			Serial.Write(0, "\" LastCheck=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->LastCheck, 32);
			Serial.Write(0, "\" CheckInterval=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->CheckInterval, 32);
			Serial.Write(0, "\" CreatorOS=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->CreatorOS, 32);
			Serial.Write(0, "\" RevLevel=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->RevLevel, 32);
			Serial.Write(0, "\" RefResUID=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->RefResUID, 16);
			Serial.Write(0, "\" DefResGID=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->DefResGID, 16);
			Serial.Write(0, "\" Padding1=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding1, 32);
			Serial.Write(0, "\" Padding2=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding2, 32);
			Serial.Write(0, "\" Padding3=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding3, 32);
			Serial.Write(0, "\" Padding4=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding4, 32);
			Serial.Write(0, "\" Padding5=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding5, 32);
			Serial.Write(0, "\" Padding6=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding6, 32);
			Serial.Write(0, "\" Padding7=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding7, 32);
			Serial.Write(0, "\" Padding8=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding8, 32);
			Serial.Write(0, "\" Padding9=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding9, 32);
			Serial.Write(0, "\" Padding10=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding10, 32);
			Serial.Write(0, "\" Padding11=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding11, 32);
			Serial.Write(0, "\" Padding12=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding12, 32);
			Serial.Write(0, "\" Padding13=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding13, 32);
			Serial.Write(0, "\" Padding14=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding14, 32);
			Serial.Write(0, "\" Padding15=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding15, 32);
			Serial.Write(0, "\" Padding16=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding16, 32);
			Serial.Write(0, "\" Padding17=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding17, 32);
			Serial.Write(0, "\" Padding18=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding18, 32);
			Serial.Write(0, "\" Padding19=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding19, 32);
			Serial.Write(0, "\" Padding20=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding20, 32);
			Serial.Write(0, "\" Padding21=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding21, 32);
			Serial.Write(0, "\" Padding22=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding22, 32);
			Serial.Write(0, "\" Padding23=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding23, 32);
			Serial.Write(0, "\" Padding24=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding24, 32);
			Serial.Write(0, "\" Padding25=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding25, 32);
			Serial.Write(0, "\" Padding26=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding26, 32);
			Serial.Write(0, "\" Padding27=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding27, 32);
			Serial.Write(0, "\" Padding28=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding28, 32);
			Serial.Write(0, "\" Padding29=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding29, 32);
			Serial.Write(0, "\" Padding30=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding30, 32);
			Serial.Write(0, "\" Padding31=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding31, 32);
			Serial.Write(0, "\" Padding32=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding32, 32);
			Serial.Write(0, "\" Padding33=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding33, 32);
			Serial.Write(0, "\" Padding34=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding34, 32);
			Serial.Write(0, "\" Padding35=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding35, 32);
			Serial.Write(0, "\" Padding36=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding36, 32);
			Serial.Write(0, "\" Padding37=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding37, 32);
			Serial.Write(0, "\" Padding38=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding38, 32);
			Serial.Write(0, "\" Padding39=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding39, 32);
			Serial.Write(0, "\" Padding40=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding40, 32);
			Serial.Write(0, "\" Padding41=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding41, 32);
			Serial.Write(0, "\" Padding42=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding42, 32);
			Serial.Write(0, "\" Padding43=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding43, 32);
			Serial.Write(0, "\" Padding44=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding44, 32);
			Serial.Write(0, "\" Padding45=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding45, 32);
			Serial.Write(0, "\" Padding46=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding46, 32);
			Serial.Write(0, "\" Padding47=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding47, 32);
			Serial.Write(0, "\" Padding48=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding48, 32);
			Serial.Write(0, "\" Padding49=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding49, 32);
			Serial.Write(0, "\" Padding50=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding50, 32);
			Serial.Write(0, "\" Padding51=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding51, 32);
			Serial.Write(0, "\" Padding52=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding52, 32);
			Serial.Write(0, "\" Padding53=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding53, 32);
			Serial.Write(0, "\" Padding54=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding54, 32);
			Serial.Write(0, "\" Padding55=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding55, 32);
			Serial.Write(0, "\" Padding56=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding56, 32);
			Serial.Write(0, "\" Padding57=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding57, 32);
			Serial.Write(0, "\" Padding58=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding58, 32);
			Serial.Write(0, "\" Padding59=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding59, 32);
			Serial.Write(0, "\" Padding60=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding60, 32);
			Serial.Write(0, "\" Padding61=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding61, 32);
			Serial.Write(0, "\" Padding62=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding62, 32);
			Serial.Write(0, "\" Padding63=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding63, 32);
			Serial.Write(0, "\" Padding64=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding64, 32);
			Serial.Write(0, "\" Padding65=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding65, 32);
			Serial.Write(0, "\" Padding66=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding66, 32);
			Serial.Write(0, "\" Padding67=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding67, 32);
			Serial.Write(0, "\" Padding68=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding68, 32);
			Serial.Write(0, "\" Padding69=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding69, 32);
			Serial.Write(0, "\" Padding70=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding70, 32);
			Serial.Write(0, "\" Padding71=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding71, 32);
			Serial.Write(0, "\" Padding72=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding72, 32);
			Serial.Write(0, "\" Padding73=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding73, 32);
			Serial.Write(0, "\" Padding74=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding74, 32);
			Serial.Write(0, "\" Padding75=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding75, 32);
			Serial.Write(0, "\" Padding76=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding76, 32);
			Serial.Write(0, "\" Padding77=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding77, 32);
			Serial.Write(0, "\" Padding78=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding78, 32);
			Serial.Write(0, "\" Padding79=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding79, 32);
			Serial.Write(0, "\" Padding80=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding80, 32);
			Serial.Write(0, "\" Padding81=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding81, 32);
			Serial.Write(0, "\" Padding82=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding82, 32);
			Serial.Write(0, "\" Padding83=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding83, 32);
			Serial.Write(0, "\" Padding84=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding84, 32);
			Serial.Write(0, "\" Padding85=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding85, 32);
			Serial.Write(0, "\" Padding86=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding86, 32);
			Serial.Write(0, "\" Padding87=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding87, 32);
			Serial.Write(0, "\" Padding88=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding88, 32);
			Serial.Write(0, "\" Padding89=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding89, 32);
			Serial.Write(0, "\" Padding90=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding90, 32);
			Serial.Write(0, "\" Padding91=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding91, 32);
			Serial.Write(0, "\" Padding92=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding92, 32);
			Serial.Write(0, "\" Padding93=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding93, 32);
			Serial.Write(0, "\" Padding94=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding94, 32);
			Serial.Write(0, "\" Padding95=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding95, 32);
			Serial.Write(0, "\" Padding96=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding96, 32);
			Serial.Write(0, "\" Padding97=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding97, 32);
			Serial.Write(0, "\" Padding98=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding98, 32);
			Serial.Write(0, "\" Padding99=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding99, 32);
			Serial.Write(0, "\" Padding100=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding100, 32);
			Serial.Write(0, "\" Padding101=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding101, 32);
			Serial.Write(0, "\" Padding102=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding102, 32);
			Serial.Write(0, "\" Padding103=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding103, 32);
			Serial.Write(0, "\" Padding104=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding104, 32);
			Serial.Write(0, "\" Padding105=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding105, 32);
			Serial.Write(0, "\" Padding106=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding106, 32);
			Serial.Write(0, "\" Padding107=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding107, 32);
			Serial.Write(0, "\"/>");
			EndLogging();
		}

		internal static void SendExt2_INodeMode(uint aIdentifier, ushort aMode) {
			StartLogging();
			Serial.Write(0, "<Ext2_INodeMode Identifier=\"");
			Hardware.DebugUtil.WriteNumber(aIdentifier, 32);
			Serial.Write(0, "\" Mode=\"");
			bool xAlreadySentContent = false;
			if ((aMode & 0x1) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "OthersAccessRightsExecute");
			}
			if ((aMode & 0x2) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "OthersAccessRightsWrite");
			}
			if ((aMode & 0x4) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "OthersAccessRightsRead");
			}
			if ((aMode & 0x7) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "OthersAccessRightsMask");
			}
			if ((aMode & 0x8) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "GroupAccessRightsExecute");
			}
			if ((aMode & 0x10) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "GroupAccessRightsWrite");
			}
			if ((aMode & 0x20) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "GroupAccessRightsRead");
			}
			if ((aMode & 0x38) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "GroupAccessRightsMask");
			}
			if ((aMode & 0x40) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "UserAccessRightsExecute");
			}
			if ((aMode & 0x80) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "UserAccessRightsWrite");
			}
			if ((aMode & 0x100) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "UserAccessRightsRead");
			}
			if ((aMode & 0x1C0) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "UserAccessRightsMask");
			}
			if ((aMode & 0x200) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "StickyBit");
			}
			if ((aMode & 0x400) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "SGID");
			}
			if ((aMode & 0x800) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "SUID");
			}
			if ((aMode & 0x1000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "Fifo");
			}
			if ((aMode & 0x2000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "CharacterDevice");
			}
			if ((aMode & 0x4000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "Directory");
			}
			if ((aMode & 0x6000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "BlockDevice");
			}
			if ((aMode & 0x8000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "RegularFile");
			}
			if ((aMode & 0xA000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "SymbolicLink");
			}
			if ((aMode & 0xC000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "Socket");
			}
			if ((aMode & 0xF000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Serial.Write(0, ", ");
				}
				Serial.Write(0, "FormatMask");
			}
			if (!xAlreadySentContent) {
				Serial.Write(0, "(None)");
			}
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		internal static unsafe void SendExt2_INode(uint aIdentifier, uint aBlockGroup, FileSystem.Ext2.INode* aINode) {
			StartLogging();
			Serial.Write(0, "<Ext2_INode BlockGroup=\"");
			Hardware.DebugUtil.WriteNumber(aBlockGroup, 32);
			Serial.Write(0, "\" Identifier=\"");
			Hardware.DebugUtil.WriteNumber(aIdentifier, 32);
			Serial.Write(0, "\" Mode=\"");
			Hardware.DebugUtil.WriteNumber((ushort)aINode->Mode, 16);
			Serial.Write(0, "\" UID=\"");
			Hardware.DebugUtil.WriteNumber(aINode->UID, 16);
			Serial.Write(0, "\" Size=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Size, 32);
			Serial.Write(0, "\" ATime=\"");
			Hardware.DebugUtil.WriteNumber(aINode->ATime, 32);
			Serial.Write(0, "\" CTime=\"");
			Hardware.DebugUtil.WriteNumber(aINode->CTime, 32);
			Serial.Write(0, "\" MTime=\"");
			Hardware.DebugUtil.WriteNumber(aINode->MTime, 32);
			Serial.Write(0, "\" DTime=\"");
			Hardware.DebugUtil.WriteNumber(aINode->DTime, 32);
			Serial.Write(0, "\" GID=\"");
			Hardware.DebugUtil.WriteNumber(aINode->GID, 16);
			Serial.Write(0, "\" LinksCount=\"");
			Hardware.DebugUtil.WriteNumber(aINode->LinksCount, 16);
			Serial.Write(0, "\" Blocks=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Blocks, 32);
			Serial.Write(0, "\" Flags=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Flags, 32);
			Serial.Write(0, "\" OSD1=\"");
			Hardware.DebugUtil.WriteNumber(aINode->OSD1, 32);
			Serial.Write(0, "\" Block1=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block1, 32);
			Serial.Write(0, "\" Block2=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block2, 32);
			Serial.Write(0, "\" Block3=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block3, 32);
			Serial.Write(0, "\" Block4=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block4, 32);
			Serial.Write(0, "\" Block5=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block5, 32);
			Serial.Write(0, "\" Block6=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block6, 32);
			Serial.Write(0, "\" Block7=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block7, 32);
			Serial.Write(0, "\" Block8=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block8, 32);
			Serial.Write(0, "\" Block9=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block9, 32);
			Serial.Write(0, "\" Block10=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block10, 32);
			Serial.Write(0, "\" Block11=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block11, 32);
			Serial.Write(0, "\" Block12=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block12, 32);
			Serial.Write(0, "\" Block13=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block13, 32);
			Serial.Write(0, "\" Block14=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block14, 32);
			Serial.Write(0, "\" Block15=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block15, 32);
			Serial.Write(0, "\" Generation=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Generation, 32);
			Serial.Write(0, "\" FileACL=\"");
			Hardware.DebugUtil.WriteNumber(aINode->FileACL, 32);
			Serial.Write(0, "\" DirACL=\"");
			Hardware.DebugUtil.WriteNumber(aINode->DirACL, 32);
			Serial.Write(0, "\" FAddr=\"");
			Hardware.DebugUtil.WriteNumber(aINode->FAddr, 32);
			Serial.Write(0, "\" OSD2_1=\"");
			Hardware.DebugUtil.WriteNumber(aINode->OSD2_1, 32);
			Serial.Write(0, "\" OSD2_2=\"");
			Hardware.DebugUtil.WriteNumber(aINode->OSD2_2, 32);
			Serial.Write(0, "\" OSD2_3=\"");
			Hardware.DebugUtil.WriteNumber(aINode->OSD2_3, 32);
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		internal static unsafe void SendExt2_DirectoryEntry(FileSystem.Ext2.DirectoryEntry* aEntryPtr) {
			StartLogging();
			Serial.Write(0, "<Ext2_DirectoryEntry INode=\"");
			Hardware.DebugUtil.WriteNumber(aEntryPtr->@INode, 32);
			Serial.Write(0, "\" RecordLength=\"");
			Hardware.DebugUtil.WriteNumber(aEntryPtr->RecordLength, 16);
			Serial.Write(0, "\" NameLength=\"");
			Hardware.DebugUtil.WriteNumber(aEntryPtr->NameLength, 8);
			Serial.Write(0, "\" FileType=\"");
			Hardware.DebugUtil.WriteNumber(aEntryPtr->FileType, 8);
			Serial.Write(0, "\" Name=\"0x");
			byte* xNameByte = &aEntryPtr->FirstNameChar;
			for (int i = 0; i < aEntryPtr->NameLength; i++) {
				Hardware.DebugUtil.WriteNumber(xNameByte[i], 8, false);
			}
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}
	}
}
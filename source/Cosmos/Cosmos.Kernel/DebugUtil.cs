
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
			Hardware.DebugUtil.WriteSerialString("<Number Module=\"");
			Hardware.DebugUtil.WriteSerialString(aModule);
			Hardware.DebugUtil.WriteSerialString("\" Description=\"");
			Hardware.DebugUtil.WriteSerialString(aDescription);
			Hardware.DebugUtil.WriteSerialString("\" Number=\"");
			Hardware.DebugUtil.WriteNumber(aNumber, aBits);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendDoubleNumber(string aModule, string aDescription, uint aNumber, byte aBits, uint aNumber2, byte aBits2) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<Number Module=\"");
			Hardware.DebugUtil.WriteSerialString(aModule);
			Hardware.DebugUtil.WriteSerialString("\" Description=\"");
			Hardware.DebugUtil.WriteSerialString(aDescription);
			Hardware.DebugUtil.WriteSerialString("\" Number1=\"");
			Hardware.DebugUtil.WriteNumber(aNumber, aBits);
			Hardware.DebugUtil.WriteSerialString("\" Number2=\"");
			Hardware.DebugUtil.WriteNumber(aNumber2, aBits2);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendMessage(string aModule, string aData) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<Message Type=\"Info\" Module=\"");
			Hardware.DebugUtil.WriteSerialString(aModule);
			Hardware.DebugUtil.WriteSerialString("\" String=\"");
			Hardware.DebugUtil.WriteSerialString(aData);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendKeyboardEvent(uint aScanCode, bool aReleased) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<KeyboardEvent ScanCode=\"");
			Hardware.DebugUtil.WriteNumber(aScanCode, 32);
			Hardware.DebugUtil.WriteSerialString("\" Released=\"");
			if (aReleased) {
				Hardware.DebugUtil.WriteSerialString("true");
			} else {
				Hardware.DebugUtil.WriteSerialString("false");
			}
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendError(string aModule, string aData) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<Error Module=\"");
			Hardware.DebugUtil.WriteSerialString(aModule);
			Hardware.DebugUtil.WriteSerialString("\" String=\"");
			Hardware.DebugUtil.WriteSerialString(aData);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendError(string aModule, string aDescription, uint aData, byte aBits) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<Error Module=\"");
			Hardware.DebugUtil.WriteSerialString(aModule);
			Hardware.DebugUtil.WriteSerialString("\" String=\"");
			Hardware.DebugUtil.WriteSerialString(aDescription);
			Hardware.DebugUtil.WriteSerialString("\" Data=\"");
			Hardware.DebugUtil.WriteNumber(aData, aBits);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendWarning(string aModule, string aData) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<Warning Module=\"");
			Hardware.DebugUtil.WriteSerialString(aModule);
			Hardware.DebugUtil.WriteSerialString("\" String=\"");
			Hardware.DebugUtil.WriteSerialString(aData);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendMM_Alloc(uint aStartAddr, uint aLength) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<MM_Alloc StartAddr=\"");
			Hardware.DebugUtil.WriteNumber(aStartAddr, 32);
			Hardware.DebugUtil.WriteSerialString("\" Length=\"");
			Hardware.DebugUtil.WriteNumber(aLength, 32);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendMM_Init(uint aStartAddr, uint aLength) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<MM_Init StartAddr=\"");
			Hardware.DebugUtil.WriteNumber(aStartAddr, 32);
			Hardware.DebugUtil.WriteSerialString("\" Length=\"");
			Hardware.DebugUtil.WriteNumber(aLength, 32);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendMM_Free(uint aStartAddr, uint aLength) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<MM_Free StartAddr=\"");
			Hardware.DebugUtil.WriteNumber(aStartAddr, 32);
			Hardware.DebugUtil.WriteSerialString("\" Length=\"");
			Hardware.DebugUtil.WriteNumber(aLength, 32);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		internal static unsafe void SendExt2_GroupDescriptor(string aDescription, uint aBlock, int aIndex, uint aAddresss, FileSystem.Ext2.GroupDescriptor* aDescriptor) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<Ext2_GroupDescriptor Description=\"");
			Hardware.DebugUtil.WriteSerialString(aDescription);
			Hardware.DebugUtil.WriteSerialString("\" Block=\"");
			Hardware.DebugUtil.WriteNumber((uint)aBlock, 32);
			Hardware.DebugUtil.WriteSerialString("\" Index=\"");
			Hardware.DebugUtil.WriteNumber((uint)aIndex, 32);
			Hardware.DebugUtil.WriteSerialString("\" Address=\"");
			Hardware.DebugUtil.WriteNumber(aAddresss, 32);
			Hardware.DebugUtil.WriteSerialString("\" BlockBitmap=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->BlockBitmap, 32);
			Hardware.DebugUtil.WriteSerialString("\" INodeBitmap=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->INodeBitmap, 32);
			Hardware.DebugUtil.WriteSerialString("\" INodeTable=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->INodeTable, 32);
			Hardware.DebugUtil.WriteSerialString("\" FreeBlocksCount=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->FreeBlocksCount, 32);
			Hardware.DebugUtil.WriteSerialString("\" FreeINodesCount=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->FreeINodesCount, 32);
			Hardware.DebugUtil.WriteSerialString("\" UsedDirsCount=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->UsedDirsCount, 32);
			Hardware.DebugUtil.WriteSerialString("\" Pad=\"");
			Hardware.DebugUtil.WriteNumber(aDescriptor->Pad, 32);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		internal static unsafe void SendExt2_SuperBlock(string aDescription, FileSystem.Ext2.SuperBlock* aSuperBlock) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<Ext2_SuperBlock1 Description=\"");
			Hardware.DebugUtil.WriteSerialString(aDescription);
			Hardware.DebugUtil.WriteSerialString("\" INodesCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->INodesCount, 32);
			Hardware.DebugUtil.WriteSerialString("\" BlockCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->BlockCount, 32);
			Hardware.DebugUtil.WriteSerialString("\" RBlocksCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->RBlocksCount, 32);
			Hardware.DebugUtil.WriteSerialString("\" FreeBlocksCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->FreeBlocksCount, 32);
			Hardware.DebugUtil.WriteSerialString("\" FreeINodesCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->FreeINodesCount, 32);
			Hardware.DebugUtil.WriteSerialString("\" FirstDataBlock=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->FirstDataBlock, 32);
			Hardware.DebugUtil.WriteSerialString("\" LogBlockSize=\"");
			Hardware.DebugUtil.WriteNumber((uint)aSuperBlock->LogBlockSize, 32);
			Hardware.DebugUtil.WriteSerialString("\" LogFragSize=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->LogFragSize, 32);
			Hardware.DebugUtil.WriteSerialString("\" BlocksPerGroup=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->BlocksPerGroup, 32);
			Hardware.DebugUtil.WriteSerialString("\" FragsPerGroup=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->FragsPerGroup, 32);
			Hardware.DebugUtil.WriteSerialString("\" INodesPerGroup=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->INodesPerGroup, 32);
			Hardware.DebugUtil.WriteSerialString("\" MTime=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->MTime, 32);
			Hardware.DebugUtil.WriteSerialString("\" WTime=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->WTime, 32);
			Hardware.DebugUtil.WriteSerialString("\" MntCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->MntCount, 16);
			Hardware.DebugUtil.WriteSerialString("\" MaxMntCount=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->MaxMntCount, 16);
			Hardware.DebugUtil.WriteSerialString("\" Magic=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Magic, 16);
			Hardware.DebugUtil.WriteSerialString("\" State=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->State, 16);
			Hardware.DebugUtil.WriteSerialString("\" Errors=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Errors, 16);
			Hardware.DebugUtil.WriteSerialString("\" MinorRevLevel=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->MinorRevLevel, 16);
			Hardware.DebugUtil.WriteSerialString("\" LastCheck=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->LastCheck, 32);
			Hardware.DebugUtil.WriteSerialString("\" CheckInterval=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->CheckInterval, 32);
			Hardware.DebugUtil.WriteSerialString("\" CreatorOS=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->CreatorOS, 32);
			Hardware.DebugUtil.WriteSerialString("\" RevLevel=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->RevLevel, 32);
			Hardware.DebugUtil.WriteSerialString("\" RefResUID=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->RefResUID, 16);
			Hardware.DebugUtil.WriteSerialString("\" DefResGID=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->DefResGID, 16);
			Hardware.DebugUtil.WriteSerialString("\" FirstINode=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->FirstINode, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding1=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding1, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding2=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding2, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding3=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding3, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding4=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding4, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding5=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding5, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding6=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding6, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding7=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding7, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding8=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding8, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding9=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding9, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding10=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding10, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding11=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding11, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding12=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding12, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding13=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding13, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding14=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding14, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding15=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding15, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding16=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding16, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding17=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding17, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding18=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding18, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding19=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding19, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding20=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding20, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding21=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding21, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding22=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding22, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding23=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding23, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding24=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding24, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding25=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding25, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding26=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding26, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding27=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding27, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding28=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding28, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding29=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding29, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding30=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding30, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding31=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding31, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding32=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding32, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding33=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding33, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding34=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding34, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding35=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding35, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding36=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding36, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding37=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding37, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding38=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding38, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding39=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding39, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding40=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding40, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding41=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding41, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding42=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding42, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding43=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding43, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding44=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding44, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding45=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding45, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding46=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding46, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding47=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding47, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding48=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding48, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding49=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding49, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding50=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding50, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding51=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding51, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding52=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding52, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding53=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding53, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding54=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding54, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding55=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding55, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding56=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding56, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding57=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding57, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding58=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding58, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding59=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding59, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding60=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding60, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding61=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding61, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding62=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding62, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding63=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding63, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding64=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding64, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding65=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding65, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding66=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding66, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding67=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding67, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding68=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding68, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding69=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding69, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding70=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding70, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding71=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding71, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding72=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding72, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding73=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding73, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding74=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding74, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding75=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding75, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding76=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding76, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding77=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding77, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding78=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding78, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding79=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding79, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding80=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding80, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding81=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding81, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding82=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding82, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding83=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding83, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding84=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding84, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding85=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding85, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding86=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding86, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding87=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding87, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding88=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding88, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding89=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding89, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding90=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding90, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding91=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding91, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding92=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding92, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding93=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding93, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding94=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding94, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding95=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding95, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding96=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding96, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding97=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding97, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding98=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding98, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding99=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding99, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding100=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding100, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding101=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding101, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding102=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding102, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding103=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding103, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding104=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding104, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding105=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding105, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding106=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding106, 32);
			Hardware.DebugUtil.WriteSerialString("\" Padding107=\"");
			Hardware.DebugUtil.WriteNumber(aSuperBlock->Padding107, 32);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		internal static void SendExt2_INodeMode(uint aIdentifier, ushort aMode) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<Ext2_INodeMode Identifier=\"");
			Hardware.DebugUtil.WriteNumber(aIdentifier, 32);
			Hardware.DebugUtil.WriteSerialString("\" Mode=\"");
			bool xAlreadySentContent = false;
			if ((aMode & 0x1) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("OthersAccessRightsExecute");
			}
			if ((aMode & 0x2) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("OthersAccessRightsWrite");
			}
			if ((aMode & 0x4) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("OthersAccessRightsRead");
			}
			if ((aMode & 0x7) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("OthersAccessRightsMask");
			}
			if ((aMode & 0x8) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("GroupAccessRightsExecute");
			}
			if ((aMode & 0x10) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("GroupAccessRightsWrite");
			}
			if ((aMode & 0x20) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("GroupAccessRightsRead");
			}
			if ((aMode & 0x38) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("GroupAccessRightsMask");
			}
			if ((aMode & 0x40) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("UserAccessRightsExecute");
			}
			if ((aMode & 0x80) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("UserAccessRightsWrite");
			}
			if ((aMode & 0x100) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("UserAccessRightsRead");
			}
			if ((aMode & 0x1C0) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("UserAccessRightsMask");
			}
			if ((aMode & 0x200) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("StickyBit");
			}
			if ((aMode & 0x400) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("SGID");
			}
			if ((aMode & 0x800) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("SUID");
			}
			if ((aMode & 0x1000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("Fifo");
			}
			if ((aMode & 0x2000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("CharacterDevice");
			}
			if ((aMode & 0x4000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("Directory");
			}
			if ((aMode & 0x6000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("BlockDevice");
			}
			if ((aMode & 0x8000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("RegularFile");
			}
			if ((aMode & 0xA000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("SymbolicLink");
			}
			if ((aMode & 0xC000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("Socket");
			}
			if ((aMode & 0xF000) != 0) {
				if (!xAlreadySentContent) {
					xAlreadySentContent = true;
				} else {
					Hardware.DebugUtil.WriteSerialString(", ");
				}
				Hardware.DebugUtil.WriteSerialString("FormatMask");
			}
			if (!xAlreadySentContent) {
				Hardware.DebugUtil.WriteSerialString("(None)");
			}
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		internal static unsafe void SendExt2_INode(uint aIdentifier, FileSystem.Ext2.INode* aINode) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<Ext2_INode Identifier=\"");
			Hardware.DebugUtil.WriteNumber(aIdentifier, 32);
			Hardware.DebugUtil.WriteSerialString("\" Mode=\"");
			Hardware.DebugUtil.WriteNumber((ushort)aINode->Mode, 16);
			Hardware.DebugUtil.WriteSerialString("\" UID=\"");
			Hardware.DebugUtil.WriteNumber(aINode->UID, 16);
			Hardware.DebugUtil.WriteSerialString("\" Size=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Size, 32);
			Hardware.DebugUtil.WriteSerialString("\" ATime=\"");
			Hardware.DebugUtil.WriteNumber(aINode->ATime, 32);
			Hardware.DebugUtil.WriteSerialString("\" CTime=\"");
			Hardware.DebugUtil.WriteNumber(aINode->CTime, 32);
			Hardware.DebugUtil.WriteSerialString("\" MTime=\"");
			Hardware.DebugUtil.WriteNumber(aINode->MTime, 32);
			Hardware.DebugUtil.WriteSerialString("\" DTime=\"");
			Hardware.DebugUtil.WriteNumber(aINode->DTime, 32);
			Hardware.DebugUtil.WriteSerialString("\" GID=\"");
			Hardware.DebugUtil.WriteNumber(aINode->GID, 16);
			Hardware.DebugUtil.WriteSerialString("\" LinksCount=\"");
			Hardware.DebugUtil.WriteNumber(aINode->LinksCount, 16);
			Hardware.DebugUtil.WriteSerialString("\" Blocks=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Blocks, 32);
			Hardware.DebugUtil.WriteSerialString("\" Flags=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Flags, 32);
			Hardware.DebugUtil.WriteSerialString("\" OSD1=\"");
			Hardware.DebugUtil.WriteNumber(aINode->OSD1, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block1=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block1, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block2=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block2, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block3=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block3, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block4=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block4, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block5=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block5, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block6=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block6, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block7=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block7, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block8=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block8, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block9=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block9, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block10=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block10, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block11=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block11, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block12=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block12, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block13=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block13, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block14=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block14, 32);
			Hardware.DebugUtil.WriteSerialString("\" Block15=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Block15, 32);
			Hardware.DebugUtil.WriteSerialString("\" Generation=\"");
			Hardware.DebugUtil.WriteNumber(aINode->Generation, 32);
			Hardware.DebugUtil.WriteSerialString("\" FileACL=\"");
			Hardware.DebugUtil.WriteNumber(aINode->FileACL, 32);
			Hardware.DebugUtil.WriteSerialString("\" DirACL=\"");
			Hardware.DebugUtil.WriteNumber(aINode->DirACL, 32);
			Hardware.DebugUtil.WriteSerialString("\" FAddr=\"");
			Hardware.DebugUtil.WriteNumber(aINode->FAddr, 32);
			Hardware.DebugUtil.WriteSerialString("\" OSD2_1=\"");
			Hardware.DebugUtil.WriteNumber(aINode->OSD2_1, 32);
			Hardware.DebugUtil.WriteSerialString("\" OSD2_2=\"");
			Hardware.DebugUtil.WriteNumber(aINode->OSD2_2, 32);
			Hardware.DebugUtil.WriteSerialString("\" OSD2_3=\"");
			Hardware.DebugUtil.WriteNumber(aINode->OSD2_3, 32);
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		internal static unsafe void SendExt2_DirectoryEntry(FileSystem.Ext2.DirectoryEntry* aEntryPtr) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<Ext2_DirectoryEntry INode=\"");
			Hardware.DebugUtil.WriteNumber(aEntryPtr->INodeNumber, 32);
			Hardware.DebugUtil.WriteSerialString("\" RecordLength=\"");
			Hardware.DebugUtil.WriteNumber(aEntryPtr->RecordLength, 16);
			Hardware.DebugUtil.WriteSerialString("\" NameLength=\"");
			Hardware.DebugUtil.WriteNumber(aEntryPtr->NameLength, 8);
			Hardware.DebugUtil.WriteSerialString("\" FileType=\"");
			Hardware.DebugUtil.WriteNumber(aEntryPtr->FileType, 8);
			Hardware.DebugUtil.WriteSerialString("\" Name=\"0x");
			byte* xNameByte = &aEntryPtr->FirstNameChar;
			for (int i = 0; i < aEntryPtr->NameLength; i++) {
				Hardware.DebugUtil.WriteNumber(xNameByte[i], 8, false);
			}
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		internal static void SendByteStream(string aModule, string aDescription, byte[] aContents) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<ByteStream Module=\"");
			Hardware.DebugUtil.WriteSerialString(aModule);
			Hardware.DebugUtil.WriteSerialString("\" Description=\"");
			Hardware.DebugUtil.WriteSerialString(aDescription);
			Hardware.DebugUtil.WriteSerialString("\" Contents=\"0x");
			for (int i = 0; i < aContents.Length; i++) {
				Hardware.DebugUtil.WriteNumber(aContents[i], 8, false);
			}
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
		}

		internal static unsafe void SendBytes(string aModule, string aDescription, byte* aContents, uint aIndex, uint aCount) {
			StartLogging();
			Hardware.DebugUtil.WriteSerialString("<ByteStream Module=\"");
			Hardware.DebugUtil.WriteSerialString(aModule);
			Hardware.DebugUtil.WriteSerialString("\" Description=\"");
			Hardware.DebugUtil.WriteSerialString(aDescription);
			Hardware.DebugUtil.WriteSerialString("\" Contents=\"0x");
			for (uint i = aIndex; i < aCount; i++) {
				Hardware.DebugUtil.WriteNumber(aContents[i], 8, false);
			}
			Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
		}
	}
}
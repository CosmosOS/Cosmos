namespace Cosmos.Kernel.Core.Runtime.ExceptionHandling;

internal static unsafe partial class ExceptionHelper
{
    private static partial void InitUnwindStateFromContext(ref UnwindState state, PAL_LIMITED_CONTEXT* pContext)
    {
        state.SetRegValue((int)DwarfReg.SP, pContext->SP);
        state.SetRegValue((int)DwarfReg.FP, pContext->FP);
        state.SetRegValue((int)DwarfReg.RA, pContext->RA);
        state.SetRegValue((int)DwarfReg.S1, pContext->S1);
        state.SetRegValue((int)DwarfReg.S2, pContext->S2);
        state.SetRegValue((int)DwarfReg.S3, pContext->S3);
        state.SetRegValue((int)DwarfReg.S4, pContext->S4);
        state.SetRegValue((int)DwarfReg.S5, pContext->S5);
        state.SetRegValue((int)DwarfReg.S6, pContext->S6);
        state.SetRegValue((int)DwarfReg.S7, pContext->S7);
        state.SetRegValue((int)DwarfReg.S8, pContext->S8);
        state.SetRegValue((int)DwarfReg.S9, pContext->S9);
        state.SetRegValue((int)DwarfReg.S10, pContext->S10);
        state.SetRegValue((int)DwarfReg.S11, pContext->S11);
        state.ReturnAddress = pContext->IP;
        DwarfCfiParser.InitRegRulesSameValue(ref state);
    }

    internal static partial void ProjectRegDisplay(ref UnwindState s, REGDISPLAY* rd)
    {
        // RISC-V's REGDISPLAY stores values directly, so there is no save-location pointer wiring.
        rd->SP = s.StackPointer;
        rd->FP = s.GetRegValue((int)DwarfReg.FP);
        rd->S1 = s.GetRegValue((int)DwarfReg.S1);
        rd->S2 = s.GetRegValue((int)DwarfReg.S2);
        rd->S3 = s.GetRegValue((int)DwarfReg.S3);
        rd->S4 = s.GetRegValue((int)DwarfReg.S4);
        rd->S5 = s.GetRegValue((int)DwarfReg.S5);
        rd->S6 = s.GetRegValue((int)DwarfReg.S6);
        rd->S7 = s.GetRegValue((int)DwarfReg.S7);
        rd->S8 = s.GetRegValue((int)DwarfReg.S8);
        rd->S9 = s.GetRegValue((int)DwarfReg.S9);
        rd->S10 = s.GetRegValue((int)DwarfReg.S10);
        rd->S11 = s.GetRegValue((int)DwarfReg.S11);
        rd->RA = s.GetRegValue((int)DwarfReg.RA);
    }

    internal static partial void SeedUnwindStateFromRegDisplay(ref UnwindState s, REGDISPLAY* rd, nuint ip)
    {
        s.StackPointer = rd->SP;
        s.SetRegValue((int)DwarfReg.FP, rd->FP);
        s.SetRegValue((int)DwarfReg.RA, rd->RA);
        s.SetRegValue((int)DwarfReg.S1, rd->S1);
        s.SetRegValue((int)DwarfReg.S2, rd->S2);
        s.SetRegValue((int)DwarfReg.S3, rd->S3);
        s.SetRegValue((int)DwarfReg.S4, rd->S4);
        s.SetRegValue((int)DwarfReg.S5, rd->S5);
        s.SetRegValue((int)DwarfReg.S6, rd->S6);
        s.SetRegValue((int)DwarfReg.S7, rd->S7);
        s.SetRegValue((int)DwarfReg.S8, rd->S8);
        s.SetRegValue((int)DwarfReg.S9, rd->S9);
        s.SetRegValue((int)DwarfReg.S10, rd->S10);
        s.SetRegValue((int)DwarfReg.S11, rd->S11);
        s.ReturnAddress = ip;
        DwarfCfiParser.InitRegRulesSameValue(ref s);
    }

    private static partial void PinFrameRegister(REGDISPLAY* regDisplay, nuint framePointer)
    {
        regDisplay->FP = framePointer;
    }

    private static partial void PinPass2RegDisplay(REGDISPLAY* regDisplay, nuint framePointer)
    {
        // As on x64, SP stays at unwindState.StackPointer: the catching method's
        // body SP at the call that threw. ILC's riscv64 frames keep s0 above sp
        // (s0 = sp + the size of the area below the saved s0/ra pair), so SP is
        // not FP; RhpCallCatchFunclet resumes on this SP, as upstream's stub does.
        regDisplay->FP = framePointer;
    }
}

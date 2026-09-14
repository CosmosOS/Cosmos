namespace Cosmos.Kernel.Core.Runtime;

internal static unsafe partial class ExceptionHelper
{
    private static partial void InitUnwindStateFromContext(ref UnwindState state, PAL_LIMITED_CONTEXT* pContext)
    {
        state.SetRegValue((int)DwarfReg.RBX, pContext->Rbx);
        state.SetRegValue((int)DwarfReg.RBP, pContext->Rbp);
        state.SetRegValue((int)DwarfReg.RSP, pContext->Rsp);
        state.SetRegValue((int)DwarfReg.R12, pContext->R12);
        state.SetRegValue((int)DwarfReg.R13, pContext->R13);
        state.SetRegValue((int)DwarfReg.R14, pContext->R14);
        state.SetRegValue((int)DwarfReg.R15, pContext->R15);
        state.ReturnAddress = pContext->IP;
        DwarfCfiParser.InitRegRulesSameValue(ref state);
    }

    internal static partial void ProjectRegDisplay(ref UnwindState s, REGDISPLAY* rd)
    {
        rd->Rbx = s.GetRegValue((int)DwarfReg.RBX);
        rd->Rbp = s.GetRegValue((int)DwarfReg.RBP);
        rd->Rsi = s.GetRegValue((int)DwarfReg.RSI);
        rd->Rdi = s.GetRegValue((int)DwarfReg.RDI);
        rd->R12 = s.GetRegValue((int)DwarfReg.R12);
        rd->R13 = s.GetRegValue((int)DwarfReg.R13);
        rd->R14 = s.GetRegValue((int)DwarfReg.R14);
        rd->R15 = s.GetRegValue((int)DwarfReg.R15);
        rd->SP = s.StackPointer;

        // The pRxx save-location pointers must point at the value slots inside `rd` itself so they
        // stay valid as long as `rd` is alive — wire them after the value slots are populated.
        rd->pRbx = &rd->Rbx;
        rd->pRbp = &rd->Rbp;
        rd->pRsi = &rd->Rsi;
        rd->pRdi = &rd->Rdi;
        rd->pR12 = &rd->R12;
        rd->pR13 = &rd->R13;
        rd->pR14 = &rd->R14;
        rd->pR15 = &rd->R15;
    }

    internal static partial void SeedUnwindStateFromRegDisplay(ref UnwindState s, REGDISPLAY* rd, nuint ip)
    {
        s.SetRegValue((int)DwarfReg.RBX, rd->Rbx);
        s.SetRegValue((int)DwarfReg.RBP, rd->Rbp);
        s.SetRegValue((int)DwarfReg.RSI, rd->Rsi);
        s.SetRegValue((int)DwarfReg.RDI, rd->Rdi);
        s.SetRegValue((int)DwarfReg.R12, rd->R12);
        s.SetRegValue((int)DwarfReg.R13, rd->R13);
        s.SetRegValue((int)DwarfReg.R14, rd->R14);
        s.SetRegValue((int)DwarfReg.R15, rd->R15);
        s.StackPointer = rd->SP;
        s.ReturnAddress = ip;
        DwarfCfiParser.InitRegRulesSameValue(ref s);
    }

    private static partial void PinPass1FrameRegister(REGDISPLAY* regDisplay, nuint framePointer)
    {
        regDisplay->Rbp = framePointer;
    }

    private static partial void PinPass2RegDisplay(REGDISPLAY* regDisplay, nuint framePointer)
    {
        // SP stays at unwindState.StackPointer = the CFA (the catching frame's top, NOT its RBP).
        // RhpCallCatchFunclet sets RSP to this before jumping to the funclet's resume IP, so the
        // catching method finds its frame intact and its epilogue restores the caller's regs.
        regDisplay->Rbp = framePointer;
    }
}

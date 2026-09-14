namespace Cosmos.Kernel.Core.Runtime;

internal static unsafe partial class ExceptionHelper
{
    private static partial void InitUnwindStateFromContext(ref UnwindState state, PAL_LIMITED_CONTEXT* pContext)
    {
        state.SetRegValue((int)DwarfReg.SP, pContext->SP);
        state.SetRegValue((int)DwarfReg.FP, pContext->FP);
        state.SetRegValue((int)DwarfReg.LR, pContext->LR);
        state.SetRegValue((int)DwarfReg.X19, pContext->X19);
        state.SetRegValue((int)DwarfReg.X20, pContext->X20);
        state.SetRegValue((int)DwarfReg.X21, pContext->X21);
        state.SetRegValue((int)DwarfReg.X22, pContext->X22);
        state.SetRegValue((int)DwarfReg.X23, pContext->X23);
        state.SetRegValue((int)DwarfReg.X24, pContext->X24);
        state.SetRegValue((int)DwarfReg.X25, pContext->X25);
        state.SetRegValue((int)DwarfReg.X26, pContext->X26);
        state.SetRegValue((int)DwarfReg.X27, pContext->X27);
        state.SetRegValue((int)DwarfReg.X28, pContext->X28);
        state.ReturnAddress = pContext->IP;
        DwarfCfiParser.InitRegRulesSameValue(ref state);
    }

    internal static partial void ProjectRegDisplay(ref UnwindState s, REGDISPLAY* rd)
    {
        // ARM64's REGDISPLAY stores values directly — no save-location pointer wiring.
        rd->SP = s.StackPointer;
        rd->FP = s.GetRegValue((int)DwarfReg.FP);
        rd->X19 = s.GetRegValue((int)DwarfReg.X19);
        rd->X20 = s.GetRegValue((int)DwarfReg.X20);
        rd->X21 = s.GetRegValue((int)DwarfReg.X21);
        rd->X22 = s.GetRegValue((int)DwarfReg.X22);
        rd->X23 = s.GetRegValue((int)DwarfReg.X23);
        rd->X24 = s.GetRegValue((int)DwarfReg.X24);
        rd->X25 = s.GetRegValue((int)DwarfReg.X25);
        rd->X26 = s.GetRegValue((int)DwarfReg.X26);
        rd->X27 = s.GetRegValue((int)DwarfReg.X27);
        rd->X28 = s.GetRegValue((int)DwarfReg.X28);
        rd->LR = s.GetRegValue((int)DwarfReg.LR);
    }

    internal static partial void SeedUnwindStateFromRegDisplay(ref UnwindState s, REGDISPLAY* rd, nuint ip)
    {
        s.StackPointer = rd->SP;
        s.SetRegValue((int)DwarfReg.FP, rd->FP);
        s.SetRegValue((int)DwarfReg.LR, rd->LR);
        s.SetRegValue((int)DwarfReg.X19, rd->X19);
        s.SetRegValue((int)DwarfReg.X20, rd->X20);
        s.SetRegValue((int)DwarfReg.X21, rd->X21);
        s.SetRegValue((int)DwarfReg.X22, rd->X22);
        s.SetRegValue((int)DwarfReg.X23, rd->X23);
        s.SetRegValue((int)DwarfReg.X24, rd->X24);
        s.SetRegValue((int)DwarfReg.X25, rd->X25);
        s.SetRegValue((int)DwarfReg.X26, rd->X26);
        s.SetRegValue((int)DwarfReg.X27, rd->X27);
        s.SetRegValue((int)DwarfReg.X28, rd->X28);
        s.ReturnAddress = ip;
        DwarfCfiParser.InitRegRulesSameValue(ref s);
    }

    private static partial void PinPass1FrameRegister(REGDISPLAY* regDisplay, nuint framePointer)
    {
        regDisplay->FP = framePointer;
    }

    private static partial void PinPass2RegDisplay(REGDISPLAY* regDisplay, nuint framePointer)
    {
        // NOTE: SP is set to the frame pointer here, not the CFA — RhpCallCatchFunclet's resume
        // tail assumes SP == FP in the handler frame. x64 uses the CFA instead (PR #351); aligning
        // ARM64 is a known follow-up (catch handlers in methods with a non-trivial frame).
        regDisplay->FP = framePointer;
        regDisplay->SP = framePointer;
    }
}

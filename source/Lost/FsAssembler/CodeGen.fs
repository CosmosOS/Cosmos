#light

namespace Lost.JIT.AMD64

open System
open System.Reflection

open Lost.JIT.AMD64

type StackItem=
    {   Size: int;
        IsInteger: bool;
        IsFloat: bool;
        IsSigned: bool;
        ContentType: Type;
        IsBox: bool;        }

module public CodeGen=
    let EAX = GeneralPurposeRegister.EAX
    let EBX = GeneralPurposeRegister.EBX
    let RIP = MemoryOperand(RipBased=true)
    
    let ADC dest source = AddWithCarry(dest, source)
    let ADD dest source = Add(dest, source)
    let AND dest source = And(dest, source)
    let CMP dest source = Compare(dest, source)
    
    let JMP dest = Jump(LabelOperand(Label = dest))
    
    let LABEL label = Label(Name = label)
    let CALL dest = Call(dest)
    let RET() = Return()
    let RETP(value) = Return(value)
    
    let MOV dest source = Move(dest, source)
    let POP dest = Pop(dest)
    let PUSH source = Push(source)
    
    let GenerateSub(left, right)=
        match left, right with
        | left, right when left.IsInteger && right.IsInteger && right.Size == 4 && left.Size == 4 ->
            [POP RAX;
            POP RBX;
            SUB EBX EAX;
            PUSH RBX;]
        | left, right when left.IsInteger && right.IsInteger && right.Size == 8 && left.Size == 8 ->
            [POP RAX;
            POP RBX;
            SUB RBX, RAX;
            PUSH RBX;]
        | _ ->
            raise (NotSupportedException("invalid sizes"))
    
    let GenerateAdd(left, right)=
        match left, right with
        | left, right when left.IsInteger && right.IsInteger && right.Size == 4 && left.Size == 4 ->
            [POP RAX;
            POP RBX;
            ADD EBX EAX;
            PUSH RBX;]
        | left, right when left.IsInteger && right.IsInteger && right.Size == 8 && left.Size == 8 ->
            [POP RAX;
            POP RBX;
            ADD RBX, RAX;
            PUSH RBX;]
        | _ ->
            raise (NotSupportedException("invalid sizes"))
        
    let rec GenerateCodeRec(il, offset, func, nametable, localtable, stack)=
        let add = byte 0x58
        let sub = byte 0x59
        let div = byte 0x5B
        match il with
        | add :: ilxs ->
            match stack with
            | right :: left :: stackxs ->
                GenerateAdd(left, right) :: GenerateCodeRec(ilxs, offset + 1, func, nametable, localtable, stackxs)
            | _ ->
                raise (InvalidProgramException("invalid stack for add"))
        | sub :: ilxs ->
            match stack with
            | right :: left :: stackxs ->
                GenerateSub(left, right) :: GenerateCodeRec(ilxs, offset + 1, func, nametable, localtable, stackxs)
            | _ ->
                raise (InvalidProgramException("invalid stack for sub"))
        | div :: ilxs ->
            | right :: left :: stackxs ->
                GenerateAdd(left, right) :: GenerateCodeRec(ilxs, offset + 1, func, nametable, localtable, stackxs)
            | _ ->
                raise (InvalidProgramException("invalid stack for add"))
    
    let GenerateCode(func: System.Reflection.MethodInfo, nametable)=
        List.of_array(func.GetMethodBody().GetILAsByteArray())
        GenerateCodeRec(il, 0, func, nametable, localtable, [])

open CodeGen

ADC EAX EBX
ADC (EAX + 0) EAX
ADC (EAX + RIP) EAX
LABEL "start"
JMP "start"

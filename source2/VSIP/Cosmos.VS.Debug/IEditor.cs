using System;
using System.Runtime.InteropServices;
using EnvDTE;
using tom;

namespace Cosmos.Cosmos_VS_Debug
{

    /// <summary>
    /// IEditor is the automation interface for EditorDocument.
    /// The implementation of the methods is just a wrapper over the rich
    /// edit control's object model.
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IEditor
    {
        float DefaultTabStop { get; set; }
        ITextRange Range { get; }
        ITextSelection Selection { get; }
        int SelectionProperties { get; set; }
        int FindText(string textToFind);
        int SetText(string textToSet);
        int TypeText(string textToType);
        int Cut();
        int Copy();
        int Paste();
        int Delete(long unit, long count);
        int MoveUp(int unit, int count, int extend);
        int MoveDown(int unit, int count, int extend);
        int MoveLeft(int unit, int count, int extend);
        int MoveRight(int unit, int count, int extend);
        int EndKey(int unit, int extend);
        int HomeKey(int unit, int extend);
    }
}

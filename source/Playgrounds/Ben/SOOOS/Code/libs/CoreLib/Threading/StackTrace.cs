using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.Threading
{

public class StackTrace
{
    // Fields
    private StackFrame[] frames;
    private int m_iMethodsToSkip;
    private int m_iNumOfFrames;
    public const int METHODS_TO_SKIP = 0;

    // Methods
    public StackTrace()
    {
        this.m_iNumOfFrames = 0;
        this.m_iMethodsToSkip = 0;
        this.CaptureStackTrace(0, false, null, null);
    }

    public StackTrace(bool fNeedFileInfo)
    {
        this.m_iNumOfFrames = 0;
        this.m_iMethodsToSkip = 0;
        this.CaptureStackTrace(0, fNeedFileInfo, null, null);
    }

    public StackTrace(StackFrame frame)
    {
        this.frames = new StackFrame[] { frame };
        this.m_iMethodsToSkip = 0;
        this.m_iNumOfFrames = 1;
    }

    public StackTrace(Exception e)
    {
        if (e == null)
        {
            throw new ArgumentNullException("e");
        }
        this.m_iNumOfFrames = 0;
        this.m_iMethodsToSkip = 0;
        this.CaptureStackTrace(0, false, null, e);
    }

    public StackTrace(int skipFrames)
    {
        if (skipFrames < 0)
        {
            throw new ArgumentOutOfRangeException("skipFrames", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }
        this.m_iNumOfFrames = 0;
        this.m_iMethodsToSkip = 0;
        this.CaptureStackTrace(skipFrames, false, null, null);
    }

    public StackTrace(Exception e, bool fNeedFileInfo)
    {
        if (e == null)
        {
            throw new ArgumentNullException("e");
        }
        this.m_iNumOfFrames = 0;
        this.m_iMethodsToSkip = 0;
        this.CaptureStackTrace(0, fNeedFileInfo, null, e);
    }

    public StackTrace(Exception e, int skipFrames)
    {
        if (e == null)
        {
            throw new ArgumentNullException("e");
        }
        if (skipFrames < 0)
        {
            throw new ArgumentOutOfRangeException("skipFrames", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }
        this.m_iNumOfFrames = 0;
        this.m_iMethodsToSkip = 0;
        this.CaptureStackTrace(skipFrames, false, null, e);
    }

    public StackTrace(int skipFrames, bool fNeedFileInfo)
    {
        if (skipFrames < 0)
        {
            throw new ArgumentOutOfRangeException("skipFrames", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }
        this.m_iNumOfFrames = 0;
        this.m_iMethodsToSkip = 0;
        this.CaptureStackTrace(skipFrames, fNeedFileInfo, null, null);
    }

    public StackTrace(Thread targetThread, bool needFileInfo)
    {
        this.m_iNumOfFrames = 0;
        this.m_iMethodsToSkip = 0;
        this.CaptureStackTrace(0, needFileInfo, targetThread, null);
    }

    public StackTrace(Exception e, int skipFrames, bool fNeedFileInfo)
    {
        if (e == null)
        {
            throw new ArgumentNullException("e");
        }
        if (skipFrames < 0)
        {
            throw new ArgumentOutOfRangeException("skipFrames", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }
        this.m_iNumOfFrames = 0;
        this.m_iMethodsToSkip = 0;
        this.CaptureStackTrace(skipFrames, fNeedFileInfo, null, e);
    }

    internal static int CalculateFramesToSkip(StackFrameHelper StackF, int iNumFrames)
    {
        int num = 0;
        string strB = "System.Diagnostics";
        for (int i = 0; i < iNumFrames; i++)
        {
            MethodBase methodBase = StackF.GetMethodBase(i);
            if (methodBase != null)
            {
                Type declaringType = methodBase.DeclaringType;
                if (declaringType == null)
                {
                    return num;
                }
                string strA = declaringType.Namespace;
                if ((strA == null) || (string.Compare(strA, strB, StringComparison.Ordinal) != 0))
                {
                    return num;
                }
            }
            num++;
        }
        return num;
    }

    private void CaptureStackTrace(int iSkip, bool fNeedFileInfo, Thread targetThread, Exception e)
    {
        this.m_iMethodsToSkip += iSkip;
        StackFrameHelper sfh = new StackFrameHelper(fNeedFileInfo, targetThread);
        GetStackFramesInternal(sfh, 0, e);
        this.m_iNumOfFrames = sfh.GetNumberOfFrames();
        if (this.m_iMethodsToSkip > this.m_iNumOfFrames)
        {
            this.m_iMethodsToSkip = this.m_iNumOfFrames;
        }
        if (this.m_iNumOfFrames != 0)
        {
            this.frames = new StackFrame[this.m_iNumOfFrames];
            for (int i = 0; i < this.m_iNumOfFrames; i++)
            {
                bool flag = true;
                bool flag2 = true;
                StackFrame frame = new StackFrame(flag, flag2);
                frame.SetMethodBase(sfh.GetMethodBase(i));
                frame.SetOffset(sfh.GetOffset(i));
                frame.SetILOffset(sfh.GetILOffset(i));
                if (fNeedFileInfo)
                {
                    frame.SetFileName(sfh.GetFilename(i));
                    frame.SetLineNumber(sfh.GetLineNumber(i));
                    frame.SetColumnNumber(sfh.GetColumnNumber(i));
                }
                this.frames[i] = frame;
            }
            if (e == null)
            {
                this.m_iMethodsToSkip += CalculateFramesToSkip(sfh, this.m_iNumOfFrames);
            }
            this.m_iNumOfFrames -= this.m_iMethodsToSkip;
            if (this.m_iNumOfFrames < 0)
            {
                this.m_iNumOfFrames = 0;
            }
        }
        else
        {
            this.frames = null;
        }
    }

    public virtual StackFrame GetFrame(int index)
    {
        if (((this.frames != null) && (index < this.m_iNumOfFrames)) && (index >= 0))
        {
            return this.frames[index + this.m_iMethodsToSkip];
        }
        return null;
    }

  
    public virtual StackFrame[] GetFrames()
    {
        if ((this.frames == null) || (this.m_iNumOfFrames <= 0))
        {
            return null;
        }
        StackFrame[] destinationArray = new StackFrame[this.m_iNumOfFrames];
        Array.Copy(this.frames, this.m_iMethodsToSkip, destinationArray, 0, this.m_iNumOfFrames);
        return destinationArray;
    }

    private static string GetManagedStackTraceStringHelper(bool fNeedFileInfo)
    {
        StackTrace trace = new StackTrace(0, fNeedFileInfo);
        return trace.ToString();
    }


    internal static extern void GetStackFramesInternal(StackFrameHelper sfh, int iSkip, Exception e);
    public override string ToString()
    {
        return this.ToString(TraceFormat.TrailingNewLine);
    }

    internal string ToString(TraceFormat traceFormat)
    {
        string resourceString = "at";
        string format = "in {0}:line {1}";
        if (traceFormat != TraceFormat.NoResourceLookup)
        {
            resourceString = Environment.GetResourceString("Word_At");
            format = Environment.GetResourceString("StackTrace_InFileLineNumber");
        }
        bool flag = true;
        StringBuilder builder = new StringBuilder(0xff);
        for (int i = 0; i < this.m_iNumOfFrames; i++)
        {
            StackFrame frame = this.GetFrame(i);
            MethodBase method = frame.GetMethod();
            if (method != null)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    builder.Append(Environment.NewLine);
                }
                builder.AppendFormat(CultureInfo.InvariantCulture, "   {0} ", new object[] { resourceString });
                Type declaringType = method.DeclaringType;
                if (declaringType != null)
                {
                    builder.Append(declaringType.FullName.Replace('+', '.'));
                    builder.Append(".");
                }
                builder.Append(method.Name);
                if ((method is MethodInfo) && ((MethodInfo) method).IsGenericMethod)
                {
                    Type[] genericArguments = ((MethodInfo) method).GetGenericArguments();
                    builder.Append("[");
                    int index = 0;
                    bool flag2 = true;
                    while (index < genericArguments.Length)
                    {
                        if (!flag2)
                        {
                            builder.Append(",");
                        }
                        else
                        {
                            flag2 = false;
                        }
                        builder.Append(genericArguments[index].Name);
                        index++;
                    }
                    builder.Append("]");
                }
                builder.Append("(");
                ParameterInfo[] parameters = method.GetParameters();
                bool flag3 = true;
                for (int j = 0; j < parameters.Length; j++)
                {
                    if (!flag3)
                    {
                        builder.Append(", ");
                    }
                    else
                    {
                        flag3 = false;
                    }
                    string name = "<UnknownType>";
                    if (parameters[j].ParameterType != null)
                    {
                        name = parameters[j].ParameterType.Name;
                    }
                    builder.Append(name + " " + parameters[j].Name);
                }
                builder.Append(")");
                if (frame.GetILOffset() != -1)
                {
                    string fileName = null;
                    try
                    {
                        fileName = frame.GetFileName();
                    }
                    catch (SecurityException)
                    {
                    }
                    if (fileName != null)
                    {
                        builder.Append(' ');
                        builder.AppendFormat(CultureInfo.InvariantCulture, format, new object[] { fileName, frame.GetFileLineNumber() });
                    }
                }
            }
        }
        if (traceFormat == TraceFormat.TrailingNewLine)
        {
            builder.Append(Environment.NewLine);
        }
        return builder.ToString();
    }

    // Properties
    public virtual int FrameCount
    {
        get
        {
            return this.m_iNumOfFrames;
        }
    }

    // Nested Types
    internal enum TraceFormat
    {
        Normal,
        TrailingNewLine,
        NoResourceLookup
    }
}


 

}

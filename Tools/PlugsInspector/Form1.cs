using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Cosmos.IL2CPU;
using System.Reflection;

namespace PlugsInspector
{
    public partial class Form1 : Form
    {
        PlugManager plugManager;
        bool LoadingPlugs = false;

        public NetClassPlugInfo SelectedNetClassInfo
        {
            get
            {
                if (NetPlugClassesListBox.SelectedIndex > -1)
                {
                    return (NetClassPlugInfo)NetPlugClassesListBox.Items[NetPlugClassesListBox.SelectedIndex];
                }
                return null;
            }
        }
        public CosmosClassPlugInfo SelectedCosmosClassInfo
        {
            get
            {
                if (CosmosPlugClassesListBox.SelectedIndex > -1)
                {
                    return (CosmosClassPlugInfo)CosmosPlugClassesListBox.Items[CosmosPlugClassesListBox.SelectedIndex];
                }
                return null;
            }
        }
        public MethodPlugInfo SelectedPluggedMethodInfo
        {
            get
            {
                if (PluggedMethodsListBox.SelectedIndex > -1)
                {
                    return (MethodPlugInfo)PluggedMethodsListBox.Items[PluggedMethodsListBox.SelectedIndex];
                }
                return null;
            }
        }
        public MethodPlugInfo SelectedUnPluggedMethodInfo
        {
            get
            {
                if (UnPluggedMethodsListBox.SelectedIndex > -1)
                {
                    return (MethodPlugInfo)UnPluggedMethodsListBox.Items[UnPluggedMethodsListBox.SelectedIndex];
                }
                return null;
            }
        }

        public Form1()
        {
            InitializeComponent();

            //Force it to load/include all plugs assemblies so all types are correctly found
            //Otherwise the CLR's delay loading techniques block us...
            AssembliesPreloader.LoadAllAssemblies();

            plugManager = new PlugManager(
                ex => { AddExceptionEntry(ex.Message); }, warning => { }) {ThrowExceptions = false};
        }

        private void LoadPlugsButton_Click(object sender, EventArgs e)
        {
            if (!LoadingPlugs)
            {
                NetPlugClassesListBox.Items.Clear();
                CosmosPlugClassesListBox.Items.Clear();
                PluggedMethodsListBox.Items.Clear();
                UnPluggedMethodsListBox.Items.Clear();
                ExceptionsListBox.Items.Clear();

                new Task(LoadPlugs).Start();
            }
        }
        private void LoadPlugs()
        {
            if (LoadingPlugs)
                return;

            try
            {
                LoadingPlugs = true;

                plugManager.Clean();

                plugManager.FindPlugImpls();
                plugManager.ScanFoundPlugs();
                foreach (Type pluggedClassType in plugManager.PlugImpls.Keys)
                {
                    AddPlugEntry(new NetClassPlugInfo(pluggedClassType), NetPlugClassesListBox);
                }
                foreach (Type pluggedClassType in plugManager.PlugImplsInhrt.Keys)
                {
                    AddPlugEntry(new NetClassPlugInfo(pluggedClassType), NetPlugClassesListBox);
                }
                foreach (Type pluggedFieldType in plugManager.PlugFields.Keys)
                {
                    AddPlugEntry(new NetClassPlugInfo(pluggedFieldType), NetPlugClassesListBox);
                }
            }
            catch(Exception ex)
            {
                AddExceptionEntry("Error loading plugs: " + ex.Message);
            }
            finally
            {
                LoadingPlugs = false;
            }
        }
        private void ScanMethod(MethodBase aMethod, bool aIsPlug, object sourceItem)
        {
            //Hmm...
        }


        private delegate void VoidDelegate();
        private delegate void AddPlugEntryCallback(PlugInfo anInfo, ListBox aBox);
        private void AddPlugEntry(PlugInfo anInfo, ListBox aBox)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AddPlugEntryCallback(this.AddPlugEntry), new object[] { anInfo, aBox });
            }
            else
            {
                aBox.Items.Add(anInfo);
            }
        }
        private delegate void AddExceptionEntryCallback(string text);
        private void AddExceptionEntry(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AddExceptionEntryCallback(this.AddExceptionEntry), new object[] { text });
            }
            else
            {
                this.ExceptionsListBox.Items.Add(text);
            }
        }


        private void NetPlugCalssesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CosmosPlugClassesListBox.Items.Clear();
            PluggedMethodsListBox.Items.Clear();
            UnPluggedMethodsListBox.Items.Clear();

            NetClassPlugInfo plugInfo = SelectedNetClassInfo;
            if(SelectedNetClassInfo != null)
            {
                List<Type> cosmosPlugClassTypes = plugManager.PlugImpls[plugInfo.NetClassType];
                foreach(Type aCosmosPlugClassType in cosmosPlugClassTypes)
                {
                    AddPlugEntry(new CosmosClassPlugInfo(aCosmosPlugClassType), CosmosPlugClassesListBox);
                }

                //Get all methods on the Net class
                //Then try and resolve them :)

                Type netType = plugInfo.NetClassType;

                var netMethods = netType.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                   BindingFlags.NonPublic | BindingFlags.Public);
                foreach (System.Reflection.MethodInfo netMethodInfo in netMethods)
                {
                    var netParams = netMethodInfo.GetParameters();
                    var netParamTypes = netParams.Select(q => q.ParameterType).ToArray();
                    var cosmosMethodInfo = plugManager.ResolvePlug(netMethodInfo, netParamTypes);

                    MethodPlugInfo methodPlugInfo = new MethodPlugInfo(netMethodInfo, cosmosMethodInfo);
                    if (cosmosMethodInfo != null)
                    {
                        AddPlugEntry(methodPlugInfo, PluggedMethodsListBox);
                    }
                    else
                    {
                        AddPlugEntry(methodPlugInfo, UnPluggedMethodsListBox);
                    }
                }
            }
        }

        private void RunWithNullParamsButton_Click(object sender, EventArgs e)
        {
            MethodPlugInfo plugInfo = SelectedPluggedMethodInfo;
            if (plugInfo != null)
            {
                MethodBase cosmosPlugMethod = plugInfo.CosmosMethodInfo;
                try
                {
                    //I'm expecting all plug methods to be static (because they are at the moment).
                    //Should this ever change, the following code will need a complete re-write

                    var netParams = cosmosPlugMethod.GetParameters();
                    object[] paramVals = new object[netParams.Length];
                    for (int i = 0; i < paramVals.Length; i++)
                    {
                        paramVals[i] = null;
                    }
                    object result = cosmosPlugMethod.Invoke(null, paramVals);
                    RunResultBox.Text = "Successful! \r\nOutput object:\r\n" + result.ToString();
                }
                catch (TargetInvocationException ex)
                {
                    RunResultBox.Text = "Target threw exception: " + ex.InnerException.GetType().Name + "\r\n" + ex.InnerException.Message;
                }
                catch (Exception ex)
                {
                    RunResultBox.Text = "Exception (probably invalid parameters): " + ex.GetType().Name + "\r\n" + ex.Message;
                }
            }
        }

    }

    public abstract class PlugInfo
    {
        public PlugInfo()
        {
        }
    }
    public class NetClassPlugInfo : PlugInfo
    {
        public Type NetClassType;
        public NetClassPlugInfo(Type aNetClassType)
        {
            NetClassType = aNetClassType;
        }

        public override string ToString()
        {
            return NetClassType.Namespace + "." + NetClassType.Name;
        }
    }
    public class CosmosClassPlugInfo : PlugInfo
    {
        public Type CosmosClassType;
        public CosmosClassPlugInfo(Type aCosmosClassType)
        {
            CosmosClassType = aCosmosClassType;
        }

        public override string ToString()
        {
            return CosmosClassType.Namespace + "." + CosmosClassType.Name;
        }
    }
    public class MethodPlugInfo : PlugInfo
    {
        public MethodBase NetMethodInfo;
        public MethodBase CosmosMethodInfo;
        public MethodPlugInfo(MethodBase aNetMethodInfo, MethodBase aCosmosMethodInfo)
        {
            NetMethodInfo = aNetMethodInfo;
            CosmosMethodInfo = aCosmosMethodInfo;
        }

        public override string ToString()
        {
            string name = NetMethodInfo.Name;

            name += "(";
            var netParams = NetMethodInfo.GetParameters();
            var netParamTypes = netParams.Select(q => q.ParameterType).ToArray();
            bool first = true;
            foreach (Type aType in netParamTypes)
            {
                if(!first)
                {
                    name += ", ";
                }
                first = false;
                name += aType.Name;
            }
            name += ")";

            return name;
        }
    }
}

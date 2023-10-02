using Godot;
using System;
using System.Collections.Specialized;
using System.Threading;

namespace DarkRift.Server.Godot
{
    public partial class XmlGodotServer : Node
    {
        /// <summary>
        ///     The actual server.
        /// </summary>
        public DarkRiftServer Server { get; private set; }

#pragma warning disable IDE0044 // Add readonly modifier, Unity can't serialize readonly fields
        [Export(PropertyHint.File, "*.xml")]
        private string configuration;

        [Export]
        //[Tooltip("Indicates whether the server will be created in the OnEnable method.")]
        private bool createOnEnable = true;

        [Export]
        //[Tooltip("Indicates whether the server events will be routed through the dispatcher or just invoked.")]
        private bool eventsFromDispatcher = true;
#pragma warning restore IDE0044 // Add readonly modifier, Unity can't serialize readonly fields

        XmlParser parser = new();

        public override void _EnterTree()
        {
            //If createOnEnable is selected create a server
            if (createOnEnable)
                Create();
        }

        public override void _Process(double delta)
        {
            //Execute all queued dispatcher tasks
            if (Server != null)
                Server.ExecuteDispatcherTasks();
        }

        /// <summary>
        ///     Creates the server.
        /// </summary>
        public void Create()
        {
            Create(new NameValueCollection());
        }

        /// <summary>
        ///     Creates the server.
        /// </summary>
        public void Create(NameValueCollection variables)
        {
            if (Server != null)
                throw new InvalidOperationException("The server has already been created! (Is CreateOnEnable enabled?)");
            
            if (FileAccess.FileExists(configuration))
            {
                // Create spawn data from config
                ServerSpawnData spawnData = ServerSpawnData.CreateFromXml(ProjectSettings.GlobalizePath(configuration), variables);

                // Allow only this thread to execute dispatcher tasks to enable deadlock protection
                spawnData.DispatcherExecutorThreadID = Thread.CurrentThread.ManagedThreadId;

                // Inaccessible from XML, set from inspector
                spawnData.EventsFromDispatcher = eventsFromDispatcher;

                // Unity is broken, work around it...
                // This is an obsolete property but is still used if the user is using obsolete <server> tag properties
#pragma warning disable 0618
                spawnData.Server.UseFallbackNetworking = true;
#pragma warning restore 0618

                // Add types
                spawnData.PluginSearch.PluginTypes.AddRange(GodotServerHelper.SearchForPlugins());
                spawnData.PluginSearch.PluginTypes.Add(typeof(GodotConsoleWriter));

                // Create server
                Server = new DarkRiftServer(spawnData);
                Server.StartServer();
            }
            else
                GD.PushError("No configuration file specified! Please ensure there is a configuration file set on the XmlUnityServer component before starting the server.");
        }

        public override void _Notification(int what)
        {
            if (what == NotificationWMCloseRequest) //The application is closing
            {
                Close();
            }
            else if(what == NotificationDisabled) //The node has been disabled
            {
                Close();
            }
            else if(what == NotificationEnabled) //The node has been re-enabled
            {
                if (createOnEnable)
                    Create();
            }
        }

        /// <summary>
        ///     Closes the server.
        /// </summary>
        public void Close()
        {
            if (Server != null)
                Server.Dispose();
        }
    }
}

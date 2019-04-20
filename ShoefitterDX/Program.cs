/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShoefitterDX
{
    static class Program
    {
        private const string INIFilename = "ShoefitterDX.ini";

        public static SAGESharp.INIConfig Config { get; private set; }
        public static Window Window { get; private set; }

        public static event EventHandler<Project> ProjectClosed;
        public static event EventHandler<Project> ProjectOpened;
        private static Project _project = null;
        public static Project Project
        {
            get => _project;
            set
            {
                if (Project != null)
                {
                    ProjectClosed?.Invoke(null, Project);
                }
                _project = value;
                AssetManager = null;
                if (Project != null)
                {
                    AssetManager = new IO.AssetManager(System.IO.Path.Combine(Project.GameDirectory, GAME_DATA_DIRECTORY), System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Project.Filename), Project.SUBDIRECTORY_DATA));
                    ProjectOpened?.Invoke(null, Project);
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Config = new SAGESharp.INIConfig(INIFilename);

            // Shoefitter-DX Entrypoint

            Window = new Window();
            Application.Run(Window);
            Config.Write(INIFilename);
        }
    }
}
